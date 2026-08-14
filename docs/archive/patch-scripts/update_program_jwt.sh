#!/bin/bash
sed -i 's/var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration\["Jwt:Key"\] ?? "super_secret_fallback_jwt_key_1234567890";/var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing JWT_KEY");/g' PosServer/Program.cs
sed -i 's/var jwtIssuer = builder.Configuration\["Jwt:Issuer"\] ?? "PosServer";/var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Missing Jwt:Issuer");/g' PosServer/Program.cs
sed -i 's/var jwtAudience = builder.Configuration\["Jwt:Audience"\] ?? "PosClient";/var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Missing Jwt:Audience");/g' PosServer/Program.cs
