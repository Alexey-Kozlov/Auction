import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../store/store";
import { setEventFlag } from "../store/processingSlice";
import { NotificationEvent, SignalREvents, ToastType, User } from "../types";
import MessageToast from "../components/signalRNotifications/MessageToast";
import { Toast } from "primereact/toast";

export default function SignalRProvider() {
  const user: User = useSelector((state: RootState) => state.authStore);
  const toastMessage: Toast | null = useSelector(
    (state: RootState) => state.serviceStore,
  ).toast;
  const dispatch = useDispatch();
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const apiUrl = process.env.REACT_APP_NOTIFY_URL;
  const tokenData = localStorage.getItem("Auction");

  useEffect(() => {
    if (user.login) {
      //при смене пользователя - закрываем старый коннект и открываем новый
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
      }
    }
    // eslint-disable-next-line
  }, [user.login]);

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
          SignalREvents[SignalREvents.SetCurrentSettings],
          (result: NotificationEvent) => {
            dispatch(
              setEventFlag({
                eventName: SignalREvents[SignalREvents.SetCurrentSettings],
                ready: false,
              }),
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
          },
        );
      }
    };
    con_execute();
    // eslint-disable-next-line
  }, [connection]);

  return <></>;
}
