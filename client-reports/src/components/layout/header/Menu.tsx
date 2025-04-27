import { Dropdown } from "flowbite-react";
import { TfiPrinter } from "react-icons/tfi";
import { RiFileExcel2Line } from "react-icons/ri";
import { useDispatch, useSelector } from "react-redux";
import { setEvent } from "../../../store/EventSlice";
import { useParams } from "react-router-dom";
import { User } from "../../../types";
import { RootState } from "../../../store/Store";

export default function Menu() {
	const dispatch = useDispatch();
	const { id } = useParams();
	const ExportPdf = () => {
		dispatch(setEvent({ exportPdfClicked: true }));
	};
	const ExportExcel = () => {
		dispatch(setEvent({ exportExcelClicked: true }));
	};
	const user: User = useSelector((state: RootState) => state.authStore);

	return (
		<Dropdown inline label={`${user.name}`}>
			<Dropdown.Item
				icon={TfiPrinter}
				onClick={ExportPdf}
				disabled={!id}
				className={!id ? "text-gray-400 cursor-not-allowed" : ""}
			>
				Экспорт в Pdf
			</Dropdown.Item>
			<Dropdown.Item
				icon={RiFileExcel2Line}
				onClick={ExportExcel}
				disabled={!id}
				className={!id ? "text-gray-400 cursor-not-allowed" : ""}
			>
				Экспорт в Excel
			</Dropdown.Item>
		</Dropdown>
	);
}
