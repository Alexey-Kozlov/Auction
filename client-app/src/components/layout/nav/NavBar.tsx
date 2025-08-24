import Logo from "./Logo";
import Search from "./Search";
import UserActions from "./UserActions";
import { User } from "../../../types";
import { useSelector } from "react-redux";
import { RootState } from "../../../store/store";
import { useNavigate } from "react-router-dom";
import { Button } from "primereact/button";

export default function NavBar() {
  const navigate = useNavigate();
  let user: User = useSelector((state: RootState) => state.authStore);

  return (
    <div className="NavBarContainer">
      <div className="NavBarHeader"></div>
      <div className="NavBar">
        <Logo />
        <Search />
        {!user.isGuest ? (
          <UserActions />
        ) : (
          <div>
            <Button
              text
              raised
              rounded
              severity="contrast"
              className="CustomButton mr-2"
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
              className="CustomButton"
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
