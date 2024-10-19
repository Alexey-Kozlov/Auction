# -

Технологии:
React + TailWind CSS
RTK Query
Microservices .Net8
PostgreSql, EF, AutoMapper
RabbitMq, MassTransit
REST, GRPC
SignalR
Saga Orchestration, StateMachine
Redis cache
Elastic Search
OpenTelemetry
Prometheus
Grafana

команды деплоя

cmd из папки Projects/Auctions

билдим образы:
docker build -f API/EventSourcingService/Dockerfile -t kozlovas/auction-eventsourcing .
docker build -f API/BiddingService/Dockerfile -t kozlovas/auction-bidding .
docker build -f API/ElasticSearchService/Dockerfile -t kozlovas/auction-elk .
docker build -f API/FinanceService/Dockerfile -t kozlovas/auction-finance .
docker build -f API/GatewayService/Dockerfile -t kozlovas/auction-gateway .
docker build -f API/IdentityService/Dockerfile -t kozlovas/auction-identity .
docker build -f API/ImageService/Dockerfile -t kozlovas/auction-image .
docker build -f API/NotificationService/Dockerfile -t kozlovas/auction-notification .
docker build -f API/ProcessingService/Dockerfile -t kozlovas/auction-processing .
docker build -f API/SearchService/Dockerfile -t kozlovas/auction-search .
docker build -f client-app/Dockerfile -t kozlovas/auction-front .

упаковка пакета с контрактами
из папки Projects/Auction/Common/Contracts
из папки Projects/Auction/Common/Utils
dotnet pack -o ~/Projects/Auctions/Packages/
dotnet pack -o /var/projects/Auction/Packages/

команды добавления пакетов в проекты
-для debian: dotnet nuget add source ~/Projects/Auctions/Packages/ -n AuctionContracts
-для arch: dotnet nuget add source /var/projects/Auction/Packages/ -n AuctionContracts
dotnet add package AuctionContracts

очистка кешей NuGet-пакетов
Если нужно изменить пакет, то из-за кеширования делаем так:

- удаляем пакет из папки пакетов (в нашем случае из ~/Projects/Auctions/Packages/)
- очищаем кеш нугет-пакетов командой - dotnet nuget locals all --clear
- компилируем проект командой dotnet build - будет ошибка, что пакет не найден и нет нужных типов
- компилируем новый пакет командой - dotnet publish, пересоздаем пакет командой - dotnet pack -o /var/projects/Auction/Packages/
- будет создан пакет, теперь снова компилируем нужный проект - теперь должны подтянутся изменения в пакете.

прочие команды

kubectl delete deployment auction-front
kubectl apply -f front.yml

миграции

добавление - dotnet ef migrations add "новая_миграция"
удаление - dotnet ef migrations remove
применение изменений в БД - dotnet ef database update
