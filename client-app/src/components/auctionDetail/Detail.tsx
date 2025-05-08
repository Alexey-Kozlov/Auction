import { useEffect, useState } from "react";
import Heading from "../auctionList/Heading";
import CountdownTimer from "../auctionList/CountDownTimer";
import {
	Auction,
	AuctionDeleted,
	NotifyUser,
	ProcessingState,
	RequestType,
	User,
} from "../../store/types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { NavLink, useNavigate, useParams } from "react-router-dom";
import ImageCard from "../auctionList/ImageCard";
import BidList from "./BidList";
import DetailedSpecs from "./DetailedSpec";
import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
import { useIsNotifyUserQuery } from "../../api/NotificationApi";
import { setEventFlag } from "../../store/processingSlice";
import {
	useDeleteAuctionMutation,
	useSetNotifyUserMutation,
} from "../../api/ProcessingApi";
import uuid from "react-native-uuid";
import SwitchInput from "../inputComponents/SwitchInput";
import { useCookies } from "react-cookie";
import { v4 as uuidv4 } from "uuid";
import { Button } from "semantic-ui-react";

export default function Detail() {
	const { id } = useParams();
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
	const user: User = useSelector((state: RootState) => state.authStore);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const [notifyUser, setNotifyUser] = useState(false);
	const [deleteAuction, setDeleteAuction] = useState(false);
	const [auctionDetail, setAuctionDetail] = useState<Auction>();
	const data = useGetDetailedViewDataQuery(id!, {
		skip: auctionDetail?.title === "",
	});
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
		if (
			!data.isLoading &&
			!data.isFetching &&
			data.data?.result &&
			data.data?.result.title
		) {
			setAuctionDetail(data.data!.result);
		}
		// eslint-disable-next-line
	}, [data]);

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

	//обновление переключателя рассылки уведомлений
	useEffect(() => {
		const eventState = procState.find(
			(p) => p.eventName === "EditNotification"
		);
		if (eventState && eventState.ready) {
			//обновление переключателя
			isNotifyUser.refetch();
			dispatch(setEventFlag({ eventName: "EditNotification", ready: false }));
		}
		// eslint-disable-next-line
	}, [procState]);

	//переход на список аукционов при удалении текущего аукциона
	useEffect(() => {
		const eventState = procState.find(
			(p) => p.eventName === "CollectionChanged" && p.ready
		);
		if (eventState && deleteAuction) {
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

	//для сохранения идентификатора запроса в логе
	useEffect(() => {
		setCookie("RequestType", RequestType[RequestType.ReadDetail]);
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

	//удаляем аукцион
	const handleDeleteAuction = async () => {
		setDeleteAuction(true);
		setCookie("RequestType", RequestType[RequestType.Delete]);
		dispatch(setEventFlag({ eventName: "AuctionDeleted", ready: false }));
		const auctionDeleted: AuctionDeleted = {
			id: uuid.v4() as string,
			auctionId: id!,
			correlationId: uuid.v4() as string,
		};
		await deleteAuctionProc(auctionDeleted);
	};

	//обработчик переключения переключателя по уведомлениям пользователя по событиям данного аукциона
	const handleSetNotifyUser = async (
		event: React.FormEvent<HTMLInputElement>
	) => {
		dispatch(setEventFlag({ eventName: "EditNotification", ready: false }));
		var notifyUser: NotifyUser = {
			auctionid: id!,
			enable: event.currentTarget.checked,
			sessionid: sessionId,
		};
		await setNotifyUserApi(notifyUser);
	};

	if (data.isLoading) return "Загрузка...";

	return (
		<div>
			{auctionDetail && (
				<>
					<div className="DetailContainer">
						<div className="DetailItem">
							<Heading title={`${auctionDetail?.title}`} />
							{user?.login === auctionDetail?.seller && (
								<>
									<Button>
										<NavLink to={`/auctions/edit/${id}`}>
											Редактировать аукцион
										</NavLink>
									</Button>
									<Button
										isProcessing={!!deleteAuction}
										onClick={handleDeleteAuction}
									>
										Удалить аукцион
									</Button>
								</>
							)}
						</div>

						<div>
							<div className="DetailCountDown">
								<h3 className="DetailCountDownItem">Осталось времени:</h3>
								<CountdownTimer
									auctionEnd={auctionDetail!.auctionEnd}
									isFinished={auctionDetail.finished}
								/>
							</div>
							{user.name && (
								<div>
									<label className="inline-flex items-center mb-5">
										<h3 className="text-2xl font-semibold mr-3">
											Получать уведомления этого аукциона:
										</h3>
										<SwitchInput
											checked={notifyUser}
											handleImageUsing={handleSetNotifyUser}
										/>
									</label>
								</div>
							)}
						</div>
					</div>
					<div className="DetailImageContainer">
						<div className="DetailImage">
							<ImageCard
								id={auctionDetail!.auctionId}
								zooming={true}
								cache={false}
							/>
						</div>
						<BidList user={user} auction={auctionDetail!} />
					</div>
					<div className="DetailSpec">
						<DetailedSpecs auction={auctionDetail!} />
					</div>

					<div className="DetailBottom">
						<Button outline onClick={() => navigate(-1)} className="mb-2">
							Назад
						</Button>
					</div>
				</>
			)}
		</div>
	);
}
