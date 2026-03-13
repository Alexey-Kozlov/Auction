import { useEffect, useRef, useState } from 'react';
import {
  ActionType,
  Auction,
  ChatComment,
  ModalTypes,
  ProcessingState,
  SignalREvents,
  SortDirection,
  User,
} from '../../../types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../../store/store';
import uuid from 'react-native-uuid';
import { setChatMessage } from '../../../store/chatSlice';
import { useGetCommunicationItemsQuery } from '../../../api/CommunicationApi';
import ChatUser from './ChatUser';
import { setEventFlag } from '../../../store/processingSlice';
import Waiter from '../../Waiter';
import { InputTextarea } from 'primereact/inputtextarea';
import { DynamicDateSort, DynamicSort } from '../../../utils/dynamicSort';
import { ScrollPanel } from 'primereact/scrollpanel';
import { ContextMenu } from 'primereact/contextmenu';
import { MenuItem } from 'primereact/menuitem';
import ModalEditText from '../../modals/ModalEditText';
import ModalYesNo from '../../modals/ModalYesNo';
import { Button } from 'primereact/button';
import { CheckEventReady } from '../../../utils/checkEvent';
import { useSetUsersCurrentPageMutation } from '../../../api/ServiceApi';

type Props = {
  auction: Auction;
  user: User;
};

