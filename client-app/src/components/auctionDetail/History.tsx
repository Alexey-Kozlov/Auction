import { useEffect, useState } from 'react';
import { Auction, HistoryItem } from '../../types';
import { ScrollPanel } from 'primereact/scrollpanel';
import { useGetHistoryListQuery } from '../../api/HistoryApi';
import Waiter from '../Waiter';

type Props = {
  auction: Auction;
};

export default function History({ auction }: Props) {
  const auctionHistory = useGetHistoryListQuery(auction.itemId!, {
    skip: auction.itemId === '' || auction.itemId === undefined,
    refetchOnMountOrArgChange: true,
  });
  const [historyList, setHistoryList] = useState<HistoryItem[]>([]);
  const [showWaiter, setShowWaiter] = useState(true);

  //получаем историю для текущего аукциона
  useEffect(() => {
    if (
      auctionHistory &&
      !auctionHistory.isFetching &&
      !auctionHistory.isLoading &&
      auctionHistory.data
    ) {
      setHistoryList(auctionHistory.data!.result);
      setShowWaiter(() => false);
    }
    // eslint-disable-next-line
  }, [auctionHistory]);

  //первоначальная загрузка - ждем списка сообщений
  useEffect(() => {
    setShowWaiter(() => historyList.length === 0);
    // eslint-disable-next-line
  }, []);

  return (
    <>
      {showWaiter ? (
        <div className="CenterItem">
          <Waiter />
        </div>
      ) : (
        <></>
      )}
      <>
        <div className="grid">
          <div className="col-12">
            <ScrollPanel
              className="DetailScrollPanel"
              style={{ height: '27rem' }}
            >
              {historyList &&
                historyList.map((item, index) => (
                  <div key={index} className="col-12 MessageItem">
                    <div className="col-3 border-right-2 px-2 py-2 CenterItem text-3xl">
                      {new Date(item.createAt).toLocaleDateString('RU-ru', {
                        timeZone: 'UTC',
                      }) +
                        ' ' +
                        new Date(item.createAt).toLocaleTimeString('RU-ru', {
                          timeZone: 'UTC',
                        })}
                    </div>
                    <div className="col-3 border-right-2 px-2 py-2 CenterItem text-3xl">
                      {item.correlationId}
                    </div>
                    <div className="col-3 border-right-2 px-2 py-2 CenterItem text-3xl">
                      {item.userLogin}
                    </div>
                    <div className="col-3 text-3xl">
                      <div className="px-6 py-4">{item.entityType}</div>
                    </div>
                  </div>
                ))}
            </ScrollPanel>
          </div>
        </div>
      </>
    </>
  );
}
