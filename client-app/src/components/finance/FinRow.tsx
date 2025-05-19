import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
import { FinanceTableItem } from "../../types";
import NumberWithSpaces from "../../utils/NumberWithSpaces";
import { GrMoney } from "react-icons/gr";
import ImageCard from "../auctionList/ImageCard";
import { NavLink } from "react-router-dom";
import { TableCell, TableRow } from "semantic-ui-react";

type Props = {
	item: FinanceTableItem;
};

export default function FinRow({ item }: Props) {
	return (
		<TableRow>
			<TableCell textAlign="center">
				{item.auctionId ? (
					<NavLink to={`/auctions/${item.auctionId}`}>
						<ImageCard
							id={item.auctionId}
							dopStyle="FinanceListImage"
							zooming={false}
							cache={true}
						/>
					</NavLink>
				) : (
					<GrMoney size={30} />
				)}
			</TableCell>
			<TableCell textAlign="center">
				{item.auctionTitle ? (
					<NavLink
						to={`/auctions/${item.auctionId}`}
						className="FinanceListText"
					>
						{item.auctionTitle}
					</NavLink>
				) : (
					""
				)}
			</TableCell>
			<TableCell textAlign="center">
				<div className="FinanceListText">
					{`${new Date(item.actionDate).toLocaleDateString("ru-RU")} 
                ${new Date(item?.actionDate).toLocaleTimeString("ru-RU")}`}
				</div>
			</TableCell>
			<TableCell textAlign="center">
				<div className="FinanceListText">{item.auctionSeller}</div>
			</TableCell>
			<TableCell textAlign="center">
				<div className="FinanceListText">
					{item.status === 0 ? "Приход" : "Расход"}
				</div>
			</TableCell>
			<TableCell textAlign="center">
				<div className="FinanceListText">
					{item.value === 0 ? "0" : NumberWithSpaces(item.value)}
				</div>
			</TableCell>
		</TableRow>
	);
}
