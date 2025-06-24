import { Auction, DataRow } from "../../types";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useEffect, useState } from "react";

type Props = {
	auction: Auction;
};

export default function TabDetailInfo({ auction }: Props) {
	const [rows, setRows] = useState<DataRow[]>([]);
	useEffect(() => {
		if (auction) {
			let _rows: DataRow[] = [];
			_rows.push({ field: "Наименование", value: auction.title });
			_rows.push({ field: "Автор аукциона", value: auction.sellerName! });
			_rows.push({
				field: "Завершение аукциона",
				value:
					auction.auctionEnd.toLocaleDateString() +
					" " +
					auction.auctionEnd.toLocaleTimeString(),
			});
			_rows.push({
				field: "Есть начальная цена?",
				value:
					auction?.reservePrice > 0
						? `Да - ${auction?.reservePrice} руб.`
						: "Нет",
			});
			_rows.push({
				field: "Описание",
				value: auction?.properties
					? auction.properties.split("\n").map((line, index) => {
							return <p key={index}>{line}</p>;
					  })
					: "",
			});
			_rows.push({
				field: "Примечание",
				value: auction?.description
					? auction.description.split("\n").map((line, index) => {
							return <p key={index}>{line}</p>;
					  })
					: "",
			});
			setRows((prev) => {
				return [...prev, ..._rows];
			});
		}
		// eslint-disable-next-line
	}, [auction]);

	return (
		<div>
			<DataTable
				value={rows}
				stripedRows
				showGridlines
				tableStyle={{ fontSize: "1.6rem" }}
			>
				<Column
					field="field"
					className="w-30rem"
					headerClassName="hidden"
				></Column>
				<Column headerClassName="hidden" field="value"></Column>
			</DataTable>
		</div>
	);
}
