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
import { Button } from "flowbite-react";
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
	const { data, isLoading } = useGetDetailedViewDataQuery(id!, {
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

	//удаляем аукцион
	const handleDeleteAuction = async () => {
		setDeleteAuction(true);
		dispatch(setEventFlag({ eventName: "AuctionDeleted", ready: false }));
		const auctionDeleted: AuctionDeleted = {
			id: uuid.v4() as string,
			auctionId: id!,
			correlationId: uuid.v4() as string,
		};
		await deleteAuctionProc(auctionDeleted);
	};

	//инициализация данных
	useEffect(() => {
		if (!isLoading && data?.isSuccess && data?.result && data?.result.title) {
			setAuctionDetail(data!.result);
		}
	}, [isLoading, data]);

	//для управления переключателем по уведомлениям пользователя - при получении сообщения из БД об изменении
	//переключателя - устанавливаем новое состояние у переключателя и он меняет отображение
	useEffect(() => {
		if (!isNotifyUser.isLoading && isNotifyUser.data) {
			setNotifyUser(isNotifyUser.data!.result!);
		}
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
	}, [procState, isNotifyUser, dispatch]);

	//переход на список аукционов при удалении текущего аукциона
	useEffect(() => {
		const eventState = procState.find(
			(p) => p.eventName === "CollectionChanged" && p.ready
		);
		if (eventState && deleteAuction) {
			navigate("/");
		}
	}, [procState, navigate, deleteAuction]);

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

	//если не нашли данных по указанному id - переход на страницу "Не найдено"
	useEffect(() => {
		if (!deleteAuction && !isLoading && (!data || !data.result)) {
			navigate("/not-found");
		}
	}, [isLoading, deleteAuction, data, navigate]);

	//для сохранения идентификатора запроса в логе
	useEffect(() => {
		setCookie("RequestType", RequestType[RequestType.ReadDetail]);
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

	if (isLoading) return "Загрузка...";

	return (
		<div>
			{auctionDetail && (
				<>
					<div className="flex justify-between">
						<div className="flex items-center gap-3">
							<Heading title={`${auctionDetail?.title}`} />
							{user?.login === auctionDetail?.seller && (
								<>
									<Button outline>
										<NavLink to={`/auctions/edit/${id}`}>
											Редактировать аукцион
										</NavLink>
									</Button>
									<Button
										isProcessing={!!deleteAuction}
										outline
										onClick={handleDeleteAuction}
									>
										Удалить аукцион
									</Button>
								</>
							)}
						</div>

						<div>
							<div className="flex gap-3 justify-end">
								<h3 className="text-2xl font-semibold">Осталось времени:</h3>
								<CountdownTimer auctionEnd={auctionDetail!.auctionEnd} />
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
					<div className="grid grid-cols-[750px_1fr] gap-6 mt-3">
						<div className="flex items-center justify-center rounded-lg">
							<ImageCard
								id={auctionDetail!.auctionId}
								zooming={true}
								cache={false}
							/>
						</div>
						<BidList user={user} auction={auctionDetail!} />
					</div>
					<div className="mt-3 grid grid-cols-1 rounded-lg">
						<DetailedSpecs auction={auctionDetail!} />
					</div>

					<div className="flex justify-center mt-2">
						<Button outline onClick={() => navigate(-1)} className="mb-2">
							Назад
						</Button>
					</div>
				</>
			)}
		</div>
	);
}
