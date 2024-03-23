FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
#эта папка внутри докерного образа
WORKDIR /app
EXPOSE 80

#копируем все файлы из проекта и восстанавливаем как отдельный слой
COPY API/IdentityService/IdentityService.csproj API/IdentityService/IdentityService.csproj
RUN dotnet restore API/IdentityService/IdentityService.csproj

#копируем остальные файлы проекта Auction
COPY API/IdentityService API/IdentityService

WORKDIR /app/API/IdentityService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "IdentityService.dll" ]