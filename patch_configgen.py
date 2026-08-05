with open('PosBuilder/ConfigurationGenerator.cs', 'r', encoding='utf-8') as f:
    content = f.read()

import re

old_call = """        public string GenerateSqlScript(ConfigModel model)
        {
            return SqlGenerator.GenerateTenantSql(
                model.CompanyName, 
                model.TenantId, 
                model.AdminUser, 
                model.AdminPassword, 
                model.EmployeeUser, 
                model.EmployeePassword);
        }"""

new_call = """        public string GenerateSqlScript(ConfigModel model)
        {
            return SqlGenerator.GenerateTenantSql(model);
        }"""

content = content.replace(old_call, new_call)

with open('PosBuilder/ConfigurationGenerator.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("ConfigurationGenerator updated")
