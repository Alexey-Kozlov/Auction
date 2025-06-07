import { useEffect, useRef, useState } from "react";
import {
	Form,
	FormTextArea,
	Menu,
	Popup,
	Table,
	TableBody,
	TableCell,
	TableRow,
} from "semantic-ui-react";
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
	const contextRef: any = useRef();
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
			<Table striped>
				<TableBody>
					<TableRow>
						<TableCell colSpan="2">
							<Form>
								<FormTextArea
									className="InputText"
									placeholder="Сообщение"
									value={newMessage.message}
									onKeyDown={(e: KeyboardEvent) => {
										if (e.key === "Enter" && e.shiftKey) return;
										if (e.key === "Enter") {
											e.preventDefault();
											handleMessageSubmit();
										}
									}}
									onChange={(e, data) => handleMessageChanged(data.value)}
								/>
							</Form>
						</TableCell>
					</TableRow>
					<TableRow>
						<TableCell>Пользователь</TableCell>
						<TableCell>Сообщение</TableCell>
					</TableRow>
					{procState.find((p) => p.eventName === "WaiterHide") &&
					!procState.find((p) => p.eventName === "WaiterHide")!.ready ? (
						<TableRow>
							<TableCell>
								<Waiter color="rgb(156 163 175)" />
							</TableCell>
						</TableRow>
					) : (
						communicationItems &&
						communicationItems.map((item, index) => (
							<TableRow
								key={index}
								onContextMenu={(e: any) => {
									if (
										Boolean(user!.isAdmin) ||
										user!.login === item.userLogin
									) {
										e.preventDefault();
										contextRef.current = createContextFromEvent(e);
										setChatPanelOpen(true);
									}
								}}
							>
								<TableCell>
									<ChatUser key={index} userLogin={item.userLogin} />
								</TableCell>
								<TableCell>{item.message}</TableCell>
							</TableRow>
						))
					)}
				</TableBody>
			</Table>

			<Popup
				basic
				context={contextRef}
				onClose={() => setChatPanelOpen(false)}
				open={chatPanelOpen}
			>
				<Menu
					items={[
						{ key: "edit", content: "Редактировать", icon: "edit" },
						{ key: "delete", content: "Удалить", icon: "delete" },
					]}
					onItemClick={(event, data) => {
						setChatPanelOpen(false);
					}}
					secondary
					vertical
				/>
			</Popup>
		</>
	);

	function createContextFromEvent(e: any) {
		const left = e.clientX;
		const top = e.clientY;
		const right = left + 1;
		const bottom = top + 1;
		return {
			getBoundingClientRect: () => ({
				left,
				top,
				right,
				bottom,
				height: 0,
				width: 0,
			}),
		};
	}
}
