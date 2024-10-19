FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY /Packages /Packages
COPY API/EventSourcingService/EventSourcingService.csproj API/EventSourcingService/EventSourcingService.csproj
RUN dotnet restore API/EventSourcingService/EventSourcingService.csproj -a $TARGETARCH --source https://api.nuget.org/v3/index.json --source /Packages 

# #копируем остальные файлы проекта Auction
COPY API/EventSourcingService API/EventSourcingService

WORKDIR /app/API/EventSourcingService
RUN dotnet publish -c Release -o /app/API/out

# #создание образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "EventSourcingService.dll" ]
