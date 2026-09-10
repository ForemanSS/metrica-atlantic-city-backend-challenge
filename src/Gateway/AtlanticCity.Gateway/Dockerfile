FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
    "src/Gateway/AtlanticCity.Gateway/AtlanticCity.Gateway.csproj"

RUN dotnet publish \
    "src/Gateway/AtlanticCity.Gateway/AtlanticCity.Gateway.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

ENV DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AtlanticCity.Gateway.dll"]