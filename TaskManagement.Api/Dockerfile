# ============================================
# Stage 1: BUILD
# SDK image (stort, har compiler + tools)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopier .csproj filer først (Docker layer caching - restore kun ved dependency ændringer)
COPY TaskManagement.Api/TaskManagement.Api.csproj TaskManagement.Api/
COPY TaskManagement.Shared/TaskManagement.Shared.csproj TaskManagement.Shared/
RUN dotnet restore TaskManagement.Api/TaskManagement.Api.csproj

# Kopier resten af koden og byg
COPY TaskManagement.Api/ TaskManagement.Api/
COPY TaskManagement.Shared/ TaskManagement.Shared/
RUN dotnet publish TaskManagement.Api/TaskManagement.Api.csproj -c Release -o /app/publish

# ============================================
# Stage 2: RUNTIME
# ASP.NET runtime image (lille, kun runtime - ingen SDK, source code eller NuGet cache)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]