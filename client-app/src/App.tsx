import { Navigate, Route, Routes } from 'react-router-dom';
import Listings from './components/auctionList/Listings';
import NavBar from './components/layout/nav/NavBar';
import Register from './components/auth/Register';
import Login from './components/auth/Login';
import AuctionForm from './components/auctionEdit/AuctionForm';
import SignalRProvider from './providers/SignalRProvider';
import NotFound from './components/layout/nav/NotFound';
import FinListings from './components/finance/FinListings';
import HandleServiceEvents from './components/services/HandleServiceEvents';
import { Toast } from 'primereact/toast';
import { useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { setServiceData } from './store/serviceSlice';
import DetailMain from './components/auctionDetail/DetailMain';
import {
  ApiResponse,
  LoginResponse,
  LogoutUser,
  ProcessingState,
  RefreshLinkType,
  SignalREvents,
  ToastType,
} from './types';
import AddTokenHeader from './api/AddTokenHeader';
import {
  clearRefreshLink,
  setAuthUser,
  setRefreshLink,
} from './store/authSlice';
import uuid from 'react-native-uuid';
import { useLoginUserMutation, useRefreshTokenMutation } from './api/AuthApi';
import { RootState } from './store/store';
import { setParams } from './store/paramSlice';
import { jwtDecode } from 'jwt-decode';
import { CustomError } from './utils/postApiProcess';
import MessageToast from './components/signalRNotifications/MessageToast';
import { CheckEventLastChangedNotReady } from './utils/checkEvent';
import AdminMode from './components/layout/nav/AdminMode';

function App() {
  const toastMessage = useRef<Toast>(null);
  const dispatch = useDispatch();
  const [loginUser] = useLoginUserMutation();
  const auth = useSelector((state: RootState) => state.authStore);
  const settings = useSelector((state: RootState) => state.settingsStore);
  const [refreshTokenApi] = useRefreshTokenMutation();
  const refreshLink = useSelector((state: RootState) => state.refreshLink);
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );

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

  //инициализация пользователя
  useEffect(() => {
    //инициализируем выпадающее сообщение для вызова в любом месте приложения
    if (toastMessage) {
      dispatch(setServiceData({ toast: toastMessage.current }));
    }
    // eslint-disable-next-line
    stopRefreshTokenTimer();
    //если уже входил в систему
    if (AddTokenHeader() && settings) {
      const tokenData = localStorage.getItem('Auction');
      const token: LoginResponse = JSON.parse(tokenData!);
      dispatch(setAuthUser(token));
      dispatch(setParams({ userLogin: token.login }));
      if (settings.adminMode && token.login !== 'admin') {
        //если админский режим - выдаем сообщение об админском режиме
        toastMessage.current!.show({
          severity: 'success',
          life: 5000,
          className: 'bg-white',
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
          localStorage.setItem('Auction', JSON.stringify(rez.data.result));
          dispatch(
            setAuthUser({
              name: rez.data.result.name,
              login: rez.data.result.login,
              isGuest: rez.data.result.isGuest,
            }),
          );
          dispatch(setParams({ userLogin: rez.data.result.login }));
        }
        //отображение уведомлений, если будут. Например, при работе когда включен админский режим
        if (rez.error && toastMessage.current) {
          toastMessage.current!.show({
            severity: 'success',
            life: 5000,
            className: 'bg-white',
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

  //запуск автоматического обновления токена доступа
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
    const tokenDate = localStorage.getItem('Auction');
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
    const tokenDate = localStorage.getItem('Auction');
    if (tokenDate) {
      const token = JSON.parse(tokenDate);
      const user: LogoutUser = { login: token.login };
      stopRefreshTokenTimer();
      try {
        const newToken: ApiResponse<LoginResponse> =
          await refreshTokenApi(user);
        if (newToken.data && newToken.data.isSuccess) {
          localStorage.setItem('Auction', JSON.stringify(newToken.data.result));
          dispatch(
            setAuthUser({
              name: newToken.data.result.name,
              login: newToken.data.result.login,
              isGuest: newToken.data.result.isGuest,
            }),
          );
          console.log(new Date() + ' Токен обновлен');
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
      <NavBar />
      <AdminMode />
      <div className="MainContainer">
        <Routes>
          <Route path="/" element={<Listings />}></Route>
          <Route path="/register" element={<Register />}></Route>
          <Route path="/login" element={<Login />}></Route>
          <Route path="/auctions/create" element={<AuctionForm />}></Route>
          <Route path="/auctions/edit/:id" element={<AuctionForm />}></Route>
          <Route path="/auctions/:id" element={<DetailMain />}></Route>
          <Route path="/finance/list" element={<FinListings />}></Route>
          <Route path="/not-found" element={<NotFound />}></Route>
          <Route
            path="*"
            element={<Navigate to="/not-found" replace={true} />}
          ></Route>
        </Routes>
        <SignalRProvider />
        <HandleServiceEvents />
        <Toast ref={toastMessage} position="bottom-right" />
      </div>
    </div>
  );
}

export default App;
