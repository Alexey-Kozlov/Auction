import React, { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/Store";
import { AuctionListTypes } from "./AuctionListTypes";
import { ParameterItem } from "../../../types";
import AuctionBidsTable from "./components/AuctionBidsTable";
import AuctionTable from "./components/AuctionTable";
import { useReactToPrint } from "react-to-print";
import { setEvent } from "../../../store/EventSlice";
import { useDownloadExcel } from "react-export-table-to-excel";
import Waiter from "../../Waiter";
import { setReportLoaded } from "../../../store/ReportSlice";
import { useAuctionListMutation } from "../../../api/ReportApiAuction";

type Props = {
  reportId: string;
};

export default function RenderReport({ reportId }: Props) {
  const reportStore = useSelector((state: RootState) => state.reportStore);
  const eventStore = useSelector((state: RootState) => state.eventStore);
  const dispatch = useDispatch();
  const [data, setData] = useState<AuctionListTypes[]>();
  const [bidderReportType, setBidderReportType] = useState(false);
  const [auctionListReport] = useAuctionListMutation();
  const contentRef = useRef<HTMLDivElement>(null);
  const reactToPrintFn = useReactToPrint({ contentRef });
  const { onDownload } = useDownloadExcel({
    currentTableRef: contentRef.current,
    filename: reportId,
    sheet: reportId,
  });

  useEffect(() => {
    const getReport = async (param: ParameterItem[]) => {
      var rezult = await auctionListReport(param);
      setData(rezult.data);
      //если был отмечен параметр "Ставки:Нет"  - ставим флаг по скрытию
      // 2-х дополнительных колонок - автор и размер ставки
      if (rezult.data && rezult.data.length > 0) {
        setBidderReportType(rezult.data[0]["Bidder"] !== undefined);
      }
      dispatch(setReportLoaded());
    };
    if (reportStore && reportStore.param && reportStore.param.length > 0) {
      getReport(reportStore.param);
    }
    // eslint-disable-next-line
  }, [reportStore]);

  useEffect(() => {
    if (eventStore && eventStore.exportPdfClicked) {
      reactToPrintFn();
      dispatch(setEvent({ exportPdfClicked: false }));
    }
    if (eventStore && eventStore.exportExcelClicked) {
      onDownload();
      dispatch(setEvent({ exportExcelClicked: false }));
    }
    // eslint-disable-next-line
  }, [eventStore]);

  return (
    <div ref={contentRef}>
      <h2 className="text-center text-4xl">Список аукционов</h2>
      {reportStore.reportLoading && (
        <div>
          <Waiter />
        </div>
      )}
      <div>
        {!reportStore.reportLoading && bidderReportType && (
          <AuctionBidsTable items={data} />
        )}
        {!reportStore.reportLoading && !bidderReportType && (
          <AuctionTable items={data} />
        )}
      </div>
    </div>
  );
}
