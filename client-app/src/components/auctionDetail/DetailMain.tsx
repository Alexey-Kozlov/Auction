import { useEffect, useState } from 'react';
import Heading from '../auctionList/Heading';
import CountdownTimer from '../auctionList/CountDownTimer';
import {
  Auction,
  AuctionDeleted,
  ModalTypes,
  NotifyUser,
  ProcessingState,
  User,
} from '../../types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { useNavigate, useParams } from 'react-router-dom';
import ImageCard from '../auctionList/ImageCard';
import BidList from './BidList';
import api, { useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import { useIsNotifyUserQuery } from '../../api/NotificationApi';
import { setEventFlag } from '../../store/processingSlice';
import {
  useDeleteAuctionMutation,
  useSetNotifyUserMutation,
} from '../../api/ProcessingApi';
import { Button } from 'primereact/button';
import { InputSwitch } from 'primereact/inputswitch';
import { Panel } from 'primereact/panel';
import DetailedSpec from './DetailedSpec';
import { useGetUserNameQuery } from '../../api/AuthApi';
import Waiter from '../Waiter';
import Footer from '../layout/Footer';
import ModalYesNo from '../modals/ModalYesNo';
import { CheckEventReady } from '../../utils/checkEvent';

export default function DetailMain() {
  const { id } = useParams();
  const user: User = useSelector((state: RootState) => state.authStore);
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );
  const [notifyUser, setNotifyUser] = useState(false);
  const [isNotifySetWaiting, notifySetWaiting] = useState(false);
  const [isDeleteSetWaiting, deleteSetWaiting] = useState(false);
  const [showConfirmDelete, setShowConfirmDelete] = useState(false);
  const [auctionDetail, setAuctionDetail] = useState<Auction | null>(null);
  const data = useGetDetailedViewDataQuery(id ? id : '', { skip: !id });

  const userSeller = useGetUserNameQuery(
    auctionDetail ? auctionDetail.seller : '',
    {
      skip: auctionDetail?.seller === '',
    },
  );
  const isNotifyUser = useIsNotifyUserQuery(id!, {
    skip: user.isGuest,
  });
  const [setNotifyUserApi] = useSetNotifyUserMutation();
  const [deleteAuctionProc] = useDeleteAuctionMutation();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  //инициализация данных
  useEffect(() => {
    if (!data.isLoading && !data.isFetching && data.data?.result) {
      setAuctionDetail(data.data!.result);
    }
    //если не нашли данных по указанному id - переход на страницу "Не найдено"
    if (!data.isLoading && (!data || !data.data?.result)) {
      navigate('/not-found');
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
      setNotifyUser(isNotifyUser.data.result!);
    }
    // eslint-disable-next-line
  }, [isNotifyUser]);

  //отслеживаем сообщения по редактированию уведомления и удалению аукциона
  useEffect(() => {
    //обновление переключателя рассылки уведомлений
    if (!CheckEventReady(procState, 'EditNotification') && isNotifySetWaiting) {
      //обновление переключателя
      isNotifyUser.refetch();
      notifySetWaiting(() => false);
    }
    //переход на список аукционов при удалении текущего аукциона
    if (isDeleteSetWaiting) {
      //функцией api.util.resetApiState() - полностью удаляем кеш RTK, иначе в пейджинге на других
      //страницах останутся старые данные
      dispatch(api.util.resetApiState());
      navigate('/');
    }
    // eslint-disable-next-line
  }, [procState]);

  //обработчик переключения переключателя уведомлений пользователя по событиям данного аукциона

  const handleSetNotifyUser = async (checked: boolean) => {
    notifySetWaiting(() => true);
    dispatch(setEventFlag({ eventName: 'EditNotification', ready: true }));
    var notifyUser: NotifyUser = {
      itemId: id!,
      enable: checked,
    };
    await setNotifyUserApi(notifyUser);
  };

  const acceptDeleteDialog = async () => {
    //подтверждено удаления аукциона
    dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: true }));
    const auctionDeleted: AuctionDeleted = {
      itemId: id!,
    };
    await deleteAuctionProc(auctionDeleted);
    deleteSetWaiting(true);
  };

  const rejectDeleteDialog = () => {
    //отмена удаления сообщения
    setShowConfirmDelete(false);
  };

  const setEditCSS = () => {
    if (isNotifySetWaiting) {
      return 'overflow-hidden ChatEditMode';
    } else {
      return 'overflow-hidden';
    }
  };

  return (
    <div>
      {CheckEventReady(procState, 'CollectionChanged') ||
      CheckEventReady(procState, 'EditNotification') ? (
        <Waiter />
      ) : (
        <></>
      )}
      {auctionDetail && auctionDetail!.sellerName && (
        <div className={setEditCSS()}>
          <div className="grid">
            <div className="col-6 CenterItem">
              <div className="CenterItem flex-column w-full">
                <Heading title={`${auctionDetail!.title}`} />
                {(user?.login === auctionDetail!.seller || user?.isAdmin) && (
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
                      onClick={() => setShowConfirmDelete(true)}
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

                {!user.isGuest && (
                  <div className="CenterItem">
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
                id={auctionDetail!.itemId}
                detail={true}
                cache={false}
              />
            </div>
            <div className="col-6">
              <Panel>
                <BidList auction={auctionDetail!} />
              </Panel>
            </div>
            <div className="col-12">
              <Panel>
                <DetailedSpec auction={auctionDetail!} />
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
