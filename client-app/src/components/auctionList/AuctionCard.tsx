import { NavLink } from "react-router-dom";
import { Auction } from "../../types";
import ImageCard from "./ImageCard";
import CountdownTimer from "./CountDownTimer";
import CurrentBid from "./CurrentBid";

type Props = {
  auction: Auction;
};

export default function AuctionCard({ auction }: Props) {
  return (
    <div className="AuctionCard">
      <div className="AuctionCardItem">
        <NavLink
          to={`/auctions/${auction.itemId}`}
          className="no-underline"
        >
          <div className="text-center">
            <ImageCard
              id={auction.itemId}
              detail={false}
              cache={true}
            />
          </div>
          <div className="AuctionCardTitleContainer">
            <div className="AuctionCardTitle">
              <h3 className="AuctionCardTitleText">{auction.title}</h3>
            </div>
          </div>
        </NavLink>
      </div>

      <div className="AuctionCardCountDownContainer">
        <CountdownTimer
          auctionEnd={auction.auctionEnd}
          isFinished={auction.finished}
        />
      </div>
      <div className="AuctionCardCurrentBidContainer">
        <CurrentBid
          reservePrice={auction.reservePrice}
          amount={auction.currentHighBid}
        />
      </div>
    </div>
  );
}
