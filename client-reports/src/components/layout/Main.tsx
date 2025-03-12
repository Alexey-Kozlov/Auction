import AuctionListParams from "../reports/auctionList/AuctionListParams";
import { useParams } from "react-router-dom";
import TestItem from "../reports/testItem/testItem";
import NotificationListParams from "../reports/notificationList/NotificationListParams";
import ReportFooter from "./ReportFooter";
import Header from "./header/Header";
import { useCookies } from "react-cookie";
import { ReportType } from "../../types";
import { v4 as uuidv4 } from "uuid";

export default function Main() {
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
	const { id } = useParams();
	const report = () => {
		setCookie("RequestId", uuidv4());
		switch (id) {
			case "AuctionList":
				setCookie("RequestType", ReportType[ReportType.AuctionList]);
				return <AuctionListParams />;
			case "NotificationList":
				setCookie("RequestType", ReportType[ReportType.NotificationList]);
				return <NotificationListParams />;
			case "TestItem":
				return <TestItem />;
			default:
				return null;
		}
	};
	return (
		<div className="flex flex-col h-screen">
			<Header />
			<div className="flex-1">{report()}</div>
			<ReportFooter />
		</div>
	);
}
