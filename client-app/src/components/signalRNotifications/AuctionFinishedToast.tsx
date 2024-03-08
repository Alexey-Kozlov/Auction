import toast from 'react-hot-toast';
import { Auction, AuctionFinished } from '../../store/types';
import { NavLink } from 'react-router-dom';
const empty = require('../../assets/Empty.png');

type Props = {
    finishedAuction: AuctionFinished;
    auction: Auction;
}

export default function AuctionFinishedToast({ finishedAuction, auction }: Props) {
    return (
        <div>
            <div className='flex flex-row-reverse' >
                <button onClick={() => toast.dismiss()}>X</button>
            </div>
            <NavLink to={`/auctions/${auction.id}`} className='flex flex-col items-center'>
                <div className='flex flex-row items-center gap-2'>
                    <img
                        src={auction.image ? `data:image/png;base64 , ${auction.image}` : empty}
                        alt=''
                        height={80}
                        width={80}
                        className='rounded-lg'
                    />
                    <div className='flex flex-col'>
                        <span>Аукцион {auction.make} {auction.model} был завершен!</span>
                        {finishedAuction.itemSold && finishedAuction.amount ? (
                            <p>Поздравления для победителя аукциона {finishedAuction.winner},
                                итоговая стоимость лота - ${finishedAuction.amount} руб</p>
                        ) : (
                            <p>Лот не был продан</p>
                        )}
                    </div>
                </div>
            </NavLink>
        </div>

    )
}
