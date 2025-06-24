import { useEffect, useState } from "react";
import Heading from "../auctionList/Heading";
import CountdownTimer from "../auctionList/CountDownTimer";
import {
	Auction,
	AuctionDeleted,
	ModalTypes,
	NotifyUser,
	ProcessingState,
	User,
} from "../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { useNavigate, useParams } from "react-router-dom";
import ImageCard from "../auctionList/ImageCard";
import BidList from "./BidList";
import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
import { useIsNotifyUserQuery } from "../../api/NotificationApi";
import { setEventFlag } from "../../store/processingSlice";
import {
	useDeleteAuctionMutation,
	useSetNotifyUserMutation,
} from "../../api/ProcessingApi";
import uuid from "react-native-uuid";
import { Button } from "primereact/button";
import { InputSwitch } from "primereact/inputswitch";
import { Panel } from "primereact/panel";
import DetailedSpec from "./DetailedSpec";
import { useGetUserNameQuery } from "../../api/AuthApi";
import Waiter from "../Waiter";
import Footer from "../layout/Footer";
import ModalYesNo from "../modals/ModalYesNo";

export default function DetailMain() {
	const { id } = useParams();
	const user: User = useSelector((state: RootState) => state.authStore);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const [notifyUser, setNotifyUser] = useState(false);
	const [showConfirmDelete, setShowConfirmDelete] = useState(false);
	const [auctionDetail, setAuctionDetail] = useState<Auction | null>(null);
	const data = useGetDetailedViewDataQuery(id!, {
		skip: auctionDetail?.title === "",
	});
	const userSeller = useGetUserNameQuery(
		auctionDetail ? auctionDetail.seller : "",
		{
			skip: auctionDetail?.seller === "",
		}
	);
	const isNotifyUser = useIsNotifyUserQuery(id!, {
		skip: user.login === "" || user.login === undefined,
	});
	const [setNotifyUserApi] = useSetNotifyUserMutation();
	const [deleteAuctionProc] = useDeleteAuctionMutation();
	const navigate = useNavigate();
	const dispatch = useDispatch();
	const sessionId = useSelector(
		(state: RootState) => state.paramStore
	).sessionId;

	//инициализация данных
	useEffect(() => {
		if (!data.isLoading && !data.isFetching && data.data!.result) {
			setAuctionDetail(data.data!.result);
		}
		// eslint-disable-next-line
	}, [data]);

	//добавляем наименование автора аукциона для отображения
	useEffect(() => {
		if (
			!userSeller.isLoading &&
			!userSeller.isFetching &&
			userSeller.data?.result &&
			auctionDetail &&
			!auctionDetail.sellerName
		) {
			setAuctionDetail((prev) => {
				return { ...prev, sellerName: userSeller.data?.result } as Auction;
			});
		}
		// eslint-disable-next-line
	}, [userSeller, auctionDetail]);

	// для управления переключателем по уведомлениям пользователя - при получении сообщения из
	// БД об изменении переключателя - устанавливаем новое состояние у переключателя и
	// он меняет отображение
	useEffect(() => {
		if (
			!isNotifyUser.isLoading &&
			!isNotifyUser.isFetching &&
			isNotifyUser.data
		) {
			setNotifyUser(isNotifyUser.data!.result!);
		}
		// eslint-disable-next-line
	}, [isNotifyUser]);

	//отслеживаем сообщения от бекенда
	useEffect(() => {
		//обновление переключателя рассылки уведомлений
		const eventState = procState.find(
			(p) => p.eventName === "EditNotification" && p.ready
		);
		if (eventState) {
			//обновление переключателя
			isNotifyUser.refetch();
			dispatch(setEventFlag({ eventName: "EditNotification", ready: false }));
		}

		//переход на список аукционов при удалении текущего аукциона
		const eventState2 = procState.find(
			(p) => p.eventName === "AuctionDeleted" && p.ready
		);
		if (eventState2) {
			navigate("/");
		}
		// eslint-disable-next-line
	}, [procState]);

	//если не нашли данных по указанному id - переход на страницу "Не найдено"
	useEffect(() => {
		if (!data.isLoading && (!data || !data.data!.result)) {
			navigate("/not-found");
		}
		// eslint-disable-next-line
	}, [data]);

	//удаляем аукцион
	const handleDeleteAuction = () => {
		//подтверждение удаления
		setShowConfirmDelete(true);
	};

	//обработчик переключения переключателя уведомлений пользователя по событиям данного аукциона
	const handleSetNotifyUser = async (checked: boolean) => {
		dispatch(setEventFlag({ eventName: "WaiterHideNotify", ready: false }));
		dispatch(setEventFlag({ eventName: "EditNotification", ready: false }));
		var notifyUser: NotifyUser = {
			auctionid: id!,
			enable: checked,
			sessionid: sessionId,
		};
		await setNotifyUserApi(notifyUser);
	};

	const acceptDeleteDialog = () => {
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
		const auctionDeleted: AuctionDeleted = {
			auctionId: id!,
			correlationId: uuid.v4() as string,
		};
		deleteAuctionProc(auctionDeleted);
		dispatch(setEventFlag({ eventName: "AuctionDeleted", ready: false }));
	};

	const rejectDeleteDialog = () => {
		//отмена удаления сообщения
		setShowConfirmDelete(false);
	};

	if (data.isLoading) return "Загрузка...";

	return (
		<div>
			{procState.find((p) => p.eventName === "WaiterHide") &&
			!procState.find((p) => p.eventName === "WaiterHide")!.ready ? (
				<Waiter />
			) : (
				<></>
			)}
			{auctionDetail && auctionDetail!.sellerName && (
				<div className="overflow-hidden">
					<div className="grid">
						<div className="col-6 CenterItem">
							<div className="CenterItem flex-column w-full">
								<Heading title={`${auctionDetail!.title}`} />
								{user?.login === auctionDetail!.seller && (
									<div className="flex w-auto">
										<Button
											text
											raised
											rounded
											onClick={() => navigate(`/auctions/edit/${id}`)}
											className="CustomButton mr-4 w-30rem"
										>
											Редактировать аукцион
										</Button>
										<Button
											text
											raised
											rounded
											className="CustomButton w-30rem"
											onClick={handleDeleteAuction}
										>
											Удалить аукцион
										</Button>
									</div>
								)}
							</div>
						</div>
						<div className="col-6">
							<Panel>
								<div className="CenterItem">
									<h3 className="DetailCountDownText">Осталось времени:</h3>
									<div className="DetailCountDownItem">
										<CountdownTimer
											auctionEnd={auctionDetail!.auctionEnd}
											isFinished={auctionDetail.finished}
										/>
									</div>
								</div>

								{user.name && (
									<div className="CenterItem">
										{procState.find(
											(p) => p.eventName === "WaiterHideNotify"
										) &&
										!procState.find((p) => p.eventName === "WaiterHideNotify")!
											.ready ? (
											<Waiter />
										) : (
											<></>
										)}
										<h3 className="DetailNotifyText">
											Получать уведомления этого аукциона:
										</h3>
										<InputSwitch
											checked={notifyUser}
											onChange={(e) => handleSetNotifyUser(e.value)}
										/>
									</div>
								)}
							</Panel>
						</div>
						<div className="col-6 CenterItem">
							<ImageCard
								id={auctionDetail!.auctionId}
								detail={true}
								cache={false}
							/>
						</div>
						<div className="col-6">
							<Panel>
								<BidList user={user} auction={auctionDetail!} />
							</Panel>
						</div>
						<div className="col-12">
							<Panel>
								<DetailedSpec auction={auctionDetail!} user={user} />
							</Panel>
						</div>
					</div>
				</div>
			)}
			<ModalYesNo
				accept={acceptDeleteDialog}
				reject={rejectDeleteDialog}
				header="Подтверждение удаления аукциона"
				label={
					"Действительно удалить аукцион - ' " + auctionDetail?.title + "'?"
				}
				visible={showConfirmDelete}
				group="deleteAuction"
				modalType={ModalTypes.warning}
			/>
			<Footer />
		</div>
	);
}
