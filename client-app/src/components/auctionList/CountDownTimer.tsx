import Countdown, { zeroPad } from 'react-countdown';
import { useDispatch } from 'react-redux';
import { setBids } from '../../store/bidSlice';
import { useLocation } from 'react-router-dom';

type Props = {
    auctionEnd: Date;
}
const renderer = ({ days, hours, minutes, seconds, completed }:
    { days: number, hours: number, minutes: number, seconds: number, completed: boolean }) => {

    return (
        <div className={`
            border-2 border-white text-white py-1 px-2 rounded-lg flex justify-center
            ${completed ? 'bg-red-600' : (days === 0 && hours < 10) ? 'bg-amber-600' : 'bg-green-600'}
        `}>
            {completed ? (
                <span>Аукцион завершен</span>
            ) : (
                <span suppressHydrationWarning={true}>
                    {zeroPad(days)}:{zeroPad(hours)}:{zeroPad(minutes)}:{zeroPad(seconds)}
                </span>
            )}
        </div>
    )
};

export default function CountdownTimer({ auctionEnd }: Props) {
    const dispatch = useDispatch();
    const pathName = useLocation().pathname;

    const auctionFinished = () => {
        if (pathName.startsWith('/auctions/details')) {
            dispatch(setBids(false));
        }
    }

    return (
        <div>
            <Countdown date={auctionEnd} renderer={renderer} onComplete={auctionFinished} />
        </div>
    )
}
