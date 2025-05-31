import React, { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/Store";
import { NotificationListTypes } from "./NotificationListTypes";
import { useRunNotifyListMutation } from "../../../api/ReportApi";
import { ParameterItem } from "../../../types";
import NotificationTable from "./NotificationTable";
import { useReactToPrint } from "react-to-print";
import { setEvent } from "../../../store/EventSlice";
import { useDownloadExcel } from "react-export-table-to-excel";
import { setReportLoaded } from "../../../store/ReportSlice";
import Waiter from "../../Waiter";

type Props = {
	reportId: string;
};

export default function RenderReport({ reportId }: Props) {
	const reportStore = useSelector((state: RootState) => state.reportStore);
	const eventStore = useSelector((state: RootState) => state.eventStore);
	const dispatch = useDispatch();
	const [data, setData] = useState<NotificationListTypes[]>();
	const [notifyListReport] = useRunNotifyListMutation();
	const contentRef = useRef<HTMLDivElement>(null);
	const reactToPrintFn = useReactToPrint({ contentRef });
	const { onDownload } = useDownloadExcel({
		currentTableRef: contentRef.current,
		filename: reportId,
		sheet: reportId,
	});

	useEffect(() => {
		const getReport = async (param: ParameterItem[]) => {
			var rezult = await notifyListReport(param);
			setData(rezult.data);
			dispatch(setReportLoaded());
		};
		if (reportStore && reportStore.param && reportStore.param.length > 0) {
			getReport(reportStore.param);
		}
	}, [reportStore, dispatch, notifyListReport]);

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
			<h2 className="text-center">Список уведомлений</h2>
			{reportStore.reportLoading && (
				<div>
					<Waiter color="rgb(156 163 175)" />
				</div>
			)}
			{!reportStore.reportLoading && data && (
				<div>
					<NotificationTable items={data!} />
				</div>
			)}
		</div>
	);
}
