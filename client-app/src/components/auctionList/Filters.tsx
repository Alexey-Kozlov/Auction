import {
	AiOutlineSortAscending,
	AiOutlineSortDescending,
} from "react-icons/ai";
import { ImSortAmountDesc, ImSortAmountAsc } from "react-icons/im";
import { BsStopwatchFill } from "react-icons/bs";
import { GiFinishLine, GiFlame } from "react-icons/gi";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { setParams } from "../../store/paramSlice";
import { useState } from "react";
import { IconType } from "react-icons/lib";
import { ButtonGroup, Button, SemanticCOLORS } from "semantic-ui-react";

const pageSizeButtons = [4, 8, 16];
const orderButtons = [
	{
		label: "Наименование",
		icon: AiOutlineSortAscending,
		value: "titleAsc",
	},
	{
		label: "Наименование",
		icon: AiOutlineSortDescending,
		value: "titleDesc",
	},
	{
		label: "Окончание",
		icon: ImSortAmountAsc,
		value: "endAsc",
	},
	{
		label: "Окончание",
		icon: ImSortAmountDesc,
		value: "endDesc",
	},
	{
		label: "Недавние",
		icon: ImSortAmountAsc,
		value: "newAsc",
	},
	{
		label: "Недавние",
		icon: ImSortAmountDesc,
		value: "newDesc",
	},
];
const filterButtons = [
	{
		label: "Текущие",
		icon: GiFlame,
		value: "live",
	},
	{
		label: "Заканчивающиеся",
		icon: GiFinishLine,
		value: "endingSoon",
	},
	{
		label: "Завершенные",
		icon: BsStopwatchFill,
		value: "finished",
	},
];

export default function Filters() {
	const dispatch = useDispatch();
	const pageSize = useSelector((state: RootState) => state.paramStore).pageSize;
	const orderBy = useSelector((state: RootState) => state.paramStore).orderBy;
	const filterBy = useSelector((state: RootState) => state.paramStore).filterBy;
	const [orderItem, setOrderItem] = useState(["titleAsc", "endAsc", "newDesc"]);
	const [lastOrder, setLastOrder] = useState("newDesc");

	const handleFilter = (val: string) => {
		setOrderItem((prev) => {
			prev.forEach((item) => {
				if (
					val.indexOf("title") !== -1 &&
					item.indexOf("title") !== -1 &&
					lastOrder.indexOf("title") !== -1
				) {
					if (item === "titleAsc") {
						prev[0] = "titleDesc";
					} else {
						prev[0] = "titleAsc";
					}
				}

				if (
					val.indexOf("end") !== -1 &&
					item.indexOf("end") !== -1 &&
					lastOrder.indexOf("end") !== -1
				) {
					if (item === "endAsc") {
						prev[1] = "endDesc";
					} else {
						prev[1] = "endAsc";
					}
				}

				if (
					val.indexOf("new") !== -1 &&
					item.indexOf("new") !== -1 &&
					lastOrder.indexOf("new") !== -1
				) {
					if (item === "newAsc") {
						prev[2] = "newDesc";
					} else {
						prev[2] = "newAsc";
					}
				}
			});
			return prev;
		});
		const valIndex =
			val.indexOf("title") !== -1 ? 0 : val.indexOf("end") !== -1 ? 1 : 2;
		setLastOrder(val);
		dispatch(setParams({ orderBy: orderItem[valIndex] }));
	};

	return (
		<div className="FilterContainer">
			<div>
				<div className="FilterItem">
					<span>Отбор по : </span>
				</div>

				<ButtonGroup>
					{filterButtons.map(({ label, icon: Icon, value }) => {
						return (
							<Button
								key={value}
								onClick={() => dispatch(setParams({ filterBy: value }))}
								color={
									`${filterBy === value ? "blue" : "grey"}` as SemanticCOLORS
								}
							>
								<Icon className="FilterIcon" />
								{label}
							</Button>
						);
					})}
				</ButtonGroup>
			</div>

			<div>
				<div className="FilterItem">
					<span>Сортировать по : </span>
				</div>
				<ButtonGroup>
					{orderItem.map((item) => {
						const Icon: IconType = orderButtons.find(
							(p) => p.value === item
						)!.icon;
						return (
							<Button
								key={orderButtons.find((p) => p.value === item)!.value}
								onClick={() =>
									handleFilter(
										orderButtons.find((p) => p.value === item)!.value
									)
								}
								color={
									`${
										orderBy ===
										orderButtons.find((p) => p.value === item)!.value
											? "blue"
											: "grey"
									}` as SemanticCOLORS
								}
							>
								<Icon className="FilterIcon" />
								{orderButtons.find((p) => p.value === item)!.label}
							</Button>
						);
					})}
				</ButtonGroup>
			</div>

			<div>
				<div className="FilterItem">
					<span>Размер страницы</span>
				</div>
				<ButtonGroup>
					{pageSizeButtons.map((value: number, index: number) => {
						return (
							<Button
								key={index}
								onClick={() => dispatch(setParams({ pageSize: value }))}
								color={
									`${pageSize === value ? "blue" : "grey"}` as SemanticCOLORS
								}
							>
								{value}
							</Button>
						);
					})}
				</ButtonGroup>
			</div>
		</div>
	);
}
