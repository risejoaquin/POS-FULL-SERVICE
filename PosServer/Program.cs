using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PosInfrastructure.Data.Server;
using PosDomain.Entities;
using BCrypt.Net;
using System.Net;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.HttpOverrides;
using System.IO;
using System;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PosApplication.Interfaces.Server.ITenantContext, PosApplication.Interfaces.Server.TenantContext>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.IOrderService, PosInfrastructure.Services.Server.OrderService>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.IProductService, PosInfrastructure.Services.Server.ProductService>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.IShiftService, PosInfrastructure.Services.Server.ShiftService>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.IUserService, PosInfrastructure.Services.Server.UserService>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.IInventoryMovementService, PosInfrastructure.Services.Server.InventoryMovementService>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.IAuthService, PosInfrastructure.Services.Server.AuthService>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.ISyncService, PosInfrastructure.Services.Server.SyncService>();
builder.Services.AddScoped<PosApplication.Interfaces.Server.ILicenseService, PosInfrastructure.Services.Server.LicenseService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("LoginPolicy", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
    options.AddFixedWindowLimiter("DefaultPolicy", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});
builder.Services.AddSwaggerGen();

// Configure Database
var connString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var envDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(envDbUrl)) {
    connString = envDbUrl;
}
if (connString.StartsWith("\"") && connString.EndsWith("\"")) {
    connString = connString.Trim('"');
}
if (connString.StartsWith("postgres://") || connString.StartsWith("postgresql://")) {
    var uri = new Uri(connString);
    var userInfo = uri.UserInfo.Split(':', 2); // Limit split to 2 in case password has colon
    var username = WebUtility.UrlDecode(userInfo[0]);
    var password = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : "";
    connString = $"Host={uri.Host};Port={(uri.IsDefaultPort ? 5432 : uri.Port)};Database={uri.LocalPath.TrimStart('/')};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=True";
}

if (builder.Configuration.GetValue<bool>("EnableLegacyTimestamp"))
{
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}
if ((connString.Contains("supabase.com") || connString.Contains("pooler")) && builder.Configuration.GetValue<bool>("ApplySupabaseFix", true))
{
    // Fix for Supabase Transaction Pooler (pgbouncer) which breaks EF Core Prepared Statements
    if (!connString.Contains("Max Auto Prepare"))
    {
        connString += ";Max Auto Prepare=0;Pooling=false;";
    }
}

builder.Services.AddDbContext<CentralDbContext>(options => {
    if (connString.Contains("sqlite") || connString.Contains("Sqlite") || connString.Contains("Data Source=") && !connString.Contains("Host="))
    {
        options.UseSqlite(connString);
    }
    else
    {
        options.UseNpgsql(connString, o => {
            o.CommandTimeout(120);
            o.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
        });
    }
});

// Configure JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];

if (builder.Environment.IsProduction())
{
    if (string.IsNullOrEmpty(jwtKey)) throw new InvalidOperationException("Missing JWT_KEY environment variable in production.");
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_ISSUER"))) throw new InvalidOperationException("Missing JWT_ISSUER environment variable in production.");
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_AUDIENCE"))) throw new InvalidOperationException("Missing JWT_AUDIENCE environment variable in production.");
}
else
{
    if (string.IsNullOrEmpty(jwtKey)) throw new InvalidOperationException("Missing JWT_KEY environment variable.");
    if (string.IsNullOrEmpty(jwtIssuer)) throw new InvalidOperationException("Missing Jwt:Issuer configuration.");
    if (string.IsNullOrEmpty(jwtAudience)) throw new InvalidOperationException("Missing Jwt:Audience configuration.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<CentralDbContext>();
                var username = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var tenantId = context.Principal?.FindFirstValue("TenantId");
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Username == username && (tenantId == null || u.TenantId == tenantId));
                    if (user == null || !user.IsActive)
                    {
                        context.Fail("Usuario inactivo o revocado.");
                    }
                }
            }
        };
    });

builder.Services.AddAuthorization();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') ?? new[] { "https://trusted-domain.com" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

var isRailwayRuntime =
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")) ||
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_PROJECT_ID")) ||
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_SERVICE_ID"));

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

