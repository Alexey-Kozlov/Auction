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
import { Auction, ProcessingState, UrlCacheList } from "../../types";
import Waiter from "../Waiter";
import { Paginator, PaginatorPageChangeEvent } from "primereact/paginator";
import {
  CheckEventLastChangedNotReady,
  CheckEventLastChangedReady,
} from "../../utils/CheckEvent";
import { useSetUsersCurrentPageMutation } from "../../api/ServiceApi";
import { setCacheQuery } from "../../store/cacheSlice";

export default function Listings() {
  const dispatch = useDispatch();
  const [setCurrentPage] = useSetUsersCurrentPageMutation();
  const params = useSelector((state: RootState) => state.paramStore);
  const data = useSelector((state: RootState) => state.auctionStore);
  const auctions: Auction[] = data.auctions;
  const url = qs.stringifyUrl({ url: "", query: params });
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore
  );
  const [isWait, setIsWait] = useState(true);
  //автоматически запускается при изменении url

  let auctionsData = useGetAuctionsQuery(url, {
    skip: params.userLogin === undefined || params.userLogin === "",
  });

  // Обновляем набор записей при поступлении новых данных из апи - пишем в локальное хранилище
  // auctionStore -> auctionSlice
  useEffect(() => {
    if (
      !CheckEventLastChangedReady(procState, "ElkSearch") &&
      !auctionsData.isLoading &&
      !auctionsData.isFetching &&
      auctionsData.data
    ) {
      //посылаем вызов в апи процессинга - для записи в кеш редиса страницы, где находится пользователь
      setCurrentPage("/root/");
      //данные готовы - заполняем локальное хранилище и скрываем иконку ожидания
      dispatch(setData(auctionsData.data.result));
      setIsWait(() => false);
      dispatch(setCacheQuery({ urlAuction: url } as UrlCacheList));
    } else {
      setIsWait(() => true);
    }
    // eslint-disable-next-line
  }, [auctionsData]);

  //запрос на обновление данных при поступлении сообщения об изменении коллекции - нужно
  //принудительно обновить все записи. Это возникает при поиске через Эластик или при завершении аукциона
  useEffect(() => {
    if (
      !auctionsData.isUninitialized &&
      CheckEventLastChangedNotReady(procState, "AuctionFinished")
    ) {
      auctionsData.refetch();
      setIsWait(() => true);
    }
    //поиск из Эластика закончен, убираем иконку ожидания
    if (CheckEventLastChangedNotReady(procState, "ElkSearch")) {
      setIsWait(() => false);
    }
    // здесь отслеживаем начало поиска для Эластика - отображаем иконку ожидания, ставим признак
    // сброса кеша для отправки запроса на поиск
    if (CheckEventLastChangedReady(procState, "ElkSearch")) {
      auctionsData.refetch();
      setIsWait(() => true);
    }
    // eslint-disable-next-line
  }, [procState]);

  function setPageNumber(e: PaginatorPageChangeEvent) {
    dispatch(
      setParams({
        pageNumber: e.page + 1,
        pageSize: e.rows,
        firstPage: e.first === 0 ? 1 : e.first,
      })
    );
  }

  return (
    <div>
      <div className="ListingFilter">
        <Filters />
      </div>

      <div className="ListingContainer">
        {isWait ? (
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
                first={params.firstPage}
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
