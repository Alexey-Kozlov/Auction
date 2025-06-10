import Logo from "./Logo";
import Search from "./Search";
import UserActions from "./UserActions";
import { User } from "../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { useNavigate } from "react-router-dom";
import { emptyUserState, setAuthUser } from "../../store/authSlice";
import AddTokenHeader from "../../api/AddTokenHeader";
import { useEffect } from "react";
import { Button } from "primereact/button";

export default function NavBar() {
	let user: User = useSelector((state: RootState) => state.authStore);
	const dispatch = useDispatch();
	//если токен просрочен - очищаем в хранилище данные о пользователе
	if (!AddTokenHeader() && user.id) {
		dispatch(setAuthUser(emptyUserState));
	}

	useEffect(() => {
		if (localStorage.getItem("Auction")) {
			dispatch(setAuthUser(JSON.parse(localStorage.getItem("Auction")!)));
		}
		// eslint-disable-next-line
	}, []);

	const navigate = useNavigate();
	return (
		<div className="NavBarContainer">
			<div className="NavBarHeader"></div>
			<div className="NavBar">
				<Logo />
				<Search />
				{user.id ? (
					<UserActions />
				) : (
					<div>
						<Button
							text
							raised
							rounded
							severity="contrast"
							className="mr-2"
							onClick={() => navigate("/register")}
						>
							Регистрация
						</Button>
						<Button
							text
							raised
							rounded
							severity="contrast"
							onClick={() => navigate("/login")}
						>
							Логин
						</Button>
					</div>
				)}
			</div>
			<div className="NavBarFooter1"></div>
			<div className="NavBarFooter2"></div>
		</div>
	);
}
