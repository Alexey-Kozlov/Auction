import ReactDOM from "react-dom/client";
import "./index.css";
import "./components/sideSlidePanel/right-nav-style.css";
import App from "./App";
import { BrowserRouter } from "react-router-dom";
import { Provider } from "react-redux";
import store from "./store/Store";
import { CookiesProvider } from "react-cookie";

const root = ReactDOM.createRoot(
	document.getElementById("root") as HTMLElement
);

root.render(
	<Provider store={store}>
		<CookiesProvider defaultSetOptions={{ path: "/" }}>
			<BrowserRouter>
				<App />
			</BrowserRouter>
		</CookiesProvider>
	</Provider>
);
