import { useState } from "react";
import {
	Button,
	Checkbox,
	Form,
	Grid,
	GridColumn,
	GridRow,
	Header,
	Modal,
	ModalActions,
	ModalContent,
} from "semantic-ui-react";
import DatePickerInput from "../inputComponents/DatePickerInput";

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
		<Modal
			dimmer="blurring"
			size="small"
			closeOnEscape={true}
			closeIcon
			open={openModal}
			onClose={() => {
				setResult(false);
			}}
		>
			<Header>{title}</Header>
			<ModalContent>
				<Form>
					<Grid columns={2}>
						<GridRow>
							<GridColumn verticalAlign="middle">
								<div className="flex ModalText">
									Укажите дату восстановления:
								</div>
							</GridColumn>
							<GridColumn verticalAlign="middle">
								<div className="z-index w-100P">
									<DatePickerInput
										showTimeSelect
										showMonthDropdown
										showYearDropdown
										setValue={dateValue!}
										getValue={(value) => returnData(value)}
									/>
								</div>
							</GridColumn>
						</GridRow>
						<GridRow>
							<GridColumn verticalAlign="middle">
								<div className="flex ModalText">
									Удаление событий после даты восстановления:
								</div>
							</GridColumn>
							<GridColumn verticalAlign="middle">
								<div>
									<Checkbox
										toggle
										checked={resLog}
										onChange={(e, data) => handleResetLog(data.checked!)}
									/>
								</div>
							</GridColumn>
						</GridRow>
					</Grid>
				</Form>

				<div className="ModalText text-center mt-30">{text}</div>
			</ModalContent>
			<ModalActions>
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
			</ModalActions>
		</Modal>
	);
}
