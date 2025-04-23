import ReportCard from "../ReportCard";
import ReportListData from "../ReportListData";

export default function List() {
	const reportsList = ReportListData();
	return (
		<div className="flex flex-wrap ml-2">
			{reportsList.map((p) => {
				return <ReportCard {...p} key={p.Id} />;
			})}
		</div>
	);
}
