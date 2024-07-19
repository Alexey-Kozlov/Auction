FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY /Packages /Packages
COPY API/AuctionService/AuctionService.csproj API/AuctionService/AuctionService.csproj
RUN dotnet restore API/AuctionService/AuctionService.csproj -a $TARGETARCH --source https://api.nuget.org/v3/index.json --source /Packages 

# #копируем остальные файлы проекта Auction
COPY API/AuctionService API/AuctionService

WORKDIR /app/API/AuctionService
RUN dotnet publish -c Release -o /app/API/out

# #создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]
