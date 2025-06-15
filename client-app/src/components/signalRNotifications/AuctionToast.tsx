import { AuctionImage } from "../../types";
import { NavLink } from "react-router-dom";
import { useGetImageForAuctionQuery } from "../../api/ImageApi";
import { Button } from "primereact/button";
//const empty = require("../../assets/Empty.png");

type Props = {
	auctionId: string;
	toastId: string;
	message: string;
};

export default function AuctionToast({ auctionId, toastId, message }: Props) {
	const { isLoading, data } = useGetImageForAuctionQuery(
		{
			id: auctionId,
			cache: true,
		},
		{ refetchOnMountOrArgChange: true }
	);
	return (
		<div>
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
						src={
							!isLoading && (data?.result as AuctionImage)!.image
								? `data:image/png;base64 , ${data?.result["image"]}`
								: empty
						}
						alt=""
						className="ToastImage"
					/> */}
					<span className="ToastItemText">{`${message}`}</span>
				</div>
			</NavLink>
		</div>
	);
}
