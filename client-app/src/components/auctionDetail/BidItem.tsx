import { format } from 'date-fns'
import { Bid } from '../../store/types';
import NumberWithSpaces from '../../utils/NumberWithSpaces';

type Props = {
    bid: Bid
}

export default function BidItem({ bid }: Props) {
    const getBidInfo = () => {
        let bgColor = '';
        let text = '';
        switch (bid.bidStatus) {
            case "Принято":
                bgColor = 'bg-green-200';
                text = 'Предложение принято';
                break;
            case 'ПринятоНижеНачальнойСтавки':
                bgColor = 'bg-orange-400';
                text = 'Предложение не соответствует начальной ставке';
                break;
            case 'СлишкомНизкая':
                bgColor = 'bg-red-200';
                text = 'Предложение недостаточно высокое';
                break;
            default:
                bgColor = 'bg-red-200';
                text = 'Предложение поступило после завершения аукциона';
                break;
        }
        return { bgColor, text };
    }

    return (
        <div className={`
            border-gray-300 border-2 px-3 py-2 rounded-lg flex justify-between
            items-center mb-2 ${getBidInfo().bgColor}
        `}>
            <div className='flex flex-col'>
                <span>Покупатель: {bid.bidder}</span>
                <span className='text-gray-700 text-sm'>Время: {format(new Date(bid.bidTime), 'dd.MM.yyyy HH:mm')}</span>
            </div>
            <div className='flex flex-col text-right'>
                <div className='text-xl font-semibold'>{NumberWithSpaces(bid.amount)} руб</div>
                <div className='flex flex-row items-center'>
                    <span>{getBidInfo().text}</span>
                </div>
            </div>

        </div>
    )
}
