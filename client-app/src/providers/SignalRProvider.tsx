import {
	HubConnection,
	HubConnectionBuilder,
	HubConnectionState,
	LogLevel,
} from "@microsoft/signalr";
import { useEffect, useState } from "react";
import {
	ActionType,
	Auction,
	AuctionFinished,
	ChatComment,
	FinanceItem,
	FinanceTableItem,
	Message,
	NotificationEvent,
	PagedResult,
	ProgressToast,
	SignalREvents,
	ToastType,
	User,
} from "../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../store/store";
import BidCreatedToast from "../components/signalRNotifications/ImageToast";
import { setEventFlag } from "../store/processingSlice";
import MessageToast from "../components/signalRNotifications/MessageToast";
import { setData } from "../store/auctionSlice";
import { setParams } from "../store/paramSlice";
import FinanceCreatedToast from "../components/signalRNotifications/FinanceCreatedToast";
import ProgressMessageToast from "../components/signalRNotifications/ProgressMessageToast";
import { setFinanceItems } from "../store/financeSlice";
import { setChatResponse } from "../store/chatSlice";
import { Toast } from "primereact/toast";
import { Avatar } from "primereact/avatar";
import { Button } from "primereact/button";
import ImageToast from "../components/signalRNotifications/ImageToast";

export default function SignalRProvider() {
	const user: User = useSelector((state: RootState) => state.authStore);
	const messageChat: ChatComment = useSelector(
		(state: RootState) => state.chatMessageStore
	);
	const toastMessage: Toast | null = useSelector(
		(state: RootState) => state.serviceStore
	).toast;
	const dispatch = useDispatch();
	const [connection, setConnection] = useState<HubConnection | null>(null);

	const apiUrl = process.env.REACT_APP_NOTIFY_URL;

	const tokenData = localStorage.getItem("Auction");

	useEffect(() => {
		if (connection && connection.state === HubConnectionState.Connected) {
			connection.stop();
			setConnection(() => null);
		}
		if (tokenData) {
			const token = JSON.parse(tokenData!).token;
			const newConnection = new HubConnectionBuilder()
				.withUrl(apiUrl!, {
					accessTokenFactory: () => token,
				})
				.withAutomaticReconnect()
				.configureLogging(LogLevel.Information)
				.build();
			setConnection(newConnection);
		} else {
			const newConnection = new HubConnectionBuilder()
				.withUrl(apiUrl!)
				.withAutomaticReconnect()
				.configureLogging(LogLevel.Information)
				.build();
			setConnection(newConnection);
		}
		// eslint-disable-next-line
	}, [tokenData]);

	useEffect(() => {
		const con_execute = async () => {
			if (connection) {
				if (connection.state === HubConnectionState.Disconnected) {
					try {
						await connection.start();
						console.log("Коннект установлен с хабом уведомлений");
					} catch (e) {
						console.log("Ошибка Коннекта с хабом - " + e);
					}
				}
				connection.on(
					SignalREvents[SignalREvents.BidPlaced],
					(message: NotificationEvent) => {
						const bid = JSON.parse(message.data);
						dispatch(
							setEventFlag({
								eventName: SignalREvents[SignalREvents.BidPlaced],
								ready: true,
								itemId: bid.ItemId,
							})
						);
						dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
						toastMessage!.show({
							severity: "success",
							life: 4000,
							className: "bg-white",
							content: (props) => (
								<ImageToast
									auctionId={bid.AuctionId}
									messageType={SignalREvents.BidPlaced}
								/>
							),
						});
					}
				);

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
						// return toast(
						// 	(p) => (
						// 		<AuctionToast
						// 			auctionId={auction.auctionId}
						// 			toastId={p.id}
						// 			message={`Создан новый аукцион - "${auction.title}"`}
						// 		/>
						// 	),
						// 	{ duration: 5000 }
						// );
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
						// return toast(
						// 	(p) => (
						// 		<AuctionToast
						// 			auctionId={auction.auctionId}
						// 			toastId={p.id}
						// 			message={`Обновлен аукцион - "${auction.title}"`}
						// 		/>
						// 	),
						// 	{ duration: 5000 }
						//);
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
					const message = finishedAuction.winner
						? `Поздравления для победителя аукциона "${finishedAuction.winner}",
                                итоговая стоимость лота - ${finishedAuction.amount} руб.`
						: `Лот не был продан.`;
					// return toast(
					// 	(p) => (
					// 		<AuctionToast
					// 			auctionId={finishedAuction.auctionId}
					// 			toastId={p.id}
					// 			message={message}
					// 		/>
					// 	),
					// 	{ duration: 10000 }
					// );
				});

				connection.on("AuctionDeleted", (auction: any) => {
					dispatch(
						setEventFlag({
							eventName: "AuctionDeleted",
							ready: true,
							itemId: auction.auctionId,
						})
					);
					if (auction.show) {
						// return toast(
						// 	(p) => (
						// 		<AuctionToast
						// 			auctionId={auction.auctionId}
						// 			toastId={p.id}
						// 			message={`Аукцион - "${auction.title}" удален`}
						// 		/>
						// 	),
						// 	{ duration: 5000 }
						// );
					}
				});

				connection.on(
					SignalREvents[SignalREvents.FinanceCreate],
					(finance: FinanceItem) => {
						dispatch(
							setEventFlag({
								eventName: SignalREvents[SignalREvents.FinanceCreate],
								ready: true,
							})
						);
						dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
						if (finance.show) {
							// return toast(
							// 	(p) => <FinanceCreatedToast finance={finance} toastId={p.id} />,
							// 	{ duration: 5000 }
							// );
						}
					}
				);

				connection.on("SessionId", (id: any) => {
					dispatch(setParams({ sessionId: id }));
				});

				connection.on("ElkSearch", (elk: any) => {
					const elkData = elk as PagedResult<Auction>;
					dispatch(setData(elkData));
					dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
				});

				connection.on("ElkIndex", (result: ProgressToast) => {
					dispatch(setEventFlag({ eventName: "ElkIndex", ready: true }));
					if (result.show) {
						// return toast(
						// 	(p) => (
						// 		<MessageToast
						// 			message={result.message}
						// 			toastId={p.id}
						// 			toastType={ToastType.Info}
						// 		/>
						// 	),
						// 	{ duration: result.duration }
						// );
					}
				});

				connection.on("SetSnapShot", (result: string) => {
					dispatch(setEventFlag({ eventName: "SetSnapShot", ready: true }));
					// return toast(
					// 	(p) => (
					// 		<MessageToast
					// 			message={result}
					// 			toastId={p.id}
					// 			toastType={ToastType.Info}
					// 		/>
					// 	),
					// 	{ duration: 5000 }
					// );
				});

				connection.on("RestoreSnapShot", (result: string) => {
					dispatch(setEventFlag({ eventName: "RestoreSnapShot", ready: true }));
					// return toast(
					// 	(p) => (
					// 		<MessageToast
					// 			message={result}
					// 			toastId={p.id}
					// 			toastType={ToastType.Info}
					// 		/>
					// 	),
					// 	{ duration: 5000 }
					// );
				});

				connection.on("ErrorMessage", (message: Message) => {
					dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
					const getMessageType = (): ToastType => {
						switch (message.messageType) {
							case 0:
								return ToastType.Error;
							case 1:
								return ToastType.Warning;
							case 2:
								return ToastType.Info;
							default:
								return ToastType.Info;
						}
					};
					//убираем иконку ожидания
					dispatch(
						setEventFlag({ eventName: "WaiterHideNotify", ready: true })
					);
					dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
					dispatch(setEventFlag({ eventName: "WaiterHideChat", ready: true }));
					toastMessage!.show({
						severity: "error",
						life: 6000,
						className: "bg-white",
						content: (props) => (
							<MessageToast
								toastType={ToastType.Error}
								message={message.message}
							/>
						),
					});
				});

				connection.on("EditNotification", (message: NotificationEvent) => {
					const event = JSON.parse(message.data);
					dispatch(
						setEventFlag({ eventName: "WaiterHideNotify", ready: true })
					);
					dispatch(
						setEventFlag({ eventName: "EditNotification", ready: true })
					);

					const text = event.Enable
						? "Уведомление для пользователя " + event.UserLogin + " создано!"
						: "Уведомление для пользователя " + event.UserLogin + " удалено!";
					if (message.show) {
						toastMessage!.show({
							severity: "info",
							life: 3000,
							className: "bg-white",
							content: (props) => (
								<MessageToast toastType={ToastType.Info} message={text} />
							),
						});
					}
				});

				connection.on("RestoreProgress", (result: ProgressToast) => {
					if (result.show) {
						// return toast(
						// 	(p) => (
						// 		<ProgressMessageToast
						// 			message={result}
						// 			toastId={progressToastId}
						// 		/>
						// 	),
						// 	{ duration: result.duration, id: progressToastId }
						// );
					}
				});

				connection.on("SetSnapShotProgress", (result: ProgressToast) => {
					// return toast(
					// 	(p) => (
					// 		<ProgressMessageToast
					// 			message={result}
					// 			toastId={progressToastId}
					// 		/>
					// 	),
					// 	{ duration: result.duration, id: progressToastId }
					// );
				});

				connection.on("ResetImageCache", (result: string) => {
					dispatch(setEventFlag({ eventName: "ResetImageCache", ready: true }));
					// return toast(
					// 	(p) => (
					// 		<MessageToast
					// 			message={result}
					// 			toastId={p.id}
					// 			toastType={ToastType.Info}
					// 		/>
					// 	),
					// 	{ duration: 2000 }
					// );
				});

				connection.on(
					"FinanceHistory",
					(finance: PagedResult<FinanceTableItem>) => {
						dispatch(setFinanceItems(finance));
						dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
					}
				);

				connection.on("CommunicationCreate", (message: NotificationEvent) => {
					const data = JSON.parse(message.data);
					dispatch(setEventFlag({ eventName: "WaiterHideChat", ready: true }));
					dispatch(
						setChatResponse({
							itemId: data.ItemId,
							message: data.Message,
							parentId: data.ParentId,
							userLogin: data.UserLogin,
							auctionId: data.AuctionId,
							updateAt: data.UpdateAt,
							actionType: ActionType.create,
						})
					);
				});

				connection.on("CommunicationUpdate", (message: NotificationEvent) => {
					const data = JSON.parse(message.data);
					dispatch(setEventFlag({ eventName: "WaiterHideChat", ready: true }));
					dispatch(
						setChatResponse({
							itemId: data.ItemId,
							message: data.Message,
							parentId: data.ParentId,
							userLogin: data.UserLogin,
							auctionId: data.AuctionId,
							updateAt: data.UpdateAt,
							actionType: ActionType.update,
						})
					);
				});

				connection.on("CommunicationDelete", (message: NotificationEvent) => {
					const data = JSON.parse(message.data);
					dispatch(setEventFlag({ eventName: "WaiterHideChat", ready: true }));
					dispatch(
						setChatResponse({
							itemId: data.ItemId,
							message: data.Message,
							parentId: data.ParentId,
							userLogin: data.UserLogin,
							auctionId: data.AuctionId,
							updateAt: data.UpdateAt,
							actionType: ActionType.delete,
						})
					);
				});
			}
		};
		con_execute();
		// eslint-disable-next-line
	}, [connection, user.login]);

	useEffect(() => {
		//посылаем новое сообщение в чате на сервер
		if (messageChat && messageChat.message && connection) {
			try {
				connection.invoke("SendComment", messageChat);
			} catch (error) {
				console.log(error);
			}
		}
		// eslint-disable-next-line
	}, [messageChat]);

	return <></>;
}
