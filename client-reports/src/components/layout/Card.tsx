import { Panel } from "primereact/panel";
import { ReportItem } from "../../types";
import { useNavigate } from "react-router-dom";

export default function Card(props: ReportItem) {
  const nav = useNavigate();
  const clickHandler = () => {
    nav(`/reports/${props.Id}`, { replace: true });
  };
  return (
    <>
      <Panel
        onClick={clickHandler}
        className="ReportCardContainer"
      >
        <div className="CenterItem">
          <props.Icon
            size={22}
            className="ReportCardIconTitle"
          />
          <h5 className="ReportCardTitle">{props.Name}</h5>
        </div>

        <p className="text-3xl text-center">{props.Description}</p>
      </Panel>
    </>
  );
}
