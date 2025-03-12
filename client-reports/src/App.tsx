import { Route, Routes } from "react-router-dom";
import Header from "./components/layout/header/Header";
import Main from "./components/layout/Main";
import List from "./components/layout/List";
import { useDispatch } from "react-redux";
import { setParamIsOpen } from "./store/ReportSlice";
import { useEffect } from "react";
import { useCookies } from "react-cookie";

function App() {
	const dispatch = useDispatch();
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
	const tokenData = localStorage.getItem("Auction");
	if (tokenData) {
		const userLogin = JSON.parse(tokenData).login;
		setCookie("User", userLogin);
	}

	const handleCloseParamWindow = () => {
		dispatch(setParamIsOpen({ isOpen: false }));
	};
	useEffect(() => {
		document.addEventListener("keydown", (e: KeyboardEvent) => {
			if (e.key === "Escape") {
				dispatch(setParamIsOpen({ isOpen: false }));
			}
		});
	}, [dispatch]);
	return (
		<>
			{tokenData && (
				<div className="container" onClick={handleCloseParamWindow}>
					<Routes>
						<Route
							path="/:root"
							element={[<Header key={1} />, <List key={2} />]}
						></Route>
						<Route path="/:root/:id" element={<Main />}></Route>
					</Routes>
				</div>
			)}
		</>
	);
}

export default App;
