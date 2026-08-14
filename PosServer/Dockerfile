# Railway Dockerfile for PosServer.
# Build context MUST be repository root, not /PosServer.
# Required Railway settings:
#   Root Directory: empty or /
#   Dockerfile Path: PosServer/Dockerfile if configured manually,
#   or let railway.json define dockerfilePath.
# Runtime port binding is handled by scripts/railway/start-posserver.sh.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN echo "===== RAILWAY CONTEXT AUDIT START =====" \
    && echo "PWD=$(pwd)" \
    && echo "Top-level files/folders visible to Docker:" \
    && ls -la \
    && echo "Detected .csproj files up to depth 3:" \
    && find . -maxdepth 3 -name "*.csproj" -print | sort \
    && echo "Required files check:" \
    && test -f PosServer/PosServer.csproj || (echo "ERROR: Missing PosServer/PosServer.csproj. Railway Root Directory is probably set to /PosServer or invalid. Set Root Directory EMPTY or / and keep railway.json at repo root." && exit 41) \
    && test -f PosDomain/PosDomain.csproj || (echo "ERROR: Missing PosDomain/PosDomain.csproj. PosServer depends on sibling projects, so build context must be repo root." && exit 42) \
    && test -f PosApplication/PosApplication.csproj || (echo "ERROR: Missing PosApplication/PosApplication.csproj. Build context must include sibling projects." && exit 43) \
    && test -f PosInfrastructure/PosInfrastructure.csproj || (echo "ERROR: Missing PosInfrastructure/PosInfrastructure.csproj. Build context must include sibling projects." && exit 44) \
    && echo "RAILWAY CONTEXT AUDIT PASS: repo root context is visible." \
    && echo "===== RAILWAY CONTEXT AUDIT END ====="

RUN dotnet restore PosServer/PosServer.csproj
RUN dotnet publish PosServer/PosServer.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY scripts/railway/start-posserver.sh /app/start-posserver.sh
RUN chmod +x /app/start-posserver.sh

# Railway injects PORT at runtime through the startup script.
# Do not set ASPNETCORE_URLS here with a build-time PORT placeholder.
EXPOSE 8080
ENTRYPOINT ["/app/start-posserver.sh"]
