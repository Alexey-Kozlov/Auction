import { Button } from "primereact/button";
import { confirmDialog, ConfirmDialog } from "primereact/confirmdialog";
import DateInput from "../inputComponents/DateInput";
import { InputSwitch } from "primereact/inputswitch";
import { useState } from "react";

type Props = {
  accept: () => void;
  reject: () => void;
  header: string;
  label: string;
  visible: boolean;
  group: string;
  dateValue: Date;
  onChangeDate: (val: Date) => void;
  onChangeResetLog: (val: boolean) => void;
};

export default function ModalRestoreSnapShot({
  accept,
  reject,
  header,
  label,
  visible,
  group,
  dateValue,
  onChangeDate,
  onChangeResetLog,
}: Props) {
  if (visible) {
    confirmDialog({
      defaultFocus: "reject",
      accept,
      reject,
      onHide: () => reject(),
      group: group,
    });
  }
  const [resetLog, setResetLog] = useState(false);
  const handleResetLog = (checked: boolean) => {
    setResetLog(() => checked);
    onChangeResetLog(checked);
  };

  return (
    <ConfirmDialog
      group={group}
      className="w-6"
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
          <span className="font-bold text-4xl block mb-2 mt-4">{header}</span>
          <p className="mb-1rem text-4xl">{label}</p>
          <div className="CenterItem">
            <div className="text-3xl mr-3">Укажите дату восстановления:</div>
            <div>
              <DateInput
                value={dateValue}
                onChange={(e) => onChangeDate(e!)}
                showOnFocus={false}
              />
            </div>
          </div>
          <div className="CenterItem mt-3">
            <div className="text-3xl mr-3">
              Удаление событий после даты восстановления:
            </div>
            <div>
              <InputSwitch
                checked={resetLog}
                onChange={(e) => handleResetLog(e.value)}
              />
            </div>
          </div>
          <div className="flex align-items-center gap-2 mt-4">
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
              className="w-16rem CustomButton"
            ></Button>
          </div>
        </div>
      )}
    />
  );
}
