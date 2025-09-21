import { ReportItem } from "../../../types";
import { RiAuctionLine } from "react-icons/ri";
import { MdOutlineNotificationImportant } from "react-icons/md";
import { FaChartPie } from "react-icons/fa";
import { LuFolderTree } from "react-icons/lu";

export default function ListData(): ReportItem[] {
  return [
    {
      Name: "Аукционы (таблица)",
      Description: "Список всех аукционов в виде таблицы",
      Id: "AuctionList",
      Icon: RiAuctionLine,
    },
    {
      Name: "Аукционы (структура)",
      Description: "Список всех аукционов в виде таблицы с раскрытием",
      Id: "AuctionListTree",
      Icon: LuFolderTree,
    },
    {
      Name: "Уведомления",
      Description: "Список уведомлений",
      Id: "NotificationList",
      Icon: MdOutlineNotificationImportant,
    },
    {
      Name: "Круговые диаграммы",
      Description: "Статистика по аукционам",
      Id: "RoundDiagrams",
      Icon: FaChartPie,
    },
  ];
}
