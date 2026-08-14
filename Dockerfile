# Railway root Dockerfile fallback.
# Build context MUST be repository root.
# Runtime port binding is handled by scripts/railway/start-posserver.sh,
# because Railway injects PORT at runtime, not at Docker build time.

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
COPY scripts/railway/start-posserver.sh /app/start-posserver.sh
RUN chmod +x /app/start-posserver.sh

# Railway injects PORT at runtime. Do not use ENV ASPNETCORE_URLS=http://+:${PORT}
# because Docker expands ${PORT} at build time and produces an invalid/empty binding.
EXPOSE 8080
ENTRYPOINT ["/app/start-posserver.sh"]
