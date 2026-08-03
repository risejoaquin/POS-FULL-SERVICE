# Usa la imagen del SDK de .NET 8 para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todo el codigo
COPY . .

# Busca el archivo del proyecto y compila, mostrando los archivos para debugging
RUN echo "Contenido de /src:" && ls -la && \
    if [ -d "PosServer" ]; then echo "Contenido de /src/PosServer:" && ls -la PosServer; fi && \
    if [ -f "PosServer.csproj" ]; then \
        echo "Found at root" && \
        dotnet restore "PosServer.csproj" && \
        dotnet publish "PosServer.csproj" -c Release -o /app/publish /p:UseAppHost=false; \
    elif [ -f "PosServer/PosServer.csproj" ]; then \
        echo "Found in PosServer/" && \
        dotnet restore "PosServer/PosServer.csproj" && \
        dotnet publish "PosServer/PosServer.csproj" -c Release -o /app/publish /p:UseAppHost=false; \
    else \
        echo "Could not find PosServer.csproj. File tree:" && \
        find . && exit 1; \
    fi

# Usa la imagen de runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway inyecta la variable de entorno PORT dinámicamente
ENV ASPNETCORE_URLS=

ENTRYPOINT ["dotnet", "PosServer.dll"]
