import { useEffect, useState } from "react";
import {
	ActionType,
	Auction,
	ChatComment,
	ProcessingState,
	User,
} from "../../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/store";
import uuid from "react-native-uuid";
import { setChatMessage } from "../../../store/chatSlice";
import { useGetCommunicationItemsQuery } from "../../../api/CommunicationApi";
import ChatUser from "./ChatUser";
import { setEventFlag } from "../../../store/processingSlice";
import Waiter from "../../Waiter";
import { DataTable } from "primereact/datatable";

type Props = {
	auction: Auction;
	user: User;
};

export default function ChatTable({ auction, user }: Props) {
	const [communicationItems, setCommunicationItems] = useState<ChatComment[]>(
		[]
	);
	const communication = useGetCommunicationItemsQuery(auction.auctionId);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const params = useSelector((state: RootState) => state.paramStore);
	const chatResponse = useSelector(
		(state: RootState) => state.chatResponseStore
	);
	const [newMessage, setNewMessage] = useState<ChatComment>({
		id: "",
		message: "",
		parentId: "",
		userLogin: "",
		auctionId: "",
		sessionId: "",
	} as ChatComment);

	const dispatch = useDispatch();
	const [chatPanelOpen, setChatPanelOpen] = useState(false);
	const handleMessageChanged = (value: string | number | undefined) => {
		setNewMessage((prev) => {
			return {
				...prev,
				message: value ? value.toString() : "",
				id: uuid.v4() as string,
				parentId: "",
				userLogin: user.login,
				auctionId: auction.auctionId,
				sessionId: params.sessionId!,
			};
		});
	};

	const handleMessageSubmit = () => {
		//обновляем новое сообщение в хранилище для инициации посылки на сервер через SignalR
		dispatch(setChatMessage(newMessage));
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
	};

	//отслеживаем обновления данных чата и обновляем отображение при изменениях
	useEffect(() => {
		if (
			!communication.isFetching &&
			!communication.isLoading &&
			communication.data
		) {
			setCommunicationItems(communication.data.result);
		}
	}, [communication]);

	//изменилось хранилище ответов чата - обновляем состояние набора записей чата
	useEffect(() => {
		switch (chatResponse.action) {
			case ActionType.create:
				const newChatMessage: ChatComment = {
					auctionId: chatResponse.auctionId,
					id: chatResponse.id,
					message: chatResponse.message,
					parentId: chatResponse.parentId,
					sessionId: "",
					updateAt: chatResponse.updateAt,
					userLogin: chatResponse.userLogin,
				};
				setCommunicationItems((prev) => [...prev, newChatMessage]);
				break;
			case ActionType.delete:
				setCommunicationItems((prev) =>
					prev.filter((p) => p.id !== chatResponse.id)
				);
				break;
			case ActionType.update:
				const updateChatMessage: ChatComment = {
					auctionId: chatResponse.auctionId,
					id: chatResponse.id,
					message: chatResponse.message,
					parentId: chatResponse.parentId,
					sessionId: "",
					updateAt: chatResponse.updateAt,
					userLogin: chatResponse.userLogin,
				};
				setCommunicationItems((prev) => [
					...prev.filter((p) => p.id !== chatResponse.id),
					updateChatMessage,
				]);
				break;
		}
	}, [chatResponse]);

	return (
		<>
			<DataTable></DataTable>
		</>
	);
}
