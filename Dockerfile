# syntax=docker/dockerfile:1

# Single build graph for both the API and the integration tests: the shared
# projects (Ordering.*, Catalog.*, Common.*, Contracts) are restored and compiled
# once and reused by both final stages.

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Project files first — this layer stays cached until the dependency graph changes.
# COPY flattens the wildcard, so put every csproj back into its own directory
# (directory name always matches the project file name in this solution).
COPY SomeShop.sln ./
COPY */*.csproj ./
RUN for proj in *.csproj; do dir="${proj%.csproj}"; mkdir -p "$dir"; mv "$proj" "$dir/"; done

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore SomeShop.sln

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet build SomeShop.Api/SomeShop.Api.csproj -c Release --no-restore \
 && dotnet build SomeShop.IntegrationTests/SomeShop.IntegrationTests.csproj -c Release --no-restore \
 && dotnet build SomeShop.Tools.KafkaInit/SomeShop.Tools.KafkaInit.csproj -c Release --no-restore

FROM build AS api-publish
# --no-build is only valid because the build above wrote to the default
# bin/Release/net7.0 of every project (a `dotnet build -o <dir>` would break it).
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish SomeShop.Api/SomeShop.Api.csproj -c Release -o /app/publish --no-restore --no-build

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS api
# curl is not shipped in the aspnet image and the compose healthcheck needs it.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 13001 8080
COPY --from=api-publish /app/publish ./
ENTRYPOINT ["dotnet", "SomeShop.Api.dll"]

FROM build AS kafka-init-publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish SomeShop.Tools.KafkaInit/SomeShop.Tools.KafkaInit.csproj -c Release -o /app/publish --no-restore --no-build

# Same base as the api stage, so this costs no extra image.
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS kafka-init
WORKDIR /app
COPY --from=kafka-init-publish /app/publish ./
ENTRYPOINT ["dotnet", "SomeShop.Tools.KafkaInit.dll"]

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS tests
WORKDIR /app/tests
# Self-contained test output: vstest runs the already compiled assemblies, so the
# container never recompiles anything at run time.
COPY --from=build /src/SomeShop.IntegrationTests/bin/Release/net7.0/ ./
ENTRYPOINT ["dotnet", "vstest", "SomeShop.IntegrationTests.dll"]
CMD ["--logger:console;verbosity=normal"]
