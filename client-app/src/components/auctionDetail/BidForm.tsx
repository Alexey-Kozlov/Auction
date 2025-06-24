import NumberWithSpaces from "../../utils/NumberWithSpaces";
import { usePlaceBidForAuctionMutation } from "../../api/ProcessingApi";
import uuid from "react-native-uuid";
import { FormErrors, ProcessingState, SessionType, User } from "../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { FormEvent, useEffect, useState } from "react";
import { setEventFlag } from "../../store/processingSlice";
import Waiter from "../Waiter";
import { useIsNotifyUserQuery } from "../../api/NotificationApi";
import { Message } from "primereact/message";
import { InputNumber } from "primereact/inputnumber";

type Props = {
	auctionId: string;
	highBid: number;
	bidList: any;
};

export default function BidForm({ auctionId, highBid, bidList }: Props) {
	const [placeBid] = usePlaceBidForAuctionMutation();
	const dispatch = useDispatch();
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const user: User = useSelector((state: RootState) => state.authStore);
	const isNotifyUser = useIsNotifyUserQuery(auctionId, {
		skip: user.login === "" || user.login === undefined,
	});
	//обновляем список ставок и переключатель уведомлений после добавления ставки
	useEffect(() => {
		const eventState = procState.find(
			(p) => p.eventName === "BidPlaced" && p.ready
		);
		if (eventState) {
			bidList.refetch();
			isNotifyUser.refetch();
			dispatch(setEventFlag({ eventName: "BidPlaced", ready: false }));
		}
		// eslint-disable-next-line
	}, [procState, user]);
	const [bidValue, setBidValue] = useState<number>(0);
	const [bidError, setBidError] = useState<FormErrors | null>(null);
	const bidErrorList: FormErrors[] = [
		{
			name: "SmallBid",
			message: `Размер новой ставки должен быть больше ${highBid}.`,
		},
	];

	const handleBidChanged = (bid: number | null) => {
		if ((bid !== null && bid <= 0) || !Number.isInteger(bid)) {
			setBidError(() => bidErrorList.find((p) => p.name === "SmallBid")!);
			return;
		}
		//сбрасываем ошибки валидации ставки
		setBidError(() => null);
		setBidValue(bid!);
	};

	const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		if (bidValue <= highBid) {
			setBidError(() => bidErrorList.find((p) => p.name === "SmallBid")!);
			return;
		}
		setBidError(null);

		dispatch(setEventFlag({ eventName: "BidPlaced", ready: false }));
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));

		await placeBid({
			amount: bidValue as number,
			auctionId: auctionId,
			sessionId: SessionType[SessionType.auctionGroup],
		});
	};

	return (
		<>
			{procState.find((p) => p.eventName === "WaiterHide") &&
			!procState.find((p) => p.eventName === "WaiterHide")!.ready ? (
				<Waiter />
			) : (
				<></>
			)}
			<div>
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
								tooltipOptions={{ position: "bottom" }}
								onChange={(e) => handleBidChanged(e.value)}
								value={bidValue}
							/>
						</div>
						<Message
							className="mt-2"
							severity="error"
							text={bidError?.message}
							pt={{
								root: {
									className:
										bidError !== null && bidError.name === "SmallBid"
											? ""
											: "hidden",
								},
							}}
						/>
					</div>
				</form>
			</div>
		</>
	);
}
