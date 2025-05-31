import { Segment } from "semantic-ui-react";
import { ReportItem } from "../../types";
import { useNavigate } from "react-router-dom";

export default function Card(props: ReportItem) {
	const nav = useNavigate();
	const clickHandler = () => {
		nav(`/reports/${props.Id}`, { replace: true });
	};
	return (
		<Segment raised onClick={clickHandler} className="ReportCardContainer">
			<h5 className="ReportCardTitle">{props.Name}</h5>
			<p>{props.Description}</p>
		</Segment>
	);
}
