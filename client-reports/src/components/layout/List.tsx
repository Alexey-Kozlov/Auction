import React from "react";
import { ReportItem } from "../../Types";
import ReportCard from "../ReportCard";

export default function List() {
	const reportsList: ReportItem[] = [
		{
			Name: "Мои аукционы",
			Description: "Список моих аукционов",
			Id: "AuctionList",
			Icon: "111",
		},
		{
			Name: "Тест",
			Description: "Тест дополнение",
			Id: "TestItem",
			Icon: "222",
		},
	];
	return (
		<div className="flex flex-wrap">
			{reportsList.map((p) => {
				return <ReportCard {...p} key={p.Id} />;
			})}
		</div>
	);
}
