FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/src/Domain/SistemaTraction.Domain.csproj Domain/
COPY backend/src/Application/SistemaTraction.Application.csproj Application/
COPY backend/src/Infrastructure/SistemaTraction.Infrastructure.csproj Infrastructure/
COPY backend/src/API/SistemaTraction.API.csproj API/

RUN dotnet restore API/SistemaTraction.API.csproj

COPY backend/src/ .

RUN dotnet publish API/SistemaTraction.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SistemaTraction.API.dll"]
