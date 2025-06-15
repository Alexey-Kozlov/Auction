import { useEffect, useRef, useState } from "react";
import {
	ActionType,
	Auction,
	ChatComment,
	ModalTypes,
	ProcessingState,
	SessionType,
	SortDirection,
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
import { InputTextarea } from "primereact/inputtextarea";
import { DynamicSort } from "../../../utils/DynamicSort";
import { ScrollPanel } from "primereact/scrollpanel";
import { ContextMenu } from "primereact/contextmenu";
import { MenuItem } from "primereact/menuitem";
import ModalEditText from "../../modals/ModalEditText";
import ModalYesNo from "../../modals/ModalYesNo";

type Props = {
	auction: Auction;
	user: User;
};

export default function TabChatTable({ auction, user }: Props) {
	const cm = useRef<ContextMenu>(null);
	const [communicationItems, setCommunicationItems] = useState<ChatComment[]>(
		[]
	);
	const [chatSelected, setChatSelected] = useState<ChatComment | null>();
	const [showConfirmEditDialog, setShowConfirmEditDialog] = useState(false);
	const [showConfirmDeleteDialog, setShowConfirmDeleteDialog] = useState(false);
	const communication = useGetCommunicationItemsQuery(auction.auctionId);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);

	const chatResponse = useSelector(
		(state: RootState) => state.chatResponseStore
	);
	const [newMessage, setNewMessage] = useState<ChatComment>({
		itemId: uuid.v4() as string,
		message: "",
		parentId: "",
		userLogin: "",
		auctionId: "",
		sessionId: SessionType[SessionType.all],
	} as ChatComment);

	const dispatch = useDispatch();
	const handleMessageChanged = (value: string | number | undefined) => {
		setNewMessage((prev) => {
			return {
				...prev,
				message: value ? value.toString() : "",
				itemId: uuid.v4() as string,
				parentId: "",
				userLogin: user.login,
				auctionId: auction.auctionId,
			};
		});
	};

	const handleMessageSubmit = () => {
		// обновляем новое сообщение в хранилище для инициации посылки на сервер через SignalR
		// начало процесса создания нового сообщения - в UseEffect свойства "messageChat" в SignalRProvider
		if (!newMessage.message) return;
		let createMessage = newMessage;
		createMessage.actionType = ActionType.create;
		dispatch(setChatMessage(createMessage));
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
	};

	//отслеживаем обновления данных чата и обновляем отображение при изменениях
	useEffect(() => {
		if (
			!communication.isFetching &&
			!communication.isLoading &&
			communication.data
		) {
			let _temp = JSON.parse(
				JSON.stringify(communication.data.result)
			) as ChatComment[];
			setCommunicationItems(
				_temp?.sort(DynamicSort("updateAt", SortDirection.descending))
			);
		}
	}, [communication]);

	//изменилось хранилище ответов чата - обновляем состояние набора записей чата
	useEffect(() => {
		switch (chatResponse.actionType) {
			case ActionType.create:
				const newChatMessage: ChatComment = {
					auctionId: chatResponse.auctionId,
					itemId: chatResponse.itemId,
					message: chatResponse.message,
					parentId: chatResponse.parentId,
					sessionId: "",
					updateAt: chatResponse.updateAt,
					userLogin: chatResponse.userLogin,
					actionType: ActionType.create,
				};
				setCommunicationItems((prev) => [...prev, newChatMessage]);
				setCommunicationItems((prev) => {
					let _temp = JSON.parse(JSON.stringify(prev)) as ChatComment[];
					return _temp?.sort(DynamicSort("updateAt", SortDirection.descending));
				});
				break;
			case ActionType.delete:
				setCommunicationItems((prev) => {
					const dd = prev;
					return prev.filter((p) => p.itemId !== chatResponse.itemId);
				});
				break;
			case ActionType.update:
				const updateChatMessage: ChatComment = {
					auctionId: chatResponse.auctionId,
					itemId: chatResponse.itemId,
					message: chatResponse.message,
					parentId: chatResponse.parentId,
					sessionId: "",
					updateAt: chatResponse.updateAt,
					userLogin: chatResponse.userLogin,
					actionType: ActionType.update,
				};
				setCommunicationItems((prev) => [
					...prev.filter((p) => p.itemId !== chatResponse.itemId),
					updateChatMessage,
				]);
				setCommunicationItems((prev) => {
					let _temp = JSON.parse(JSON.stringify(prev)) as ChatComment[];
					return _temp?.sort(DynamicSort("updateAt", SortDirection.descending));
				});
				break;
		}
		//сбрасываем введенное сообщение (если было)
		setNewMessage((prev) => {
			return { ...prev, message: "" };
		});
	}, [chatResponse]);

	const contextItems: MenuItem[] = [
		{
			label: "Редактировать",
			icon: "pi pi-file-edit",
			command: () => setShowConfirmEditDialog(true),
		},
		{
			label: "Удалить",
			icon: "pi pi-trash",
			command: () => setShowConfirmDeleteDialog(true),
		},
	];

	const acceptEditDialog = () => {
		//подтвердили редактирование сообщения
		let updateMessage = chatSelected!;
		updateMessage.actionType = ActionType.update;
		updateMessage.sessionId = SessionType[SessionType.all];
		setShowConfirmEditDialog(false);
		dispatch(setChatMessage(updateMessage));
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
	};

	const rejectEditDialog = () => {
		//отмена редактирования сообщения
		setShowConfirmEditDialog(false);
	};

	const acceptDeleteDialog = () => {
		// удаляем сообщение через SignalR
		// начало процесса создания нового сообщения - в UseEffect свойства "messageChat" в SignalRProvider
		let deleteMessage = chatSelected!;
		deleteMessage.sessionId = SessionType[SessionType.all];
		deleteMessage.actionType = ActionType.delete;
		setShowConfirmDeleteDialog(false);
		dispatch(setChatMessage(deleteMessage));
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
	};

	const rejectDeleteDialog = () => {
		//отмена удаления сообщения
		setShowConfirmDeleteDialog(false);
	};

	const onRightClick = (
		//по правой кглпке - отображаем контекстное меню "удалить/редактировать"
		event: React.MouseEvent<HTMLDivElement, MouseEvent>,
		chatItem: ChatComment
	) => {
		if (cm.current) {
			setChatSelected(() => chatItem);
			cm.current.show(event);
		}
	};

	return (
		<>
			<div className="grid">
				<div className="col-12 MessageInputItem">
					<InputTextarea
						variant="filled"
						autoResize
						placeholder="Новое сообщение (для перевода строки нажмите Shift-Enter)"
						value={newMessage.message}
						onKeyDown={(e) => {
							if (e.key === "Enter" && e.shiftKey) return;
							if (e.key === "Enter") {
								e.preventDefault();
								handleMessageSubmit();
							}
						}}
						onChange={(e) => handleMessageChanged(e.currentTarget.value)}
						rows={3}
						className="w-full"
					/>
				</div>
				{procState.find((p) => p.eventName === "WaiterHide") &&
				!procState.find((p) => p.eventName === "WaiterHide")!.ready ? (
					<div className="col-12 CenterItem">
						<Waiter />
					</div>
				) : (
					<ScrollPanel
						style={{ width: "100%", height: "275px" }}
						className="custombar1"
					>
						{communicationItems &&
							communicationItems.map((item, index) => (
								<div
									key={index}
									className="col-12 MessageItem"
									onContextMenu={(event) => onRightClick(event, item)}
								>
									<div className="w-15rem border-right-2 px-1 py-1 CenterItem">
										<ChatUser userLogin={item.userLogin} />
									</div>
									<div className="flex flex-column">
										<div className="px-3 ">
											{new Date(item.updateAt).toLocaleDateString() +
												" " +
												new Date(item.updateAt).toLocaleTimeString()}
										</div>
										<div className="px-3 py-2">
											{item.message.split("\n").map((line, index) => {
												return (
													<p key={index} className="p-0 m-0">
														{line}
													</p>
												);
											})}
										</div>
									</div>
								</div>
							))}
					</ScrollPanel>
				)}
			</div>
			<ContextMenu ref={cm} model={contextItems} />

			<ModalEditText
				accept={acceptEditDialog}
				reject={rejectEditDialog}
				header="Редактирование сообщения"
				label={"Автор сообщения - " + chatSelected?.userLogin!}
				message={chatSelected?.message ? chatSelected?.message : ""}
				editValue={(val: string) =>
					setChatSelected((prev) => {
						return { ...prev, message: val } as ChatComment;
					})
				}
				visible={showConfirmEditDialog}
				group="edit"
			/>

			<ModalYesNo
				accept={acceptDeleteDialog}
				reject={rejectDeleteDialog}
				header="Подтверждение удаления сообщения"
				label={
					"Действительно удалить сообщение - ' " + chatSelected?.message! + "'?"
				}
				visible={showConfirmDeleteDialog}
				group="delete"
				modalType={ModalTypes.warning}
			/>
		</>
	);
}
