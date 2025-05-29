type Props = {
	amount?: number;
	reservePrice: number;
};

export default function CurrentBid({ reservePrice, amount }: Props) {
	const text = amount ? amount + " руб." : "Нет предложений";
	return (
		<div
			className={
				amount !== 0 ? "AuctionCardCurrentBidDop" : "AuctionCardCurrentBid"
			}
		>
			{text}
		</div>
	);
}
