import AuctionListParams from "../reports/auctionList/AuctionListParams";
import { useParams } from "react-router-dom";
import NotificationListParams from "../reports/notificationList/NotificationListParams";
import Header from "./header/Header";

export default function Main() {
  const { id } = useParams();
  const report = () => {
    switch (id) {
      case "AuctionList":
        return <AuctionListParams />;
      case "NotificationList":
        return <NotificationListParams />;
      default:
        return null;
    }
  };
  return (
    <div className="Main">
      <Header />
      <div>{report()}</div>
    </div>
  );
}
