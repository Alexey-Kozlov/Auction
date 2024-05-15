import { RiAuctionLine } from "react-icons/ri";
import { NavLink } from 'react-router-dom';

export default function Logo() {
    return (
        <NavLink
            className='flex items-center gap-2 text-3xl font-semibold text-red-500'
            to='/'
        >
            <RiAuctionLine size={34} />
            <div>Аукцион</div>
        </NavLink>

    )
}
