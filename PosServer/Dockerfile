# Railway deployment Dockerfile for PosServer (.NET 8)
# Location: PosServer/Dockerfile
# Required Railway setting:
#   Root Directory: empty
# Config-as-code:
#   railway.json at repository root sets dockerfilePath = PosServer/Dockerfile
# Build context must be repository root because PosServer depends on sibling projects.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to improve Docker layer caching.
COPY PosDomain/PosDomain.csproj PosDomain/
COPY PosApplication/PosApplication.csproj PosApplication/
COPY PosInfrastructure/PosInfrastructure.csproj PosInfrastructure/
COPY PosServer/PosServer.csproj PosServer/

RUN dotnet restore PosServer/PosServer.csproj

# Copy the rest of the source tree.
COPY . .

# Publish only the API service. Do not publish Windows desktop projects here.
RUN dotnet publish PosServer/PosServer.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway injects PORT at runtime. The shell wrapper maps it to ASPNETCORE_URLS.
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet PosServer.dll"]
