import { useEffect, useState } from 'react'
import Heading from '../auctionList/Heading';
import CountdownTimer from '../auctionList/CountDownTimer';
import { ApiResponse, Auction, User } from '../../store/types';
import { useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { Button } from 'flowbite-react';
import { NavLink, useNavigate, useParams } from 'react-router-dom';
import CarImage from '../auctionList/CarImage';
import BidList from './BidList';
import DetailedSpecs from './DetailedSpec';
import { useDeleteAuctionMutation, useGetAuctionByIdQuery, useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import { useIsNotifyUserQuery, useSetNotifyUserMutation } from '../../api/NotificationApi';
import toast from 'react-hot-toast';

export default function Detail() {
    const { id } = useParams();
    const user: User = useSelector((state: RootState) => state.authStore);
    const [notifyUser, setNotifyUser] = useState(false);
    const [deleteAuctionId, setDeleteAuctionId] = useState('');
    const [auctionDetail, setAuctionDetail] = useState<Auction>();
    const { data, isLoading } = useGetDetailedViewDataQuery(id!);
    const isNotifyUser = useIsNotifyUserQuery(id!, {
        skip: user.login === '' || user.login === undefined
    });
    const [setNotifyUserApi] = useSetNotifyUserMutation();

    const [deleteAuction] = useDeleteAuctionMutation();
    const deleteAuctionItem = useGetAuctionByIdQuery(deleteAuctionId, {
        skip: deleteAuctionId === ''
    });
    const navigate = useNavigate();

    //удаляем аукцион
    const handleDeleteAuction = async () => {
        await deleteAuction(id!);
        setDeleteAuctionId(id!); //делаем запрос на удаляемый аукцион, когда вернется результат - аукцион удален
        //если запись аукциона еще присутствует в Search, делаем задержку и повторяем проверку
        let deleteCounter = 10;
        const refInterval = setInterval(() => {
            if (deleteCounter === 0) {
                //не удалось удалить запись из Search после 10 попыток - ошибка
                clearInterval(refInterval);
                toast.error(`Ошибка удаления аукциона "${auctionDetail?.title}".`, { duration: 15000 });
                navigate('/');
            }
            deleteCounter--;
            try {
                deleteAuctionItem.refetch()
                    .then(rez => {
                        if (rez.data && !rez.data.result) {
                            clearInterval(refInterval);
                            toast.success(`Аукцион "${auctionDetail?.title}" удален.`);
                            navigate('/');
                        }
                    })
            } catch (e) {
                clearInterval(refInterval);
            }
        }, 500);
    }

    useEffect(() => {
        if (!isLoading && data?.isSuccess && data?.result && data?.result.title) {
            setAuctionDetail(data!.result);
        }
    }, [isLoading, data]);

    //для управления переключателем по уведомлениям польбзователя
    useEffect(() => {
        if (!isNotifyUser.isLoading && isNotifyUser.data) {
            setNotifyUser(isNotifyUser.data!.result!);
        }
    }, [isNotifyUser.isLoading, isNotifyUser.data]);

    //запоминаем переключатель по уведомлениям пользователя по событиям данного аукциона
    const handleSetNotifyUser = async (event: React.FormEvent<HTMLInputElement>) => {
        var data = { id: id, enable: event.currentTarget.checked };
        const result: ApiResponse<{}> = await setNotifyUserApi(JSON.stringify(data));
        if (result && result!.data!.isSuccess) {
            setNotifyUser(data.enable);
        }
    };

    useEffect(() => {
        if (!deleteAuctionId && !isLoading && data?.isSuccess && data?.result && !data?.result.title) {
            navigate('/not-found');
        }
    }, [isLoading, deleteAuctionId, data, navigate]);


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
                                <Button isProcessing={!!deleteAuctionId} outline onClick={handleDeleteAuction}>
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
                    <div className='w-full bg-gray-200 aspect-h-10 aspect-w-16 rounded-lg overflow-hidden'>
                        <CarImage id={auctionDetail!.id} />
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
