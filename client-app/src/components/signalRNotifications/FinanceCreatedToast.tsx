import toast from 'react-hot-toast';
import { FinanceItem } from '../../store/types';
import { NavLink } from 'react-router-dom';
import { GrMoney } from "react-icons/gr";
const empty = require('../../assets/Empty.png');

type Props = {
    finance: FinanceItem;
    toastId: string;
}

export default function FinanceCreatedToast({ finance, toastId }: Props) {
    return (
        <div>
            <div className='flex flex-row-reverse' >
                <button onClick={() => toast.dismiss(toastId)}>X</button>
            </div>
            <NavLink to={``} className='flex flex-col items-center'>
                <div className='flex flex-row items-center gap-2'>
                <GrMoney size={30}/>
                    <span>{`Пополнен баланс на  "${finance.value}" руб.`}</span>
                </div>
            </NavLink>
        </div>

    )
}
