FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
    "src/Services/Notification/AtlanticCity.Notification.Worker/AtlanticCity.Notification.Worker.csproj"

RUN dotnet publish \
    "src/Services/Notification/AtlanticCity.Notification.Worker/AtlanticCity.Notification.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

ENV DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AtlanticCity.Notification.Worker.dll"]