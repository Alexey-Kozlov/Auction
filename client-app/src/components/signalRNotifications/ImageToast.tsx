import { AuctionImage, SignalREvents } from "../../types";
import { NavLink } from "react-router-dom";
import { useGetImageForAuctionQuery } from "../../api/ImageApi";
import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
const empty = require("../../assets/Empty.png");

type Props = {
  auctionId: string;
  messageType: SignalREvents;
  messageText?: string;
};

export default function ImageToast({
  auctionId,
  messageType,
  messageText,
}: Props) {
  const { isLoading, data } = useGetImageForAuctionQuery({
    id: auctionId,
    cache: true,
  });

  const auctionData = useGetDetailedViewDataQuery(auctionId, {
    skip: !auctionId,
  });
  let text = "";
  switch (messageType) {
    //BidPlaced
    case SignalREvents.BidPlaced:
      text = `Сделана новая ставка для аукциона "${auctionData.data?.result.title}" - ${auctionData.data?.result.currentHighBid} руб.`;
      break;
    //AuctionCreated
    case SignalREvents.AuctionCreate:
      text = `Создан новый аукцион - "${auctionData.data?.result.title}"`;
      break;
    //AuctionUpdate
    case SignalREvents.AuctionUpdate:
      text = `Обновлен аукцион - "${auctionData.data?.result.title}"`;
      break;
    //AuctionFinished
    case SignalREvents.AuctionFinished:
      text = messageText!;
      break;
  }

  return (
    <div className="w-full">
      <NavLink
        to={`/auctions/${auctionId}`}
        className="no-underline"
      >
        <div className="ToastMessageContainer">
          <img
            className="ToastImage"
            src={
              !isLoading && (data?.result as AuctionImage)!.image
                ? `data:image/png;base64 , ${data?.result["image"]}`
                : empty
            }
            alt=""
          />
          <span className="ToastItemText">{text}</span>
        </div>
      </NavLink>
    </div>
  );
}
