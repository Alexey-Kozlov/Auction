import { RiAuctionLine } from "react-icons/ri";
import { useDispatch } from "react-redux";
import { NavLink, useNavigate } from 'react-router-dom';
import { setParams } from "../../store/paramSlice";

export default function Logo() {
    const dispatch = useDispatch();
    const handleClickHome = () => {
        dispatch(setParams({ searchTerm: '', searchAdv:'' }));
    }

    return (
        <NavLink onClick={handleClickHome}
            className='flex items-center gap-2 text-3xl font-semibold text-red-500'
            to='/'
        >
            <RiAuctionLine size={34} />
            <div>Аукцион</div>
        </NavLink>

    )
}
