# ImageCards API image. Build from the repository root:
#   docker build -f backend/Dockerfile -t imagecards-api .
# No configuration or secrets are baked in; everything comes from environment variables at runtime.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so this layer is cached until the project file changes.
COPY global.json ./
COPY backend/ImageCards.Api/ImageCards.Api.csproj backend/ImageCards.Api/
RUN dotnet restore backend/ImageCards.Api/ImageCards.Api.csproj

COPY backend/ImageCards.Api/ backend/ImageCards.Api/
RUN dotnet publish backend/ImageCards.Api/ImageCards.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Non-root user provided by the official .NET images.
USER $APP_UID

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ImageCards.Api.dll"]
