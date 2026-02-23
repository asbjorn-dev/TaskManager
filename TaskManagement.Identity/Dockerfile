FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopier begge .csproj filer
COPY TaskManagement.Identity/TaskManagement.Identity.csproj TaskManagement.Identity/
COPY TaskManagement.Shared/TaskManagement.Shared.csproj TaskManagement.Shared/
RUN dotnet restore TaskManagement.Identity/TaskManagement.Identity.csproj

COPY TaskManagement.Identity/ TaskManagement.Identity/
COPY TaskManagement.Shared/ TaskManagement.Shared/
RUN dotnet publish TaskManagement.Identity/TaskManagement.Identity.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManagement.Identity.dll"]
