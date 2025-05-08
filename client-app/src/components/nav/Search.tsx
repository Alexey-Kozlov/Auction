import React, { useEffect, useState } from "react";
import { FaSearch } from "react-icons/fa";
import { useDispatch, useSelector } from "react-redux";
import { setParams } from "../../store/paramSlice";
import { useLocation, useNavigate } from "react-router-dom";
import { setEventFlag } from "../../store/processingSlice";
import { RootState } from "../../store/store";

export default function Search() {
	const [search, setSearch] = useState("");
	const [searchAdv, setSearchAdv] = useState("");
	const dispatch = useDispatch();
	const navigate = useNavigate();
	const location = useLocation();

	const params = useSelector((state: RootState) => state.paramStore);

	const onSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
		setSearch(event.target.value);
		setSearchAdv("");
	};

	const Search = () => {
		if (location.pathname !== "/") navigate("/");
		dispatch(setParams({ searchTerm: search, searchAdv: "" }));
	};

	const onAdvSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
		setSearchAdv(event.target.value);
		setSearch("");
	};

	const AdvSearch = () => {
		if (location.pathname !== "/") navigate("/");
		if (params.searchAdv === searchAdv) {
			return;
		}
		dispatch(setParams({ searchAdv: searchAdv, searchTerm: "" }));
		dispatch(setEventFlag({ eventName: "ElkSearch", ready: true }));
	};
	//для сброса значений поиска при щелчке на сброс фильтров
	useEffect(() => {
		setSearch(params.searchTerm);
		setSearchAdv(params.searchAdv);
	}, [params.searchTerm, params.searchAdv]);

	return (
		<div className="SearchContainer">
			<div className="SearchHeader">
				<input
					type="text"
					placeholder="Поиск по точному совпадению"
					className="SearchInput"
					value={search}
					onChange={(e) => onSearchChange(e)}
					onKeyDown={(e: any) => {
						if (e.key === "Enter") Search();
					}}
				/>
				<button className="SearchButton" onClick={() => Search()}>
					<FaSearch size={34} className="SearchIcon" />
				</button>
			</div>
			<div className="SearchHeader">
				<input
					type="text"
					placeholder="Расширенный поиск"
					className="SearchInput"
					value={searchAdv}
					onChange={(e) => onAdvSearchChange(e)}
					onKeyDown={(e: any) => {
						if (e.key === "Enter") AdvSearch();
					}}
				/>
				<button className="SearchButton" onClick={() => AdvSearch()}>
					<FaSearch size={34} className="SearchIconAdv" />
				</button>
			</div>
		</div>
	);
}
