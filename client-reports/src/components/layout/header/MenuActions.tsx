import { TfiPrinter } from "react-icons/tfi";
import { RiFileExcel2Line } from "react-icons/ri";
import { useDispatch, useSelector } from "react-redux";
import { setEvent } from "../../../store/EventSlice";
import { useParams } from "react-router-dom";
import { User } from "../../../types";
import { RootState } from "../../../store/Store";
import { Menu } from "primereact/menu";
import { useRef } from "react";

export default function MenuActions() {
  const dispatch = useDispatch();
  const menuActionsRef = useRef<any>(null);
  const { id } = useParams();
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
          icon: <TfiPrinter size={30} />,
          command: () => {},
        },
        {
          label: "Экспорт в Excel",
          icon: <RiFileExcel2Line size={30} />,
          command: () => {},
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
        className="w-30rem p-menu-list mt-3"
      />
    </div>
  );
}
