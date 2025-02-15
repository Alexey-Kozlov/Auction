import React, { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/Store";
import { NotificationListTypes } from "./NotificationListTypes";
import { useRunNotifyListMutation } from "../../../api/ReportApi";
import { ParameterItem } from "../../../Types";
import NotificationTable from "./NotificationTable";
import { useReactToPrint } from "react-to-print";
import { setEvent } from "../../../store/EventSlice";

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

	useEffect(() => {
		const getReport = async (param: ParameterItem[]) => {
			var rezult = await notifyListReport(param);
			setData(rezult.data);
		};
		if (reportStore && reportStore.param && reportStore.param.length > 0) {
			getReport(reportStore.param);
		}
	}, [reportStore.param]);

	useEffect(() => {
		if (eventStore && eventStore.exportPdfClicked) {
			reactToPrintFn();
			dispatch(setEvent({ exportPdfClicked: false }));
		}
	}, [eventStore.exportPdfClicked]);

	return (
		<div ref={contentRef}>
			<h2 className="text-center text-2xl m-4">Список уведомлений</h2>
			<div>{data && <NotificationTable items={data!} />}</div>
		</div>
	);
}
