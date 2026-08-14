with open("PosInfrastructure/Data/Local/PosDbContext.cs", "r") as f:
    text = f.read()

text = text.replace("using PosCore.Services;", "using PosDomain.Interfaces;\nusing PosCore.Models;")
text = text.replace("private readonly SessionManager _sessionManager;", "private readonly ITenantService _sessionManager;")
text = text.replace("public PosDbContext(DbContextOptions<PosDbContext> options, SessionManager sessionManager)", "public PosDbContext(DbContextOptions<PosDbContext> options, ITenantService sessionManager)")

with open("PosInfrastructure/Data/Local/PosDbContext.cs", "w") as f:
    f.write(text)
