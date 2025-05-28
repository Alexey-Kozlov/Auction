import NumberWithSpaces from "../../utils/NumberWithSpaces";
import { usePlaceBidForAuctionMutation } from "../../api/ProcessingApi";
import uuid from "react-native-uuid";
import { FormErrors, ProcessingState, User } from "../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { useEffect, useState } from "react";
import { setEventFlag } from "../../store/processingSlice";
import Waiter from "../Waiter";
import { useIsNotifyUserQuery } from "../../api/NotificationApi";
import { Form, FormInput, Message } from "semantic-ui-react";

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
	const [bidValue, setBidValue] = useState<number | string>(0);
	const [bidError, setBidError] = useState<FormErrors | null>(null);
	const bidErrorList: FormErrors[] = [
		{
			name: "SmallBid",
			topic: "Ошибка - малый размер ставки!",
			detail: `Размер новой ставки должен быть больше ${highBid}.`,
		},
	];

	const handleBidChanged = (bid: number | "") => {
		if (bid !== "" && bid !== null && bid <= 0) {
			setBidError(() => bidErrorList.find((p) => p.name === "SmallBid")!);
			return;
		}
		//сбрасываем ошибки валидации ставки
		setBidError(() => null);
		setBidValue(bid);
	};

	const handleSubmit = async () => {
		if (Number.isInteger(bidValue) && (bidValue as number) <= highBid) {
			setBidError(() => bidErrorList.find((p) => p.name === "SmallBid")!);
			return;
		}
		setBidError(null);

		dispatch(setEventFlag({ eventName: "BidPlaced", ready: false }));
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
		dispatch(setEventFlag({ eventName: "CollectionChanged", ready: false }));

		await placeBid({
			amount: bidValue as number,
			auctionId: auctionId,
			correlationId: uuid.v4() as string,
		});

		setBidValue("");
	};

	return (
		<>
			{procState.find((p) => p.eventName === "WaiterHide") &&
			!procState.find((p) => p.eventName === "WaiterHide")!.ready ? (
				<Waiter color="rgb(156 163 175)" />
			) : (
				<div>
					<Form onSubmit={handleSubmit} error={bidError !== null}>
						<div className="text-center">
							<div className="BidFormInput">
								<label className="DetailHeadingTitle">
									{`Ваша ставка (мин. ${NumberWithSpaces(highBid + 1)} руб)`}
								</label>
								<FormInput
									size="huge"
									className="ml-10 mr-10 w-100P"
									type="number"
									name="amount"
									placeholder={`Ваша ставка (мин. ${highBid + 1}) руб`}
									error={bidError !== null}
									value={bidValue}
									onChange={(e, { name, value }) =>
										handleBidChanged(value === "" ? "" : parseInt(value))
									}
								/>
							</div>
							<Message
								error
								hidden={bidError !== null && bidError.name !== "SmallBid"}
								header={bidError?.topic}
								content={bidError?.detail}
							/>
						</div>
					</Form>
				</div>
			)}
		</>
	);
}
