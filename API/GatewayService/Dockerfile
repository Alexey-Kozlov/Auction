FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
#эта папка внутри докерного образа
WORKDIR /app
EXPOSE 80

COPY API/GatewayService/GatewayService.csproj API/GatewayService/GatewayService.csproj
RUN dotnet restore API/GatewayService/GatewayService.csproj

#копируем остальные файлы проекта Auction
COPY API/GatewayService API/GatewayService

WORKDIR /app/API/GatewayService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]
