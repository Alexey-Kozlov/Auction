import React from "react";
import { NotificationListTypes } from "./NotificationListTypes";
import { Table } from "flowbite-react";

type Props = {
	items: NotificationListTypes[];
};

export default function NotificationTable({ items }: Props) {
	return (
		<>
			<Table hoverable>
				<Table.Head>
					<Table.HeadCell className="text-left">Пользователь</Table.HeadCell>
					<Table.HeadCell className="text-left">
						Наименование аукциона
					</Table.HeadCell>
				</Table.Head>
				<Table.Body className="divide-y scrollable-body">
					{items!.map((item, index) => (
						<Table.Row
							key={index}
							className="bg-white dark:border-gray-700 dark:bg-gray-800"
						>
							<Table.Cell>{item.UserLogin}</Table.Cell>
							<Table.Cell>{item.Title}</Table.Cell>
						</Table.Row>
					))}
				</Table.Body>
			</Table>
		</>
	);
}
