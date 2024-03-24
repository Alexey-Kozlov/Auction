import React, { useState } from 'react'
import Heading from '../auctionList/Heading';
import CountdownTimer from '../auctionList/CountDownTimer';
import { User } from '../../store/types';
import { useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { Button } from 'flowbite-react';
import { NavLink, useNavigate, useParams } from 'react-router-dom';
import CarImage from '../auctionList/CarImage';
import BidList from './BidList';
import DetailedSpecs from './DetailedSpec';
import { useDeleteAuctionMutation, useGetAuctionsQuery, useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import toast from 'react-hot-toast';

export default function Detail() {
    const { id } = useParams();
    const [isDeleting, setIsDeleting] = useState(false);
    const { data, isLoading, isError } = useGetDetailedViewDataQuery(id!);
    const user: User = useSelector((state: RootState) => state.authStore);
    const [deleteAuction] = useDeleteAuctionMutation();
    const auctions = useGetAuctionsQuery('');
    const navigate = useNavigate();
    const handleDeleteAuction = async () => {
        setIsDeleting(true);
        const result: any = await deleteAuction(id!);
        if (result.data) {
            setTimeout(() => {
                auctions.refetch();
                navigate('/');
            }, 1000);
        } else {
            toast.error('Ошибка удаления - ' + result.error.message);
        }
    }

    if (isLoading) return 'Загрузка...';
    if (isError && !isDeleting) {
        navigate('/not-found');
    }

    return (
        <div>
            <div className='flex justify-between'>
                <div className='flex items-center gap-3'>
                    <Heading title={`${data?.make} ${data?.model}`} />
                    {user?.login === data?.seller && (
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

                <div className='flex gap-3'>
                    <h3 className='text-2xl font-semibold'>Осталось времени:</h3>
                    <CountdownTimer auctionEnd={data!.auctionEnd} />
                </div>
            </div>
            <div className='grid grid-cols-2 gap-6 mt-3'>
                <div className='w-full bg-gray-200 aspect-h-10 aspect-w-16 rounded-lg overflow-hidden'>
                    <CarImage id={data?.id} />
                </div>
                <BidList user={user} auction={data!} />
            </div>
            <div className='mt-3 grid grid-cols-1 rounded-lg'>
                <DetailedSpecs auction={data!} />
            </div>
            <div className='flex justify-center mt-2'>
                <Button onClick={() => navigate(-1)}>Назад</Button>
            </div>
        </div>

    )
}
