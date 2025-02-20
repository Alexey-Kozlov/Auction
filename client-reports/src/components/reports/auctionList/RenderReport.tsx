import React, { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/Store";
import { AuctionListTypes } from "./AuctionListTypes";
import { useRunAuctionListMutation } from "../../../api/ReportApi";
import { ParameterItem } from "../../../Types";
import AuctionBidsTable from "./components/AuctionBidsTable";
import AuctionTable from "./components/AuctionTable";
import { useReactToPrint } from "react-to-print";
import { setEvent } from "../../../store/EventSlice";
import { useDownloadExcel } from "react-export-table-to-excel";

type Props = {
	reportId: string;
};

export default function RenderReport({ reportId }: Props) {
	const reportStore = useSelector((state: RootState) => state.reportStore);
	const eventStore = useSelector((state: RootState) => state.eventStore);
	const dispatch = useDispatch();
	const [data, setData] = useState<AuctionListTypes[]>();
	const [auctionListReport] = useRunAuctionListMutation();
	const contentRef = useRef<HTMLDivElement>(null);
	const reactToPrintFn = useReactToPrint({ contentRef });
	const { onDownload } = useDownloadExcel({
		currentTableRef: contentRef.current,
		filename: reportId,
		sheet: reportId,
	});

	useEffect(() => {
		const getReport = async (param: ParameterItem[]) => {
			var rezult = await auctionListReport(param);
			setData(rezult.data);
		};
		if (reportStore && reportStore.param && reportStore.param.length > 0) {
			getReport(reportStore.param);
		}
	}, [reportStore, auctionListReport]);

	useEffect(() => {
		if (eventStore && eventStore.exportPdfClicked) {
			reactToPrintFn();
			dispatch(setEvent({ exportPdfClicked: false }));
		}
		if (eventStore && eventStore.exportExcelClicked) {
			onDownload();
			dispatch(setEvent({ exportExcelClicked: false }));
		}
	}, [eventStore, dispatch, reactToPrintFn, onDownload]);

	return (
		<div ref={contentRef}>
			<h2 className="text-center text-2xl m-4">Список аукционов</h2>
			<div>
				{data && reportStore.param[1].Value.toLocaleLowerCase() === "true" && (
					<AuctionBidsTable items={data} />
				)}
				{data && reportStore.param[1].Value.toLocaleLowerCase() !== "true" && (
					<AuctionTable items={data} />
				)}
			</div>
		</div>
	);
}
