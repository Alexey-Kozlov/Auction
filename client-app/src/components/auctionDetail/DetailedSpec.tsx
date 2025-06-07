import { Auction, User } from "../../types";
import { useGetUserNameQuery } from "../../api/AuthApi";
import { BiCommentDetail } from "react-icons/bi";
import { PiChats } from "react-icons/pi";

import {
	MenuItem,
	Tab,
	Table,
	TableBody,
	TableCell,
	TableRow,
	TabPane,
} from "semantic-ui-react";
import ChatTable from "./Chat/ChatTable";

type Props = {
	auction: Auction;
	user: User;
};
export default function DetailedSpecs({ auction, user }: Props) {
	const { data, isLoading } = useGetUserNameQuery(auction.seller, {
		skip: !auction.seller,
	});
	const panes = [
		{
			menuItem: (
				<MenuItem key="Detail">
					<span className="MenuItemsText">
						<BiCommentDetail className="MenuItems" size={20} />
						Описание аукциона
					</span>
				</MenuItem>
			),
			render: () => (
				<TabPane>
					<Table celled striped>
						<TableBody>
							<TableRow>
								<TableCell>Продавец</TableCell>
								<TableCell>{!isLoading && data?.result}</TableCell>
							</TableRow>
							<TableRow>
								<TableCell>Наименование</TableCell>
								<TableCell>{auction.title}</TableCell>
							</TableRow>
							<TableRow>
								<TableCell>Описание</TableCell>
								<TableCell>
									{auction?.properties &&
										auction.properties.split("\n").map((line, index) => {
											return <p key={index}>{line}</p>;
										})}
								</TableCell>
							</TableRow>
							<TableRow>
								<TableCell>Есть начальная цена?</TableCell>
								<TableCell>
									{auction?.reservePrice > 0
										? `Да - ${auction?.reservePrice} руб.`
										: "Нет"}
								</TableCell>
							</TableRow>
							<TableRow>
								<TableCell>Примечание</TableCell>
								<TableCell>
									{auction?.description &&
										auction.description.split("\n").map((line, index) => {
											return <p key={index}>{line}</p>;
										})}
								</TableCell>
							</TableRow>
							<TableRow>
								<TableCell>Дата завершения</TableCell>
								<TableCell>
									{auction.auctionEnd.toLocaleDateString() +
										" " +
										auction.auctionEnd.toLocaleTimeString()}
								</TableCell>
							</TableRow>
						</TableBody>
					</Table>
				</TabPane>
			),
		},
		{
			menuItem: (
				<MenuItem key="Chat">
					<span className="MenuItemsText">
						<PiChats className="MenuItems" size={20} />
						Обсуждение
					</span>
				</MenuItem>
			),
			render: () => (
				<TabPane>
					<ChatTable auction={auction} user={user} />
				</TabPane>
			),
		},
	];

	return <Tab panes={panes} />;
}
