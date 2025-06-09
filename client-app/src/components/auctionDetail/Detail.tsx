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
import DetailedSpecs from "./DetailedSpec";
import { useGetDetailedViewDataQuery } from "../../api/AuctionApi";
import { useIsNotifyUserQuery } from "../../api/NotificationApi";
import { setEventFlag } from "../../store/processingSlice";
import {
	useDeleteAuctionMutation,
	useSetNotifyUserMutation,
} from "../../api/ProcessingApi";
import uuid from "react-native-uuid";
import ModalConfirm from "../modals/ModalConfirm";
import { Button } from "primereact/button";

export default function Detail() {
	const { id } = useParams();
	const user: User = useSelector((state: RootState) => state.authStore);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const [notifyUser, setNotifyUser] = useState(false);
	const [showConfirm, setShowConfirm] = useState(false);
	const [confirmResult, setConfirmResult] = useState<boolean | null>(null);
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
				id: uuid.v4() as string,
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
		<div className="mt-10 ">
			<ModalConfirm
				openModal={showConfirm}
				text={"Действительно удалить аукцион '" + auctionDetail?.title + "' ?"}
				title={"Подтверждение удаления аукциона"}
				setResult={setConfirmResult}
			/>
			{auctionDetail && (
				<>
					<div></div>
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
