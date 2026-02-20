import { AiFillTrophy, AiOutlineLogout } from 'react-icons/ai';
import { FaTrashRestoreAlt } from 'react-icons/fa';
import { RiRestartFill, RiAuctionFill } from 'react-icons/ri';
import { TbSettings, TbSettingsOff } from 'react-icons/tb';
import { HiUser } from 'react-icons/hi2';
import { GoCodescanCheckmark, GoDatabase } from 'react-icons/go';
import { GrMoney } from 'react-icons/gr';
import { HiOutlineDocumentReport } from 'react-icons/hi';
import { useLocation, useNavigate } from 'react-router-dom';
import { CurrentSettings, ModalTypes, ToastType, User } from '../../../types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../../store/store';
import { emptyUserState, setAuthUser } from '../../../store/authSlice';
import { setParams } from '../../../store/paramSlice';
import { setEventFlag } from '../../../store/processingSlice';
import { useEffect, useRef, useState } from 'react';
import { useLogoutUserMutation } from '../../../api/AuthApi';
import MessageToast from '../../signalRNotifications/MessageToast';
import { Menu } from 'primereact/menu';
import { Toast } from 'primereact/toast';
import ModalYesNo from '../../modals/ModalYesNo';
import ModalRestoreSnapShot from '../../modals/ModalRestoreSnapShot';

