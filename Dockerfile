# Optional root Dockerfile mirror for platforms that only autodetect Dockerfile at repo root.
# Railway Option A uses railway.json -> PosServer/Dockerfile.
# Keep this file as a fallback. It uses the same diagnostic-friendly build flow.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN echo "===== RAILWAY ROOT DOCKERFILE CONTEXT AUDIT START =====" \
    && echo "PWD=$(pwd)" \
    && ls -la \
    && find . -maxdepth 3 -name "*.csproj" -print | sort \
    && test -f PosServer/PosServer.csproj || (echo "ERROR: Missing PosServer/PosServer.csproj. Build context must be repo root." && exit 41) \
    && test -f PosDomain/PosDomain.csproj || (echo "ERROR: Missing PosDomain/PosDomain.csproj." && exit 42) \
    && test -f PosApplication/PosApplication.csproj || (echo "ERROR: Missing PosApplication/PosApplication.csproj." && exit 43) \
    && test -f PosInfrastructure/PosInfrastructure.csproj || (echo "ERROR: Missing PosInfrastructure/PosInfrastructure.csproj." && exit 44) \
    && echo "RAILWAY ROOT DOCKERFILE CONTEXT AUDIT PASS." \
    && echo "===== RAILWAY ROOT DOCKERFILE CONTEXT AUDIT END ====="

RUN dotnet restore PosServer/PosServer.csproj
RUN dotnet publish PosServer/PosServer.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080
ENTRYPOINT ["dotnet", "PosServer.dll"]
