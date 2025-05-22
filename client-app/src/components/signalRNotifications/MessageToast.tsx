import toast from "react-hot-toast";
import { Message, ToastType } from "../../types";
import { BsInfoCircle } from "react-icons/bs";
import { Button } from "semantic-ui-react";
import { VscError } from "react-icons/vsc";
import { ImWarning } from "react-icons/im";

type Props = {
	message: Message;
	toastId: string;
	toastType: ToastType;
};

export default function MessageToast({ message, toastId, toastType }: Props) {
	const getToastType = () => {
		switch (toastType) {
			case ToastType.Info:
				return 0;
			case ToastType.Error:
				return 1;
			case ToastType.Warning:
				return 2;
		}
	};
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
					<div>
						{getToastType() === 0 ? (
							<BsInfoCircle size={36} />
						) : getToastType() === 1 ? (
							<VscError size={36} />
						) : (
							<ImWarning size={36} />
						)}
					</div>
					<span className="ToastItemText">{message.message}</span>
				</div>
			</>
		</div>
	);
}
