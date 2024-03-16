import { NavLink } from 'react-router-dom';
import { Auction } from '../../store/types';
import CarImage from './CarImage';
import CountdownTimer from './CountDownTimer';
import CurrentBid from './CurrentBid';

type Props = {
    auction: Auction;
}

export default function AuctionCard({ auction }: Props) {
    return (
        <NavLink to={`/auctions/${auction.id}`} className='group'>
            <div className='w-full bg-gray-200 aspect-w-16 aspect-h-10 rounded-lg overflow-hidden'>
                <div>
                    <CarImage id={auction.id} />
                    <div className='absolute bottom-2 left-2'>
                        <CountdownTimer auctionEnd={auction.auctionEnd} />
                    </div>
                    <div className='absolute top-2 right-2'>
                        <CurrentBid reservePrice={auction.reservePrice} amount={auction.currentHighBid} />
                    </div>
                </div>
            </div>
            <div className='flex justify-between items-center mt-4`'>
                <h3 className='text-gray-700'>{auction?.make} {auction.model}</h3>
                <p className='font-semibold text-sm'>{auction.year}</p>
            </div>
        </NavLink>
    )
}
