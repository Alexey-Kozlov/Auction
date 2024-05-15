import toast from 'react-hot-toast';
import { useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import { useGetBalanceQuery } from '../../api/FinanceApi';
import { BsInfoCircle } from "react-icons/bs";
import { Message } from '../../store/types';

type Props = {
    message: Message;
    toastId: string;
}

export default function FinanceUnsufficientToast({ message, toastId }: Props) {
    const bidAuction = useGetDetailedViewDataQuery(message.id);
    const balance = useGetBalanceQuery(null);
    return (
        <div>
            {!bidAuction.isLoading && !balance.isLoading && (
                <>
                    <div className='flex flex-row-reverse' >
                        <button onClick={() => toast.dismiss(toastId)}>X</button>
                    </div>
                    <div className='flex flex-col items-center'>
                        <div className='flex flex-row  gap-2'>
                            <div>
                                <BsInfoCircle size={36} />
                            </div>
                            <span>{`Ошибка создания новой ставки для аукциона "${bidAuction.data?.result.title}" 
                                - недостаточно средств, в наличии имеются ${balance.data?.result}`}</span>
                        </div>
                    </div>
                </>
            )}
        </div>
    )
}
