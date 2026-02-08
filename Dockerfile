# ============================================
# Stage 1: BUILD
# Bruger SDK image (stort, har compiler + tools)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopier .csproj filer først og restore dependencies
# Dette caches separat så Docker ikke re-downloader packages ved hver kodeændring
COPY TaskManagement.Api/TaskManagement.Api.csproj TaskManagement.Api/
RUN dotnet restore TaskManagement.Api/TaskManagement.Api.csproj

# Kopier resten af koden og byg
COPY TaskManagement.Api/ TaskManagement.Api/
RUN dotnet publish TaskManagement.Api/TaskManagement.Api.csproj -c Release -o /app/publish

# ============================================
# Stage 2: RUNTIME
# Bruger ASP.NET runtime image (lille, kun det der kræves for at køre)
# Dette image indeholder kun runtime + vores compiled kode (ik SDK, source code eller nuget cache) --> giver mindre image i prod
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Kopier bygget applikation fra build stage
COPY --from=build /app/publish .

# Fortæl Docker hvilken port applikationen lytter på
EXPOSE 8080

# Start applikationen
ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]