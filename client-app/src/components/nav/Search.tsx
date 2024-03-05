import React, { useEffect, useRef, useState } from 'react';
import { FaSearch } from 'react-icons/fa';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { setParams } from '../../store/paramSlice';
import { useLocation, useNavigate } from 'react-router-dom';

export default function Search() {
    const [search, setSearch] = useState('');
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const location = useLocation();
    const searchTerm = useSelector((state: RootState) => state.paramStore.searchTerm);

    const onSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setSearch(event.target.value);
    };

    const Search = () => {
        if (location.pathname != '/') navigate('/');
        dispatch(setParams({ searchTerm: search }));
    }

    useEffect(() => {
        setSearch(searchTerm);
    }, [searchTerm])

    return (
        <div className='flex items-center border-2 rounded-full py-2 shadow-sm' style={{ width: '600px' }}>
            <input
                type='text'
                placeholder='Поиск предложения по производителю, модели или цвету'
                className='input-custom text-sm text-gray-600'
                value={search}
                onChange={e => onSearchChange(e)}
                onKeyDown={(e: any) => {
                    if (e.key === 'Enter') Search();
                }}
            />
            <button
                onClick={() => Search()}
            >
                <FaSearch size={34} className='bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2' />
            </button>
        </div>
    )
}
