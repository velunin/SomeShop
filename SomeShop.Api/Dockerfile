FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 13001

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SomeShop.Api/SomeShop.Api.csproj", "SomeShop.Api/"]
COPY ["SomeShop.Ordering.App/SomeShop.Ordering.App.csproj", "SomeShop.Ordering.App/"]
COPY ["SomeShop.Ordering.Domain/SomeShop.Ordering.Domain.csproj", "SomeShop.Ordering.Domain/"]
COPY ["SomeShop.Common.Domain/SomeShop.Common.Domain.csproj", "SomeShop.Common.Domain/"]
COPY ["SomeShop.Common.App/SomeShop.Common.App.csproj", "SomeShop.Common.App/"]
COPY ["SomeShop.Ordering.EF/SomeShop.Ordering.EF.csproj", "SomeShop.Ordering.EF/"]
COPY ["SomeShop.Common.EF/SomeShop.Common.EF.csproj", "SomeShop.Common.EF/"]
COPY ["SomeShop.Ordering.Contracts/SomeShop.Ordering.Contracts.csproj", "SomeShop.Ordering.Contracts/"]
RUN dotnet restore "SomeShop.Api/SomeShop.Api.csproj"
COPY . .
WORKDIR "/src/SomeShop.Api"
RUN dotnet build "SomeShop.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SomeShop.Api.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SomeShop.Api.dll"]
