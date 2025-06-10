import {
	AiOutlineSortAscending,
	AiOutlineSortDescending,
} from "react-icons/ai";
import { ImSortAmountDesc, ImSortAmountAsc } from "react-icons/im";
import { BsStopwatchFill } from "react-icons/bs";
import { GiFinishLine, GiFlame } from "react-icons/gi";
import { useDispatch, useSelector } from "react-redux";
import { setParams } from "../../store/paramSlice";
import { useEffect, useState } from "react";
import { SelectButton, SelectButtonChangeEvent } from "primereact/selectbutton";
import { State } from "../../types";
import { SelectItem } from "primereact/selectitem";
import { RootState } from "../../store/store";

const pageSizeButtons = [
	{
		label: "4",
		value: 4,
	},
	{
		label: "8",
		value: 8,
	},
	{
		label: "16",
		value: 16,
	},
];

const filterButtons = [
	{
		label: "Текущие",
		icon: <GiFlame />,
		value: "live",
	},
	{
		label: "Заканчивающиеся",
		icon: <GiFinishLine />,
		value: "endingSoon",
	},
	{
		label: "Завершенные",
		icon: <BsStopwatchFill />,
		value: "finished",
	},
];

export default function Filters() {
	const dispatch = useDispatch();
	const [orderItem, setOrderItem] = useState<SelectItem[]>([
		{
			label: "Наименование",
			icon: <AiOutlineSortAscending />,
			value: "titleAsc",
		},
		{ label: "Окончание", icon: <ImSortAmountAsc />, value: "endAsc" },
		{ label: "Недавние", icon: <ImSortAmountAsc />, value: "newDesc" },
	]);
	const [lastOrder, setLastOrder] = useState("newDesc");
	const pageSize = useSelector((state: RootState) => state.paramStore).pageSize;
	const orderBy = useSelector((state: RootState) => state.paramStore).orderBy;
	const filterBy = useSelector((state: RootState) => state.paramStore).filterBy;

	const filterTemplate = (option: any) => {
		return (
			<>
				{option.icon}
				<label className="ml-2 cursor-pointer">{option.label}</label>
			</>
		);
	};

	const handleMenuClick = (e: SelectButtonChangeEvent, paramName: string) => {
		let param: State = {
			filterBy: paramName === "filterBy" ? e.value : "",
		};
		if (paramName !== "filterBy") {
			if (!paramName) {
				paramName = orderBy!;
			}
			setOrderItem((prev) => {
				prev.forEach((item) => {
					if (
						paramName.indexOf("title") !== -1 &&
						item.value.indexOf("title") !== -1 &&
						lastOrder.indexOf("title") !== -1
					) {
						if (item.value === "titleAsc") {
							prev[0].value = "titleDesc";
							prev[0].icon = <AiOutlineSortDescending />;
						} else {
							prev[0].value = "titleAsc";
							prev[0].icon = <AiOutlineSortAscending />;
						}
					}

					if (
						paramName.indexOf("end") !== -1 &&
						item.value.indexOf("end") !== -1 &&
						lastOrder.indexOf("end") !== -1
					) {
						if (item.value === "endAsc") {
							prev[1].value = "endDesc";
							prev[1].icon = <ImSortAmountDesc />;
						} else {
							prev[1].value = "endAsc";
							prev[1].icon = <ImSortAmountAsc />;
						}
					}

					if (
						paramName.indexOf("new") !== -1 &&
						item.value.indexOf("new") !== -1 &&
						lastOrder.indexOf("new") !== -1
					) {
						if (item.value === "newAsc") {
							prev[2].value = "newDesc";
							prev[2].icon = <ImSortAmountDesc />;
						} else {
							prev[2].value = "newAsc";
							prev[2].icon = <ImSortAmountAsc />;
						}
					}
				});
				return prev;
			});
			const valIndex =
				paramName.indexOf("title") !== -1
					? 0
					: paramName.indexOf("end") !== -1
					? 1
					: 2;
			setLastOrder(paramName);
			param.orderBy = orderItem[valIndex].value;
		}
		dispatch(setParams(param));
	};

	return (
		<div className="FilterContainer">
			<div>
				<div className="FilterItem">
					<span>Отбор по : </span>
				</div>
				<SelectButton
					onChange={(e) => handleMenuClick(e, "filterBy")}
					options={filterButtons}
					itemTemplate={filterTemplate}
					value={filterBy}
				/>
			</div>

			<div>
				<div className="FilterItem">
					<span>Сортировать по :</span>
				</div>
				<SelectButton
					options={orderItem}
					onChange={(e) => handleMenuClick(e, e.value)}
					itemTemplate={filterTemplate}
					value={orderBy}
				/>
			</div>
		</div>
	);
}
