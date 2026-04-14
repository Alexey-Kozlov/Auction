FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/SearchService/SearchService.csproj API/SearchService/SearchService.csproj
RUN dotnet restore API/SearchService/SearchService.csproj -a $TARGETARCH --source /tmp

COPY Auction/API/SearchService API/SearchService

WORKDIR /app/API/SearchService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "SearchService.dll" ]
