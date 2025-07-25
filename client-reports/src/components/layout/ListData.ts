import { ReportItem } from "../../types";

export default function ListData(): ReportItem[] {
	return [
		{
			Name: "Аукционы",
			Description: "Список всех аукционов",
			Id: "AuctionList",
			Icon: "111",
		},
		{
			Name: "Уведомления",
			Description: "Список уведомлений",
			Id: "NotificationList",
			Icon: "333",
		},
	];
}
