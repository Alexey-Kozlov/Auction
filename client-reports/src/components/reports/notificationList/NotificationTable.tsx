import { NotificationListTypes } from "./NotificationListTypes";
import {
	Table,
	TableBody,
	TableCell,
	TableHeader,
	TableHeaderCell,
	TableRow,
} from "semantic-ui-react";

type Props = {
	items: NotificationListTypes[];
};

export default function NotificationTable({ items }: Props) {
	return (
		<>
			<Table celled striped selectable>
				<TableHeader>
					<TableRow>
						<TableHeaderCell textAlign="center">Пользователь</TableHeaderCell>
						<TableHeaderCell>Наименование аукциона</TableHeaderCell>
					</TableRow>
				</TableHeader>
				<TableBody>
					{items!.map((item, index) => (
						<TableRow key={index}>
							<TableCell textAlign="center" collapsing>
								{item.UserLogin}
							</TableCell>
							<TableCell>{item.Title}</TableCell>
						</TableRow>
					))}
				</TableBody>
			</Table>
		</>
	);
}
