import SlidePanel from "../../SlidePanel";
import { ParameterItem, ParameterType } from "../../../types";
import RenderReport from "./RenderReport";

export default function DiagramParams() {
  return (
    <div>
      <RenderReport reportId="RoundDiagrams" />
      <SlidePanel
        params={[
          {
            Label: "Дата начала",
            Type: ParameterType.Date,
            Value: "",
            Id: "BeginDate",
          } as ParameterItem,
          {
            Label: "Дата окончания",
            Type: ParameterType.Date,
            Value: new Date(),
            Id: "EndDate",
          } as ParameterItem,
        ]}
        reportName="Статистика аукционов"
      />
    </div>
  );
}
