FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY /Packages /Packages
COPY API/SearchService/SearchService.csproj API/SearchService/SearchService.csproj
RUN dotnet restore API/SearchService/SearchService.csproj -a $TARGETARCH --source https://api.nuget.org/v3/index.json --source /Packages 

COPY API/SearchService API/SearchService

WORKDIR /app/API/SearchService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "SearchService.dll" ]
