import { AiFillTrophy, AiOutlineLogout } from "react-icons/ai";
import { FaTrashRestoreAlt } from "react-icons/fa";
import { RiRestartFill } from "react-icons/ri";
import { RiAuctionFill } from "react-icons/ri";
import { HiUser } from "react-icons/hi2";
import { GoCodescanCheckmark, GoDatabase } from "react-icons/go";
import { GrMoney } from "react-icons/gr";
import { HiOutlineDocumentReport } from "react-icons/hi";
import { useLocation, useNavigate } from "react-router-dom";
import { ModalParams, ModalTypes, ToastType, User } from "../../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/store";
import { emptyUserState, setAuthUser } from "../../../store/authSlice";
import { setParams } from "../../../store/paramSlice";
import { setEventFlag } from "../../../store/processingSlice";
import { useEffect, useRef, useState } from "react";
import { useLogoutUserMutation } from "../../../api/AuthApi";
import MessageToast from "../../signalRNotifications/MessageToast";
import { Menu } from "primereact/menu";
import { Toast } from "primereact/toast";
import ModalYesNo from "../../modals/ModalYesNo";

export default function UserActions() {
  const user: User = useSelector((state: RootState) => state.authStore);
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const location = useLocation();
  const [confirmParam, setConfirmParam] = useState<ModalParams>({
    confirmText: "",
    confirmTitle: "",
    handler: "",
  });
  const [showConfirmSet, setShowConfirmSet] = useState(false);
  const [showConfirmRestore, setShowConfirmRestore] = useState(false);
  const [dateValue, setDateValue] = useState<Date | null>(new Date());
  const [confirmResult, setConfirmResult] = useState<boolean | undefined>(
    undefined
  );
  const [resetLog, setResetLog] = useState(false);
  const [logoutUser] = useLogoutUserMutation();
  const menuActionsRef = useRef<any>(null);
  const toastMessage: Toast | null = useSelector(
    (state: RootState) => state.serviceStore
  ).toast;

  useEffect(() => {
    if (confirmResult) {
      //нажали "Ок" в модалке восстановления SnapShot
      //устанавливаем параметр в хранилище, отслеживаем изменение в ShowEventsPopUo.tsx
      if (confirmParam.handler === "RestoreSnapShot") {
        dispatch(
          setEventFlag({
            eventName: "RestoreSnapShot",
            ready: false,
            param: { dateValue: dateValue, resetLog: resetLog },
          })
        );
        // toast(
        // 	(p) => (
        // 		<MessageToast
        // 			message={"Старт восстановления БД из ES..."}
        // 			toastId={p.id}
        // 			toastType={ToastType.Info}
        // 		/>
        // 	),
        // 	{
        // 		duration: 5000,
        // 	}
        // );
      }
    }

    // eslint-disable-next-line
  }, [confirmResult]);

  const handleSetDate = (result: Date) => {
    setDateValue(result);
  };

  const handleSetWinnerClick = () => {
    dispatch(setParams({ winner: user.login, seller: undefined }));
    dispatch(setEventFlag({ eventName: "CollectionChanged", ready: false }));
    if (location.pathname !== "/") navigate("/");
  };

  const handleSetSellerClick = () => {
    dispatch(setParams({ seller: user.login, winner: undefined }));
    dispatch(setEventFlag({ eventName: "CollectionChanged", ready: false }));
    if (location.pathname !== "/") navigate("/");
  };

  const handleLogoutClick = async () => {
    await logoutUser({ login: user.login });
    localStorage.removeItem("Auction");
    dispatch(setAuthUser(emptyUserState));
  };

  const handleElkReindexClick = () => {
    dispatch(setEventFlag({ eventName: "ElkIndex", ready: false }));
    toastMessage!.show({
      severity: "success",
      life: 4000,
      className: "bg-white",
      content: (props) => (
        <MessageToast
          toastType={ToastType.Info}
          message={`Старт переиндексации ELK...`}
        />
      ),
    });
  };

  const acceptSetSnapShotDialog = () => {
    //создаем снимок БД
    dispatch(setEventFlag({ eventName: "SetSnapShot", ready: false }));
    setShowConfirmSet(false);
  };

  const rejectSetSnapShotDialog = () => {
    setShowConfirmSet(false);
  };

  const handlerRestoreSnapShotClick = () => {
    setDateValue(new Date());
    setResetLog(false);
    setConfirmParam({
      confirmText: "Произвести восстановление БД из лога?",
      confirmTitle: "Восстановление БД из лога",
      handler: "RestoreSnapShot",
    });
    setShowConfirmRestore(() => true);
  };

  const handleResetImageCacheClick = () => {
    dispatch(setEventFlag({ eventName: "ResetImageCache", ready: false }));
    toastMessage!.show({
      severity: "success",
      life: 4000,
      className: "bg-white",
      content: (props) => (
        <MessageToast
          toastType={ToastType.Info}
          message={`Сброс кеша изображений Redis...`}
        />
      ),
    });
  };

  const handleFinanceClick = () => {
    navigate("/finance/list");
  };

  // const handleReportClick = () => {
  // 	window.location.href = process.env.REACT_APP_REPORT_URL!;
  // };

  const handleCreateAuctionClick = () => {
    navigate("/auctions/create");
  };

  const menuItems = [
    {
      label: "Ваши действия :",
      className: "text-center text-4xl",
      items: [
        {
          label: "Мои аукционы",
          icon: (
            <HiUser
              className="MenuItems"
              size={30}
            />
          ),
          command: () => handleSetSellerClick(),
        },
        {
          label: "Аукционы выигранные",
          icon: (
            <AiFillTrophy
              className="MenuItems"
              size={30}
            />
          ),
          command: () => handleSetWinnerClick(),
        },
        {
          label: "Создать аукцион",
          icon: (
            <RiAuctionFill
              className="MenuItems"
              size={30}
            />
          ),
          command: () => handleCreateAuctionClick(),
        },
        {
          label: "Финансы",
          icon: (
            <GrMoney
              className="MenuItems"
              size={30}
            />
          ),
          command: () => handleFinanceClick(),
        },
        {
          label: "Elk индексация",
          icon: (
            <GoCodescanCheckmark
              className="MenuItems"
              size={30}
            />
          ),
          command: () => handleElkReindexClick(),
        },
        {
          label: "Создать SnapShot",
          icon: (
            <GoDatabase
              className="MenuItems"
              size={30}
            />
          ),
          command: () => setShowConfirmSet(true),
        },
        {
          label: "Восстановить из SnapShot",
          icon: (
            <FaTrashRestoreAlt
              className="MenuItems"
              size={30}
            />
          ),
        },
        {
          label: "Сбросить Кеш изображений",
          icon: (
            <RiRestartFill
              className="MenuItems"
              size={30}
            />
          ),
          command: () => handleResetImageCacheClick(),
        },
        {
          label: "Отчеты",
          icon: (
            <HiOutlineDocumentReport
              className="MenuItems"
              size={30}
            />
          ),
        },
        {
          label: "Выход",
          icon: (
            <AiOutlineLogout
              className="MenuItems"
              size={30}
            />
          ),
          command: () => handleLogoutClick(),
        },
      ],
    },
  ];

  return (
    <div className="UserActionsPanel">
      <Menu
        ref={menuActionsRef}
        model={menuItems}
        popup
        popupAlignment="right"
        id="menuActions"
        className="w-30rem p-menu-list mt-3"
      />
      <div
        aria-controls="menuActions"
        onClick={(event) => menuActionsRef.current!.toggle(event)}
        className="flex cursor-pointer align-items-center"
      >
        <div className="font-semibold text-4xl">{`Здравствуйте ${user.name}`}</div>
        <div className="mt-1 ml-2 font-semibold pi pi-angle-double-down"></div>
      </div>
      <ModalYesNo
        accept={acceptSetSnapShotDialog}
        reject={rejectSetSnapShotDialog}
        header="Подтверждение создания снимка БД"
        label={"Действительно создать снимок БД?"}
        visible={showConfirmSet}
        group="confirmSnapShot"
        modalType={ModalTypes.warning}
      />
    </div>
  );
}
