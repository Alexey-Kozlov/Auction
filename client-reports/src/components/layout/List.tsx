import Card from "./Card";
import ListData from "./ListData";

export default function List() {
	const reportsList = ListData();
	return (
		<div id="ReportList">
			{reportsList.map((p) => {
				return <Card {...p} key={p.Id} />;
			})}
		</div>
	);
}
