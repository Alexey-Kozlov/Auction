import { format } from 'date-fns'
import { Bid } from '../../store/types';
import NumberWithSpaces from '../../utils/NumberWithSpaces';
import { useGetUserNameQuery } from '../../api/AuthApi';

type Props = {
    bid: Bid
}

export default function BidItem({ bid }: Props) {
    const { data, isLoading } = useGetUserNameQuery(bid.bidder, {
        skip: !bid.bidder
    });

    return (
        <div className='border-gray-300 border-2 px-3 py-2 rounded-lg flex justify-between
            items-center mb-2 bg-green-200'>
            <div className='flex flex-col'>
                <span>Покупатель: {!isLoading && data?.result}</span>
                <span className='text-gray-700 text-sm'>Время: {format(new Date(bid?.bidTime), 'dd.MM.yyyy HH:mm')}</span>
            </div>
            <div className='flex flex-col text-right'>
                <div className='text-xl font-semibold'>{NumberWithSpaces(bid?.amount)} руб</div>
                <div className='flex flex-row items-center'>
                    <span>Предложение принято</span>
                </div>
            </div>

        </div>
    )
}
