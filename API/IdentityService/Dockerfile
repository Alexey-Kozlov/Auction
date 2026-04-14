FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/IdentityService/IdentityService.csproj API/IdentityService/IdentityService.csproj
RUN dotnet restore API/IdentityService/IdentityService.csproj -a $TARGETARCH --source /tmp

#копируем остальные файлы проекта Auction
COPY Auction/API/IdentityService API/IdentityService

WORKDIR /app/API/IdentityService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "IdentityService.dll" ]