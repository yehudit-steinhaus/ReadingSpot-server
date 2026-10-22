# בנייה
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ReadingSpot.API/ReadingSpot.API.csproj -c Release -o /app

# הרצה
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ReadingSpot.API.dll"]
