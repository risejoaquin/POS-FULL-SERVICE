# Railway diagnostic Dockerfile for PosServer
# Build context MUST be repository root, not /PosServer.
# Required Railway settings:
#   Root Directory: empty
#   Dockerfile Path: PosServer/Dockerfile (provided by railway.json)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the whole repository first so Railway build logs can show the real build context.
# This is intentionally diagnostic-friendly. It avoids opaque COPY cache-key failures and
# prints the visible project structure before restore/publish.
COPY . .

RUN echo "===== RAILWAY CONTEXT AUDIT START =====" \
    && echo "PWD=$(pwd)" \
    && echo "Top-level files/folders visible to Docker:" \
    && ls -la \
    && echo "Detected .csproj files up to depth 3:" \
    && find . -maxdepth 3 -name "*.csproj" -print | sort \
    && echo "Required files check:" \
    && test -f PosServer/PosServer.csproj || (echo "ERROR: Missing PosServer/PosServer.csproj. Railway Root Directory is probably set to /PosServer or invalid. Set Root Directory EMPTY and keep railway.json at repo root." && exit 41) \
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

ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080

ENTRYPOINT ["dotnet", "PosServer.dll"]
