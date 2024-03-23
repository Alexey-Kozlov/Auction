FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80

COPY API/Contracts/Contracts.csproj API/Contracts/Contracts.csproj
COPY API/BiddingService/BiddingService.csproj API/BiddingService/BiddingService.csproj
RUN dotnet restore API/BiddingService/BiddingService.csproj

#копируем остальные файлы проекта Auction
COPY API/BiddingService API/BiddingService
COPY API/Contracts API/Contracts

WORKDIR /app/API/BiddingService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "BiddingService.dll" ]
