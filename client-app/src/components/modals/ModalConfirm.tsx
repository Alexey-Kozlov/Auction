import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";

type Props = {
	openModal: boolean;
	setResult: (rezult: boolean) => void;
	title: string;
	text: string;
};

export default function ModalConfirm({
	openModal,
	setResult,
	title,
	text,
}: Props) {
	return (
		<div>
			<Button
				id="ModalConfirmYesButton"
				onClick={() => {
					setResult(true);
				}}
			>
				Да
			</Button>
			<Button
				id="ModalConfirmNoButton"
				onClick={() => {
					setResult(false);
				}}
			>
				Нет
			</Button>
		</div>
	);
}
