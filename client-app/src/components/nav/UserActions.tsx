import { Dropdown } from "flowbite-react";
import { AiFillTrophy, AiOutlineLogout } from "react-icons/ai";
import { FaTrashRestoreAlt } from "react-icons/fa";
import { RiRestartFill } from "react-icons/ri";
import { RiAuctionFill } from "react-icons/ri";
import { HiUser } from "react-icons/hi2";
import { GoCodescanCheckmark, GoDatabase } from "react-icons/go";
import { GrMoney } from "react-icons/gr";
import { HiOutlineDocumentReport } from "react-icons/hi";
import { NavLink, useLocation, useNavigate } from "react-router-dom";
import { Message, ModalParams, RequestType, User } from "../../store/types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { emptyUserState, setAuthUser } from "../../store/authSlice";
import { setParams } from "../../store/paramSlice";
import { setEventFlag } from "../../store/processingSlice";
import InfoMessageToast from "../signalRNotifications/InfoMessageToast";
import toast from "react-hot-toast";
import ModalConfirm from "../modals/ModalConfirm";
import { useEffect, useState } from "react";
import { useLogoutUserMutation } from "../../api/AuthApi";
import { useCookies } from "react-cookie";

export default function UserActions() {
	const user: User = useSelector((state: RootState) => state.authStore);
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
	const navigate = useNavigate();
	const dispatch = useDispatch();
	const location = useLocation();
	const [confirmParam, setConfirmParam] = useState<ModalParams>({
		confirmText: "",
		confirmTitle: "",
		handler: "",
	});
	const [showConfirm, setShowConfirm] = useState(false);
	const [dateValue, setDateValue] = useState<Date | null>(new Date());
	const [confirmResult, setConfirmResult] = useState<boolean | undefined>(
		undefined
	);
	const [resetLog, setResetLog] = useState(false);
	const [logoutUser] = useLogoutUserMutation();

	const SetWinner = () => {
		dispatch(setParams({ winner: user.login, seller: undefined }));
		dispatch(setEventFlag({ eventName: "CollectionChanged", ready: true }));
		if (location.pathname !== "/") navigate("/");
	};

	const SetSeller = () => {
		dispatch(setParams({ seller: user.login, winner: undefined }));
		dispatch(setEventFlag({ eventName: "CollectionChanged", ready: true }));
		if (location.pathname !== "/") navigate("/");
	};

	const handleLogout = async () => {
		//посылаем сообщение о выходе пользователя из системы - для логирования
		setCookie("RequestType", RequestType[RequestType.Logout]);
		await logoutUser({ login: user.login });
		localStorage.removeItem("Auction");
		dispatch(setAuthUser(emptyUserState));
	};

	const handlerElkReindex = () => {
		dispatch(setEventFlag({ eventName: "ElkIndex", ready: true }));
		const message: Message = {
			message: "Старт переиндексации ELK...",
			auctionId: "",
			messageType: 0,
		};
		return toast((p) => <InfoMessageToast message={message} toastId={p.id} />, {
			duration: 5000,
		});
	};

	const handlerSetSnapShot = () => {
		setDateValue(null);
		setConfirmParam({
			confirmText: "Произвести сохранение состояния БД?",
			confirmTitle: "Сохранение состояния БД",
			handler: "SetSnapShot",
		});
		setShowConfirm(true);
	};

	const handlerRestoreSnapShot = () => {
		setDateValue(new Date());
		setResetLog(false);
		setConfirmParam({
			confirmText: "Произвести восстановление БД из лога?",
			confirmTitle: "Восстановление БД из лога",
			handler: "RestoreSnapShot",
		});
		setShowConfirm(true);
	};

	useEffect(() => {
		if (confirmResult) {
			//нажали "Ок" в модалке восстановления SnapShot
			if (confirmParam.handler === "RestoreSnapShot") {
				dispatch(
					setEventFlag({
						eventName: "RestoreSnapShot",
						ready: false,
						param: { dateValue: dateValue, resetLog: resetLog },
					})
				);
				const message: Message = {
					message: "Старт восстановления БД из ES...",
					auctionId: "",
					messageType: 0,
				};
				toast((p) => <InfoMessageToast message={message} toastId={p.id} />, {
					duration: 5000,
				});
			}
			//нажали "Ок" в окне создания SnapShot
			if (confirmParam.handler === "SetSnapShot") {
				dispatch(setEventFlag({ eventName: "SetSnapShot", ready: false }));
				const message: Message = {
					message: "Старт создания снимка БД в ES...",
					auctionId: "",
					messageType: 0,
				};
				toast((p) => <InfoMessageToast message={message} toastId={p.id} />, {
					duration: 5000,
				});
			}
		}
		setShowConfirm(false);
		setConfirmResult(undefined);
		// eslint-disable-next-line
	}, [confirmResult]);

	const handleSetDate = (result: Date) => {
		setDateValue(result);
	};

	const handlerResetImageCache = () => {
		dispatch(setEventFlag({ eventName: "ResetImageCache", ready: false }));
		const message: Message = {
			message: "Сброс кеша изобюражений Redis...",
			auctionId: "",
			messageType: 0,
		};
		toast((p) => <InfoMessageToast message={message} toastId={p.id} />, {
			duration: 2000,
		});
	};

	return (
		<>
			<Dropdown inline label={`Здравствуйте ${user.name}`}>
				<Dropdown.Item icon={HiUser} onClick={SetSeller}>
					Мои аукционы
				</Dropdown.Item>
				<Dropdown.Item icon={AiFillTrophy} onClick={SetWinner}>
					Аукционы выигранные
				</Dropdown.Item>
				<NavLink to="/auctions/create">
					<Dropdown.Item icon={RiAuctionFill}>Создать аукцион</Dropdown.Item>
				</NavLink>
				<NavLink to="/finance/list">
					<Dropdown.Item icon={GrMoney}>Финансы</Dropdown.Item>
				</NavLink>
				{user.isAdmin && (
					<>
						<Dropdown.Divider />
						<Dropdown.Item
							icon={GoCodescanCheckmark}
							onClick={handlerElkReindex}
						>
							Elk индексация
						</Dropdown.Item>
						<Dropdown.Item icon={GoDatabase} onClick={handlerSetSnapShot}>
							Выполнить SnapShot Db
						</Dropdown.Item>
						<Dropdown.Item
							icon={FaTrashRestoreAlt}
							onClick={handlerRestoreSnapShot}
						>
							Восстановить Db из SnapShot
						</Dropdown.Item>
						<Dropdown.Item
							icon={RiRestartFill}
							onClick={handlerResetImageCache}
						>
							Сбросить Кеш изображений
						</Dropdown.Item>
					</>
				)}
				<Dropdown.Divider />
				<a href={process.env.REACT_APP_REPORT_URL}>
					<Dropdown.Item icon={HiOutlineDocumentReport}>Отчеты</Dropdown.Item>
				</a>
				<Dropdown.Divider />
				<Dropdown.Item icon={AiOutlineLogout} onClick={handleLogout}>
					Выход
				</Dropdown.Item>
			</Dropdown>
			<ModalConfirm
				openModal={showConfirm}
				text={confirmParam.confirmText}
				title={confirmParam.confirmTitle}
				setResult={setConfirmResult}
				returnData={handleSetDate}
				dateValue={dateValue}
				resetLog={(rezult) => {
					setResetLog(rezult);
				}}
			/>
		</>
	);
}
