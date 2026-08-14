#!/bin/sh
set -eu

# Railway injects PORT at runtime. Do not expand $PORT inside Dockerfile ENV during build.
# Default to 8080 for local Docker runs.
: "${PORT:=8080}"

export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"

echo "RAILWAY RUNTIME PORT BINDING START"
echo "PORT=${PORT}"
echo "ASPNETCORE_URLS=${ASPNETCORE_URLS}"
echo "Starting PosServer..."

exec dotnet PosServer.dll
