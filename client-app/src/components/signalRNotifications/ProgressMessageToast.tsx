import toast from "react-hot-toast";
import { Message } from "../../types";
import { BsInfoCircle } from "react-icons/bs";
import { Button, Progress } from "semantic-ui-react";

type Props = {
	message: Message;
	toastId: string;
};

export default function ProgressMessageToast({ message, toastId }: Props) {
	return (
		<div>
			<>
				<div className="ToastCloseButton">
					<Button
						className="MainButton w-40"
						onClick={() => toast.dismiss(toastId)}
					>
						X
					</Button>
				</div>
				<div className="ToastMessageContainer">
					<div className="flex">
						<div>
							<BsInfoCircle size={36} />
						</div>

						<div className="text-center ToastItemText">
							{message.message !== "-1"
								? message.message + ", завершено - " + message.messageType + "%"
								: "Записи не найдены"}
						</div>
					</div>
					<div className="flex">
						<div>
							{message.message !== "-1" && (
								<Progress
									percent={message.messageType}
									textLabel="Выполнено"
									labelProgress
									labelText
									color="blue"
								/>
							)}
						</div>
					</div>
				</div>
			</>
		</div>
	);
}
