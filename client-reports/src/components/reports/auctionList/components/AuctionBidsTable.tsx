import React from "react";
import { AuctionListTypes } from "../AuctionListTypes";
import { Table } from "flowbite-react";

type Props = {
	items: AuctionListTypes[] | undefined;
};

export default function AuctionBidsTable({ items }: Props) {
	return (
		<>
			<Table hoverable>
				<Table.Head>
					<Table.HeadCell className="text-left">Продавец</Table.HeadCell>
					<Table.HeadCell className="text-left">Наименование</Table.HeadCell>
					<Table.HeadCell className="text-center">Дата начала</Table.HeadCell>
					<Table.HeadCell className="text-center">
						Дата завершения
					</Table.HeadCell>
					<Table.HeadCell className="text-left">Автор ставки</Table.HeadCell>
					<Table.HeadCell className="text-left">Размер ставки</Table.HeadCell>
				</Table.Head>
				<Table.Body className="divide-y scrollable-body">
					{items!.map((item, index) => (
						<Table.Row
							key={index}
							className="bg-white dark:border-gray-700 dark:bg-gray-800"
						>
							<Table.Cell>{item.Seller}</Table.Cell>
							<Table.Cell>{item.Title}</Table.Cell>
							<Table.Cell className="text-center">
								{new Date(item.StartDate).toLocaleDateString()}
							</Table.Cell>
							<Table.Cell className="text-center">
								{new Date(item.EndDate).toLocaleDateString()}
							</Table.Cell>
							<Table.Cell>{item.Bidder}</Table.Cell>
							<Table.Cell>{item.Amount === 0 ? "" : item.Amount}</Table.Cell>
						</Table.Row>
					))}
				</Table.Body>
			</Table>
		</>
	);
}
