import React, { useEffect, useState } from "react";
import {
  AuctionBidSortType,
  AuctionListSortColumn,
  AuctionListTypes,
} from "../AuctionListTypes";
import { SortDirection } from "../../../../types";
import { dynamicSort } from "../../../../utils";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";

type Props = {
  items: AuctionListTypes[] | undefined;
};

export default function AuctionBidsTable({ items }: Props) {
  const [repItems, setRepItems] = useState<AuctionListTypes[] | null>();

  const [sortState, setSortState] = useState<AuctionBidSortType>({
    column: AuctionListSortColumn.Title,
    direction: SortDirection.ascending,
  });

  const getSortedValue = (val: AuctionListSortColumn) => {
    return sortState.column === val
      ? sortState.direction === SortDirection.ascending
        ? "ascending"
        : "descending"
      : undefined;
  };

  const handleSetSort = (value: AuctionBidSortType) => {
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
      let _temp = JSON.parse(JSON.stringify(prev)) as AuctionListTypes[];
      return _temp?.sort(
        dynamicSort(AuctionListSortColumn[value.column], value.direction)
      );
    });
  };

  useEffect(() => {
    setRepItems(() => items);
  }, []);
  return (
    <>
      <DataTable value={repItems!}>
        <Column
          field="Seller"
          header="Продавец"
        ></Column>
        <Column
          field="Title"
          header="Наименование"
        ></Column>
        <Column
          field="StartDate"
          header="Дата начала"
        ></Column>
        <Column
          field="EndDate"
          header="Дата завершения"
        ></Column>
        <Column
          field="Bidder"
          header="Автор ставки"
        ></Column>
        <Column
          field="Amount"
          header="Размер ставки"
        ></Column>
        {/* <TableHeader>
					<TableRow>
						<TableHeaderCell
							textAlign="center"
							sorted={getSortedValue(AuctionListSortColumn.Seller)}
							onClick={() =>
								handleSetSort({
									column: AuctionListSortColumn.Seller,
									direction: sortState.direction,
								})
							}
						>
							Продавец
						</TableHeaderCell>
						<TableHeaderCell
							textAlign="center"
							sorted={getSortedValue(AuctionListSortColumn.Title)}
							onClick={() =>
								handleSetSort({
									column: AuctionListSortColumn.Title,
									direction: sortState.direction,
								})
							}
						>
							Наименование
						</TableHeaderCell>
						<TableHeaderCell
							textAlign="center"
							sorted={getSortedValue(AuctionListSortColumn.StartDate)}
							onClick={() =>
								handleSetSort({
									column: AuctionListSortColumn.StartDate,
									direction: sortState.direction,
								})
							}
						>
							Дата начала
						</TableHeaderCell>
						<TableHeaderCell
							textAlign="center"
							sorted={getSortedValue(AuctionListSortColumn.EndDate)}
							onClick={() =>
								handleSetSort({
									column: AuctionListSortColumn.EndDate,
									direction: sortState.direction,
								})
							}
						>
							Дата завершения
						</TableHeaderCell>
						<TableHeaderCell
							textAlign="center"
							sorted={getSortedValue(AuctionListSortColumn.Bidder)}
							onClick={() =>
								handleSetSort({
									column: AuctionListSortColumn.Bidder,
									direction: sortState.direction,
								})
							}
						>
							Автор ставки
						</TableHeaderCell>
						<TableHeaderCell
							textAlign="center"
							sorted={getSortedValue(AuctionListSortColumn.Amount)}
							onClick={() =>
								handleSetSort({
									column: AuctionListSortColumn.Amount,
									direction: sortState.direction,
								})
							}
						>
							Размер ставки
						</TableHeaderCell>
					</TableRow>
				</TableHeader>
				<TableBody>
					{repItems?.map((item, index) => (
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
				</TableBody> */}
      </DataTable>
    </>
  );
}
