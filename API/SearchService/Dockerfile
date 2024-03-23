FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
#эта папка внутри докерного образа
WORKDIR /app
EXPOSE 80

COPY API/Contracts/Contracts.csproj API/Contracts/Contracts.csproj
COPY API/SearchService/SearchService.csproj API/SearchService/SearchService.csproj
RUN dotnet restore API/SearchService/SearchService.csproj

#копируем остальные файлы проекта Auction
COPY API/SearchService API/SearchService
COPY API/Contracts API/Contracts

WORKDIR /app/API/SearchService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "SearchService.dll" ]
