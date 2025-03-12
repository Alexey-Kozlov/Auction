import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
import { FinanceItem } from "../../store/types";
import NumberWithSpaces from "../../utils/NumberWithSpaces";
import { GrMoney } from "react-icons/gr";
import ImageCard from "../auctionList/ImageCard";
import { NavLink } from "react-router-dom";

type Props = {
	item: FinanceItem;
};

export default function FinRow({ item }: Props) {
	const auction = useGetDetailedViewDataQuery(item.auctionId, {
		skip: !item.auctionId,
	});
	return (
		<>
			<div className="flex items-center">{`${new Date(
				item.actionDate
			).toLocaleDateString("ru-RU")} 
                ${new Date(item?.actionDate).toLocaleTimeString(
									"ru-RU"
								)}`}</div>
			<div className="flex items-center">
				{!auction.isLoading &&
				auction.data?.result?.auctionId &&
				auction.status === "fulfilled" ? (
					<NavLink to={`/auctions/${item.auctionId}`} className="group">
						<ImageCard
							id={item.auctionId}
							dopStyle=" max-h-20"
							zooming={false}
							cache={true}
						/>
					</NavLink>
				) : (
					<GrMoney size={30} />
				)}
			</div>
			<div className="flex items-center">
				{!auction.isLoading &&
				auction.data?.result?.title &&
				auction.status === "fulfilled" ? (
					<NavLink to={`/auctions/${item.auctionId}`} className="group">
						{auction.data?.result?.title}
					</NavLink>
				) : (
					""
				)}
			</div>

			<div className="flex items-center">
				{item.status === 0 ? "Приход" : "Расход"}
			</div>
			<div className="flex items-center">
				{item.value === 0 ? "0" : NumberWithSpaces(item.value)}
			</div>
		</>
	);
}