export default function TabChatTable({ auction, user }: Props) {
  const [setCurrentPage] = useSetUsersCurrentPageMutation();
  const cm = useRef<ContextMenu>(null);
  const [communicationItems, setCommunicationItems] = useState<ChatComment[]>(
    [],
  );
  const [chatSelected, setChatSelected] = useState<ChatComment | null>();
  const [showConfirmEditDialog, setShowConfirmEditDialog] = useState(false);
  const [showConfirmDeleteDialog, setShowConfirmDeleteDialog] = useState(false);
  const [chatEditClass, setChatEditClass] = useState('grid');
  const communication = useGetCommunicationItemsQuery(auction.itemId);
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );

  const chatResponse = useSelector(
    (state: RootState) => state.chatResponseStore,
  );
  const [newMessage, setNewMessage] = useState<ChatComment>({
    itemId: uuid.v4() as string,
    message: '',
    parentId: '',
    userLogin: '',
    auctionId: '',
  } as ChatComment);

  const dispatch = useDispatch();
  const handleMessageChanged = (value: string | number | undefined) => {
    setNewMessage((prev) => {
      return {
        ...prev,
        message: value ? value.toString() : '',
        itemId: uuid.v4() as string,
        parentId: '',
        userLogin: user.login,
        auctionId: auction.itemId,
      };
    });
  };

  const handleMessageSubmit = () => {
    // начало процесса создания нового сообщения - в UseEffect свойства "messageChat" в SignalRProvider
    if (!newMessage.message) return;
    //отображаем страницу размыто - режим редактирования
    setChatEditClass('grid ChatEditMode');
    let createMessage = newMessage;
    createMessage.actionType = ActionType.create;
    dispatch(setChatMessage(createMessage));
    dispatch(setEventFlag({ eventName: 'CommunicationChanged', ready: true }));
  };

  useEffect(() => {
    //посылаем вызов в апи процессинга - для записи в кеш редиса страницы, где находится пользователь
    setCurrentPage('/communication/' + auction.itemId);
    //сортируем при первоначальной загрузке
    if (
      !communication.isFetching &&
      !communication.isLoading &&
      communication.data
    ) {
      let _temp = JSON.parse(
        JSON.stringify(communication.data.result),
      ) as ChatComment[];
      setCommunicationItems(
        _temp?.sort(DynamicSort('updateAt', SortDirection.descending)),
      );
      dispatch(
        setEventFlag({ eventName: 'CommunicationChanged', ready: false }),
      );
    }
    setChatEditClass('grid');
    // eslint-disable-next-line
  }, [communication]);

  //если на странице поменяли пользователя
  useEffect(() => {
    communication.refetch();
    // eslint-disable-next-line
  }, [user]);

  //изменилось хранилище ответов чата - обновляем состояние набора записей чата
  useEffect(() => {
    switch (chatResponse.actionType) {
      case ActionType.create:
        const newChatMessage: ChatComment = {
          auctionId: chatResponse.auctionId,
          itemId: chatResponse.itemId,
          message: chatResponse.message,
          parentId: chatResponse.parentId,
          updateAt: chatResponse.updateAt,
          userLogin: chatResponse.userLogin,
          actionType: ActionType.create,
        };
        setCommunicationItems((prev) => {
          let _temp = [...prev, newChatMessage];
          _temp = JSON.parse(JSON.stringify(_temp)) as ChatComment[];
          return _temp?.sort(
            DynamicDateSort('updateAt', SortDirection.descending),
          );
        });
        break;
      case ActionType.delete:
        setCommunicationItems((prev) => {
          return prev.filter((p) => p.itemId !== chatResponse.itemId);
        });
        break;
      case ActionType.update:
        const updateChatMessage: ChatComment = {
          auctionId: chatResponse.auctionId,
          itemId: chatResponse.itemId,
          message: chatResponse.message,
          parentId: chatResponse.parentId,
          updateAt: chatResponse.updateAt,
          userLogin: chatResponse.userLogin,
          actionType: ActionType.update,
        };
        setCommunicationItems((prev) => {
          let _temp = [
            ...prev.filter((p) => p.itemId !== chatResponse.itemId),
            updateChatMessage,
          ];
          _temp = JSON.parse(JSON.stringify(_temp)) as ChatComment[];
          return _temp?.sort(
            DynamicDateSort('updateAt', SortDirection.descending),
          );
        });
        break;
    }
    //сбрасываем введенное сообщение (если было)
    setNewMessage((prev) => {
      return { ...prev, message: '' };
    });
    setChatEditClass('grid');
  }, [chatResponse]);

  //первоначальная загрузка - ждем списка сообщений
  useEffect(() => {
    setChatEditClass('grid ChatEditMode');
  }, []);

  const contextItems: MenuItem[] = [
    {
      label: 'Редактировать',
      icon: (
        <i
          className="pi pi-file-edit"
          style={{ fontSize: '2rem', marginRight: '1rem' }}
        />
      ),
      className: 'text-4xl',
      command: () => setShowConfirmEditDialog(true),
    },
    {
      label: 'Удалить',
      icon: (
        <i
          className="pi pi-trash"
          style={{ fontSize: '2rem', marginRight: '1rem' }}
        />
      ),
      className: 'text-4xl',
      command: () => setShowConfirmDeleteDialog(true),
    },
  ];

  const acceptEditDialog = () => {
    //подтвердили редактирование сообщения чата
    // начало процесса редактирования сообщения чата - в UseEffect свойства "messageChat" в SignalRProvider
    let updateMessage = chatSelected!;
    updateMessage.actionType = ActionType.update;
    setShowConfirmEditDialog(false);
    setChatEditClass('grid ChatEditMode');
    dispatch(setChatMessage(updateMessage));
    dispatch(setEventFlag({ eventName: 'CommunicationChanged', ready: true }));
  };

  const rejectEditDialog = () => {
    //отмена редактирования сообщения
    setShowConfirmEditDialog(false);
  };

  const acceptDeleteDialog = () => {
    // удаляем сообщение через SignalR
    // начало процесса удаления сообщения чата - в UseEffect свойства "messageChat" в SignalRProvider
    let deleteMessage = chatSelected!;
    deleteMessage.actionType = ActionType.delete;
    setShowConfirmDeleteDialog(false);
    setChatEditClass('grid ChatEditMode');
    dispatch(setChatMessage(deleteMessage));
    dispatch(setEventFlag({ eventName: 'CommunicationChanged', ready: true }));
  };

  const rejectDeleteDialog = () => {
    //отмена удаления сообщения
    setShowConfirmDeleteDialog(false);
  };

  const onRightClick = (
    //по правой копке - отображаем контекстное меню "удалить/редактировать"
    event: React.MouseEvent<HTMLDivElement, MouseEvent>,
    chatItem: ChatComment,
  ) => {
    //показываем меню редактирования для админов или для автора записи
    if (cm.current && (user.isAdmin || user.login === chatItem.userLogin)) {
      setChatSelected(() => chatItem);
      cm.current.show(event);
    }
  };

  return (
    <>
      {CheckEventReady(
        procState,
        SignalREvents[SignalREvents.CommunicationChanged],
      ) ? (
        <div className="CenterItem">
          <Waiter />
        </div>
      ) : (
        <></>
      )}
      <>
        <div className={chatEditClass}>
          <div className="col-12 MessageInputItem">
            <InputTextarea
              variant="filled"
              disabled={user.isGuest}
              autoResize
              placeholder="Новое сообщение (для отправления - нажмите Enter, для перевода строки нажмите Shift-Enter)"
              value={newMessage.message}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && e.shiftKey) return;
                if (e.key === 'Enter') {
                  e.preventDefault();
                  handleMessageSubmit();
                }
              }}
              onChange={(e) => handleMessageChanged(e.currentTarget.value)}
              rows={5}
              className="w-full text-4xl"
            />
            <Button
              text
              icon="pi pi-send"
              size="large"
              className="MessageInputButton"
              onClick={handleMessageSubmit}
            />
          </div>
          <ScrollPanel
            className="DetailScrollPanel"
            style={{ height: '27rem' }}
          >
            {communicationItems &&
              communicationItems.map((item, index) => (
                <div
                  key={index}
                  className="col-12 MessageItem"
                  onContextMenu={(event) => onRightClick(event, item)}
                >
                  <div className="w-30rem border-right-2 px-2 py-2 CenterItem">
                    <ChatUser userLogin={item.userLogin} />
                  </div>
                  <div className="flex flex-column text-3xl">
                    <div className="px-6 ">
                      {new Date(item.updateAt).toLocaleDateString('RU-ru', {
                        timeZone: 'UTC',
                      }) +
                        ' ' +
                        new Date(item.updateAt).toLocaleTimeString('RU-ru', {
                          timeZone: 'UTC',
                        })}
                    </div>
                    <div className="px-6 py-4">
                      {item.message.split('\n').map((line, index) => {
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
        </div>
      </>

      <ContextMenu ref={cm} model={contextItems} className="w-18rem" />

      <ModalEditText
        accept={acceptEditDialog}
        reject={rejectEditDialog}
        header="Редактирование сообщения"
        label={'Автор сообщения - ' + chatSelected?.userLogin!}
        message={chatSelected?.message ? chatSelected?.message : ''}
        editValue={(val: string) =>
          setChatSelected((prev) => {
            return { ...prev, message: val } as ChatComment;
          })
        }
        visible={showConfirmEditDialog}
        group="editChat"
      />

      <ModalYesNo
        accept={acceptDeleteDialog}
        reject={rejectDeleteDialog}
        header="Подтверждение удаления сообщения"
        label={
          "Действительно удалить сообщение - ' " + chatSelected?.message! + "'?"
        }
        visible={showConfirmDeleteDialog}
        group="deleteChat"
        modalType={ModalTypes.warning}
      />
    </>
  );
}
