import Logo from './Logo'
import Search from './Search'
import UserActions from './UserActions'
import { Button } from 'flowbite-react';
import { User } from '../../store/types'
import { useDispatch, useSelector } from 'react-redux'
import { RootState } from '../../store/store'
import { useNavigate } from 'react-router-dom'
import { setAuthUser } from '../../store/authSlice';

export default function NavBar() {
    let user: User = useSelector((state: RootState) => state.authStore);
    const dispatch = useDispatch();
    if (!user.id && localStorage.getItem('Auction')) {
        user = JSON.parse(localStorage.getItem('Auction')!);
        dispatch(setAuthUser(user));
    }
    const navigate = useNavigate();
    return (
        <header className='sticky top-0 z-50 flex justify-between bg-white p-5 
        items-center text-gray-800 shadow-md'>
            <Logo />
            <Search />
            {user.id ? (
                <UserActions />
            ) : (<div className='flex'>
                <Button outline onClick={e => navigate('/register')}>
                    Регистрация
                </Button>
                <Button className='ml-2' outline onClick={e => navigate('/login')}>
                    Логин
                </Button>
            </div>
            )}

        </header>
    )
}
