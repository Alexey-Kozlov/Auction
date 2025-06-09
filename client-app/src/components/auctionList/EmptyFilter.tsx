import { useDispatch } from "react-redux";
import Heading from "./Heading";
import { reset } from "../../store/paramSlice";
import { Button } from "primereact/button";

type Props = {
	title?: string;
	subtitle?: string;
	showReset?: boolean;
};

export default function EmptyFilter({
	title = "Нет совпадений для указанного фильтра",
	subtitle = "Попробуйте измененить или сбросить фильтр",
	showReset,
}: Props) {
	const dispather = useDispatch();
	const clearFilters = () => {
		dispather(reset(null));
	};
	return (
		<div>
			<Heading title={title} subtitle={subtitle} center />
			<div className="AuctionCardTitleContainer">
				{showReset && (
					<Button className="MainButton w-200" onClick={clearFilters}>
						Удалить фильтры
					</Button>
				)}
			</div>
		</div>
	);
}
