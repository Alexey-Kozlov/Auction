import { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/Store";
import { ParameterItem } from "../../../types";
import { useReactToPrint } from "react-to-print";
import { setEvent } from "../../../store/EventSlice";
import { useDownloadExcel } from "react-export-table-to-excel";
import Waiter from "../../Waiter";
import { setReportLoaded } from "../../../store/ReportSlice";
import { useCommentsListMutation } from "../../../api/ReportApiAuction";
import { CommentTreeItem } from "./CommentsTypes";
import Comments from "./Comments";

type Props = {
  reportId: string;
};

export default function RenderReport({ reportId }: Props) {
  const reportStore = useSelector((state: RootState) => state.reportStore);
  const eventStore = useSelector((state: RootState) => state.eventStore);
  const dispatch = useDispatch();
  const [data, setData] = useState<CommentTreeItem[]>();
  const [commentsList] = useCommentsListMutation();
  const contentRef = useRef<HTMLDivElement>(null);
  const reactToPrintFn = useReactToPrint({ contentRef });
  const { onDownload } = useDownloadExcel({
    currentTableRef: contentRef.current,
    filename: reportId,
    sheet: reportId,
  });

  useEffect(() => {
    const getReport = async (param: ParameterItem[]) => {
      var rezult = await commentsList(param);
      setData(rezult.data);
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
      <h2 className="text-center text-4xl">Аукционы с комментариями</h2>
      {reportStore.reportLoading && (
        <div>
          <Waiter />
        </div>
      )}
      <div>{!reportStore.reportLoading && <Comments items={data} />}</div>
    </div>
  );
}
