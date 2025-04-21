import SlidePanel from "../../sideSlidePanel/SlidePanel";
import { ParameterItem, ParameterSelect, ParameterType } from "../../../types";
import RenderReport from "./RenderReport";

export default function AuctionListParams() {
	return (
		<div>
			<RenderReport reportId="AuctionList" />
			<SlidePanel
				params={[
					{
						Label: "Автор аукциона (логин)",
						Type: ParameterType.Text,
						Value: "",
						Id: "Seller",
					} as ParameterItem,
					{
						Label: "Отображать аукционы:",
						Type: ParameterType.Select,
						Value: JSON.stringify([
							{
								Label: "Все",
								Value: "All",
								Default: true,
							} as ParameterSelect,
							{
								Label: "Со ставками",
								Value: "Bids",
								Default: false,
							} as ParameterSelect,
							{
								Label: "Без ставок",
								Value: "NoBids",
								Default: false,
							} as ParameterSelect,
						]),
						Id: "Bids",
					} as ParameterItem,
				]}
				reportName="Список аукционов"
			/>
		</div>
	);
}
