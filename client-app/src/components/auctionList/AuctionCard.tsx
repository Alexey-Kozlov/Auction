import { NavLink } from 'react-router-dom';
import { Auction } from '../../store/types';
import ImageCard from './ImageCard';
import CountdownTimer from './CountDownTimer';
import CurrentBid from './CurrentBid';

type Props = {
    auction: Auction;
}

export default function AuctionCard({ auction }: Props) {
    return (
        <NavLink to={`/auctions/${auction.id}`} className='group'>
            <div className='rounded-lg'>
                <div className='flex justify-center relative'>
                    <ImageCard id={auction.id} dopStyle=' max-h-60' />
                    <div className='absolute bottom-2 left-2'>
                        <CountdownTimer auctionEnd={auction.auctionEnd} />
                    </div>
                    <div className='absolute top-2 right-2'>
                        <CurrentBid reservePrice={auction.reservePrice} amount={auction.currentHighBid} />
                    </div>
                </div>
            </div>
            <div className='flex justify-center items-center mt-4`'>
                <h3 className='text-gray-700'>{auction.title}</h3>
            </div>
        </NavLink>
    )
}
