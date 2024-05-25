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
        <div className='h-full relative flex items-center justify-center'>
            <NavLink to={`/auctions/${auction.id}`} className='group'>
                <div className='rounded-lg group-hover:drop-shadow-4xl duration-700 '>
                    <div>
                        <ImageCard id={auction.id} dopStyle=' max-h-60' zooming={false} />
                    </div>
                </div>
                <div className='flex justify-center mb-12 mt-2'>
                    <div className='backdrop-brightness-200 p-2 rounded-xl
                border-2 border-white group-hover:border-gray-700 duration-700'>
                        <h3 className='text-gray-700'>{auction.title}</h3>
                    </div>
                </div>
            </NavLink>
            <div className='absolute bottom-2 left-2'>
                <CountdownTimer auctionEnd={auction.auctionEnd} />
            </div>
            <div className='absolute top-2 right-2'>
                <CurrentBid reservePrice={auction.reservePrice} amount={auction.currentHighBid} />
            </div>
        </div>
    )
}
