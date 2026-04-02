#!/usr/bin/env sh
set -eu

PORT_TO_USE="${PORT:-8080}"
export ASPNETCORE_URLS="http://+:${PORT_TO_USE}"

mkdir -p /app/data /app/logs

exec dotnet EnterpriseHospitalManagement.dll
