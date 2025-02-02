import React from "react";
import Menu from "./menu";
import BreadCrumb from "./breadCrumb";

export default function Header() {
	return (
		<header
			className="sticky top-0 z-50 grid grid-cols-3 items-center bg-white p-5 
         text-gray-800 shadow-md"
		>
			<BreadCrumb />
			<div className="text-center">Отчеты</div>
			<div className="ml-auto">
				<Menu />
			</div>
		</header>
	);
}
