import { ToastType } from "../../types";
import { BsInfoCircle } from "react-icons/bs";
import { VscError } from "react-icons/vsc";
import { ImWarning } from "react-icons/im";

type Props = {
  message: string;
  toastType: ToastType;
};

export default function MessageToast({ message, toastType }: Props) {
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
    <div className="w-full">
      <div className="ToastMessageContainer">
        <div className="align-self-start">
          {getToastType() === "info" ? (
            <BsInfoCircle size={36} />
          ) : getToastType() === "error" ? (
            <VscError size={36} />
          ) : (
            <ImWarning size={36} />
          )}
        </div>
        <div className="ToastMessageTextContainer">
          <p className="m-2 text-3xl text-center">{message}</p>
          <div className="flex flex-column gap-2"></div>
        </div>
      </div>
    </div>
  );
}
