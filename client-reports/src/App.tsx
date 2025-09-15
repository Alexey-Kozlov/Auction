import { Route, Routes } from "react-router-dom";
import Header from "./components/layout/header/Header";
import Main from "./components/layout/DataComponents/Main";
import List from "./components/layout/List";
import { useDispatch } from "react-redux";
import { setParamIsOpen } from "./store/ReportSlice";
import { useEffect } from "react";
import { User } from "./types";
import { setAuthUser } from "./store/authSlice";

function App() {
  const dispatch = useDispatch();
  const tokenData = localStorage.getItem("Auction");

  const handleCloseParamWindow = () => {
    dispatch(setParamIsOpen({ isOpen: false }));
  };
  useEffect(() => {
    document.addEventListener("keydown", (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        dispatch(setParamIsOpen({ isOpen: false }));
      }
    });

    if (tokenData) {
      let user: User = { isAdmin: false, login: "", name: "", id: "" };
      user.login = JSON.parse(tokenData).login;
      user.id = JSON.parse(tokenData).id;
      user.name = JSON.parse(tokenData).name;
      dispatch(setAuthUser({ user }));
    }
    // eslint-disable-next-line
  }, []);
  return (
    <div>
      {tokenData && (
        <div
          className="container"
          onClick={handleCloseParamWindow}
        >
          <Routes>
            <Route
              path="/:root"
              element={[<Header key={1} />, <List key={2} />]}
            ></Route>
            <Route
              path="/:root/:id"
              element={<Main />}
            ></Route>
          </Routes>
        </div>
      )}
    </div>
  );
}

export default App;
