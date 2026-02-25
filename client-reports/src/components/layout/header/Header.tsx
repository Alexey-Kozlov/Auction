import Menu from "./MenuActions";
import BreadCrumb from "./BreadCrumbNav";
import Logo from "./Logo";
import AdminMode from "./AdminMode";

export default function Header() {
  return (
    <div className="NavBarContainer">
      <div className="NavBarHeader"></div>
      <div className="NavBar">
        <Logo />

        <div className="text-center">
          <BreadCrumb />
        </div>
        <div>
          <AdminMode />
        </div>
        <div className="mr-10">
          <Menu />
        </div>
      </div>
      <div className="NavBarFooter1"></div>
      <div className="NavBarFooter2"></div>
    </div>
  );
}
