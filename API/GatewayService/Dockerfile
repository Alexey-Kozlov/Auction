FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH

WORKDIR /app
EXPOSE 80

COPY /Packages /Packages
COPY API/GatewayService/GatewayService.csproj API/GatewayService/GatewayService.csproj
RUN dotnet restore API/GatewayService/GatewayService.csproj -a $TARGETARCH --source https://api.nuget.org/v3/index.json --source /Packages 

#копируем остальные файлы проекта Auction
COPY API/GatewayService API/GatewayService

WORKDIR /app/API/GatewayService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]
