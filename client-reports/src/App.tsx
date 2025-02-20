import { Route, Routes } from "react-router-dom";
import Header from "./components/layout/header/Header";
import Main from "./components/layout/Main";
import List from "./components/layout/List";
import { useDispatch } from "react-redux";
import { setIsOpen } from "./store/ReportSlice";
import { useEffect } from "react";

function App() {
	const dispatch = useDispatch();
	const handleCloseParamWindow = () => {
		dispatch(setIsOpen({ isOpen: false }));
	};
	useEffect(() => {
		document.addEventListener("keydown", (e: KeyboardEvent) => {
			if (e.key === "Escape") {
				dispatch(setIsOpen({ isOpen: false }));
			}
		});
	}, [dispatch]);
	return (
		<>
			<div className="container" onClick={handleCloseParamWindow}>
				<Routes>
					<Route
						path="/:root"
						element={[<Header key={1} />, <List key={2} />]}
					></Route>
					<Route path="/:root/:id" element={<Main />}></Route>
				</Routes>
			</div>
		</>
	);
}

export default App;
