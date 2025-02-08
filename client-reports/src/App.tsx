import { Route, Routes } from "react-router-dom";
import ReportFooter from "./components/layout/ReportFooter";
import Header from "./components/layout/header/Header";
import Main from "./components/layout/Main";
import List from "./components/layout/List";

function App() {
	return (
		<>
			<div className="container pl-10 pt-4">
				<Routes>
					<Route
						path="/:root"
						element={[<Header key={1} />, <List key={2} />]}
					></Route>
					<Route
						path="/:root/:id"
						element={[<Header key={1} />, <Main key={2} />]}
					></Route>
				</Routes>
			</div>
			<ReportFooter />
		</>
	);
}

export default App;
