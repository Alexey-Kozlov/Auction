FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH

WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/GatewayService/GatewayService.csproj API/GatewayService/GatewayService.csproj
RUN dotnet restore API/GatewayService/GatewayService.csproj -a $TARGETARCH --source /tmp

#копируем остальные файлы проекта Auction
COPY Auction/API/GatewayService API/GatewayService

WORKDIR /app/API/GatewayService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]
