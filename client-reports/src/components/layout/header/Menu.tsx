import { Dropdown, DropdownItem, DropdownMenu } from "semantic-ui-react";
import { TfiPrinter } from "react-icons/tfi";
import { RiFileExcel2Line } from "react-icons/ri";
import { useDispatch, useSelector } from "react-redux";
import { setEvent } from "../../../store/EventSlice";
import { useParams } from "react-router-dom";
import { User } from "../../../types";
import { RootState } from "../../../store/Store";
import { Children } from "react";

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
		<div className="UserActionsPanel">
			<Dropdown className="UserTitle" labeled text={user.name}>
				<DropdownMenu label={`${user.name}`}>
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<TfiPrinter className="MenuItems" size={20} />
									Экспорт в Pdf
								</span>
							</>
						}
						onClick={ExportPdf}
						disabled={!id}
						className={!id ? "MenuItemsAction" : ""}
					/>
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<RiFileExcel2Line className="MenuItems" size={20} />
									Экспорт в Excel
								</span>
							</>
						}
						image={<RiFileExcel2Line />}
						onClick={ExportExcel}
						disabled={!id}
						className={!id ? "MenuItemsAction" : ""}
					/>
				</DropdownMenu>
			</Dropdown>
		</div>
	);
}
