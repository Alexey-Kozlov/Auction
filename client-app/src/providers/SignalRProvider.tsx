import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";
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
  Progress,
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
import { setChatResponse } from "../store/chatSlice";
import { Toast } from "primereact/toast";
import ImageToast from "../components/signalRNotifications/ImageToast";
import CamelToSnake from "../utils/CamelToSnake";
import ProgressToast from "../components/signalRNotifications/ProgressToast";

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
  const progressToast = useRef<null | Toast>(null);
  const [progressData, setProgressData] = useState<Progress>();
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
        let progressShow = false;
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

        connection.on(
          SignalREvents[SignalREvents.AuctionCreate],
          (message: NotificationEvent) => {
            const auction = JSON.parse(message.data);
            dispatch(
              setEventFlag({
                eventName: "CollectionChanged",
                ready: true,
                itemId: auction.ItemId,
              })
            );
            dispatch(
              setEventFlag({
                eventName: "ImageChanged",
                ready: true,
                itemId: auction.ItemId,
              })
            );
            if (message.show) {
              toastMessage!.show({
                severity: "success",
                life: 4000,
                className: "bg-white",
                content: (props) => (
                  <ImageToast
                    auctionId={auction.ItemId}
                    messageType={SignalREvents.AuctionCreate}
                  />
                ),
              });
            }
          }
        );

        connection.on(
          SignalREvents[SignalREvents.AuctionUpdate],
          (message: NotificationEvent) => {
            const auction = JSON.parse(message.data);
            dispatch(
              setEventFlag({
                eventName: "CollectionChanged",
                ready: true,
                itemId: auction.ItemId,
              })
            );
            dispatch(
              setEventFlag({
                eventName: "ImageChanged",
                ready: true,
                itemId: auction.ItemId,
              })
            );
            if (message.show) {
              toastMessage!.show({
                severity: "success",
                life: 4000,
                className: "bg-white",
                content: (props) => (
                  <ImageToast
                    auctionId={auction.ItemId}
                    messageType={SignalREvents.AuctionUpdate}
                  />
                ),
              });
            }
          }
        );

        connection.on(
          SignalREvents[SignalREvents.AuctionFinished],
          (message: NotificationEvent) => {
            const auction = CamelToSnake(
              JSON.parse(message.data)
            ) as AuctionFinished;
            dispatch(
              setEventFlag({
                eventName: "CollectionChanged",
                ready: true,
                itemId: auction.itemId,
              })
            );
            const mess = auction.winner
              ? `Поздравления для победителя аукциона "${auction.winner}!" Итоговая стоимость лота - ${auction.soldAmount} руб.`
              : `Аукцион закончен, лот не был продан.`;
            toastMessage!.show({
              severity: "success",
              life: 6000,
              className: "bg-white",
              content: (props) => (
                <ImageToast
                  auctionId={auction.itemId}
                  messageType={SignalREvents.AuctionFinished}
                  messageText={mess}
                />
              ),
            });
          }
        );

        connection.on(
          SignalREvents[SignalREvents.AuctionDelete],
          (message: NotificationEvent) => {
            const auction = JSON.parse(message.data);
            dispatch(
              setEventFlag({
                eventName: "AuctionDeleted",
                ready: true,
                itemId: auction.ItemId,
              })
            );
            if (message.show) {
              toastMessage!.show({
                severity: "success",
                life: 4000,
                className: "bg-white",
                content: (props) => (
                  <MessageToast
                    message={`Аукцион - "${auction.Title}" удален`}
                    toastType={ToastType.Info}
                  />
                ),
              });
            }
          }
        );

        connection.on(
          SignalREvents[SignalREvents.FinanceCreate],
          (message: NotificationEvent) => {
            const data = JSON.parse(message.data);
            dispatch(
              setEventFlag({
                eventName: SignalREvents[SignalREvents.FinanceCreate],
                ready: true,
              })
            );
            dispatch(setEventFlag({ eventName: "Waiter", ready: true }));
            if (message.show) {
              toastMessage!.show({
                severity: "success",
                life: 4000,
                className: "bg-white",
                content: (props) => (
                  <MessageToast
                    toastType={ToastType.Info}
                    message={`Пополнен баланс на  "${data.value}" руб.`}
                  />
                ),
              });
            }
          }
        );

        connection.on("SessionId", (id: any) => {
          dispatch(setParams({ sessionId: id }));
        });

        connection.on(
          SignalREvents[SignalREvents.ElkSearch],
          (elk: NotificationEvent) => {
            let elkData = JSON.parse(elk.data).Result as any;
            elkData = CamelToSnake(elkData);
            dispatch(setData(elkData));
            dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
          }
        );

        connection.on(
          SignalREvents[SignalREvents.ElkIndexReset],
          (result: NotificationEvent) => {
            dispatch(
              setEventFlag({
                eventName: SignalREvents[SignalREvents.ElkIndexReset],
                ready: true,
              })
            );
            if (result.show) {
              toastMessage!.show({
                severity: "info",
                life: 2000,
                className: "bg-white",
                content: (props) => (
                  <MessageToast
                    toastType={ToastType.Info}
                    message={result.data}
                  />
                ),
              });
            }
          }
        );

        connection.on(
          SignalREvents[SignalREvents.OperationProgress],
          (result: NotificationEvent) => {
            const data = JSON.parse(result.data);
            setProgressData((prev) => {
              return {
                ...prev,
                title: data.title && !prev?.title ? data.title : prev!.title,
                message: data.message,
                percent: parseInt(data.percent),
              };
            });
            if (!progressShow) {
              progressShow = true;
              progressToast.current!.show({});
            }
          }
        );

        connection.on(
          SignalREvents[SignalREvents.SetSnapShot],
          (result: NotificationEvent) => {
            setProgressData((prev) => {
              return {
                ...prev,
                message: result.data,
                percent: 100,
              };
            });
            progressShow = false;
            dispatch(
              setEventFlag({
                eventName: SignalREvents[SignalREvents.SetSnapShot],
                ready: true,
              })
            );
          }
        );

        connection.on(
          SignalREvents[SignalREvents.RestoreSnapShot],
          (result: NotificationEvent) => {
            dispatch(
              setEventFlag({
                eventName: SignalREvents[SignalREvents.RestoreSnapShot],
                ready: true,
              })
            );
            setProgressData((prev) => {
              return {
                ...prev,
                message: result.data,
                percent: 100,
              };
            });
            progressShow = false;
          }
        );

        connection.on(
          SignalREvents[SignalREvents.ErrorMessage],
          (message: Message) => {
            dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
            //убираем иконку ожидания
            dispatch(
              setEventFlag({ eventName: "WaiterHideNotify", ready: true })
            );
            dispatch(setEventFlag({ eventName: "WaiterHide", ready: true }));
            dispatch(
              setEventFlag({ eventName: "WaiterHideChat", ready: true })
            );
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
          }
        );

        connection.on(
          SignalREvents[SignalREvents.EditNotification],
          (message: NotificationEvent) => {
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
                  <MessageToast
                    toastType={ToastType.Info}
                    message={text}
                  />
                ),
              });
            }
          }
        );

        connection.on(
          SignalREvents[SignalREvents.ResetImageCache],
          (result: string) => {
            dispatch(
              setEventFlag({
                eventName: SignalREvents[SignalREvents.ResetImageCache],
                ready: true,
              })
            );
            toastMessage!.show({
              severity: "info",
              life: 2000,
              className: "bg-white",
              content: (props) => (
                <MessageToast
                  toastType={ToastType.Info}
                  message={result}
                />
              ),
            });
          }
        );

        connection.on(
          SignalREvents[SignalREvents.CommunicationCreate],
          (message: NotificationEvent) => {
            const data = JSON.parse(message.data);
            dispatch(
              setEventFlag({ eventName: "WaiterHideChat", ready: true })
            );
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
          }
        );

        connection.on(
          SignalREvents[SignalREvents.CommunicationUpdate],
          (message: NotificationEvent) => {
            const data = JSON.parse(message.data);
            dispatch(
              setEventFlag({ eventName: "WaiterHideChat", ready: true })
            );
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
          }
        );

        connection.on(
          SignalREvents[SignalREvents.CommunicationDelete],
          (message: NotificationEvent) => {
            const data = JSON.parse(message.data);
            dispatch(
              setEventFlag({ eventName: "WaiterHideChat", ready: true })
            );
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
          }
        );
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

  return (
    <>
      <ProgressToast
        toast={progressToast}
        data={progressData!}
      />
    </>
  );
}
