import SlidePanel from "../../sideSlidePanel/SlidePanel";
import { ParameterItem, ParameterSelect, ParameterType } from "../../../Types";
import RenderReport from "./RenderReport";

export default function AuctionListParams() {
	return (
		<div>
			<RenderReport reportId="AuctionList" />
			<SlidePanel
				params={[
					{
						Label: "Автор аукциона",
						Type: ParameterType.Text,
						Value: "",
						Id: "Seller",
					} as ParameterItem,
					{
						Label: "Отображать ставки по лоту",
						Type: ParameterType.Bool,
						Value: "false",
						Id: "ShowBids",
					} as ParameterItem,
				]}
				reportName="Список аукционов"
			/>
		</div>
	);
}
