import toast from "react-hot-toast";
import { Auction, AuctionImage } from "../../types";
import { NavLink } from "react-router-dom";
import { useGetImageForAuctionQuery } from "../../api/ImageApi";
const empty = require("../../assets/Empty.png");

type Props = {
	auction: Auction;
	toastId: string;
};

export default function AuctionDeletedToast({ auction, toastId }: Props) {
	const { isLoading, data } = useGetImageForAuctionQuery({
		id: auction.auctionId,
		cache: true,
	});
	return (
		<div>
			<div className="flex flex-row-reverse">
				<button onClick={() => toast.dismiss(toastId)}>X</button>
			</div>
			<NavLink
				to={`/auctions/${auction.auctionId}`}
				className="flex flex-col items-center"
			>
				<div className="flex flex-row items-center gap-2">
					<img
						src={
							!isLoading && (data?.result as AuctionImage)!.image
								? `data:image/png;base64 , ${data?.result["image"]}`
								: empty
						}
						alt=""
						height={80}
						width={80}
						className="rounded-lg"
					/>
					<span>{`Аукцион - "${auction.title}" удален`}</span>
				</div>
			</NavLink>
		</div>
	);
}
