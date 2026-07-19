FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY backend/NorvixHub.sln backend/
COPY backend/src/NorvixHub.Api/NorvixHub.Api.csproj backend/src/NorvixHub.Api/
COPY backend/src/NorvixHub.Application/NorvixHub.Application.csproj backend/src/NorvixHub.Application/
COPY backend/src/NorvixHub.Contracts/NorvixHub.Contracts.csproj backend/src/NorvixHub.Contracts/
COPY backend/src/NorvixHub.Domain/NorvixHub.Domain.csproj backend/src/NorvixHub.Domain/
COPY backend/src/NorvixHub.Infrastructure/NorvixHub.Infrastructure.csproj backend/src/NorvixHub.Infrastructure/
RUN dotnet restore backend/src/NorvixHub.Api/NorvixHub.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/src/NorvixHub.Api/NorvixHub.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ARG GIT_SHA=unknown
ARG BUILD_DATE=unknown
ENV ASPNETCORE_HTTP_PORTS=8080
ENV Build__GitSha=$GIT_SHA
ENV Build__Date=$BUILD_DATE
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "NorvixHub.Api.dll"]
