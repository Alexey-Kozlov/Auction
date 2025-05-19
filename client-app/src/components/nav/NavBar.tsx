import Logo from "./Logo";
import Search from "./Search";
import UserActions from "./UserActions";
import { User } from "../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { useNavigate } from "react-router-dom";
import { emptyUserState, setAuthUser } from "../../store/authSlice";
import AddTokenHeader from "../../api/AddTokenHeader";
import { createRef, useEffect } from "react";

export default function NavBar() {
	let user: User = useSelector((state: RootState) => state.authStore);
	const dispatch = useDispatch();
	const contextRef = createRef();
	//если токен просрочен - очищаем в хранилище данные о пользователе
	if (!AddTokenHeader() && user.id) {
		dispatch(setAuthUser(emptyUserState));
	}

	useEffect(() => {
		if (localStorage.getItem("Auction")) {
			dispatch(setAuthUser(JSON.parse(localStorage.getItem("Auction")!)));
		}
	}, [dispatch]);

	const navigate = useNavigate();
	return (
		<>
			<div className="NavBarHeader"></div>
			<div className="NavBar">
				<Logo />
				<Search />
				{user.id ? (
					<UserActions />
				) : (
					<div>
						<button
							className="MainButton"
							onClick={() => navigate("/register")}
						>
							Регистрация
						</button>
						<button className="MainButton" onClick={() => navigate("/login")}>
							Логин
						</button>
					</div>
				)}
			</div>
			<div className="NavBarFooter1"></div>
			<div className="NavBarFooter2"></div>
		</>
	);
}
