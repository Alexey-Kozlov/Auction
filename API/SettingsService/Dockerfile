FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/SettingsService/SettingsService.csproj API/SettingsService/SettingsService.csproj
RUN dotnet restore API/SettingsService/SettingsService.csproj -a $TARGETARCH --source /tmp

#копируем остальные файлы проекта Auction
COPY Auction/API/SettingsService API/SettingsService

WORKDIR /app/API/SettingsService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/API/out .
RUN apt update && apt install -y libgssapi-krb5-2
ENTRYPOINT [ "dotnet", "SettingsService.dll" ]
