# -

React+Microservices

команды деплоя

cmd из папки Projects/Auctions

билдим образы:
docker build -f API/AuctionService/Dockerfile -t kozlovas/auction-auction-api .
docker build -f API/BiddingService/Dockerfile -t kozlovas/auction-bidding-api .
docker build -f API/GatewayService/Dockerfile -t kozlovas/auction-gateway-api .
docker build -f API/IdentityService/Dockerfile -t kozlovas/auction-identity-api .
docker build -f API/ImageService/Dockerfile -t kozlovas/auction-image-api .
docker build -f API/NotificationService/Dockerfile -t kozlovas/auction-notification-api .
docker build -f API/SearchService/Dockerfile -t kozlovas/auction-search-api .
docker build -f client-app/Dockerfile -t kozlovas/auction-front .

пушим

docker push kozlovas/auction-auction-api
docker push kozlovas/auction-bidding-api
docker push kozlovas/auction-gateway-api
docker push kozlovas/auction-identity-api
docker push kozlovas/auction-image-api
docker push kozlovas/auction-notification-api
docker push kozlovas/auction-search-api
docker push kozlovas/auction-front
