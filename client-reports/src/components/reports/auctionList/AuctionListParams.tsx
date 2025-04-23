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
						Label: "Автор аукционов:",
						Type: ParameterType.Select,
						Value: JSON.stringify([
							{
								Label: "Все",
								Value: "All",
								Default: true,
							} as ParameterSelect,
							{
								Label: "Мои аукционы",
								Value: "My",
								Default: false,
							} as ParameterSelect,
						]),
						Id: "Author",
					} as ParameterItem,
					{
						Label: "Ставки:",
						Type: ParameterType.Select,
						Value: JSON.stringify([
							{
								Label: "Не учитывать",
								Value: "All",
								Default: true,
							} as ParameterSelect,
							{
								Label: "Есть",
								Value: "Bids",
								Default: false,
							} as ParameterSelect,
							{
								Label: "Нет",
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