export default function UserActions() {
  const user: User = useSelector((state: RootState) => state.authStore);
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const location = useLocation();
  const [showConfirmSetSnapShot, setShowConfirmSetSnapShot] = useState(false);
  const [showConfirmRestore, setShowConfirmRestore] = useState(false);
  const [dateValue, setDateValue] = useState<Date>(new Date());
  const [resetLog, setResetLog] = useState(false);
  const [showConfirmSetSettings, setShowConfirmSetSettings] = useState(false);
  const settings: CurrentSettings = useSelector(
    (state: RootState) => state.settingsStore,
  );

  let adminModeLabel = '';
  let adminModeIcon = <TbSettingsOff size={30} />;

  let adminMenus: any = [];

  useEffect(() => {
    if (settings.adminMode) {
      adminModeLabel = 'Перейти в обычный режим';
      adminModeIcon = <TbSettingsOff size={30} />;
    } else {
      adminModeLabel = 'Перейти в админский режим';
      adminModeIcon = <TbSettings size={30} />;
    }

    adminMenus = [
      {
        separator: true,
      },
      {
        label: adminModeLabel,
        icon: adminModeIcon,
        command: () => setShowConfirmSetSettings(true),
      },
      {
        label: 'Elk индексация',
        icon: <GoCodescanCheckmark size={30} />,
        command: () => handleElkReindexClick(),
      },
      {
        label: 'Создать SnapShot',
        icon: <GoDatabase size={30} />,
        command: () => setShowConfirmSetSnapShot(true),
      },
      {
        label: 'Восстановить из SnapShot',
        icon: <FaTrashRestoreAlt size={30} />,
        command: () => setShowConfirmRestore(true),
      },
      {
        label: 'Сбросить Кеш изображений',
        icon: <RiRestartFill size={30} />,
        command: () => handleResetImageCacheClick(),
      },
    ];
  }, [settings.adminMode]);

  const mainMenuItems = [
    {
      label: 'Ваши действия :',
      className: 'text-center text-4xl',
      items: [
        {
          label: 'Мои аукционы',
          icon: <HiUser size={30} />,
          command: () => handleSetSellerClick(),
        },
        {
          label: 'Аукционы выигранные',
          icon: <AiFillTrophy size={30} />,
          command: () => handleSetWinnerClick(),
        },
        {
          label: 'Создать аукцион',
          icon: <RiAuctionFill size={30} />,
          command: () => handleCreateAuctionClick(),
        },
        {
          label: 'Финансы',
          icon: <GrMoney size={30} />,
          command: () => handleFinanceClick(),
        },
        {
          label: 'Отчеты',
          icon: <HiOutlineDocumentReport size={30} />,
          command: () => handleReportClick(),
        },
      ],
    },
  ];

  const logoutMenuItem = [
    {
      separator: true,
    },
    {
      label: 'Выход',
      icon: <AiOutlineLogout size={30} />,
      command: () => handleLogoutClick(),
    },
  ];

  const [stateMenuItems, setStateMenuItems] = useState<any>();

  useEffect(() => {
    //заполняем выпадающее меню основными позициями
    setStateMenuItems(mainMenuItems);
    if (user.isAdmin) {
      //заполняем выпадающее меню админскими позициями
      setStateMenuItems((prev: any) => {
        let menuItems: any = prev;
        menuItems.push(...adminMenus);
        return prev;
      });
    }
    //заполняем выпадающее меню позицией выхода - logout
    setStateMenuItems((prev: any) => {
      let menuItems: any = prev;
      menuItems.push(...logoutMenuItem);
      return prev;
    });
    // eslint-disable-next-line
  }, [user, settings.adminMode]);

  const [logoutUser] = useLogoutUserMutation();
  const menuActionsRef = useRef<any>(null);
  const toastMessage: Toast | null = useSelector(
    (state: RootState) => state.serviceStore,
  ).toast;

  const acceptCurrentSettingsClick = () => {
    dispatch(setEventFlag({ eventName: 'AdminCurrentChanged', ready: true }));
    setShowConfirmSetSettings(false);
  };

  const rejectCurrentSettingsClick = () => {
    setShowConfirmSetSettings(false);
  };

  const handleSetWinnerClick = () => {
    dispatch(setParams({ winner: user.login, seller: undefined }));
    dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: false }));
    if (location.pathname !== '/') navigate('/');
  };

  const handleSetSellerClick = () => {
    dispatch(setParams({ seller: user.login, winner: undefined }));
    dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: false }));
    if (location.pathname !== '/') navigate('/');
  };

  const handleLogoutClick = async () => {
    await logoutUser({ login: user.login });
    localStorage.removeItem('Auction');
    dispatch(setAuthUser(emptyUserState));
  };

  const handleElkReindexClick = () => {
    dispatch(setEventFlag({ eventName: 'ElkIndex', ready: true }));
    toastMessage!.show({
      severity: 'success',
      life: 4000,
      className: 'bg-white',
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
    dispatch(setEventFlag({ eventName: 'SetSnapShot', ready: true }));
    setShowConfirmSetSnapShot(false);
  };

  const rejectSetSnapShotDialog = () => {
    setShowConfirmSetSnapShot(false);
  };

  const acceptRestoreSnapShot = () => {
    setResetLog(false);
    dispatch(
      setEventFlag({
        eventName: 'RestoreSnapShot',
        ready: true,
        param: { dateValue: dateValue, resetLog: resetLog },
      }),
    );
    setShowConfirmRestore(false);
  };

  const rejectRestoreSnapShot = () => {
    setShowConfirmRestore(false);
  };

  const handleResetImageCacheClick = () => {
    dispatch(setEventFlag({ eventName: 'ResetImageCache', ready: true }));
    toastMessage!.show({
      severity: 'success',
      life: 4000,
      className: 'bg-white',
      content: (props) => (
        <MessageToast
          toastType={ToastType.Info}
          message={`Сброс кеша изображений Redis...`}
        />
      ),
    });
  };

  const handleFinanceClick = () => {
    navigate('/finance/list');
  };

  const handleReportClick = () => {
    window.location.href = process.env.REACT_APP_REPORT_URL!;
  };

  const handleCreateAuctionClick = () => {
    navigate('/auctions/create');
  };

  return (
    <div className="UserActionsPanel">
      <Menu
        ref={menuActionsRef}
        model={stateMenuItems}
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
        <div className="font-semibold text-4xl">{`Здравствуйте ${user.name}`}</div>
        <div className="mt-1 ml-2 font-semibold pi pi-angle-double-down"></div>
      </div>
      <ModalYesNo
        accept={acceptSetSnapShotDialog}
        reject={rejectSetSnapShotDialog}
        header="Подтверждение создания снимка БД"
        label={'Действительно создать снимок БД?'}
        visible={showConfirmSetSnapShot}
        group="confirmSnapShot"
        modalType={ModalTypes.warning}
      />
      <ModalRestoreSnapShot
        accept={acceptRestoreSnapShot}
        reject={rejectRestoreSnapShot}
        header="Восстановление БД"
        label={'Параметры восстановления БД'}
        visible={showConfirmRestore}
        group="editRestore"
        onChangeDate={(val) => setDateValue(val)}
        dateValue={dateValue}
        onChangeResetLog={(val) => setResetLog(val)}
      />
      <ModalYesNo
        accept={acceptCurrentSettingsClick}
        reject={rejectCurrentSettingsClick}
        header={
          settings.adminMode
            ? 'Подтверждение перехода в обычный режим'
            : 'Подтверждение перехода в админский режим'
        }
        label={
          settings.adminMode
            ? 'Действительно перейти в обычный режим?'
            : 'Действительно перейти в админский режим?'
        }
        visible={showConfirmSetSettings}
        group="confirmCurrentSettings"
        modalType={ModalTypes.warning}
      />
    </div>
  );
}
