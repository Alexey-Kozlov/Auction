import { Navigate, Route, Routes } from "react-router-dom";
import Listings from "./components/auctionList/Listings";
import NavBar from "./components/layout/nav/NavBar";
import Register from "./components/auth/Register";
import Login from "./components/auth/Login";
import AuctionForm from "./components/auctionEdit/AuctionForm";
import SignalRProvider from "./providers/SignalRProvider";
import NotFound from "./components/layout/nav/NotFound";
import FinListings from "./components/finance/FinListings";
import HandleServiceEvents from "./components/services/HandleServiceEvents";
import { Toast } from "primereact/toast";
import { useEffect, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setServiceData } from "./store/serviceSlice";
import DetailMain from "./components/auctionDetail/DetailMain";
import { ApiResponse, LoginResponse } from "./types";
import AddTokenHeader from "./api/AddTokenHeader";
import { setAuthUser } from "./store/authSlice";
import uuid from "react-native-uuid";
import { useLoginUserMutation } from "./api/AuthApi";
import { RootState } from "./store/store";

function App() {
  const toastMessage = useRef<Toast>(null);
  const dispatch = useDispatch();
  const [loginUser] = useLoginUserMutation();
  const auth = useSelector((state: RootState) => state.authStore);

  //инициализируем выпадающее сообщение для вызова в любом месте приложения
  useEffect(() => {
    if (toastMessage) {
      dispatch(setServiceData({ toast: toastMessage.current }));
    }
    // eslint-disable-next-line
  }, [toastMessage]);

  //инициализация пользователя
  useEffect(() => {
    //если уже входил в систему
    if (localStorage.getItem("Auction")) {
      dispatch(
        setAuthUser(
          JSON.parse(localStorage.getItem("Auction")!) as LoginResponse
        )
      );
    }
    //если еще не входил в систему - регистрируем пользователя в системе как гостя
    if (!AddTokenHeader()) {
      const _login = uuid.v4() as string;
      loginUser({
        login: _login,
        password: _login,
        isGuest: true,
      }).then((rez2: ApiResponse<LoginResponse>) => {
        if (rez2.data && rez2.data.isSuccess) {
          localStorage.setItem("Auction", JSON.stringify(rez2.data.result));
          dispatch(
            setAuthUser({
              name: rez2.data.result.name,
              login: rez2.data.result.login,
              isGuest: rez2.data.result.isGuest,
            })
          );
        }
      });
    }
    // eslint-disable-next-line
  }, [auth]);

  return (
    <div>
      <NavBar />
      <div className="MainContainer">
        <Routes>
          <Route
            path="/"
            element={<Listings />}
          ></Route>
          <Route
            path="/register"
            element={<Register />}
          ></Route>
          <Route
            path="/login"
            element={<Login />}
          ></Route>
          <Route
            path="/auctions/create"
            element={<AuctionForm />}
          ></Route>
          <Route
            path="/auctions/edit/:id"
            element={<AuctionForm />}
          ></Route>
          <Route
            path="/auctions/:id"
            element={<DetailMain />}
          ></Route>
          <Route
            path="/finance/list"
            element={<FinListings />}
          ></Route>
          <Route
            path="/not-found"
            element={<NotFound />}
          ></Route>
          <Route
            path="*"
            element={
              <Navigate
                to="/not-found"
                replace={true}
              />
            }
          ></Route>
        </Routes>
        <SignalRProvider />
        <HandleServiceEvents />
        <Toast
          ref={toastMessage}
          position="bottom-right"
        />
      </div>
    </div>
  );
}

export default App;
