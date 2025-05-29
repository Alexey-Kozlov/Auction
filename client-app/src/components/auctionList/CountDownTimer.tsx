import Countdown, { zeroPad } from "react-countdown";
import { useDispatch } from "react-redux";
import { setOpen } from "../../store/bidSlice";
import { useLocation } from "react-router-dom";

type Props = {
	auctionEnd: Date;
	isFinished: boolean;
};

export default function CountdownTimer({ auctionEnd, isFinished }: Props) {
	const dispatch = useDispatch();
	const pathName = useLocation().pathname;

	//скрываем создание ставок на странице аукциона в момент завершения аукциона
	const auctionFinished = () => {
		if (pathName.startsWith("/auctions")) {
			dispatch(setOpen(false));
		}
	};

	return (
		<div>
			{isFinished ? (
				<div className="AuctionCardCountDownFinished">
					<span>Аукцион завершен</span>
				</div>
			) : (
				<Countdown
					autoStart={true}
					date={auctionEnd}
					renderer={renderer}
					onComplete={auctionFinished}
				/>
			)}
		</div>
	);
}

const renderer = ({
	days,
	hours,
	minutes,
	seconds,
	completed,
}: {
	days: number;
	hours: number;
	minutes: number;
	seconds: number;
	completed: boolean;
}) => {
	return (
		<div
			className={
				days < 1
					? "AuctionCardCountDownCurrentDop"
					: "AuctionCardCountDownCurrent"
			}
		>
			<span suppressHydrationWarning={true}>
				{zeroPad(days)}:{zeroPad(hours)}:{zeroPad(minutes)}:{zeroPad(seconds)}
			</span>
		</div>
	);
};
