FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/ImageService/ImageService.csproj API/ImageService/ImageService.csproj
RUN dotnet restore API/ImageService/ImageService.csproj -a $TARGETARCH --source /tmp

#копируем остальные файлы проекта Auction
COPY Auction/API/ImageService API/ImageService

WORKDIR /app/API/ImageService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "ImageService.dll" ]
