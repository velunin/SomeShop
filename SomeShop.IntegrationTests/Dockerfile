FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SomeShop.IntegrationTests/SomeShop.IntegrationTests.csproj", "SomeShop.IntegrationTests/"]
COPY ["SomeShop.Ordering.Contracts/SomeShop.Ordering.Contracts.csproj", "SomeShop.Ordering.Contracts/"]
COPY ["SomeShop.Catalog.Contracts/SomeShop.Catalog.Contracts.csproj", "SomeShop.Catalog.Contracts/"]
RUN dotnet restore "SomeShop.IntegrationTests/SomeShop.IntegrationTests.csproj"
COPY . .
WORKDIR "/src/SomeShop.IntegrationTests"
RUN dotnet build "SomeShop.IntegrationTests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SomeShop.IntegrationTests.csproj" -c Release -o /app/publish --no-restore
