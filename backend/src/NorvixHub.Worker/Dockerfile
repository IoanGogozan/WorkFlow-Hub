FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY backend/NorvixHub.sln backend/
COPY backend/src/NorvixHub.Application/NorvixHub.Application.csproj backend/src/NorvixHub.Application/
COPY backend/src/NorvixHub.Contracts/NorvixHub.Contracts.csproj backend/src/NorvixHub.Contracts/
COPY backend/src/NorvixHub.Domain/NorvixHub.Domain.csproj backend/src/NorvixHub.Domain/
COPY backend/src/NorvixHub.Infrastructure/NorvixHub.Infrastructure.csproj backend/src/NorvixHub.Infrastructure/
COPY backend/src/NorvixHub.Worker/NorvixHub.Worker.csproj backend/src/NorvixHub.Worker/
RUN dotnet restore backend/src/NorvixHub.Worker/NorvixHub.Worker.csproj

COPY backend/ backend/
RUN dotnet publish backend/src/NorvixHub.Worker/NorvixHub.Worker.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NorvixHub.Worker.dll"]
