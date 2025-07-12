import { Button } from "primereact/button";
import { confirmDialog, ConfirmDialog } from "primereact/confirmdialog";
import { ModalTypes } from "../../types";
import { IoWarningOutline } from "react-icons/io5";
import { BsInfoCircle } from "react-icons/bs";

type Props = {
  accept: () => void;
  reject: () => void;
  header: string;
  label: string;
  visible: boolean;
  group: string;
  modalType: ModalTypes;
};

export default function ModalYesNo({
  accept,
  reject,
  header,
  label,
  visible,
  group,
  modalType,
}: Props) {
  if (visible) {
    confirmDialog({
      accept,
      reject,
      onHide: () => reject(),
      group: group,
    });
  }
  const getIcon = () => {
    switch (modalType) {
      case ModalTypes.warning:
        return (
          <IoWarningOutline
            size={40}
            color="red"
          />
        );
        break;
      case ModalTypes.info:
        return (
          <BsInfoCircle
            size={40}
            color="rgba(131, 147, 236, 0.12);"
          />
        );
        break;
    }
  };
  const getStyle = () => {
    switch (modalType) {
      case ModalTypes.warning:
        return " WarningButton";
        break;
      case ModalTypes.info:
        return "";
        break;
    }
  };

  return (
    <ConfirmDialog
      group={group}
      className="w-6 "
      visible={visible}
      pt={{
        icon: {
          className: "DialogIconSize",
        },
        content: {
          className: "DialogIconPlace",
        },
      }}
      content={({ hide }) => (
        <div className="flex flex-column align-items-center p-5 border-round-lg bg-white w-full">
          <div className="align-self-start">{getIcon()}</div>
          <span className="font-bold text-4xl block mb-2 mt-2">{header}</span>
          <p className="mb-2rem text-4xl">{label}</p>

          <div className="flex align-items-center gap-2 mt-2">
            <Button
              label="Отмена"
              text
              raised
              rounded
              onClick={(event) => {
                hide(event);
                reject();
              }}
              className="w-16rem CustomButton mr-2"
            ></Button>
            <Button
              label="Ок"
              text
              raised
              rounded
              onClick={(event) => {
                hide(event);
                accept();
              }}
              className={"w-16rem CustomButton" + getStyle()}
              autoFocus
            ></Button>
          </div>
        </div>
      )}
    />
  );
}
