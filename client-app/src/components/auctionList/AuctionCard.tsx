import { NavLink } from "react-router-dom";
import { Auction } from "../../store/types";
import ImageCard from "./ImageCard";
import CountdownTimer from "./CountDownTimer";
import CurrentBid from "./CurrentBid";

type Props = {
	auction: Auction;
};

export default function AuctionCard({ auction }: Props) {
	return (
		<div className="AuctionCard">
			<NavLink to={`/auctions/${auction.auctionId}`}>
				<div className="AuctionImageContainer">
					<ImageCard
						id={auction.auctionId}
						dopStyle=" max-h-60"
						zooming={false}
						cache={true}
					/>
				</div>
				<div className="AuctionCardTitleContainer">
					<div className="AuctionCardTitle">
						<h3 className="AuctionCardTitleText">{auction.title}</h3>
					</div>
				</div>
			</NavLink>
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
