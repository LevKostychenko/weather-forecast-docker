FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# Copy the project file and restore dependencies
COPY ["WeatherForecastService.csproj", "./"]
RUN dotnet restore "./WeatherForecastService.csproj"

COPY . .
RUN dotnet publish "WeatherForecastService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WeatherForecastService.dll"]