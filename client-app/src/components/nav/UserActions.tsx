import { AiFillTrophy, AiOutlineLogout } from "react-icons/ai";
import { FaTrashRestoreAlt } from "react-icons/fa";
import { RiRestartFill } from "react-icons/ri";
import { RiAuctionFill } from "react-icons/ri";
import { HiUser } from "react-icons/hi2";
import { GoCodescanCheckmark, GoDatabase } from "react-icons/go";
import { GrMoney } from "react-icons/gr";
import { HiOutlineDocumentReport } from "react-icons/hi";
import { useLocation, useNavigate } from "react-router-dom";
import { ModalParams, ToastType, User } from "../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { emptyUserState, setAuthUser } from "../../store/authSlice";
import { setParams } from "../../store/paramSlice";
import { setEventFlag } from "../../store/processingSlice";
import toast from "react-hot-toast";
import { useEffect, useState } from "react";
import { useLogoutUserMutation } from "../../api/AuthApi";
import {
	Dropdown,
	DropdownDivider,
	DropdownHeader,
	DropdownItem,
	DropdownMenu,
} from "semantic-ui-react";
import MessageToast from "../signalRNotifications/MessageToast";
import ModalConfirm from "../modals/ModalConfirm";
import ModalRestore from "../modals/ModalRestore";

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
	const [showConfirm, setShowConfirm] = useState(false);
	const [showConfirmRestore, setShowConfirmRestore] = useState(false);
	const [dateValue, setDateValue] = useState<Date | null>(new Date());
	const [confirmResult, setConfirmResult] = useState<boolean | undefined>(
		undefined
	);
	const [resetLog, setResetLog] = useState(false);
	const [logoutUser] = useLogoutUserMutation();

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
				toast(
					(p) => (
						<MessageToast
							message={"Старт восстановления БД из ES..."}
							toastId={p.id}
							toastType={ToastType.Info}
						/>
					),
					{
						duration: 5000,
					}
				);
			}
			//нажали "Ок" в окне создания SnapShot
			if (confirmParam.handler === "SetSnapShot") {
				dispatch(setEventFlag({ eventName: "SetSnapShot", ready: false }));
				toast(
					(p) => (
						<MessageToast
							message={"Старт создания снимка БД в ES..."}
							toastId={p.id}
							toastType={ToastType.Info}
						/>
					),
					{
						duration: 5000,
					}
				);
			}
		}
		setShowConfirm(false);
		setShowConfirmRestore(false);
		setConfirmResult(undefined);
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

	const handlerElkReindexClick = () => {
		dispatch(setEventFlag({ eventName: "ElkIndex", ready: false }));
		return toast(
			(p) => (
				<MessageToast
					message={"Старт переиндексации ELK..."}
					toastId={p.id}
					toastType={ToastType.Info}
				/>
			),
			{
				duration: 5000,
			}
		);
	};

	const handlerSetSnapShotClick = () => {
		setDateValue(null);
		setConfirmParam({
			confirmText: "Произвести сохранение состояния БД?",
			confirmTitle: "Сохранение состояния БД",
			handler: "SetSnapShot",
		});
		setShowConfirm(() => true);
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

	const handlerResetImageCacheClick = () => {
		dispatch(setEventFlag({ eventName: "ResetImageCache", ready: false }));
		toast(
			(p) => (
				<MessageToast
					message={"Сброс кеша изобюражений Redis..."}
					toastId={p.id}
					toastType={ToastType.Info}
				/>
			),
			{
				duration: 2000,
			}
		);
	};

	const handleFinanceClick = () => {
		navigate("/finance/list");
	};

	const handleReportClick = () => {
		navigate(process.env.REACT_APP_REPORT_URL!);
	};

	const handleCreateAuctionClick = () => {
		navigate("/auctions/create");
	};

	return (
		<div className="UserActionsPanel">
			<Dropdown
				className="UserTitle"
				labeled
				text={`Здравствуйте ${user.name}`}
			>
				<DropdownMenu>
					<DropdownHeader content="Выберите действие:" />
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<HiUser className="MenuItems" size={20} />
									Мои аукционы
								</span>
							</>
						}
						onClick={handleSetSellerClick}
					/>
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<AiFillTrophy className="MenuItems" size={20} />
									Аукционы выигранные
								</span>
							</>
						}
						onClick={handleSetWinnerClick}
					/>
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<RiAuctionFill className="MenuItems" size={20} />
									Создать аукцион
								</span>
							</>
						}
						onClick={handleCreateAuctionClick}
					/>
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<GrMoney className="MenuItems" size={20} />
									Финансы
								</span>
							</>
						}
						onClick={handleFinanceClick}
					/>
					{user.isAdmin && (
						<>
							<DropdownDivider />
							<DropdownItem
								content={
									<>
										<span className="MenuItemsText">
											<GoCodescanCheckmark className="MenuItems" size={20} />
											Elk индексация
										</span>
									</>
								}
								onClick={handlerElkReindexClick}
							/>
							<DropdownItem
								content={
									<>
										<span className="MenuItemsText">
											<GoDatabase className="MenuItems" size={20} />
											Создать SnapShot
										</span>
									</>
								}
								onClick={handlerSetSnapShotClick}
							/>
							<DropdownItem
								content={
									<>
										<span className="MenuItemsText">
											<FaTrashRestoreAlt className="MenuItems" size={20} />
											Восстановить из SnapShot
										</span>
									</>
								}
								onClick={handlerRestoreSnapShotClick}
							/>
							<DropdownItem
								content={
									<>
										<span className="MenuItemsText">
											<RiRestartFill className="MenuItems" size={20} />
											Сбросить Кеш изображений
										</span>
									</>
								}
								onClick={handlerResetImageCacheClick}
							/>
						</>
					)}
					<DropdownDivider />
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<HiOutlineDocumentReport className="MenuItems" size={20} />
									Отчеты
								</span>
							</>
						}
						onClick={handleReportClick}
					/>
					<DropdownItem
						content={
							<>
								<span className="MenuItemsText">
									<AiOutlineLogout className="MenuItems" size={20} />
									Выход
								</span>
							</>
						}
						onClick={handleLogoutClick}
					/>
				</DropdownMenu>
			</Dropdown>
			<ModalConfirm
				openModal={showConfirm}
				text={confirmParam.confirmText}
				title={confirmParam.confirmTitle}
				setResult={setConfirmResult}
			/>
			<ModalRestore
				openModal={showConfirmRestore}
				text={confirmParam.confirmText}
				title={confirmParam.confirmTitle}
				setResult={setConfirmResult}
				returnData={handleSetDate}
				dateValue={dateValue}
				resetLog={(rezult) => {
					setResetLog(rezult);
				}}
			/>
		</div>
	);
}
