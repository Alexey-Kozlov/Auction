import { useEffect, useState } from "react";
import Heading from "../auctionList/Heading";
import CountdownTimer from "../auctionList/CountDownTimer";
import {
	Auction,
	AuctionDeleted,
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

export default function DetailMain() {
	const { id } = useParams();
	const user: User = useSelector((state: RootState) => state.authStore);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const [notifyUser, setNotifyUser] = useState(false);
	const [showConfirm, setShowConfirm] = useState(false);
	const [confirmResult, setConfirmResult] = useState<boolean | null>(null);
	const [deleteAuction, setDeleteAuction] = useState(false);
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

	//для управления переключателем по уведомлениям пользователя - при получении сообщения из БД об изменении
	//переключателя - устанавливаем новое состояние у переключателя и он меняет отображение
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

	useEffect(() => {
		//обновление переключателя рассылки уведомлений
		const eventState = procState.find(
			(p) => p.eventName === "EditNotification" && p.ready && p.lastChanged
		);
		if (eventState) {
			//обновление переключателя
			isNotifyUser.refetch();
			dispatch(setEventFlag({ eventName: "EditNotification", ready: false }));
		}
		//переход на список аукционов при удалении текущего аукциона
		const eventState2 = procState.find(
			(p) => p.eventName === "CollectionChanged" && p.ready && p.lastChanged
		);
		if (eventState2 && deleteAuction) {
			navigate("/");
		}
		// eslint-disable-next-line
	}, [procState]);

	//если не нашли данных по указанному id - переход на страницу "Не найдено"
	useEffect(() => {
		if (!deleteAuction && !data.isLoading && (!data || !data.data!.result)) {
			navigate("/not-found");
		}
		// eslint-disable-next-line
	}, [data]);

	//удаляем аукцион
	const handleDeleteAuction = () => {
		//подтверждение удаления
		setShowConfirm(true);
	};

	//обработчик переключения переключателя по уведомлениям пользователя по событиям данного аукциона
	const handleSetNotifyUser = async (checked: boolean) => {
		dispatch(setEventFlag({ eventName: "EditNotification", ready: false }));
		var notifyUser: NotifyUser = {
			auctionid: id!,
			enable: checked,
			sessionid: sessionId,
		};
		await setNotifyUserApi(notifyUser);
	};

	useEffect(() => {
		const deleteAction = async (val: AuctionDeleted) => {
			await deleteAuctionProc(val);
		};
		if (confirmResult) {
			setDeleteAuction(true);
			dispatch(setEventFlag({ eventName: "AuctionDeleted", ready: false }));
			const auctionDeleted: AuctionDeleted = {
				auctionId: id!,
				correlationId: uuid.v4() as string,
			};
			deleteAction(auctionDeleted);
		}
		setShowConfirm(false);
		setConfirmResult(null);
		// eslint-disable-next-line
	}, [confirmResult]);

	if (data.isLoading) return "Загрузка...";

	return (
		<div className="mt-2 ">
			{auctionDetail && auctionDetail!.sellerName && (
				<>
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
											disabled={!!deleteAuction}
											className="CustomButton mr-2 w-16rem"
										>
											Редактировать аукцион
										</Button>
										<Button
											text
											raised
											rounded
											className="CustomButton w-16rem"
											onClick={handleDeleteAuction}
											loading={!!deleteAuction}
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
										<h3 className="DetailNotifyText">
											Получать уведомления этого аукциона:
										</h3>
										<InputSwitch
											checked={notifyUser}
											onChange={(e) => handleSetNotifyUser(e.checked!)}
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
							<Panel className="BidPanel">
								<BidList user={user} auction={auctionDetail!} />
							</Panel>
						</div>
						<div className="col-12">
							<Panel>
								<DetailedSpec auction={auctionDetail!} user={user} />
							</Panel>
						</div>

						<Button
							text
							raised
							rounded
							className="CustomButton"
							onClick={() => navigate(-1)}
						>
							Назад
						</Button>
					</div>
				</>
			)}
		</div>
	);
}
