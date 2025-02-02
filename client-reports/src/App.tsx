import { Route, Routes } from "react-router-dom";
import ReportFooter from "./components/layout/reportFooter";
import Header from "./components/layout/header/header";
import Main from "./components/layout/main";
import List from "./components/layout/list";

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
