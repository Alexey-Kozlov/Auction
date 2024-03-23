FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80

COPY API/Contracts/Contracts.csproj API/Contracts/Contracts.csproj
COPY API/AuctionService/AuctionService.csproj API/AuctionService/AuctionService.csproj
RUN dotnet restore API/AuctionService/AuctionService.csproj

#копируем остальные файлы проекта Auction
COPY API/AuctionService API/AuctionService
COPY API/Contracts API/Contracts

WORKDIR /app/API/AuctionService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]
