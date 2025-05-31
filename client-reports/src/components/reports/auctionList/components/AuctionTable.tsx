import { AuctionListTypes } from "../AuctionListTypes";
import {
	Table,
	TableBody,
	TableCell,
	TableHeader,
	TableHeaderCell,
	TableRow,
} from "semantic-ui-react";

type Props = {
	items: AuctionListTypes[];
};

export default function AuctionTable({ items }: Props) {
	return (
		<div>
			<Table celled selectable striped>
				<TableHeader>
					<TableRow>
						<TableHeaderCell textAlign="center">Продавец</TableHeaderCell>
						<TableHeaderCell textAlign="center">Наименование</TableHeaderCell>
						<TableHeaderCell textAlign="center">Дата начала</TableHeaderCell>
						<TableHeaderCell textAlign="center">
							Дата завершения
						</TableHeaderCell>
					</TableRow>
				</TableHeader>
				<TableBody>
					{items!.map((item, index) => (
						<TableRow
							key={index}
							className="bg-white dark:border-gray-700 dark:bg-gray-800"
						>
							<TableCell textAlign="center">{item.Seller}</TableCell>
							<TableCell>{item.Title}</TableCell>
							<TableCell className="text-center">
								{new Date(item.StartDate).toLocaleDateString()}
							</TableCell>
							<TableCell className="text-center">
								{new Date(item.EndDate).toLocaleDateString()}
							</TableCell>
						</TableRow>
					))}
				</TableBody>
			</Table>
		</div>
	);
}
