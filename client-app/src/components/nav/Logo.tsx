import { RiAuctionLine } from "react-icons/ri";
import { useDispatch } from "react-redux";
import { NavLink } from "react-router-dom";
import { setParams } from "../../store/paramSlice";

export default function Logo() {
	const dispatch = useDispatch();
	const handleClickHome = () => {
		dispatch(setParams({ searchTerm: "", searchAdv: "" }));
	};

	return (
		<NavLink
			onClick={handleClickHome}
			style={{
				display: "flex",
				alignItems: "center",
				gap: "5px",
				fontWeight: "600",
				fontSize: "30px",
				lineHeight: "36px",
				color: "rgb(240 82 82)",
			}}
			to="/"
		>
			<RiAuctionLine size={34} />
			<div>Аукцион</div>
		</NavLink>
	);
}
