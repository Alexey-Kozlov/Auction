import React from "react";
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
	items: AuctionListTypes[] | undefined;
};

export default function AuctionBidsTable({ items }: Props) {
	return (
		<>
			<Table celled selectable striped>
				<TableHeader>
					<TableRow>
						<TableHeaderCell textAlign="center">Продавец</TableHeaderCell>
						<TableHeaderCell textAlign="center">Наименование</TableHeaderCell>
						<TableHeaderCell textAlign="center">Дата начала</TableHeaderCell>
						<TableHeaderCell textAlign="center">
							Дата завершения
						</TableHeaderCell>
						<TableHeaderCell textAlign="center">Автор ставки</TableHeaderCell>
						<TableHeaderCell textAlign="center">Размер ставки</TableHeaderCell>
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
							<TableCell textAlign="center">{item.Bidder}</TableCell>
							<TableCell textAlign="center">
								{item.Amount === 0 ? "" : item.Amount}
							</TableCell>
						</TableRow>
					))}
				</TableBody>
			</Table>
		</>
	);
}
