import toast from "react-hot-toast";
import { FinanceItem } from "../../types";
import { GrMoney } from "react-icons/gr";
import { Button } from "semantic-ui-react";

type Props = {
	finance: FinanceItem;
	toastId: string;
};

export default function FinanceCreatedToast({ finance, toastId }: Props) {
	return (
		<div>
			<div className="ToastCloseButton">
				<Button
					className="MainButton w-40"
					onClick={() => toast.dismiss(toastId)}
				>
					X
				</Button>
			</div>

			<div className="ToastMessageContainer">
				<GrMoney size={30} />
				<span className="ToastItemText">{`Пополнен баланс на  "${finance.value}" руб.`}</span>
			</div>
		</div>
	);
}
