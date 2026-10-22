# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
# Install curl for container health check
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Stage 2: SDK Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files for optimal Docker layer caching
COPY ["Mimo.API/Mimo.API.csproj", "Mimo.API/"]
COPY ["Mimo.Application/Mimo.Application.csproj", "Mimo.Application/"]
COPY ["Mimo.Domain/Mimo.Domain.csproj", "Mimo.Domain/"]
COPY ["Mimo.Infrastructure/Mimo.Infrastructure.csproj", "Mimo.Infrastructure/"]

RUN dotnet restore "Mimo.API/Mimo.API.csproj"

# Copy remaining source code and build
COPY . .
WORKDIR "/src/Mimo.API"
RUN dotnet build "Mimo.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mimo.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Production Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mimo.API.dll"]