import { useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import Heading from '../auctionList/Heading';
import {
  Auction,
  Bid,
  ProcessingState,
  SignalREvents,
  User,
} from '../../types';
import BidItem from './BidItem';
import BidForm from './BidForm';
import { useGetBidsForAuctionQuery } from '../../api/BidApi';
import { setBids, setOpen } from '../../store/bidSlice';
import NumberWithSpaces from '../../utils/numberWithSpaces';
import { Panel } from 'primereact/panel';
import { useIsNotifyUserQuery } from '../../api/NotificationApi';
import { useGetAuctionsQuery } from '../../api/AuctionApi';
import { CheckEventLastChangedNotReady } from '../../utils/checkEvent';

type Props = {
  auction: Auction;
};

export default function BidList({ auction }: Props) {
  const user: User = useSelector((state: RootState) => state.authStore);
  const dispatch = useDispatch();
  const [lastBidId, setLastBidId] = useState('');
  const bidList = useGetBidsForAuctionQuery(auction?.itemId);
  const bidStore = useSelector((state: RootState) => state.bidStore);
  const bids = bidStore.bids;
  const open = bidStore.open;
  const cacheStore = useSelector((state: RootState) => state.cacheStore);
  const openForBids = new Date(auction?.auctionEnd) > new Date();
  const isNotifyUser = useIsNotifyUserQuery(auction?.itemId, {
    skip: user?.isGuest,
  });
  const auctionList = useGetAuctionsQuery(cacheStore.urlAuction, {
    skip: cacheStore.urlAuction === '',
  });
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );

  //вычисляем самую большую ставку. Делать ставку меньше нельзя
  const bidRestriction = () => {
    let result = bids?.reduce((prev, current) => {
      return prev > current?.amount ? prev : current?.amount;
    }, 0);
    if (auction.reservePrice && auction.reservePrice > result) {
      result = auction.reservePrice;
    }
    return result;
  };

  const itemsRef = useRef<null | HTMLDivElement>(null);

  //при каждом обновлении заявок - вычисление последней заявки для прокрутки
  //списка заявок вверх (если заявок много)
  useEffect(() => {
    if (bids && bids.length > 0) {
      const maxBidId: Bid = Array.from(bids).sort((a: Bid, b: Bid) => {
        return Date.parse(b.bidTime) - Date.parse(a.bidTime);
      })[0];
      setLastBidId(maxBidId.itemId);
    }
    // eslint-disable-next-line
  }, [bids]);

  //первоначальное заполнение списка заявок
  useEffect(() => {
    if (!bidList.isLoading && !bidList.isFetching) {
      dispatch(setBids(bidList.data?.result));
    }
    // eslint-disable-next-line
  }, [bidList]);

  //закрытие аукциона
  useEffect(() => {
    dispatch(setOpen(openForBids));
    // eslint-disable-next-line
  }, [openForBids]);

  //перемотка списка завок - самые послеДние - в самом верху,
  //и потом перемотка всей странички вверх - чтобы были видны последние изменения
  useEffect(() => {
    if (lastBidId && itemsRef && itemsRef.current) {
      itemsRef.current.scrollIntoView({
        behavior: 'auto',
        block: 'start',
        inline: 'nearest',
      });
      setTimeout(() => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
      }, 1000);
    }

    // eslint-disable-next-line
  }, [lastBidId]);

  //обновляем список ставок и переключатель уведомлений после добавления ставки
  useEffect(() => {
    if (
      CheckEventLastChangedNotReady(
        procState,
        SignalREvents[SignalREvents.BidPlaced],
      )
    ) {
      //ставим признак по обновлению переключателя по уведомлениям рассылки аукциона
      if (!isNotifyUser.isUninitialized) {
        isNotifyUser.refetch();
      }
      //ставим признак по обновлению списка ставок
      if (!bidList.isUninitialized) {
        bidList.refetch();
      }
      //ставим признак по обновлению списка аукционов - чтобы обновился банер ставки для аукциона
      if (!auctionList.isUninitialized) {
        auctionList.refetch();
      }
    }
    // eslint-disable-next-line
  }, [procState]);

  return (
    <div className="BidPanel">
      <div>
        {bids?.length === 0 ? (
          <Heading
            title="Нет предложений для этого аукциона"
            subtitle="Сделайте предложение"
          />
        ) : (
          <Heading
            title={`Текущее лучшее предложение - ${NumberWithSpaces(
              bidRestriction(),
            )} руб`}
          />
        )}
      </div>
      <div>
        <div className="BidListHeight">
          {bids?.map((bid, index) => (
            <div key={index} ref={itemsRef} className="BidListItem">
              <Panel className="mt-2 PanelItem">
                <BidItem bid={bid} />
              </Panel>
            </div>
          ))}
        </div>
      </div>
      <div className="DetailNotifyText text-center">
        {!open ? (
          <div>Аукцион завершен</div>
        ) : user!.isGuest ? (
          <div>Войдите в систему чтобы делать заявки</div>
        ) : user!.login === auction?.seller ? (
          <div>Невозможно сделать заявку для собственного аукциона</div>
        ) : (
          <BidForm auctionId={auction?.itemId} highBid={bidRestriction()} />
        )}
      </div>
    </div>
  );
}
