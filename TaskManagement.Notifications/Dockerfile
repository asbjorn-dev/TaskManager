FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Notifications refererer til Shared (event models)
COPY TaskManagement.Notifications/TaskManagement.Notifications.csproj TaskManagement.Notifications/
COPY TaskManagement.Shared/TaskManagement.Shared.csproj TaskManagement.Shared/
RUN dotnet restore TaskManagement.Notifications/TaskManagement.Notifications.csproj

COPY TaskManagement.Notifications/ TaskManagement.Notifications/
COPY TaskManagement.Shared/ TaskManagement.Shared/
RUN dotnet publish TaskManagement.Notifications/TaskManagement.Notifications.csproj -c Release -o /app/publish

# Worker Service - bruger base runtime (ingen web server nødvendig, mindre image)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TaskManagement.Notifications.dll"]
