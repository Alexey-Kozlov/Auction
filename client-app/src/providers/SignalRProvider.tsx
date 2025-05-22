import {
	HubConnection,
	HubConnectionBuilder,
	HubConnectionState,
} from "@microsoft/signalr";
import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import {
	Auction,
	AuctionFinished,
	Bid,
	FinanceItem,
	FinanceTableItem,
	Message,
	PagedResult,
	ProgressToast,
	ToastType,
	User,
} from "../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../store/store";
import BidCreatedToast from "../components/signalRNotifications/BidCreatedToast";
import { setEventFlag } from "../store/processingSlice";
import MessageToast from "../components/signalRNotifications/MessageToast";
import { setData } from "../store/auctionSlice";
import { setParams } from "../store/paramSlice";
import FinanceCreatedToast from "../components/signalRNotifications/FinanceCreatedToast";
import ProgressMessageToast from "../components/signalRNotifications/ProgressMessageToast";
import { setFinanceItems } from "../store/financeSlice";
import AuctionToast from "../components/signalRNotifications/AuctionToast";

export default function SignalRProvider() {
	const user: User = useSelector((state: RootState) => state.authStore);
	const dispatch = useDispatch();
	const [connection, setConnection] = useState<HubConnection | null>(null);

	const apiUrl = process.env.REACT_APP_NOTIFY_URL;

	const tokenData = localStorage.getItem("Auction");
	const progressToastId = "RestoreToastId";

	useEffect(() => {
		if (tokenData) {
			const token = JSON.parse(tokenData!).token;
			const newConnection = new HubConnectionBuilder()
				.withUrl(apiUrl!, {
					accessTokenFactory: () => token,
				})
				.withAutomaticReconnect()
				.build();
			setConnection(newConnection);
		} else {
			const newConnection = new HubConnectionBuilder()
				.withUrl(apiUrl!)
				.withAutomaticReconnect()
				.build();
			setConnection(newConnection);
		}
		// eslint-disable-next-line
	}, [tokenData]);

	useEffect(() => {
		const con_execute = async () => {
			if (connection) {
				if (connection.state === HubConnectionState.Disconnected) {
					await connection.start();
					console.log("Коннект установлен с хабом уведомлений");
				}
				connection.on("BidPlaced", (bid: Bid) => {
					//устанавливаем флаг что данные для данного пользователя готовы и нужно обновить запрос
					dispatch(
						setEventFlag({
							eventName: "BidPlaced",
							ready: true,
							itemId: bid.auctionId,
						})
					);
					dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
					//для обновления плашки ставки на страничке аукциона в списке аукционов
					dispatch(
						setEventFlag({
							eventName: "CollectionChanged",
							ready: true,
							itemId: bid.auctionId,
						})
					);
					if (bid.show) {
						return toast(
							(p) => (
								<BidCreatedToast auctionId={bid.auctionId} toastId={p.id} />
							),
							{ duration: 5000 }
						);
					}
				});

				connection.on("AuctionCreated", (auction: Auction) => {
					dispatch(
						setEventFlag({
							eventName: "CollectionChanged",
							ready: true,
							itemId: auction.auctionId,
						})
					);
					dispatch(
						setEventFlag({
							eventName: "ImageChanged",
							ready: true,
							itemId: auction.auctionId,
						})
					);
					if (user?.login !== auction.seller && auction.show) {
						return toast(
							(p) => (
								<AuctionToast
									auctionId={auction.auctionId}
									toastId={p.id}
									message={`Создан новый аукцион - "${auction.title}"`}
								/>
							),
							{ duration: 5000 }
						);
					}
				});

				connection.on("AuctionUpdated", (auction: Auction) => {
					dispatch(
						setEventFlag({
							eventName: "CollectionChanged",
							ready: true,
							itemId: auction.auctionId,
						})
					);
					dispatch(
						setEventFlag({
							eventName: "ImageChanged",
							ready: true,
							itemId: auction.auctionId,
						})
					);
					if (auction.show) {
						return toast(
							(p) => (
								<AuctionToast
									auctionId={auction.auctionId}
									toastId={p.id}
									message={`Обновлен аукцион - "${auction.title}"`}
								/>
							),
							{ duration: 5000 }
						);
					}
				});

				connection.on("AuctionFinished", (finishedAuction: AuctionFinished) => {
					dispatch(
						setEventFlag({
							eventName: "CollectionChanged",
							ready: true,
							itemId: finishedAuction.auctionId,
						})
					);
					return toast(
						(p) => (
							<AuctionToast
								auctionId={finishedAuction.auctionId}
								toastId={p.id}
								message={`Поздравления для победителя аукциона "${finishedAuction.winner}",
                                итоговая стоимость лота - ${finishedAuction.amount} руб.`}
							/>
						),
						{ duration: 10000 }
					);
				});

				connection.on("AuctionDeleted", (auction: any) => {
					dispatch(
						setEventFlag({
							eventName: "CollectionChanged",
							ready: true,
							itemId: auction.auctionId,
						})
					);
					if (auction.show) {
						return toast(
							(p) => (
								<AuctionToast
									auctionId={auction.auctionId}
									toastId={p.id}
									message={`Аукцион - "${auction.title}" удален`}
								/>
							),
							{ duration: 5000 }
						);
					}
				});

				connection.on("FinanceCreate", (finance: FinanceItem) => {
					dispatch(setEventFlag({ eventName: "FinanceCreate", ready: true }));
					dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
					if (finance.show) {
						return toast(
							(p) => <FinanceCreatedToast finance={finance} toastId={p.id} />,
							{ duration: 5000 }
						);
					}
				});

				connection.on("SessionId", (id: any) => {
					dispatch(setParams({ sessionId: id }));
				});

				connection.on("ElkSearch", (elk: any) => {
					const elkData = elk as PagedResult<Auction>;
					dispatch(setData(elkData));
					dispatch(setEventFlag({ eventName: "ElkSearch", ready: false }));
				});

				connection.on("ElkIndex", (result: any) => {
					dispatch(setEventFlag({ eventName: "ElkIndex", ready: false }));
					const mes: Message = {
						message: result.message,
						auctionId: "",
						messageType: 0,
					};
					if (result.show) {
						return toast(
							(p) => (
								<MessageToast
									message={mes}
									toastId={p.id}
									toastType={ToastType.Info}
								/>
							),
							{ duration: 5000 }
						);
					}
				});

				connection.on("SetSnapShot", (result: string) => {
					dispatch(setEventFlag({ eventName: "SetSnapShot", ready: true }));
					const mes: Message = {
						message: result,
						auctionId: "",
						messageType: 0,
					};
					return toast(
						(p) => (
							<MessageToast
								message={mes}
								toastId={p.id}
								toastType={ToastType.Info}
							/>
						),
						{ duration: 5000 }
					);
				});

				connection.on("RestoreSnapShot", (result: string) => {
					dispatch(setEventFlag({ eventName: "RestoreSnapShot", ready: true }));
					const mes: Message = {
						message: result,
						auctionId: "",
						messageType: 0,
					};
					return toast(
						(p) => (
							<MessageToast
								message={mes}
								toastId={p.id}
								toastType={ToastType.Info}
							/>
						),
						{ duration: 5000 }
					);
				});

				connection.on("ErrorMessage", (message: Message) => {
					dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
					switch (message.messageType) {
						case 0:
							//убираем иконку ожидания в случае ошибки
							dispatch(setEventFlag({ eventName: "ElkSearch", ready: false }));
							return toast(
								(p) => (
									<MessageToast
										message={message}
										toastId={p.id}
										toastType={ToastType.Error}
									/>
								),
								{ duration: 5000 }
							);
						case 1:
							return toast(
								(p) => (
									<MessageToast
										message={message}
										toastId={p.id}
										toastType={ToastType.Warning}
									/>
								),
								{ duration: 5000 }
							);

						case 2:
							return toast(
								(p) => (
									<MessageToast
										message={message}
										toastId={p.id}
										toastType={ToastType.Info}
									/>
								),
								{ duration: 5000 }
							);
					}
				});

				connection.on("EditNotification", (result: any) => {
					dispatch(
						setEventFlag({ eventName: "EditNotification", ready: true })
					);
					const mes: Message = {
						message: result.message,
						auctionId: "",
						messageType: 0,
					};
					if (result.show) {
						return toast(
							(p) => (
								<MessageToast
									message={mes}
									toastId={p.id}
									toastType={ToastType.Info}
								/>
							),
							{ duration: 5000 }
						);
					}
				});

				connection.on("RestoreProgress", (result: ProgressToast) => {
					const mes: Message = {
						message: "Восстановление БД",
						auctionId: "",
						messageType: Math.trunc(Number(result.percent)),
					};
					if (result.show) {
						return toast(
							(p) => (
								<ProgressMessageToast message={mes} toastId={progressToastId} />
							),
							{ duration: result.duration, id: progressToastId }
						);
					}
				});

				connection.on("SetSnapShotProgress", (result: ProgressToast) => {
					const mes: Message = {
						message: "Создание SnapShot",
						auctionId: "",
						messageType: Math.trunc(Number(result.percent)),
					};
					return toast(
						(p) => (
							<ProgressMessageToast message={mes} toastId={progressToastId} />
						),
						{ duration: result.duration, id: progressToastId }
					);
				});

				connection.on("ResetImageCache", (result: string) => {
					dispatch(setEventFlag({ eventName: "ResetImageCache", ready: true }));
					const mes: Message = {
						message: result,
						auctionId: "",
						messageType: 0,
					};
					return toast(
						(p) => (
							<MessageToast
								message={mes}
								toastId={p.id}
								toastType={ToastType.Info}
							/>
						),
						{ duration: 2000 }
					);
				});

				connection.on(
					"FinanceHistory",
					(finance: PagedResult<FinanceTableItem>) => {
						dispatch(setFinanceItems(finance));
						dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
					}
				);
			}
		};
		con_execute();
		return () => {
			connection?.stop();
		};
		// eslint-disable-next-line
	}, [connection, user.login]);

	return <></>;
}
