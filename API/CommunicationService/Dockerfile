FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/CommunicationService/CommunicationService.csproj API/CommunicationService/CommunicationService.csproj
RUN dotnet restore API/CommunicationService/CommunicationService.csproj -a $TARGETARCH --source /tmp

COPY Auction/API/CommunicationService API/CommunicationService

WORKDIR /app/API/CommunicationService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/API/out .

ENTRYPOINT [ "dotnet", "CommunicationService.dll" ]
