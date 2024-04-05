import toast from 'react-hot-toast';
import { Auction, AuctionFinished, AuctionImage } from '../../store/types';
import { NavLink } from 'react-router-dom';
import { useGetImageForAuctionQuery } from '../../api/ImageApi';
const empty = require('../../assets/Empty.png');

type Props = {
    finishedAuction: AuctionFinished;
    auction: Auction;
}

export default function AuctionFinishedToast({ finishedAuction, auction }: Props) {
    const { isLoading, data } = useGetImageForAuctionQuery(auction.id);
    return (
        <div>
            <div className='flex flex-row-reverse' >
                <button onClick={() => toast.dismiss()}>X</button>
            </div>
            <NavLink to={`/auctions/${auction.id}`} className='flex flex-col items-center'>
                <div className='flex flex-row items-center gap-2'>
                    <img src={!isLoading && (data?.result as AuctionImage)!.image ? (`data:image/png;base64 , ${data?.result['image']}`) : empty}
                        alt=''
                        height={80}
                        width={80}
                        className='rounded-lg'
                    />
                    <div className='flex flex-col'>
                        <span>Аукцион {auction.title} был завершен!</span>
                        {finishedAuction.itemSold && finishedAuction.amount ? (
                            <p>{`Поздравления для победителя аукциона "${finishedAuction.winner}",
                                итоговая стоимость лота - ${finishedAuction.amount} руб.`}</p>
                        ) : (
                            <p>Лот не был продан</p>
                        )}
                    </div>
                </div>
            </NavLink>
        </div>

    )
}
