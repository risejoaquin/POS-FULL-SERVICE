# Usa la imagen del SDK de .NET 8 para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todo el codigo
COPY . .
WORKDIR /src/PosServer

RUN dotnet restore "PosServer.csproj"

# Publica la aplicación
RUN dotnet publish "PosServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Usa la imagen de runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway inyecta la variable de entorno PORT dinámicamente
ENV ASPNETCORE_URLS=

ENTRYPOINT ["dotnet", "PosServer.dll"]
