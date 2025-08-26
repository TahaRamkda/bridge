# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

RUN dotnet restore WhatsAppBridge/WhatsAppBridge/WhatsAppBridge.csproj
RUN dotnet publish WhatsAppBridge/WhatsAppBridge/WhatsAppBridge.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "WhatsAppBridge.dll"]
