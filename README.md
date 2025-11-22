# -

Технологии:
EventSourcing, Microservices, Microfrontend
React + TailWind CSS
RTK Query
.Net9
PostgreSql, EF, AutoMapper
RabbitMq, Kafka, MassTransit
REST, GRPC
SignalR
Saga Orchestration, StateMachine
Redis cache
Elastic Search
OpenTelemetry
Prometheus
Grafana

Перед билдом - обновляем кеш сборок, выполняем команды:
cd /home/alex/Projects/Auction/Common/CollectNugetPackages/bin/Debug/net9.0/
./CollectNugetPackages
будет вывод - колько пакетов обновлены в кеше

команды деплоя

cmd из папки Projects

билдим образы:

docker build -f Auction/API/ReportService/Dockerfile -t kozlovas/auction-api-reports .
docker build -f Auction/API/BiddingService/Dockerfile -t kozlovas/auction-bidding .
docker build -f Auction/API/ElasticSearchService/Dockerfile -t kozlovas/auction-elk .
docker build -f Auction/API/EventSourcingService/Dockerfile -t kozlovas/auction-eventsourcing .
docker build -f Auction/API/FinanceService/Dockerfile -t kozlovas/auction-finance .
docker build -f Auction/API/GatewayService/Dockerfile -t kozlovas/auction-gateway .
docker build -f Auction/API/IdentityService/Dockerfile -t kozlovas/auction-identity .
docker build -f Auction/API/ImageService/Dockerfile -t kozlovas/auction-image .
docker build -f Auction/API/LoggingService/Dockerfile -t kozlovas/auction-logging .
docker build -f Auction/API/NotificationService/Dockerfile -t kozlovas/auction-notification .
docker build -f Auction/API/ProcessingService/Dockerfile -t kozlovas/auction-processing .
docker build -f Auction/API/SearchService/Dockerfile -t kozlovas/auction-search .
docker build -f Auction/client-app/Dockerfile -t kozlovas/auction-front .
docker build -f Auction/client-reports/Dockerfile -t kozlovas/auction-front-reports .
docker build -f Auction/API/CommunicationService/Dockerfile -t kozlovas/auction-communication .
docker build -f Auction/API/CheckVaultUnblocked/Dockerfile -t kozlovas/auction-checkvault .

!!!!!
Если при создании образа будет ошибка вроде - ERROR: failed to build: failed to solve: failed to compute cache key /LocalNugetPackages": not found
- это означает, что в папке LocalNugetPackages есть не все пакеты, что используются в проекте.
Для исправления ошибки - обновляем кеш пакетов в папку LocalNugetPackages так:
cd /home/alex/Projects/Auction/Common/CollectNugetPackages/bin/Debug/net9.0/
./CollectNugetPackages
Все, папка с нугет-пакетами обновлена, можно билдить образы


упаковка пакета с контрактами
из папки Projects/Auction/Common/Contracts
из папки Projects/Auction/Common/Utils
dotnet pack -o /home/alex/Projects/Auctions/Packages/
dotnet pack -o /home/alex/Projects/Auction/Packages/

команды добавления пакетов в проекты
-для debian: dotnet nuget add source /home/alex/Projects/Auctions/Packages/ -n AuctionContracts
-для arch: dotnet nuget add source /home/alex/Projects/Auction/Packages/ -n AuctionContracts
dotnet add package AuctionContracts

получение релиза:
dotnet build --configuration Release

очистка кешей NuGet-пакетов
Если нужно изменить пакет, то из-за кеширования делаем так:

упаковка пакета с контрактами
из папки Projects/Auction/Common/Contracts
из папки Projects/Auction/Common/Utils

- очищаем кеш нугет-пакетов командой - dotnet nuget locals all --clear
- компилируем новый пакет командой - dotnet publish, пересоздаем пакет командой - dotnet pack -o /home/alex/Projects/Auction/Packages/
- будет создан пакет, теперь снова компилируем нужный проект - теперь должны подтянутся изменения в пакете.

прочие команды

kubectl delete deployment auction-front
kubectl apply -f front.yml

миграции

добавление - dotnet ef migrations add "новая_миграция"
удаление - dotnet ef migrations remove
применение изменений в БД - dotnet ef database update
