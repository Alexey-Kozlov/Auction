import { useEffect, useState } from 'react'
import Heading from '../auctionList/Heading';
import CountdownTimer from '../auctionList/CountDownTimer';
import { ApiResponse, Auction, AuctionDeleted, ProcessingState, User } from '../../store/types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { Button } from 'flowbite-react';
import { NavLink, useNavigate, useParams } from 'react-router-dom';
import ImageCard from '../auctionList/ImageCard';
import BidList from './BidList';
import DetailedSpecs from './DetailedSpec';
import { useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import { useIsNotifyUserQuery, useSetNotifyUserMutation } from '../../api/NotificationApi';
import { setEventFlag } from '../../store/processingSlice';
import { useDeleteAuctionMutation } from '../../api/ProcessingApi';

export default function Detail() {
    const { id } = useParams();
    const user: User = useSelector((state: RootState) => state.authStore);
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const [notifyUser, setNotifyUser] = useState(false);
    const [deleteAuction, setDeleteAuction] = useState(false);
    const [auctionDetail, setAuctionDetail] = useState<Auction>();
    const { data, isLoading } = useGetDetailedViewDataQuery(id!);
    const isNotifyUser = useIsNotifyUserQuery(id!, {
        skip: user.login === '' || user.login === undefined
    });
    const [setNotifyUserApi] = useSetNotifyUserMutation();
    const [deleteAuctionProc] = useDeleteAuctionMutation();
    const navigate = useNavigate();
    const dispatch = useDispatch();

    //удаляем аукцион
    const handleDeleteAuction = async () => {
        setDeleteAuction(true);
        dispatch(setEventFlag({ eventName: 'AuctionDeleted', ready: false }));
        const auctionDeleted: AuctionDeleted = { auctionId: id! };
        await deleteAuctionProc(auctionDeleted);
    }

    //инициализация данных
    useEffect(() => {
        if (!isLoading && data?.isSuccess && data?.result && data?.result.title) {
            setAuctionDetail(data!.result);
        }
    }, [isLoading, data]);

    //для управления переключателем по уведомлениям пользователя
    useEffect(() => {
        if (!isNotifyUser.isLoading && isNotifyUser.data) {
            setNotifyUser(isNotifyUser.data!.result!);
        }
    }, [isNotifyUser.isLoading, isNotifyUser.data]);

    //обновление переключателя рассылки уведомлений или переход в случае удалени записи
    useEffect(() => {
        const eventState = procState.find(p => p.eventName === 'BidPlaced');
        if (eventState && eventState.ready && !notifyUser) {
            //обновление переключателя
            isNotifyUser.refetch();
            dispatch(setEventFlag({ eventName: 'BidPlaced', ready: false }));
        }
    }, [procState, isNotifyUser, dispatch, notifyUser]);

    useEffect(() => {
        const eventState = procState.find(p => p.eventName === 'CollectionChanged' && p.ready);
        if (eventState && deleteAuction) {
            navigate('/');
        }
    }, [procState, navigate, deleteAuction]);

    //запоминаем переключатель по уведомлениям пользователя по событиям данного аукциона
    const handleSetNotifyUser = async (event: React.FormEvent<HTMLInputElement>) => {
        dispatch(setEventFlag({ eventName: 'BidPlaced', ready: false }));
        var data = { id: id, enable: event.currentTarget.checked };
        const result: ApiResponse<{}> = await setNotifyUserApi(JSON.stringify(data));
        if (result && result!.data!.isSuccess) {
            setNotifyUser(data.enable);
        }
    };

    useEffect(() => {
        if (!deleteAuction && !isLoading && data?.isSuccess && data?.result && !data?.result.title) {
            navigate('/not-found');
        }
    }, [isLoading, deleteAuction, data, navigate]);


    if (isLoading) return 'Загрузка...';

    return (
        <div>
            {auctionDetail && (<>
                <div className='flex justify-between'>
                    <div className='flex items-center gap-3'>
                        <Heading title={`${auctionDetail?.title}`} />
                        {user?.login === auctionDetail?.seller && (
                            <>
                                <Button outline>
                                    <NavLink to={`/auctions/edit/${id}`}>Редактировать аукцион</NavLink>
                                </Button>
                                <Button isProcessing={!!deleteAuction} outline onClick={handleDeleteAuction}>
                                    Удалить аукцион
                                </Button>
                            </>
                        )}
                    </div>

                    <div>
                        <div className='flex gap-3 justify-end'>
                            <h3 className='text-2xl font-semibold'>Осталось времени:</h3>
                            <CountdownTimer auctionEnd={auctionDetail!.auctionEnd} />
                        </div>
                        {user.name && (
                            <div>
                                <label className="inline-flex items-center mb-5 cursor-pointer">
                                    <h3 className='text-2xl font-semibold mr-3'>Получать уведомления этого аукциона:</h3>
                                    <input type="checkbox" onChange={handleSetNotifyUser} value="."
                                        className="sr-only peer" checked={notifyUser} />
                                    <div className="relative w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:w-5 after:h-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                                </label>
                            </div>
                        )}
                    </div>

                </div>
                <div className='grid grid-cols-2 gap-6 mt-3'>
                    <div className='flex items-center justify-center rounded-lg'>
                        <ImageCard id={auctionDetail!.id} dopStyle=' max-h-96' />
                    </div>
                    <BidList user={user} auction={auctionDetail!} />
                </div>
                <div className='mt-3 grid grid-cols-1 rounded-lg'>
                    <DetailedSpecs auction={auctionDetail!} />
                </div>

                <div className='flex justify-center mt-2'>
                    <Button onClick={() => navigate(-1)}>Назад</Button>
                </div>
            </>)}
        </div>

    )
}
