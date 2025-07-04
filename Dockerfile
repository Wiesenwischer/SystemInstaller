FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SystemInstaller.Web.csproj", "."]
RUN dotnet restore "SystemInstaller.Web.csproj"
COPY . .
RUN dotnet publish "SystemInstaller.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SystemInstaller.Web.dll"]
