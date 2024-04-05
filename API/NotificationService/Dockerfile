FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY /Packages /Packages
COPY API/NotificationService/NotificationService.csproj API/NotificationService/NotificationService.csproj
RUN dotnet restore API/NotificationService/NotificationService.csproj -a $TARGETARCH --source https://api.nuget.org/v3/index.json --source /Packages 

COPY API/NotificationService API/NotificationService


WORKDIR /app/API/NotificationService
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "NotificationService.dll" ]
