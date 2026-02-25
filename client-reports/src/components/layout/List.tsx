import { useSelector } from "react-redux";
import Card from "./Card";
import { ProcessingState, ReportItem, User } from "../../types";
import { RootState } from "../../store/store";
import { useEffect, useState } from "react";
import { RiAuctionLine } from "react-icons/ri";
import { MdOutlineNotificationImportant } from "react-icons/md";
import { FaChartPie } from "react-icons/fa";
import { LuFolderTree } from "react-icons/lu";
import { AiOutlineComment } from "react-icons/ai";

export default function List() {
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );
  const user: User = useSelector((state: RootState) => state.authStore);
  const settings = useSelector((state: RootState) => state.settingsStore);
  const [reportList, setReportList] = useState<ReportItem[]>([]);

  useEffect(() => {
    if (settings.adminMode && user.login !== "admin") {
      setReportList(() => []);
    } else {
      setReportList([
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
          Name: "Комментарии",
          Description: "Комментарии к аукционам",
          Id: "CommentsList",
          Icon: AiOutlineComment,
        },
        {
          Name: "Круговые диаграммы",
          Description: "Статистика по аукционам",
          Id: "RoundDiagrams",
          Icon: FaChartPie,
        },
      ]);
    }
    // eslint-disable-next-line
  }, [procState, settings, user]);
  return (
    <div id="ReportList">
      {reportList.map((p) => {
        return <Card {...p} key={p.Id} />;
      })}
    </div>
  );
}
