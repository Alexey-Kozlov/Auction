import { useState } from "react";
import DatePickerInput from "../inputComponents/DatePickerInput";
import { Button } from "primereact/button";

type Props = {
	openModal: boolean;
	setResult: (rezult?: boolean) => void;
	title: string;
	text: string;
	returnData: (rezult: Date) => void;
	resetLog: (rezult: boolean) => void;
	dateValue: Date | null;
};

export default function ModalRestore({
	openModal,
	setResult,
	title,
	text,
	returnData,
	resetLog,
	dateValue,
}: Props) {
	const [resLog, setResetLog] = useState(false);
	const handleResetLog = (checked: boolean) => {
		setResetLog(() => checked);
		resetLog(checked);
	};

	return (
		<div>
			<div>
				<Button
					id="ModalConfirmYesButton"
					onClick={() => {
						setResult(true);
						setResetLog((prev) => false);
					}}
				>
					Да
				</Button>
				<Button
					id="ModalConfirmNoButton"
					onClick={() => {
						setResult(false);
						setResetLog((prev) => false);
					}}
				>
					Нет
				</Button>
			</div>
		</div>
	);
}
