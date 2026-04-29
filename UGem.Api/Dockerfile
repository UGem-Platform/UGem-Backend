FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UGem.Api/UGem.Api.csproj", "UGem.Api/"]
COPY ["UGem.Services/UGem.Services.csproj", "UGem.Services/"]
COPY ["UGem.Repositories/UGem.Repositories.csproj", "UGem.Repositories/"]
RUN dotnet restore "UGem.Api/UGem.Api.csproj"
COPY . .
WORKDIR "/src/UGem.Api"
RUN dotnet build "./UGem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UGem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UGem.Api.dll"]
