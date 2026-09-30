FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files to leverage Docker layer caching
COPY ["Mimo.API/Mimo.API.csproj", "Mimo.API/"]
COPY ["Mimo.Application/Mimo.Application.csproj", "Mimo.Application/"]
COPY ["Mimo.Domain/Mimo.Domain.csproj", "Mimo.Domain/"]
COPY ["Mimo.Infrastructure/Mimo.Infrastructure.csproj", "Mimo.Infrastructure/"]

RUN dotnet restore "Mimo.API/Mimo.API.csproj"

# Copy remaining source code and build
COPY . .
WORKDIR "/src/Mimo.API"
RUN dotnet build "Mimo.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mimo.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mimo.API.dll"]