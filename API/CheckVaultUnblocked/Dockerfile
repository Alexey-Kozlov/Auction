FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ARG TARGETARCH
WORKDIR /app
EXPOSE 80

COPY LocalNugetPackages /tmp
COPY Auction/API/CheckVaultUnblocked/CheckVaultUnblocked.csproj API/CheckVaultUnblocked/CheckVaultUnblocked.csproj
RUN dotnet restore API/CheckVaultUnblocked/CheckVaultUnblocked.csproj -a $TARGETARCH --source /tmp

COPY Auction/API/CheckVaultUnblocked API/CheckVaultUnblocked

WORKDIR /app/API/CheckVaultUnblocked
RUN dotnet publish -c Release -o /app/API/out

#создание образа
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/API/out .
ENTRYPOINT [ "dotnet", "CheckVaultUnblocked.dll" ]
