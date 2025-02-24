import AuctionListParams from "../reports/auctionList/AuctionListParams";
import { useParams } from "react-router-dom";
import TestItem from "../reports/testItem/testItem";
import NotificationListParams from "../reports/notificationList/NotificationListParams";
import ReportFooter from "./ReportFooter";
import Header from "./header/Header";

export default function Main() {
	const { id } = useParams();
	const report = () => {
		switch (id) {
			case "AuctionList":
				return <AuctionListParams />;
			case "NotificationList":
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
