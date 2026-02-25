import { useEffect, useState } from 'react';
import qs from 'query-string';
import { useGetAuctionsQuery } from '../../api/AuctionApi';
import EmptyFilter from './EmptyFilter';
import AuctionCard from './AuctionCard';
import { useDispatch, useSelector } from 'react-redux';
import { setParams } from '../../store/paramSlice';
import { RootState } from '../../store/store';
import { setData } from '../../store/auctionSlice';
import Filters from './Filters';
import {
  Auction,
  ProcessingState,
  SignalREvents,
  UrlCacheList,
} from '../../types';
import Waiter from '../Waiter';
import { Paginator, PaginatorPageChangeEvent } from 'primereact/paginator';
import { CheckEventLastChangedNotReady } from '../../utils/checkEvent';
import { useSetUsersCurrentPageMutation } from '../../api/ServiceApi';
import { setCacheQuery } from '../../store/cacheSlice';
import { Toast } from 'primereact/toast';

export default function Listings() {
  const dispatch = useDispatch();
  const [setCurrentPage] = useSetUsersCurrentPageMutation();
  const params = useSelector((state: RootState) => state.paramStore);
  const data = useSelector((state: RootState) => state.auctionStore);
  const auctions: Auction[] = data.auctions;
  const url = qs.stringifyUrl({ url: '', query: params });
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );
  const [isWait, setIsWait] = useState(true);
  const [currentPageNumber, setCurrentPageNumber] = useState(0);

  //автоматически запускается при изменении url
  let auctionsData = useGetAuctionsQuery(url, {
    skip: params.userLogin === '',
  });

  useEffect(() => {
    //запоминаем в кеше URL списка аукционов, с которой переходим на просмотр или редактирование
    // аукциона. Это для функционала обновления ставок на странице списка аукционов, если изменили ставку
    // на каком-то аукционе
    if (url) {
      dispatch(setCacheQuery({ urlAuction: url } as UrlCacheList));
    }
    //для функционала сброса пагинатора на первую страницу при поиске
    setCurrentPageNumber(params.firstPage!);
    // eslint-disable-next-line
  }, [params]);

  // Обновляем набор записей при изменении url строки запроса - пишем в локальное хранилище
  // auctionStore -> auctionSlice
  useEffect(() => {
    if (
      !auctionsData.isLoading &&
      !auctionsData.isFetching &&
      !auctionsData.isError &&
      auctionsData.data
    ) {
      //посылаем вызов в апи процессинга - для записи в кеш редиса страницы, где находится пользователь
      setCurrentPage('/root/');
      //данные готовы - заполняем локальное хранилище и скрываем иконку ожидания
      dispatch(setData(auctionsData.data.result));
      setIsWait(() => false);
    } else {
      setIsWait(() => true);
    }
    // eslint-disable-next-line
  }, [auctionsData]);

  //запрос на обновление данных при поступлении сообщения об изменении коллекции - нужно
  //принудительно обновить все записи. Это возникает при завершении аукциона или при изменении ставок
  useEffect(() => {
    if (
      !auctionsData.isUninitialized &&
      //отслеживаем окончание аукциона
      (CheckEventLastChangedNotReady(
        procState,
        SignalREvents[SignalREvents.AuctionFinished],
      ) ||
        //отслеживаем размещение новых ставок
        CheckEventLastChangedNotReady(
          procState,
          SignalREvents[SignalREvents.BidPlaced],
        ))
    ) {
      auctionsData.refetch();
      setIsWait(() => true);
    }
    //поиск из Эластика закончен, убираем иконку ожидания
    if (
      CheckEventLastChangedNotReady(
        procState,
        SignalREvents[SignalREvents.ElkSearch],
      )
    ) {
      setIsWait(() => false);
    }
    // eslint-disable-next-line
  }, [procState]);

  function setPageNumber(e: PaginatorPageChangeEvent) {
    dispatch(
      setParams({
        pageNumber: e.page + 1,
        pageSize: e.rows,
        firstPage: e.first === 0 ? 1 : e.first,
      }),
    );
    setCurrentPageNumber(e.first === 0 ? 1 : e.first);
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
                return <AuctionCard auction={auction} key={auction.itemId} />;
              })}
            </div>
            <div className="ListPagination">
              <Paginator
                alwaysShow={true}
                onPageChange={setPageNumber}
                first={currentPageNumber}
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
