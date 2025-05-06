import { useDispatch } from "react-redux";
import Heading from "./Heading";
import { reset } from "../../store/paramSlice";
import { Button } from "semantic-ui-react";

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
			<div className="mt-4">
				{showReset && <Button onClick={clearFilters}>Удалить фильтры</Button>}
			</div>
		</div>
	);
}
