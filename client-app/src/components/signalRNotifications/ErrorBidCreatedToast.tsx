import toast from 'react-hot-toast';
import { AuctionImage } from '../../store/types';
import { NavLink } from 'react-router-dom';
import { useGetImageForAuctionQuery } from '../../api/ImageApi';
import { useGetDetailedViewDataQuery } from '../../api/AuctionApi';
import { useGetBalanceQuery } from '../../api/FinanceApi';
const empty = require('../../assets/Empty.png');

type Props = {
    auctionId: string;
    toastId: string;
}

export default function ErrorBidCreatedToast({ auctionId, toastId }: Props) {
    const { isLoading, data } = useGetImageForAuctionQuery(auctionId);
    const bidAuction = useGetDetailedViewDataQuery(auctionId);
    const balance = useGetBalanceQuery(null);
    return (
        <div>
            {!bidAuction.isLoading && !balance.isLoading && (
                <>
                    <div className='flex flex-row-reverse' >
                        <button onClick={() => toast.dismiss(toastId)}>X</button>
                    </div>
                    <NavLink to={`/auctions/${auctionId}`} className='flex flex-col items-center'>
                        <div className='flex flex-row items-center gap-2'>
                            <img src={!isLoading && (data?.result as AuctionImage)!.image ? (`data:image/png;base64 , ${data?.result['image']}`) : empty}
                                alt=''
                                height={80}
                                width={80}
                                className='rounded-lg'
                            />
                            <span>{`Ошибка создания новой ставки для аукциона "${bidAuction.data?.result.title}" - недостаточно средств,
                            в наличии имеются ${balance.data?.result}`}</span>
                        </div>
                    </NavLink>
                </>
            )}
        </div>
    )
}
