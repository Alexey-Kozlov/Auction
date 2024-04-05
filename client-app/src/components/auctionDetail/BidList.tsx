import React, { useEffect, useRef, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import Heading from '../auctionList/Heading';
import { Auction, Bid, User } from '../../store/types';
import EmptyFilter from '../auctionList/EmptyFilter';
import BidItem from './BidItem';
import BidForm from './BidForm';
import { useGetBidsForAuctionQuery } from '../../api/BidApi';
import { setBids, setOpen } from '../../store/bidSlice';
import NumberWithSpaces from '../../utils/NumberWithSpaces';

type Props = {
    user: User | null;
    auction: Auction
}

export default function BidList({ user, auction }: Props) {
    const dispatch = useDispatch();
    const [lastBidId, setLastBidId] = useState('');
    const { isLoading, data } = useGetBidsForAuctionQuery(auction?.id);
    const bidStore = useSelector((state: RootState) => state.bidStore);
    const bids = bidStore.bids;
    const open = bidStore.open;
    const openForBids = new Date(auction?.auctionEnd) > new Date();

    //вычисляем самую большую ставку
    const bidRestriction = () => {
        let result = bids?.reduce((prev, current) => {
            return prev > current?.amount
                ? prev
                : current?.bidStatus?.includes('Принято') ? current?.amount : prev
        }, 0);
        if (auction.reservePrice && auction.reservePrice > result) {
            result = auction.reservePrice;
        }
        return result;
    }

    const itemsRef = useRef<null | HTMLLIElement>(null);

    //при каждом обновлении заявок - вычисление последней заявки для прокрутки
    //списка заявок вверх (если заявок много)
    useEffect(() => {
        if (!isLoading && bids && bids.length > 0) {
            const maxBidId: Bid = Array.from(bids).sort((a: Bid, b: Bid) => {
                return Date.parse(b.bidTime) - Date.parse(a.bidTime);
            })[0];
            setLastBidId(maxBidId.id);
        }
    }, [isLoading, bids])

    //первоначальное заполнение списка заявок
    useEffect(() => {
        if (!isLoading) {
            dispatch(setBids(data?.result));
        }
    }, [auction?.id, isLoading, data?.result, dispatch]);

    //закрытие аукциона
    useEffect(() => {
        dispatch(setOpen(openForBids));
    }, [openForBids, dispatch]);

    //перемотка списка завок - самые послелние - в самом верху, 
    //и потом перемотка всей странички вверх - чтобы были видны последние изменения
    useEffect(() => {
        if (itemsRef && itemsRef.current) {
            itemsRef.current.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest',
            });
        }
        setTimeout(() => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }, 1000);
    }, [lastBidId])

    if (isLoading) return <span>Загрузка предложений...</span>

    return (
        <div className='rounded-lg shadow-md'>
            <div className='py-2 px-4 bg-white'>
                <div className='sticky top-0 bg-white p-2'>
                    {bids?.length !== 0 && (
                        <Heading title={`Текущее лучшее предложение - ${NumberWithSpaces(bidRestriction())} руб`} />
                    )}
                </div>
            </div>

            <div className='overflow-auto h-[400px] flex flex-col-reverse px-2'>
                {bids?.length === 0 ? (
                    <EmptyFilter title='Нет предложений для этого аукциона'
                        subtitle='Сделайте предложение' />
                ) : (
                    <>
                        {bids?.map((bid, index) => (
                            <li
                                key={bid?.id} className='list-none'
                                ref={bid?.id === lastBidId ? itemsRef : null}
                            >
                                <BidItem bid={bid} />
                            </li>
                        )
                        )}
                    </>
                )}
            </div>
            <div className='px-2 pb-2 text-gray-500'>
                {!open ? (
                    <div className='flex items-center justify-center p-2 text-lg font-semibold'>
                        Аукцион завершен
                    </div>
                ) : !user?.id ? (
                    <div className='flex items-center justify-center p-2 text-lg font-semibold'>
                        Войдите в систему чтобы делать заявки
                    </div>
                ) : user && user.login === auction?.seller ? (
                    <div className='flex items-center justify-center p-2 text-lg font-semibold'>
                        Невозможно сделать заявку для собственного аукциона
                    </div>
                ) : (
                    <BidForm auctionId={auction?.id} highBid={bidRestriction()} />
                )}
            </div>
        </div>
    )
}
