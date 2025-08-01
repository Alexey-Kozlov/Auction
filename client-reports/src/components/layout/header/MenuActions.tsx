import { TfiPrinter } from "react-icons/tfi";
import { RiFileExcel2Line } from "react-icons/ri";
import { useDispatch, useSelector } from "react-redux";
import { setEvent } from "../../../store/EventSlice";
import { User } from "../../../types";
import { RootState } from "../../../store/Store";
import { Menu } from "primereact/menu";
import { useRef } from "react";

export default function MenuActions() {
  const dispatch = useDispatch();
  const menuActionsRef = useRef<any>(null);
  const ExportPdf = () => {
    dispatch(setEvent({ exportPdfClicked: true }));
  };
  const ExportExcel = () => {
    dispatch(setEvent({ exportExcelClicked: true }));
  };
  const user: User = useSelector((state: RootState) => state.authStore);

  const mainMenuItems = [
    {
      label: "Ваши действия :",
      className: "text-center text-4xl",
      items: [
        {
          label: "Экспорт в PDF",
          icon: (
            <TfiPrinter
              size={30}
              className="mr-3"
            />
          ),
          command: () => ExportPdf(),
        },
        {
          label: "Экспорт в Excel",
          icon: (
            <RiFileExcel2Line
              size={30}
              className="mr-3"
            />
          ),
          command: () => ExportExcel(),
        },
      ],
    },
  ];

  return (
    <div className="UserActionsPanel">
      <Menu
        ref={menuActionsRef}
        model={mainMenuItems}
        popup
        popupAlignment="right"
        id="menuActions"
        className="w-30rem mt-3"
      />
      <div
        aria-controls="menuActions"
        onClick={(event) => menuActionsRef.current!.toggle(event)}
        className="flex cursor-pointer align-items-center"
      >
        <div className="font-semibold text-4xl">{user.name}</div>
        <div className="mt-1 ml-3 font-semibold pi pi-angle-double-down"></div>
      </div>
    </div>
  );
}
