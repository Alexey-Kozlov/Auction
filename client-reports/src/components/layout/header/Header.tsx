import { RiAuctionLine } from "react-icons/ri";
import Menu from "./Menu";
import BreadCrumb from "./BreadCrumb";
import Logo from "./Logo";

export default function Header() {
	return (
		<div className="NavBarContainer">
			<div className="NavBarHeader"></div>
			<div className="NavBar">
				<Logo />

				<div className="text-center">
					<BreadCrumb />
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
