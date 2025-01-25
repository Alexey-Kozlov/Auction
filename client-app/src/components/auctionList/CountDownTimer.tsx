import Countdown, { zeroPad } from "react-countdown";
import { useDispatch } from "react-redux";
import { setOpen } from "../../store/bidSlice";
import { useLocation } from "react-router-dom";

type Props = {
	auctionEnd: Date;
};

export default function CountdownTimer({ auctionEnd }: Props) {
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
			<Countdown
				autoStart={true}
				date={auctionEnd}
				renderer={renderer}
				onComplete={auctionFinished}
			/>
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
			className={`
            border-2 border-white text-white py-1 px-2 rounded-lg flex justify-center
            ${
							completed
								? "bg-red-600"
								: days === 0 && hours < 10
								? "bg-amber-600"
								: "bg-green-600"
						}
        `}
		>
			{completed ? (
				<span>Аукцион завершен</span>
			) : (
				<span suppressHydrationWarning={true}>
					{zeroPad(days)}:{zeroPad(hours)}:{zeroPad(minutes)}:{zeroPad(seconds)}
				</span>
			)}
		</div>
	);
};
