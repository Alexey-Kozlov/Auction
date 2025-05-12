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
		const eventState = procState.find((p) => p.eventName === "BidPlaced");
		if (eventState && eventState.ready) {
			bidList.refetch();
			isNotifyUser.refetch();
			dispatch(setEventFlag({ eventName: "BidPlaced", ready: false }));
		}
		// eslint-disable-next-line
	}, [procState, user]);
	const [bidValue, setBidValue] = useState<number | string>("");
	const [bidError, setBidError] = useState<FormErrors | null>(null);
	const bidErrorList: FormErrors[] = [
		{
			name: "NegativeValue",
			topic: "Ошибка - отрицательная ставка!",
			detail: "Ставка должна быть больше 0",
		},
		{
			name: "SmallBid",
			topic: "Ошибка - малый размер ставки!",
			detail: `Размер новой ставки должен быть больше ${highBid}.`,
		},
		{
			name: "NullBid",
			topic: "Ошибка - не указана ставка!",
			detail: `Нужно указать размер новой ставки`,
		},
	];

	const handleSubmit = async () => {
		if (!bidValue || bidValue === "") {
			setBidError(() => bidErrorList.find((p) => p.name === "NullBid")!);
			return;
		}
		if (Number.isInteger(bidValue) && (bidValue as number) <= 0) {
			setBidError(() => bidErrorList.find((p) => p.name === "NegativeValue")!);
			return;
		} else if (Number.isInteger(bidValue) && (bidValue as number) <= highBid) {
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
									{`Ваша ставка (мин. - ${NumberWithSpaces(highBid + 1)} руб)`}
								</label>
								<FormInput
									size="huge"
									className="ml-10 mr-10 w-100"
									type="number"
									name="amount"
									placeholder={`Ваша ставка (мин. - ${highBid + 1}) руб`}
									error={bidError !== null}
									value={bidValue}
									onChange={(e, { name, value }) =>
										setBidValue(value === "" ? "" : parseInt(value))
									}
								/>
							</div>
							<Message
								error
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
