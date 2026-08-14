with open('PosServer/Controllers/AuthController.cs', 'r') as f:
    c = f.read()

import re
c = re.sub(r'\[EnableRateLimiting\("LoginPolicy"\)\]\s+\[EnableRateLimiting\("LoginPolicy"\)\]', '[EnableRateLimiting("LoginPolicy")]', c)

with open('PosServer/Controllers/AuthController.cs', 'w') as f:
    f.write(c)
