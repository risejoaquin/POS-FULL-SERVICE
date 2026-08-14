with open('PosServer/Program.cs', 'r') as f:
    c = f.read()

c = c.replace("builder.Services.AddEndpointsApiExplorer();", """builder.Services.AddEndpointsApiExplorer();

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
});""")

c = c.replace("// app.UseRateLimiter();", "app.UseRateLimiter();")

with open('PosServer/Program.cs', 'w') as f:
    f.write(c)
