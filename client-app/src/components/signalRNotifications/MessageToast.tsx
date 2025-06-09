import toast from "react-hot-toast";
import { ToastType } from "../../types";
import { BsInfoCircle } from "react-icons/bs";
import { VscError } from "react-icons/vsc";
import { ImWarning } from "react-icons/im";
import { Button } from "primereact/button";

type Props = {
	message: string;
	toastId: string;
	toastType: ToastType;
};

export default function MessageToast({ message, toastId, toastType }: Props) {
	const getToastType = () => {
		switch (toastType) {
			case ToastType.Info:
				return "info";
			case ToastType.Error:
				return "error";
			case ToastType.Warning:
				return "warn";
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
					<div className="mr-10">
						{getToastType() === "info" ? (
							<BsInfoCircle size={36} />
						) : getToastType() === "error" ? (
							<VscError size={36} />
						) : (
							<ImWarning size={36} />
						)}
					</div>
					<div className="ToastItemText">{message}</div>
				</div>
			</>
		</div>
	);
}
