FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY . ./

RUN dotnet restore

WORKDIR /source/

RUN dotnet build -c Release -o /app -p:DeployOnBuild=true -p:PublishProfile="Release (linux-x64)" -p:SatelliteResourceLanguages="en-US"

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

FROM base AS final

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "Polyphonic.TelegramBot.dll"]