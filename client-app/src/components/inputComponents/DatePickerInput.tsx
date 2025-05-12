import DatePicker, { registerLocale } from "react-datepicker";
import { ru } from "date-fns/locale";

type Props = {
	showTimeSelect: boolean;
	showMonthDropdown: boolean;
	showYearDropdown: boolean;
	getValue: (value: Date) => void;
	setValue: Date;
};

registerLocale("ru", ru);

export default function DatePickerInput({ ...rest }: Partial<Props>) {
	return (
		<DatePicker
			{...rest}
			showTimeSelect={rest.showTimeSelect}
			showMonthDropdown={rest.showMonthDropdown}
			showYearDropdown={rest.showYearDropdown}
			wrapperClassName="datepicker"
			selected={rest.setValue || null}
			locale={ru}
			todayButton="Сегодня"
			closeOnScroll={true}
			timeCaption="time"
			dateFormat="dd.MM.yyyy HH:mm"
			timeIntervals={60}
			onChange={(value) => {
				rest.getValue!(value ? value : new Date());
			}}
		/>
	);
}
