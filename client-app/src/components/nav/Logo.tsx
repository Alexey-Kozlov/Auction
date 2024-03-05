import { AiOutlineCar } from 'react-icons/ai';
import { NavLink } from 'react-router-dom';

export default function Logo() {
    return (
        <NavLink
            className='flex items-center gap-2 text-3xl font-semibold text-red-500'
            to='/'
        >
            <AiOutlineCar size={34} />
            <div>Аукцион</div>
        </NavLink>
    )
}
