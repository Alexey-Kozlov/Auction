FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/LoggingService/LoggingService.csproj API/LoggingService/LoggingService.csproj
RUN dotnet restore API/LoggingService/LoggingService.csproj -a $TARGETARCH --source /tmp

COPY Auction/API/LoggingService API/LoggingService

WORKDIR /app/API/LoggingService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "LoggingService.dll" ]
