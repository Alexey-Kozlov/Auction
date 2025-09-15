import { ReportItem } from "../../../types";
import { RiAuctionLine } from "react-icons/ri";
import { MdOutlineNotificationImportant } from "react-icons/md";
import { FaChartPie } from "react-icons/fa";

export default function ListData(): ReportItem[] {
	return [
		{
			Name: "Аукционы",
			Description: "Список всех аукционов",
			Id: "AuctionList",
			Icon: RiAuctionLine
		},
		{
			Name: "Уведомления",
			Description: "Список уведомлений",
			Id: "NotificationList",
			Icon: MdOutlineNotificationImportant
		},
		{
			Name: "Круговые диаграммы",
			Description: "Статистика по аукционам",
			Id: "RoundDiagrams",
			Icon: FaChartPie
		},
	];
}
