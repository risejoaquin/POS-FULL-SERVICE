    [HttpPost("provision")]
    public async Task<IActionResult> Provision([FromBody] ProvisionRequest request)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"] ?? "super_secret_fallback_jwt_key_1234567890";
        if (request.ProvisionKey != jwtKey)
        {
            return Unauthorized(new { Message = "ProvisionKey inválida" });
        }
        
        var tenantId = request.TenantId;
        if (string.IsNullOrEmpty(tenantId))
        {
            return BadRequest(new { Message = "TenantId requerido" });
        }
        
        var adminExists = await _dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.AdminUsername.ToLower());
        if (!adminExists)
        {
            _dbContext.Users.Add(new User {
                Username = request.AdminUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
                Role = "Admin",
                TenantId = tenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var adminUser = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.AdminUsername.ToLower());
            if (adminUser != null)
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword);
                adminUser.Role = "Admin";
            }
        }
        
        if (!string.IsNullOrEmpty(request.EmpUsername))
        {
            var empExists = await _dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.EmpUsername.ToLower());
            if (!empExists)
            {
                _dbContext.Users.Add(new User {
                    Username = request.EmpUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.EmpPassword),
                    Role = "Cajero",
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else 
            {
                var empUser = await _dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username.ToLower() == request.EmpUsername.ToLower());
                if (empUser != null)
                {
                    empUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.EmpPassword);
                    empUser.Role = "Cajero";
                }
            }
        }
        
        await _dbContext.SaveChangesAsync();
        return Ok(new { Message = "Tenant aprovisionado exitosamente." });
    }
