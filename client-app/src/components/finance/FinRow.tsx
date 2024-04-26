import { FinanceItem } from '../../store/types';
import NumberWithSpaces from '../../utils/NumberWithSpaces';
import { Checkbox } from 'flowbite-react';

type Props = {
    item: FinanceItem;
}

export default function FinRow({ item }: Props) {
    return (
        <>
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
