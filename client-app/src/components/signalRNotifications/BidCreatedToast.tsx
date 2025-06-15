import { AuctionImage } from "../../types";
import { NavLink } from "react-router-dom";
import { useGetImageForAuctionQuery } from "../../api/ImageApi";
import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
import { Button } from "primereact/button";
//const empty = require("../../assets/Empty.png");

type Props = {
	auctionId: string;
	toastId: string;
};

export default function BidCreatedToast({ auctionId, toastId }: Props) {
	const { isLoading, data } = useGetImageForAuctionQuery({
		id: auctionId,
		cache: true,
	});
	const bidAuction = useGetDetailedViewDataQuery(auctionId, {
		refetchOnMountOrArgChange: true,
	});

	return (
		<div>
			{!bidAuction.isLoading && !bidAuction.isFetching && (
				<>
					<div className="ToastCloseButton">
						<Button
							className="MainButton w-40"
							//onClick={() => toast.dismiss(toastId)}
						>
							X
						</Button>
					</div>
					<NavLink to={`/auctions/${auctionId}`}>
						<div className="ToastMessageContainer">
							{/* <img
								className="ToastImage"
								src={
									!isLoading && (data?.result as AuctionImage)!.image
										? `data:image/png;base64 , ${data?.result["image"]}`
										: empty
								}
								alt=""
							/> */}
							<span className="ToastItemText">
								{`Сделана новая ставка для аукциона "${bidAuction.data?.result.title}" - ${bidAuction.data?.result.currentHighBid} руб.`}
							</span>
						</div>
					</NavLink>
				</>
			)}
		</div>
	);
}
