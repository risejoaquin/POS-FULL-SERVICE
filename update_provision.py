import re

# Update ProvisionRequest.cs
with open('./PosServer/Models/ProvisionRequest.cs', 'r', encoding='utf-8') as f:
    content = f.read()

if "ExtraUsers" not in content:
    content = content.replace(
        "public string EmpPassword { get; set; } = string.Empty;",
        "public string EmpPassword { get; set; } = string.Empty;\n        public System.Collections.Generic.List<PosBuilder.Models.UserModel> ExtraUsers { get; set; } = new();"
    )
    with open('./PosServer/Models/ProvisionRequest.cs', 'w', encoding='utf-8') as f:
        f.write(content)

# Update AuthController.cs
with open('./PosServer/Controllers/AuthController.cs', 'r', encoding='utf-8') as f:
    content = f.read()

extra_users_logic = """
        if (request.ExtraUsers != null && request.ExtraUsers.Any())
        {
            foreach (var extraUser in request.ExtraUsers)
            {
                var extraExists = await _dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Username.ToLower() == extraUser.Username.ToLower());
                if (!extraExists && !string.IsNullOrEmpty(extraUser.Username) && !string.IsNullOrEmpty(extraUser.Password))
                {
                    _dbContext.Users.Add(new User {
                        Username = extraUser.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(extraUser.Password),
                        Role = extraUser.Role ?? "Empleado",
                        TenantId = tenantId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }
"""
if "request.ExtraUsers" not in content:
    content = content.replace(
        "await _dbContext.SaveChangesAsync();\n\n        return Ok(",
        "await _dbContext.SaveChangesAsync();\n" + extra_users_logic + "\n        await _dbContext.SaveChangesAsync();\n\n        return Ok("
    )
    with open('./PosServer/Controllers/AuthController.cs', 'w', encoding='utf-8') as f:
        f.write(content)
