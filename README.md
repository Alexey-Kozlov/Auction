# -

React+Microservices

создание образов

cmd из папки 'Projects/Auctions

выполняем для каждого образа:
docker build -f API/AuctionService/Dockerfile -t kozlovas/auction-auction-api .
docker build -f API/BiddingService/Dockerfile -t kozlovas/auction-bidding-api .
docker build -f API/GatewayService/Dockerfile -t kozlovas/auction-gateway-api .
docker build -f API/IdentityService/Dockerfile -t kozlovas/auction-identity-api .
docker build -f API/ImageService/Dockerfile -t kozlovas/auction-image-api .
docker build -f API/NotificationService/Dockerfile -t kozlovas/auction-notificaction-api .
docker build -f API/SearchService/Dockerfile -t kozlovas/auction-search-api .
docker build -f client-app/Dockerfile -t kozlovas/auction-front .
