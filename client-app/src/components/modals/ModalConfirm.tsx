import {
	Button,
	Header,
	Modal,
	ModalActions,
	ModalContent,
} from "semantic-ui-react";

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
		<Modal
			dimmer="blurring"
			size="tiny"
			closeOnEscape={true}
			closeIcon
			open={openModal}
			onClose={() => {
				setResult(false);
			}}
		>
			<Header>{title}</Header>
			<ModalContent>
				<div className="ModalText">{text}</div>
			</ModalContent>
			<ModalActions>
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
			</ModalActions>
		</Modal>
	);
}
