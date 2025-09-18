import { Route, Routes } from "react-router-dom";
import Header from "./components/layout/header/Header";
import Main from "./components/layout/DataComponents/Main";
import List from "./components/layout/List";
import { useDispatch, useSelector } from "react-redux";
import { setParamIsOpen } from "./store/ReportSlice";
import { useEffect } from "react";
import {
  ApiResponse,
  LoginResponse,
  LogoutUser,
  RefreshLinkType,
} from "./types";
import {
  clearRefreshLink,
  setAuthUser,
  setRefreshLink,
} from "./store/authSlice";
import { RootState } from "./store/Store";
import { jwtDecode } from "jwt-decode";
import { useLoginUserMutation, useRefreshTokenMutation } from "./api/AuthApi";
import AddTokenHeader from "./api/AddTokenHeader";
import uuid from "react-native-uuid";
import { setParams } from "./store/paramSlice";

function App() {
  const dispatch = useDispatch();
  const refreshLink = useSelector((state: RootState) => state.refreshLink);
  const auth = useSelector((state: RootState) => state.authStore);
  const [refreshTokenApi] = useRefreshTokenMutation();
  const [loginUser] = useLoginUserMutation();

  const handleCloseParamWindow = () => {
    dispatch(setParamIsOpen({ isOpen: false }));
  };

  useEffect(() => {
    document.addEventListener("keydown", (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        dispatch(setParamIsOpen({ isOpen: false }));
      }
    });

    stopRefreshTokenTimer();
    //если уже входил в систему
    if (AddTokenHeader()) {
      const tokenData = localStorage.getItem("Auction");
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
          localStorage.setItem("Auction", JSON.stringify(rez.data.result));
          dispatch(
            setAuthUser({
              name: rez.data.result.name,
              login: rez.data.result.login,
              isGuest: rez.data.result.isGuest,
            })
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
    const tokenDate = localStorage.getItem("Auction");
    if (tokenDate) {
      const token = jwtDecode(tokenDate);
      const expires = new Date(token.exp! * 1000);
      const timeOut = expires.getTime() - Date.now() - 30 * 1000; //начать обновлять токен за 30 секунд до его истечения
      dispatch(
        setRefreshLink({
          value: setTimeout(refreshToken, timeOut),
          setClear: false,
        } as RefreshLinkType)
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
        const newToken: ApiResponse<LoginResponse> = await refreshTokenApi(
          user
        );
        if (newToken.data && newToken.data.isSuccess) {
          localStorage.setItem("Auction", JSON.stringify(newToken.data.result));
          dispatch(
            setAuthUser({
              name: newToken.data.result.name,
              login: newToken.data.result.login,
              isGuest: newToken.data.result.isGuest,
            })
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
        <div
          className="container"
          onClick={handleCloseParamWindow}
        >
          <Routes>
            <Route
              path="/:root"
              element={[<Header key={1} />, <List key={2} />]}
            ></Route>
            <Route
              path="/:root/:id"
              element={<Main />}
            ></Route>
          </Routes>
        </div>
      )}
    </div>
  );
}

export default App;
