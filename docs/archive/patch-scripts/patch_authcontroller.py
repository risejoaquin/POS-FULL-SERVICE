with open('PosServer/Controllers/AuthController.cs', 'r') as f:
    c = f.read()

c = c.replace("using System.Threading.Tasks;", "using System.Threading.Tasks;\nusing Microsoft.AspNetCore.RateLimiting;")
c = c.replace("[HttpPost(\"login\")]", "[HttpPost(\"login\")]\n    [EnableRateLimiting(\"LoginPolicy\")]")
c = c.replace("[HttpPost(\"provision\")]", "[HttpPost(\"provision\")]\n    [EnableRateLimiting(\"LoginPolicy\")]")
c = c.replace("public class AuthController", "[EnableRateLimiting(\"DefaultPolicy\")]\npublic class AuthController")

with open('PosServer/Controllers/AuthController.cs', 'w') as f:
    f.write(c)
