FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Depot.Cache.Migrations/Depot.Cache.Migrations.csproj", "src/Depot.Cache.Migrations/"]
RUN dotnet restore "src/Depot.Cache.Migrations/Depot.Cache.Migrations.csproj"
COPY . .
WORKDIR "/src/src/Depot.Cache.Migrations"
RUN dotnet build "./Depot.Cache.Migrations.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Depot.Cache.Migrations.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Depot.Cache.Migrations.dll"]
