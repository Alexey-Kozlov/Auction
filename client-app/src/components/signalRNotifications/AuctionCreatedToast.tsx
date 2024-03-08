import toast from 'react-hot-toast';
import { Auction } from '../../store/types';
import { NavLink } from 'react-router-dom';
const empty = require('../../assets/Empty.png');

type Props = {
    auction: Auction;
    toastId: string;
}

export default function AuctionCreatedToast({ auction, toastId }: Props) {
    return (
        <div>
            <div className='flex flex-row-reverse' >
                <button onClick={() => toast.dismiss(toastId)}>X</button>
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
                    <span>Новый аукцион {auction.make} {auction.model} был создан</span>
                </div>
            </NavLink>
        </div>

    )
}
