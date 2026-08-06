with open('PosBuilder/ConfigurationGenerator.cs', 'r', encoding='utf-8') as f:
    content = f.read()

old_config = """            var config = new
            {
                Logging = new { LogLevel = new { Default = "Information", Microsoft_AspNetCore = "Warning" } },
                AllowedHosts = "*",
                ConnectionStrings = new { DefaultConnection = connString },
                Jwt = new { Key = model.JwtSecret, Issuer = "PosServer", Audience = "PosClient" },
                ADMIN_USER = model.AdminUser,
                ADMIN_PASSWORD = model.AdminPassword,
                EMP_USER = model.EmployeeUser,
                EMP_PASSWORD = model.EmployeePassword,
                TENANT_ID = model.TenantId,
                BUSINESS_TYPE = model.BusinessType
            };"""

new_config = """            var config = new
            {
                Logging = new { LogLevel = new { Default = "Information", Microsoft_AspNetCore = "Warning" } },
                AllowedHosts = "*",
                ConnectionStrings = new { DefaultConnection = connString },
                Jwt = new { Key = model.JwtSecret, Issuer = "PosServer", Audience = "PosClient" },
                ADMIN_USER = model.AdminUser,
                ADMIN_PASSWORD = model.AdminPassword,
                EMP_USER = model.EmployeeUser,
                EMP_PASSWORD = model.EmployeePassword,
                EXTRA_USERS = model.ExtraUsers,
                TENANT_ID = model.TenantId,
                BUSINESS_TYPE = model.BusinessType
            };"""

content = content.replace(old_config, new_config)

with open('PosBuilder/ConfigurationGenerator.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("ConfigurationGenerator updated")
