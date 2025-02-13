import { RiAuctionLine } from "react-icons/ri";
import Menu from "./Menu";
import BreadCrumb from "./BreadCrumb";

export default function Header() {
	return (
		<header
			className="sticky top-0 z-50 grid grid-cols-3 items-center bg-white p-5 
         text-gray-800 shadow-md h-24"
		>
			<a
				className="flex items-center gap-2 text-3xl font-semibold text-red-500"
				href="/"
			>
				<RiAuctionLine size={34} />
				<div>Аукцион</div>
			</a>

			<div className="text-center">
				<BreadCrumb />
			</div>
			<div className="ml-auto">
				<Menu />
			</div>
		</header>
	);
}
