FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
#эта папка внутри докерного образа
WORKDIR /app
EXPOSE 80

#копируем все файлы из проекта и восстанавливаем как отдельный слой
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

#Восстанавливаем пакеты проекта
RUN dotnet restore src/IdentityService/IdentityService.csproj

#копируем остальные файлы проекта Auction
COPY src/IdentityService src/IdentityService

WORKDIR /app/src/IdentityService
RUN dotnet publish -c Release -o /app/src/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "IdentityService.dll" ]