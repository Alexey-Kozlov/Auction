import { useEffect, useState } from 'react'
import Heading from '../auctionList/Heading';
import CountdownTimer from '../auctionList/CountDownTimer';
import { ApiResponse, User } from '../../store/types';
import { useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { Button } from 'flowbite-react';
import { NavLink, useNavigate, useParams } from 'react-router-dom';
import CarImage from '../auctionList/CarImage';
import BidList from './BidList';
import DetailedSpecs from './DetailedSpec';
import { useDeleteAuctionMutation, useGetAuctionsQuery, useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import { useIsNotifyUserQuery, useSetNotifyUserMutation } from '../../api/NotificationApi';

export default function Detail() {
    const { id } = useParams();
    const user: User = useSelector((state: RootState) => state.authStore);
    const [isDeleting, setIsDeleting] = useState(false);
    const [notifyUser, setNotifyUser] = useState(false);
    const { data, isLoading, isError } = useGetDetailedViewDataQuery(id!);
    const isNotifyUser = useIsNotifyUserQuery(id!, {
        skip: user.login === '' || user.login === undefined
    });
    const [setNotifyUserApi] = useSetNotifyUserMutation();

    const [deleteAuction] = useDeleteAuctionMutation();
    const auctions = useGetAuctionsQuery('');
    const navigate = useNavigate();
    const handleDeleteAuction = async () => {
        setIsDeleting(true);
        const result: ApiResponse<{}> = await deleteAuction(id!);
        if (result && result!.data!.isSuccess) {
            setTimeout(() => {
                auctions.refetch();
                navigate('/');
            }, 1000);
        }
    }

    useEffect(() => {
        if (!isNotifyUser.isLoading && isNotifyUser.data) {
            setNotifyUser(isNotifyUser.data!.result!);
        }
    }, [isNotifyUser.isLoading, isNotifyUser.data]);

    const handleSetNotifyUser = async (event: React.FormEvent<HTMLInputElement>) => {
        var data = { id: id, enable: event.currentTarget.checked };
        const result: ApiResponse<{}> = await setNotifyUserApi(JSON.stringify(data));
        if (result && result!.data!.isSuccess) {
            setNotifyUser(data.enable);
        }
    }
    useEffect(() => {
        if (!isLoading && ((isError && !isDeleting) || !data?.isSuccess)) {
            navigate('/not-found');
        }
    }, [isLoading]);
    if (isLoading) return 'Загрузка...';



    return (
        <div>
            <div className='flex justify-between'>
                <div className='flex items-center gap-3'>
                    <Heading title={`${data!.result.title}`} />
                    {user?.login === data!.result.seller && (
                        <>
                            <Button outline>
                                <NavLink to={`/auctions/edit/${id}`}>Редактировать аукцион</NavLink>
                            </Button>
                            <Button isProcessing={isDeleting} outline onClick={handleDeleteAuction}>
                                Удалить аукцион
                            </Button>
                        </>
                    )}
                </div>

                <div>
                    <div className='flex gap-3 justify-end'>
                        <h3 className='text-2xl font-semibold'>Осталось времени:</h3>
                        <CountdownTimer auctionEnd={data!.result.auctionEnd} />
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
                    <CarImage id={data!.result.id} />
                </div>
                <BidList user={user} auction={data!.result} />
            </div>
            <div className='mt-3 grid grid-cols-1 rounded-lg'>
                <DetailedSpecs auction={data!.result} />
            </div>

            <div className='flex justify-center mt-2'>
                <Button onClick={() => navigate(-1)}>Назад</Button>
            </div>
        </div>

    )
}
