import { useGetAuctionByIdQuery } from '../../api/AuctionApi';
import { FinanceItem } from '../../store/types';
import NumberWithSpaces from '../../utils/NumberWithSpaces';
import { Checkbox } from 'flowbite-react';
import ImageCard from '../auctionList/ImageCard';
import { NavLink } from 'react-router-dom';

type Props = {
    item: FinanceItem;
}

export default function FinRow({ item }: Props) {
    const auction = useGetAuctionByIdQuery(item.auctionId, {
        skip: !item.auctionId
    });
    return (
        <>
            <div>
                {!auction.isLoading && auction.data?.result?.id && auction.status === 'fulfilled' ? (
                    <NavLink to={`/auctions/${item.auctionId}`} className='group'>
                        <ImageCard id={item.auctionId} dopStyle=' max-h-20' />
                    </NavLink>
                ) : ""}
            </div>
            <div>{!auction.isLoading && auction.data?.result?.title && auction.status === 'fulfilled' ? (
                <NavLink to={`/auctions/${item.auctionId}`} className='group'>
                    {auction.data?.result?.title}
                </NavLink>
            )
                : ""}</div>
            <div className='font-bold'>{NumberWithSpaces(item.balance)}</div>
            <div>{`${new Date(item.actionDate).toLocaleDateString('ru-RU')} 
                        ${new Date(item?.actionDate).toLocaleTimeString('ru-RU')}`}</div>
            <div>{item.credit === 0 ? '' : NumberWithSpaces(item.credit)}</div>
            <div>{item.debit === 0 ? '' : NumberWithSpaces(item.debit)}</div>
            <div>
                <Checkbox checked={item.status === 1} disabled />
            </div>
        </>
    )
}
