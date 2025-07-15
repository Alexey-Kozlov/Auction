import { useEffect, useState } from "react";
import qs from "query-string";
import { useGetAuctionsQuery } from "../../api/AuctionApi";
import EmptyFilter from "./EmptyFilter";
import AuctionCard from "./AuctionCard";
import { useDispatch, useSelector } from "react-redux";
import { setParams } from "../../store/paramSlice";
import { RootState } from "../../store/store";
import { setData } from "../../store/auctionSlice";
import Filters from "./Filters";
import { Auction, ProcessingState } from "../../types";
import { setEventFlag } from "../../store/processingSlice";
import Waiter from "../Waiter";
import { Paginator, PaginatorPageChangeEvent } from "primereact/paginator";

export default function Listings() {
  const dispatch = useDispatch();
  // eslint-disable-next-line
  const params = useSelector((state: RootState) => state.paramStore);
  const data = useSelector((state: RootState) => state.auctionStore);
  const auctions: Auction[] = data.auctions;
  const url = qs.stringifyUrl({ url: "", query: params });
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore
  );
  const [firstRecord, setFirstRecord] = useState(0);

  //автоматически запускается при изменении url
  let auctionsData = useGetAuctionsQuery(url, {
    skip: !params.sessionId,
    refetchOnMountOrArgChange: true,
  });

  // Обновляем набор записей при поступлении новых данных из апи - пишем в локальное хранилище
  // auctionStore -> auctionSlice
  useEffect(() => {
    if (
      !auctionsData.isLoading &&
      !auctionsData.isFetching &&
      auctionsData.data
    ) {
      //данные готовы - заполняем локальное хранилище и скрываем иконку ожидания
      dispatch(setData(auctionsData.data.result));
      dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
    }
    // eslint-disable-next-line
  }, [auctionsData]);

  //запрос на обновление данных при поступлении сообщения об изменении коллекции - нужно
  //принудительно обновить все записи.
  useEffect(() => {
    const eventStateChanged = procState.find(
      (p) => p.eventName === "CollectionChanged" && p.ready && p.lastChanged
    );
    if (eventStateChanged) {
      auctionsData.refetch();
      dispatch(setEventFlag({ eventName: "CollectionChanged", ready: false }));
    }
    // eslint-disable-next-line
  }, [procState]);

  function setPageNumber(e: PaginatorPageChangeEvent) {
    setFirstRecord(e.first);
    dispatch(setParams({ pageNumber: e.page + 1, pageSize: e.rows }));
  }

  if (auctionsData.isLoading && auctionsData.isFetching)
    return <h3>Загрузка...</h3>;

  return (
    <div>
      <div className="ListingFilter">
        <Filters />
      </div>

      <div className="ListingContainer">
        {procState.find((p) => p.eventName === "WaiterHide") &&
        !procState.find((p) => p.eventName === "WaiterHide")!.ready ? (
          <Waiter />
        ) : auctions.length === 0 ? (
          <EmptyFilter showReset />
        ) : (
          <div>
            <div className="ListItem">
              {auctions.map((auction: Auction) => {
                return (
                  <AuctionCard
                    auction={auction}
                    key={auction.itemId}
                  />
                );
              })}
            </div>
            <div className="ListPagination">
              <Paginator
                onPageChange={setPageNumber}
                first={firstRecord}
                rows={params.pageSize}
                totalRecords={data.totalCount}
                rowsPerPageOptions={[4, 8, 16]}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
