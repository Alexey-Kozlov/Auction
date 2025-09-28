import SlidePanel from "../../SlidePanel";
import { ParameterItem, ParameterType } from "../../../types";
import RenderReport from "./RenderReport";

export default function AuctionListTreeParams() {
  return (
    <div>
      <RenderReport reportId="AuctionListTree" />
      <SlidePanel
        params={[
          {
            Label: "Автор аукциона (логин)",
            Type: ParameterType.Text,
            Value: "",
            Id: "Seller",
          } as ParameterItem,
        ]}
        reportName="Список аукционов (структура)"
      />
    </div>
  );
}
