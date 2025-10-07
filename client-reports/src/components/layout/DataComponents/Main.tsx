import AuctionListParams from "../../reports/auctionList/AuctionListParams";
import { useParams } from "react-router-dom";
import NotificationListParams from "../../reports/notificationList/NotificationListParams";
import Header from "../header/Header";
import DiagramParams from "../../reports/diagrams/DiagramParams";
import AuctionListTreeParams from "../../reports/auctionListTree/AuctionListTreeParams";
import CommentsParams from "../../reports/comments/CommentsParams";

export default function Main() {
  const { id } = useParams();
  const report = () => {
    switch (id) {
      case "AuctionList":
        return <AuctionListParams />;
      case "AuctionListTree":
        return <AuctionListTreeParams />;
      case "NotificationList":
        return <NotificationListParams />;
      case "RoundDiagrams":
        return <DiagramParams />;
      case "CommentsList":
        return <CommentsParams />;
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
