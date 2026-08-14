with open('PosServer/Program.cs', 'r') as f:
    c = f.read()

c = c.replace("app.UseSwaggerUI();\n}", "app.UseSwaggerUI();")

with open('PosServer/Program.cs', 'w') as f:
    f.write(c)
