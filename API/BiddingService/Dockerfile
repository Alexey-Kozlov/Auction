FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/BiddingService/BiddingService.csproj API/BiddingService/BiddingService.csproj
RUN dotnet restore API/BiddingService/BiddingService.csproj -a $TARGETARCH --source /tmp

#копируем остальные файлы проекта Auction
COPY Auction/API/BiddingService API/BiddingService

WORKDIR /app/API/BiddingService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/API/out .
#RUN rm -rf /tmp
ENTRYPOINT [ "dotnet", "BiddingService.dll" ]
