# Railway deployment Dockerfile for PosServer (.NET 8)
# Location: repository root, next to Pos.sln.
# This Dockerfile publishes only PosServer and its non-Windows dependencies.

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

# Publish the API only. Do not publish WPF projects here.
RUN dotnet publish PosServer/PosServer.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway provides PORT at runtime. Fallback to 8080 for local Docker runs.
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet PosServer.dll"]
