# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SpendSmart/SpendSmart.csproj", "SpendSmart/"]
RUN dotnet restore "./SpendSmart/SpendSmart.csproj"
COPY . .
WORKDIR "/src/SpendSmart"
RUN dotnet build "./SpendSmart.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SpendSmart.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy the Docker-specific appsettings
COPY SpendSmart/appsettings.Docker.json ./appsettings.Docker.json
ENTRYPOINT ["dotnet", "SpendSmart.dll"]
# Create folder
RUN mkdir -p /https
# Copy the HTTPS certificate
COPY https/aspnetapp.pfx /https/aspnetapp.pfx

# Set environment variables for Kestrel
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="Project@2025"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx"