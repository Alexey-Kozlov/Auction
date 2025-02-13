import { Dropdown } from "flowbite-react";
import { TfiPrinter } from "react-icons/tfi";
import { useDispatch } from "react-redux";
import { setEvent } from "../../../store/EventSlice";

export default function Menu() {
	const dispatch = useDispatch();
	const Export = () => {
		dispatch(setEvent({ exportPdfClicked: true }));
	};

	return (
		<Dropdown inline label={`Действия`}>
			<Dropdown.Item icon={TfiPrinter} onClick={Export}>
				Экспорт в Pdf
			</Dropdown.Item>
		</Dropdown>
	);
}
