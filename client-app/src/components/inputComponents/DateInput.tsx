import { addLocale } from "primereact/api";
import { Calendar } from "primereact/calendar";

type Props = {
	value: Date;
	onChange: (val: Date) => void;
};

export default function DateInput({ value, onChange }: Props) {
	addLocale("ru", {
		firstDayOfWeek: 0,
		dayNames: [
			"Понедельник",
			"Вторник",
			"Среда",
			"Четверг",
			"Пятница",
			"Суббота",
			"Воскресенье",
		],
		dayNamesShort: ["пн", "вт", "ср", "чт", "пт", "сб", "вс"],
		dayNamesMin: ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"],
		monthNames: [
			"Январь",
			"Февраль",
			"Март",
			"Апрель",
			"Май",
			"Июнь",
			"Июль",
			"Август",
			"Сентябрь",
			"Октябрь",
			"Ноябрь",
			"Декабрь",
		],
		monthNamesShort: [
			"янв",
			"фев",
			"мар",
			"апр",
			"май",
			"июнь",
			"июль",
			"авг",
			"сен",
			"окт",
			"ноб",
			"дек",
		],
		now: "Сегодня",
		clear: "Очистить",
	});
	return (
		<div>
			<Calendar
				inputId="AuctionEnd"
				variant="filled"
				value={value}
				onChange={(e) => onChange(e.value!)}
				showTime
				hourFormat="24"
				locale="ru"
				dateFormat="dd.mm.yy"
				showButtonBar
				className="w-18rem h-3rem"
			/>
		</div>
	);
}
