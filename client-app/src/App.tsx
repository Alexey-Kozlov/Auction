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
  RefreshLinkType,
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

function App() {
  const toastMessage = useRef<Toast>(null);
  const dispatch = useDispatch();
  const [loginUser] = useLoginUserMutation();
  const auth = useSelector((state: RootState) => state.authStore);
  const [refreshTokenApi] = useRefreshTokenMutation();
  const refreshLink = useSelector((state: RootState) => state.refreshLink);

  //инициализируем выпадающее сообщение для вызова в любом месте приложения
  useEffect(() => {
    if (toastMessage) {
      dispatch(setServiceData({ toast: toastMessage.current }));
    }
    // eslint-disable-next-line
  }, [toastMessage]);

  //инициализация пользователя
  useEffect(() => {
    stopRefreshTokenTimer();
    //если уже входил в систему
    if (AddTokenHeader()) {
      const tokenData = localStorage.getItem('Auction');
      const token: LoginResponse = JSON.parse(tokenData!);
      dispatch(setAuthUser(token));
      dispatch(setParams({ userLogin: token.login }));
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
        }
      });
    }
    // eslint-disable-next-line
  }, []);

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
