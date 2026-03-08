import { Route, Routes } from "react-router-dom";
import Header from "./components/layout/header/Header";
import Main from "./components/layout/Main";
import List from "./components/layout/List";
import { useDispatch, useSelector } from "react-redux";
import { setParamIsOpen } from "./store/reportSlice";
import { useEffect, useRef } from "react";
import {
  ApiResponse,
  LoginResponse,
  LogoutUser,
  ProcessingState,
  RefreshLinkType,
  SignalREvents,
  ToastType,
} from "./types";
import {
  clearRefreshLink,
  setAuthUser,
  setRefreshLink,
} from "./store/authSlice";
import { RootState } from "./store/store";
import { jwtDecode } from "jwt-decode";
import { useLoginUserMutation, useRefreshTokenMutation } from "./api/AuthApi";
import AddTokenHeader from "./api/AddTokenHeader";
import uuid from "react-native-uuid";
import { setParams } from "./store/paramSlice";
import { CheckEventLastChangedNotReady } from "./utils/checkEvent";
import SignalRProvider from "./providers/SignalRProvider";
import { Toast } from "primereact/toast";
import { setServiceData } from "./store/serviceSlice";
import MessageToast from "./components/signalRNotifications/MessageToast";
import { CustomError } from "./utils/postApiProcess";

function App() {
  const toastMessage = useRef<Toast>(null);
  const dispatch = useDispatch();
  const refreshLink = useSelector((state: RootState) => state.refreshLink);
  const auth = useSelector((state: RootState) => state.authStore);
  const settings = useSelector((state: RootState) => state.settingsStore);
  const [refreshTokenApi] = useRefreshTokenMutation();
  const [loginUser] = useLoginUserMutation();
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );

  const handleCloseParamWindow = () => {
    dispatch(setParamIsOpen({ isOpen: false }));
  };

  //обновление приложения при поступлении сигнала об установке или выходе из админ.режима
  useEffect(() => {
    if (
      CheckEventLastChangedNotReady(
        procState,
        SignalREvents[SignalREvents.SetCurrentSettings],
      )
    ) {
      window.location.reload();
    }
  }, [procState]);

  useEffect(() => {
    document.addEventListener("keydown", (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        dispatch(setParamIsOpen({ isOpen: false }));
      }
    });
    //инициализируем выпадающее сообщение для вызова в любом месте приложения
    if (toastMessage) {
      dispatch(setServiceData({ toast: toastMessage.current }));
    }
    // eslint-disable-next-line
    stopRefreshTokenTimer();
    //если уже входил в систему
    if (AddTokenHeader() && settings) {
      const tokenData = localStorage.getItem("Auction");
      const token: LoginResponse = JSON.parse(tokenData!);
      dispatch(setAuthUser(token));
      dispatch(setParams({ userLogin: token.login }));
      if (settings.adminMode && token.login !== "admin") {
        //если админский режим - работать можно только администратору
        toastMessage.current!.show({
          severity: "success",
          life: 4000,
          className: "bg-white",
          content: (props) => (
            <MessageToast
              message="Система на обслуживании."
              toastType={ToastType.Warning}
            />
          ),
        });
      }
    } else {
      //если еще не входил в систему - регистрируем пользователя в системе как гостя
      const _login = uuid.v4() as string;
      loginUser({
        login: _login,
        password: _login,
        isGuest: true,
      }).then((rez: ApiResponse<LoginResponse>) => {
        if (rez.data && rez.data.isSuccess) {
          localStorage.setItem("Auction", JSON.stringify(rez.data.result));
          dispatch(
            setAuthUser({
              name: rez.data.result.name,
              login: rez.data.result.login,
              isGuest: rez.data.result.isGuest,
            }),
          );
        }
        //отображение уведомлений, если будут. Например, при работе когда включен админский режим
        if (rez.error && toastMessage.current) {
          toastMessage.current!.show({
            severity: "success",
            life: 4000,
            className: "bg-white",
            content: (props) => (
              <MessageToast
                message={(rez.error as CustomError).message}
                toastType={ToastType.Warning}
              />
            ),
          });
        }
      });
    }
    // eslint-disable-next-line
  }, [toastMessage, settings]);

  useEffect(() => {
    if (auth && auth.login) {
      startRefreshTokenTimer();
    }
    // eslint-disable-next-line
  }, [auth]);

  useEffect(() => {
    //останавливаем прежний таймер, когда поступил сигнал об установке
    if (refreshLink && refreshLink.value && refreshLink.setClear) {
      clearTimeout(refreshLink.value);
    }
  }, [refreshLink]);

  const startRefreshTokenTimer = () => {
    const tokenDate = localStorage.getItem("Auction");
    if (tokenDate) {
      const token = jwtDecode(tokenDate);
      const expires = new Date(token.exp! * 1000);
      const timeOut = expires.getTime() - Date.now() - 30 * 1000; //начать обновлять токен за 30 секунд до его истечения
      dispatch(
        setRefreshLink({
          value: setTimeout(refreshToken, timeOut),
          setClear: false,
        } as RefreshLinkType),
      );
    }
  };

  const refreshToken = async () => {
    const tokenDate = localStorage.getItem("Auction");
    if (tokenDate) {
      const token = JSON.parse(tokenDate);
      const user: LogoutUser = { login: token.login };
      stopRefreshTokenTimer();
      try {
        const newToken: ApiResponse<LoginResponse> =
          await refreshTokenApi(user);
        if (newToken.data && newToken.data.isSuccess) {
          localStorage.setItem("Auction", JSON.stringify(newToken.data.result));
          dispatch(
            setAuthUser({
              name: newToken.data.result.name,
              login: newToken.data.result.login,
              isGuest: newToken.data.result.isGuest,
            }),
          );
          console.log(new Date() + " Токен обновлен");
        }
        startRefreshTokenTimer();
      } catch (error) {
        console.log(error);
        return Promise.reject(error);
      }
    }
  };

  const stopRefreshTokenTimer = () => {
    dispatch(clearRefreshLink({}));
  };

  return (
    <div>
      {auth.login && (
        <div className="container" onClick={handleCloseParamWindow}>
          <Routes>
            <Route
              path="/:root"
              element={[<Header key={1} />, <List key={2} />]}
            ></Route>
            <Route path="/:root/:id" element={<Main />}></Route>
          </Routes>
          <SignalRProvider />
          <Toast ref={toastMessage} position="bottom-right" />
        </div>
      )}
    </div>
  );
}

export default App;
