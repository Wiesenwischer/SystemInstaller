FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and all project files
COPY ["SystemInstaller.sln", "."]
COPY ["SystemInstaller.Web.csproj", "."]
COPY ["Domain/SystemInstaller.Domain.csproj", "Domain/"]
COPY ["Application/SystemInstaller.Application.csproj", "Application/"]
COPY ["Infrastructure/SystemInstaller.Infrastructure.csproj", "Infrastructure/"]

# Restore packages
RUN dotnet restore "SystemInstaller.Web.csproj"

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish "SystemInstaller.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SystemInstaller.Web.dll"]
