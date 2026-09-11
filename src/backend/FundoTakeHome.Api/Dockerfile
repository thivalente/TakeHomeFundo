FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/backend/FundoTakeHome.Api/FundoTakeHome.Api.csproj src/backend/FundoTakeHome.Api/
RUN dotnet restore src/backend/FundoTakeHome.Api/FundoTakeHome.Api.csproj
COPY . .
RUN dotnet publish src/backend/FundoTakeHome.Api/FundoTakeHome.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FundoTakeHome.Api.dll"]
