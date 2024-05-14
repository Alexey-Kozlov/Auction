import toast from 'react-hot-toast';
import { ErrorMessage } from '../../store/types';
import { NavLink } from 'react-router-dom';
import { useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import { useGetBalanceQuery } from '../../api/FinanceApi';
import { VscError } from "react-icons/vsc";

type Props = {
    errorMessage: ErrorMessage;
    toastId: string;
}

export default function ErrorMessageToast({ errorMessage, toastId }: Props) {
    const bidAuction = useGetDetailedViewDataQuery(errorMessage.id);
    const balance = useGetBalanceQuery(null);
    return (
        <div>
            {!bidAuction.isLoading && !balance.isLoading && (
                <>
                    <div className='flex flex-row-reverse' >
                        <button onClick={() => toast.dismiss(toastId)}>X</button>
                    </div>
                    <NavLink to={`/auctions/${errorMessage.id}`} className='flex flex-col items-center'>
                        <div className='flex flex-row  gap-2'>
                            <div>
                                <VscError size={36} />
                            </div>
                            {errorMessage.errorMessage.indexOf('не хватает денег') === -1 ?
                                <span>{errorMessage.errorMessage + ', аукцион - ' + bidAuction.data?.result.title}</span> :
                                <span>{`Ошибка создания новой ставки для аукциона "${bidAuction.data?.result.title}" - недостаточно средств,
                            в наличии имеются ${balance.data?.result}`}</span>
                            }
                        </div>
                    </NavLink>
                </>
            )}
        </div>
    )
}
