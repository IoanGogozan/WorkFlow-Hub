FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY backend/Directory.Build.props backend/
COPY backend/src/NorvixHub.ErpDemoReceiver/NorvixHub.ErpDemoReceiver.csproj backend/src/NorvixHub.ErpDemoReceiver/
RUN dotnet restore backend/src/NorvixHub.ErpDemoReceiver/NorvixHub.ErpDemoReceiver.csproj

COPY backend/src/NorvixHub.ErpDemoReceiver/ backend/src/NorvixHub.ErpDemoReceiver/
RUN dotnet publish backend/src/NorvixHub.ErpDemoReceiver/NorvixHub.ErpDemoReceiver.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "NorvixHub.ErpDemoReceiver.dll"]
