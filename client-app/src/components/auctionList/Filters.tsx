import { BsStopwatchFill } from "react-icons/bs";
import { GiFinishLine, GiFlame } from "react-icons/gi";
import { useDispatch, useSelector } from "react-redux";
import { setParams } from "../../store/paramSlice";
import { useState } from "react";
import { SelectButton, SelectButtonChangeEvent } from "primereact/selectbutton";
import { State } from "../../types";
import { SelectItem } from "primereact/selectitem";
import { RootState } from "../../store/store";

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
			icon: <i className="pi pi-sort-alpha-down" />,
			value: "titleAsc",
		},
		{
			label: "Окончание",
			icon: <i className="pi pi-sort-amount-down" />,
			value: "endAsc",
		},
		{
			label: "Недавние",
			icon: <i className="pi pi-sort-amount-up" />,
			value: "newDesc",
		},
	]);
	const [lastOrder, setLastOrder] = useState("newDesc");
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
							prev[0].icon = <i className="pi pi-sort-alpha-up" />;
						} else {
							prev[0].value = "titleAsc";
							prev[0].icon = <i className="pi pi-sort-alpha-down" />;
						}
					}

					if (
						paramName.indexOf("end") !== -1 &&
						item.value.indexOf("end") !== -1 &&
						lastOrder.indexOf("end") !== -1
					) {
						if (item.value === "endAsc") {
							prev[1].value = "endDesc";
							prev[1].icon = <i className="pi pi-sort-amount-up" />;
						} else {
							prev[1].value = "endAsc";
							prev[1].icon = <i className="pi pi-sort-amount-down" />;
						}
					}

					if (
						paramName.indexOf("new") !== -1 &&
						item.value.indexOf("new") !== -1 &&
						lastOrder.indexOf("new") !== -1
					) {
						if (item.value === "newAsc") {
							prev[2].value = "newDesc";
							prev[2].icon = <i className="pi pi-sort-amount-up" />;
						} else {
							prev[2].value = "newAsc";
							prev[2].icon = <i className="pi pi-sort-amount-down" />;
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
