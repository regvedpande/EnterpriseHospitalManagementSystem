FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EnterpriseHospitalManagement.sln ./
COPY EnterpriseHospitalManagement/EnterpriseHospitalManagement.csproj EnterpriseHospitalManagement/
RUN dotnet restore EnterpriseHospitalManagement/EnterpriseHospitalManagement.csproj

COPY EnterpriseHospitalManagement/ EnterpriseHospitalManagement/
RUN dotnet publish EnterpriseHospitalManagement/EnterpriseHospitalManagement.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV Database__Provider=Sqlite
ENV ConnectionStrings__SqliteConnection="Data Source=/app/data/medcore-hms.db"

COPY --from=build /app/publish ./
COPY docker-entrypoint.sh ./docker-entrypoint.sh

RUN chmod +x /app/docker-entrypoint.sh \
    && mkdir -p /app/data /app/logs

EXPOSE 8080

ENTRYPOINT ["/app/docker-entrypoint.sh"]
