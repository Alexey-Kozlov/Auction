
import { Dropdown } from 'flowbite-react';
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from 'react-icons/ai';
import { HiCog, HiUser } from 'react-icons/hi2';
import { NavLink, useNavigate } from 'react-router-dom';
import { User } from '../../store/types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { emptyUserState, setAuthUser } from '../../store/authSlice';

type Props = {
    user: Partial<any>;
}

export default function UserActions() {
    const user: User = useSelector((state: RootState) => state.authStore);
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const SetWinner = () => {

    }

    const SetSeller = () => {

    }

    const handleLogout = () => {
        localStorage.removeItem('Auction');
        dispatch(setAuthUser({ ...emptyUserState }));
        navigate('/');
    }

    return (
        <Dropdown inline label={`Здравствуйте ${user.name}`}>
            <Dropdown.Item icon={HiUser} onClick={SetSeller}>
                Мои аукционы
            </Dropdown.Item>
            <Dropdown.Item icon={AiFillTrophy} onClick={SetWinner}>
                Аукционы выигранные
            </Dropdown.Item>
            <Dropdown.Item icon={AiFillCar}>
                <NavLink to='/auctions/create'>Выставить на аукцион</NavLink>
            </Dropdown.Item>
            <Dropdown.Divider />
            <Dropdown.Item icon={AiOutlineLogout} onClick={handleLogout}>
                Выход
            </Dropdown.Item>
        </Dropdown>
    )
}