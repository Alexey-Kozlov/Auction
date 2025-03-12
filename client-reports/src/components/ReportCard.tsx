import React from "react";
import { ReportItem } from "../types";
import { useNavigate } from "react-router-dom";

export default function ReportCard(props: ReportItem) {
	const nav = useNavigate();
	const clickHandler = () => {
		nav(`/reports/${props.Id}`);
	};
	return (
		<div
			onClick={clickHandler}
			className="block w-48 h-40 px-2 my-4 mr-2 py-4 text-center
         bg-white border border-gray-200 rounded-lg shadow-sm hover:bg-gray-100
          dark:bg-gray-800 dark:border-gray-700 dark:hover:bg-gray-700
          cursor-pointer  "
		>
			<h5 className="mb-2 text-2xl font-bold tracking-tight text-gray-900 dark:text-white">
				{props.Name}
			</h5>
			<p className="font-normal text-gray-700 dark:text-gray-400">
				{props.Description}
			</p>
		</div>
	);
}
