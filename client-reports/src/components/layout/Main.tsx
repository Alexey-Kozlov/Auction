import AuctionListParams from "../reports/auctionList/AuctionListParams";
import { useParams } from "react-router-dom";
import TestItem from "../reports/testItem/testItem";
import NotificationListParams from "../reports/notificationList/NotificationListParams";

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
	return <div>{report()}</div>;
}
