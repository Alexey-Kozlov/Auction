import { Auction } from "../../types";
import { useGetUserNameQuery } from "../../api/AuthApi";
import { Table, TableBody, TableCell, TableRow } from "semantic-ui-react";

type Props = {
	auction: Auction;
};
export default function DetailedSpecs({ auction }: Props) {
	const { data, isLoading } = useGetUserNameQuery(auction.seller, {
		skip: !auction.seller,
	});
	return (
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
	);
}
