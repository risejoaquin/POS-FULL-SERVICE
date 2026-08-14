with open('PosServer/Controllers/LicenseController.cs', 'r') as f:
    c = f.read()

c = c.replace("using Microsoft.AspNetCore.Mvc;", "using Microsoft.AspNetCore.Mvc;\nusing Microsoft.AspNetCore.RateLimiting;")
c = c.replace('[HttpPost("validate")]', '[HttpPost("validate")]\n    [EnableRateLimiting("LoginPolicy")]')

with open('PosServer/Controllers/LicenseController.cs', 'w') as f:
    f.write(c)
