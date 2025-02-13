import AuctionList from "../reports/auctionList/AuctionList";
import { useParams } from "react-router-dom";
import TestItem from "../reports/testItem/testItem";

export default function Main() {
	const { id } = useParams();
	const report = () => {
		switch (id) {
			case "AuctionList":
				return <AuctionList />;
			case "TestItem":
				return <TestItem />;
			default:
				return null;
		}
	};
	return <div>{report()}</div>;
}
