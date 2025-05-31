import { useEffect, useState } from "react";
import {
	NotificationListTypes,
	NotificationSortColumn,
	NotificationSortType,
} from "./NotificationListTypes";
import {
	Table,
	TableBody,
	TableCell,
	TableHeader,
	TableHeaderCell,
	TableRow,
} from "semantic-ui-react";
import { SortDirection } from "../../../types";
import { dynamicSort } from "../../../utils";

type Props = {
	items: NotificationListTypes[];
};

export default function NotificationTable({ items }: Props) {
	const [repItems, setRepItems] = useState<NotificationListTypes[] | null>();

	const [sortState, setSortState] = useState<NotificationSortType>({
		column: NotificationSortColumn.Title,
		direction: SortDirection.ascending,
	});

	const getSortedValue = (val: NotificationSortColumn) => {
		return sortState.column === val
			? sortState.direction === SortDirection.ascending
				? "ascending"
				: "descending"
			: undefined;
	};

	const handleSetSort = (value: NotificationSortType) => {
		//направление сортировки
		setSortState((prev) => {
			return {
				...value,
				direction:
					prev.direction === SortDirection.ascending
						? SortDirection.descending
						: SortDirection.ascending,
			};
		});
		//сортируем данные
		setRepItems((prev) => {
			let _temp = JSON.parse(JSON.stringify(prev)) as NotificationListTypes[];
			return _temp?.sort(
				dynamicSort(NotificationSortColumn[value.column], value.direction)
			);
		});
	};

	useEffect(() => {
		setRepItems(() => items);
	}, []);
	return (
		<>
			<Table sortable celled striped selectable>
				<TableHeader>
					<TableRow>
						<TableHeaderCell
							textAlign="center"
							sorted={getSortedValue(NotificationSortColumn.UserLogin)}
							onClick={() =>
								handleSetSort({
									column: NotificationSortColumn.UserLogin,
									direction: sortState.direction,
								})
							}
						>
							Пользователь
						</TableHeaderCell>
						<TableHeaderCell
							sorted={getSortedValue(NotificationSortColumn.Title)}
							onClick={() =>
								handleSetSort({
									column: NotificationSortColumn.Title,
									direction: sortState.direction,
								})
							}
						>
							Наименование аукциона
						</TableHeaderCell>
					</TableRow>
				</TableHeader>
				<TableBody>
					{repItems?.map((item, index) => (
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
