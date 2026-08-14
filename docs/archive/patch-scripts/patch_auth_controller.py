with open('PosServer/Controllers/AuthController.cs', 'r') as f:
    c = f.read()

c = c.replace('[HttpPost("refresh")]', '[HttpPost("refresh")]\n    [EnableRateLimiting("LoginPolicy")]')
c = c.replace('[HttpPost("provision")]', '[HttpPost("provision")]\n    [EnableRateLimiting("LoginPolicy")]')

with open('PosServer/Controllers/AuthController.cs', 'w') as f:
    f.write(c)
