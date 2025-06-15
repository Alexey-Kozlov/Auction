import { Navigate, Route, Routes } from "react-router-dom";
import Listings from "./components/auctionList/Listings";
import NavBar from "./components/nav/NavBar";
import Register from "./components/auth/Register";
import Login from "./components/auth/Login";
import AuctionForm from "./components/auctionEdit/AuctionForm";
import SignalRProvider from "./providers/SignalRProvider";
import NotFound from "./components/nav/NotFound";
import FinListings from "./components/finance/FinListings";
import HandleServiceEvents from "./components/services/HandleServiceEvents";
import { Toast } from "primereact/toast";
import { useEffect, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setServiceData } from "./store/serviceSlice";
import DetailMain from "./components/auctionDetail/DetailMain";
import { User } from "./types";
import { RootState } from "./store/store";
import AddTokenHeader from "./api/AddTokenHeader";
import { emptyUserState, setAuthUser } from "./store/authSlice";

function App() {
	const toastMessage = useRef<Toast>(null);
	const dispatch = useDispatch();
	let user: User = useSelector((state: RootState) => state.authStore);

	//если токен просрочен - очищаем в хранилище данные о пользователе
	if (!AddTokenHeader() && user.login) {
		dispatch(setAuthUser(emptyUserState));
	}

	useEffect(() => {
		if (toastMessage) {
			dispatch(setServiceData({ toast: toastMessage.current }));
		}
		// eslint-disable-next-line
	}, [toastMessage]);

	useEffect(() => {
		if (localStorage.getItem("Auction")) {
			dispatch(setAuthUser(JSON.parse(localStorage.getItem("Auction")!)));
		}
		// eslint-disable-next-line
	}, []);

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
