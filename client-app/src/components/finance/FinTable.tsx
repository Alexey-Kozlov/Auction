import { FinanceItem } from '../../store/types'
import FinRow from './FinRow';

type Props = {
    items: FinanceItem[];
}

export default function FinTable({ items }: Props) {
    return (
        <div className='grid grid-cols-5 gap-6 justify-items-center'>
            <>
                <div>Баланс</div>
                <div>Дата</div>
                <div>Поступления</div>
                <div>Расходы</div>
                <div>Текущая ставка</div>
            </>
            {items.map((item: FinanceItem, index: number) => <FinRow key={index} item={item} />)}
        </div>
    )
}