Console.WriteLine($"POS Server runtime audit: Environment={app.Environment.EnvironmentName}");
Console.WriteLine($"POS Server runtime audit: RailwayRuntime={isRailwayRuntime}");
Console.WriteLine($"POS Server runtime audit: ASPNETCORE_URLS={Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "<not set>"}");
Console.WriteLine($"POS Server runtime audit: PORT={Environment.GetEnvironmentVariable("PORT") ?? "<not set>"}");

// MACROFASE 13C V2: Short-circuit hardened public diagnostic routes before endpoint routing.
// This prevents duplicate /metrics or /health/metrics mappings from reaching controller/minimal endpoint resolution.
app.Use(async (context, next) =>
{
    var requestPath = context.Request.Path.Value ?? string.Empty;

    if (requestPath.Equals("/metrics", StringComparison.OrdinalIgnoreCase) ||
        requestPath.Equals("/health/metrics", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            code = "METRICS_NOT_PUBLIC",
            message = "Metrics are not exposed publicly in production.",
            timestamp = DateTime.UtcNow
        });
        return;
    }

    if (requestPath.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});

// Configure the HTTP request pipeline.
app.UseCors("AllowAll");
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
        }
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        if (exception != null)
        {
            Serilog.Log.Error(exception, "Unhandled Exception (CorrelationId: {CorrelationId})", correlationId);
        }

        var isDev = builder.Environment.IsDevelopment();
        var responseObj = new
        {
            code = "INTERNAL_SERVER_ERROR",
            message = isDev ? (exception?.Message ?? "Ha ocurrido un error inesperado en el servidor.") : "Ha ocurrido un error inesperado en el servidor.",
            correlationId = correlationId,
            details = isDev ? exception?.StackTrace : null
        };

        await context.Response.WriteAsJsonAsync(responseObj);
    });
});
var swaggerEnabledInProduction = string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase);
if (app.Environment.IsDevelopment() || swaggerEnabledInProduction)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    Console.WriteLine("POS Server security hardening: Swagger UI is disabled in production unless ENABLE_SWAGGER=true.");
}

app.UseMiddleware<PosServer.Middlewares.CorrelationIdMiddleware>();
app.UseMiddleware<PosServer.Middlewares.ExceptionHandlingMiddleware>();

app.MapGet("/", (IHostEnvironment environment) => Results.Ok(new
{
    service = "POS-FULL-SERVICE API",
    status = "running",
    environment = environment.EnvironmentName,
    timestamp = DateTime.UtcNow
})).AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "POS-FULL-SERVICE API",
    timestamp = DateTime.UtcNow
})).AllowAnonymous();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "POS-FULL-SERVICE API",
    timestamp = DateTime.UtcNow
})).AllowAnonymous();

app.MapGet("/metrics", () => Results.NotFound(new
{
    code = "METRICS_NOT_PUBLIC",
    message = "Metrics are not exposed publicly in production.",
    timestamp = DateTime.UtcNow
})).AllowAnonymous();

app.MapGet("/health/metrics", () => Results.NotFound(new
{
    code = "METRICS_NOT_PUBLIC",
    message = "Health metrics are not exposed publicly in production.",
    timestamp = DateTime.UtcNow
})).AllowAnonymous();

app.MapGet("/favicon.ico", () => Results.NoContent()).AllowAnonymous();

// Servir la carpeta releases estáticamente para Squirrel
var releasesPath = Path.Combine(builder.Environment.ContentRootPath, "releases");
if (!Directory.Exists(releasesPath))
{
    Directory.CreateDirectory(releasesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(releasesPath),
    RequestPath = "/releases",
    ServeUnknownFileTypes = true // Importante para .nupkg y RELEASES
});

if (!isRailwayRuntime)
{
    app.UseHttpsRedirection();
}
else
{
    Console.WriteLine("POS Server runtime audit: Railway runtime detected; HTTPS redirection is skipped because Railway terminates TLS at the edge.");
}

app.UseAuthentication();
app.UseMiddleware<PosServer.Middlewares.TenantMiddleware>();
app.UseMiddleware<PosServer.Middlewares.PostgresTenantMiddleware>();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("DefaultPolicy");

// Init Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
    
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error aplicando migraciones: {ex.Message}");
        throw;
    }
}

Console.WriteLine("POS Server runtime audit: startup completed; entering app.Run().");
app.Run();
