import NumberWithSpaces from '../../utils/numberWithSpaces';
import { usePlaceBidForAuctionMutation } from '../../api/ProcessingApi';
import { FormErrors, ProcessingState, SignalREvents } from '../../types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { FormEvent, useState } from 'react';
import { setEventFlag } from '../../store/processingSlice';
import Waiter from '../Waiter';
import { Message } from 'primereact/message';
import { InputNumber } from 'primereact/inputnumber';
import { CheckEventReady } from '../../utils/checkEvent';
import { Button } from 'primereact/button';

type Props = {
  auctionId: string;
  highBid: number;
};

export default function BidForm({ auctionId, highBid }: Props) {
  const [placeBid] = usePlaceBidForAuctionMutation();
  const dispatch = useDispatch();
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );
  const [bidValue, setBidValue] = useState<number>(0);
  const [bidError, setBidError] = useState<FormErrors | null>(null);
  const bidErrorList: FormErrors[] = [
    {
      name: 'SmallBid',
      message: `Размер новой ставки должен быть больше ${highBid}.`,
    },
  ];

  const handleBidChanged = (bid: number | null) => {
    if ((bid !== null && bid <= 0) || !Number.isInteger(bid)) {
      setBidError(() => bidErrorList.find((p) => p.name === 'SmallBid')!);
      return;
    }
    //сбрасываем ошибки валидации ставки
    setBidError(() => null);
    setBidValue(bid!);
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (bidValue <= highBid) {
      setBidError(() => bidErrorList.find((p) => p.name === 'SmallBid')!);
      return;
    }
    setBidError(null);

    dispatch(
      setEventFlag({
        eventName: SignalREvents[SignalREvents.BidPlaced],
        ready: true,
      }),
    );

    await placeBid({
      amount: bidValue as number,
      auctionId: auctionId,
    });
  };

  const setEditCSS = () => {
    if (CheckEventReady(procState, SignalREvents[SignalREvents.BidPlaced])) {
      return 'ChatEditMode';
    } else {
      return '';
    }
  };

  return (
    <>
      {CheckEventReady(procState, SignalREvents[SignalREvents.BidPlaced]) ? (
        <Waiter />
      ) : (
        <></>
      )}
      <div className={setEditCSS()}>
        <form onSubmit={(e) => handleSubmit(e)}>
          <div className="text-center">
            <div className="flex align-items-center mt-4">
              <label className="BidInputLabel">
                {`Ваша ставка (мин. ${NumberWithSpaces(highBid + 1)} руб):`}
              </label>
              <InputNumber
                name="amount"
                step={5}
                variant="filled"
                className="BidInputControl"
                placeholder={`Ваша ставка (мин. ${highBid + 1}) руб`}
                tooltip="Стрелки вверх/вниз - шаг 5 руб."
                tooltipOptions={{ position: 'bottom' }}
                onChange={(e) => handleBidChanged(e.value)}
                value={bidValue}
              />
              <Button text raised rounded className="CustomButton ml-4 w-18rem">
                Сделать ставку
              </Button>
            </div>
            <Message
              className="mt-2"
              severity="error"
              text={bidError?.message}
              pt={{
                root: {
                  className:
                    bidError !== null && bidError.name === 'SmallBid'
                      ? ''
                      : 'hidden',
                },
              }}
            />
          </div>
        </form>
      </div>
    </>
  );
}
