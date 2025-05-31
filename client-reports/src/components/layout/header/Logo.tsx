import { RiAuctionLine } from "react-icons/ri";
import { NavLink } from "react-router-dom";

export default function Logo() {
	return (
		<NavLink
			id="NavBarLogo"
			onClick={() => (window.location.href = process.env.REACT_APP_API_URL!)}
			to="/"
		>
			<RiAuctionLine size={34} />
			<div>Аукцион</div>
		</NavLink>
	);
}
