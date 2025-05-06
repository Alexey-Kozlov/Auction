import Logo from "./Logo";
import Search from "./Search";
import UserActions from "./UserActions";
import { User } from "../../store/types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { useNavigate } from "react-router-dom";
import { emptyUserState, setAuthUser } from "../../store/authSlice";
import AddTokenHeader from "../../api/AddTokenHeader";
import { createRef, useEffect } from "react";
import { Button, Sticky } from "semantic-ui-react";

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
		<div
			style={{
				backgroundColor: "white",
				display: "flex",
				justifyContent: "space-between",
				alignItems: "center",
				padding: "10px",
				boxShadow: "0 3px 5px #e5e7eb",
			}}
		>
			<Logo />
			<Search />
			{user.id ? (
				<UserActions />
			) : (
				<div>
					<Button onClick={() => navigate("/register")}>Регистрация</Button>
					<Button onClick={() => navigate("/login")}>Логин</Button>
				</div>
			)}
		</div>
	);
}
