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
} from "../../types";
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
import { useCookies } from "react-cookie";
import { v4 as uuidv4 } from "uuid";
import {
	Button,
	Checkbox,
	Grid,
	GridColumn,
	GridRow,
	Segment,
} from "semantic-ui-react";

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
	const handleSetNotifyUser = async (checked: boolean) => {
		dispatch(setEventFlag({ eventName: "EditNotification", ready: false }));
		var notifyUser: NotifyUser = {
			auctionid: id!,
			enable: checked,
			sessionid: sessionId,
		};
		await setNotifyUserApi(notifyUser);
	};

	if (data.isLoading) return "Загрузка...";

	return (
		<div className="mt-10 ">
			{auctionDetail && (
				<>
					<Grid columns={2} divided>
						<GridRow>
							<GridColumn textAlign="center" verticalAlign="middle">
								<Heading title={`${auctionDetail?.title}`} />
								{user?.login === auctionDetail?.seller && (
									<>
										<Button
											className="MainButton w-200"
											onClick={() => navigate(`/auctions/edit/${id}`)}
										>
											Редактировать аукцион
										</Button>
										<Button
											className="MainButton w-200"
											onClick={handleDeleteAuction}
											loading={!!deleteAuction}
										>
											Удалить аукцион
										</Button>
									</>
								)}
							</GridColumn>
							<GridColumn>
								<Segment>
									<div>
										<div className="DetailCountDown">
											<h3 className="DetailCountDownItem">Осталось времени:</h3>
											<CountdownTimer
												auctionEnd={auctionDetail!.auctionEnd}
												isFinished={auctionDetail.finished}
											/>
										</div>
										{user.name && (
											<div className="DetailNotify">
												<h3 className="DetailNotifyText">
													Получать уведомления этого аукциона:
												</h3>
												<Checkbox
													toggle
													checked={notifyUser}
													onChange={(e, data) =>
														handleSetNotifyUser(data.checked!)
													}
												/>
											</div>
										)}
									</div>
								</Segment>
							</GridColumn>
						</GridRow>
						<GridRow>
							<GridColumn>
								<div className="DetailImage">
									<ImageCard
										id={auctionDetail!.auctionId}
										zooming={true}
										cache={false}
									/>
								</div>
							</GridColumn>
							<GridColumn>
								<Segment className="mt-0 h-100">
									<BidList user={user} auction={auctionDetail!} />
								</Segment>
							</GridColumn>
						</GridRow>
					</Grid>

					<Segment>
						<div className="DetailSpec">
							<DetailedSpecs auction={auctionDetail!} />
						</div>
					</Segment>
					<div className="DetailBottom">
						<Button className="MainButton" onClick={() => navigate(-1)}>
							Назад
						</Button>
					</div>
				</>
			)}
		</div>
	);
}
