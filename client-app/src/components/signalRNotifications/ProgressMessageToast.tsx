import toast from "react-hot-toast";
import { ProgressToast } from "../../types";
import { BsInfoCircle } from "react-icons/bs";
import { Button, Progress } from "semantic-ui-react";

type Props = {
	message: ProgressToast;
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
				<div className="ToastMessageContainerProgress">
					<div className="ToastMessageProgressText">
						<div className="ToastMessageProgressIcon">
							<BsInfoCircle size={36} />
						</div>

						<div className="text-center ToastItemText">{message.message}</div>
					</div>
					<div className="text-center ToastItemText">
						{"Восстановлено " + Math.trunc(message.percent) + " %"}
					</div>
					<div className="flex w-100P">
						{message.show && (
							<Progress
								className="w-100P"
								percent={message.percent}
								indicating
								size="small"
								color="blue"
							/>
						)}
					</div>
				</div>
			</>
		</div>
	);
}
