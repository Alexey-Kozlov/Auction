import React, { useEffect, useState } from 'react';
import { FaSearch } from 'react-icons/fa';
import { useDispatch, useSelector } from 'react-redux';
import { setParams } from '../../store/paramSlice';
import { useLocation, useNavigate } from 'react-router-dom';
import { setEventFlag } from '../../store/processingSlice';
import { RootState } from '../../store/store';

export default function Search() {
    const [search, setSearch] = useState('');
    const [searchAdv, setSearchAdv] = useState('');
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const location = useLocation();

    const params = useSelector((state: RootState) => state.paramStore);

    const onSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setSearch(event.target.value);
        setSearchAdv("");
    };

    const Search = () => {
        if (location.pathname !== '/') navigate('/');
        dispatch(setParams({ searchTerm: search }));
    }

    const onAdvSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setSearchAdv(event.target.value);
        setSearch("");
    };

    const AdvSearch = () => {
        if (location.pathname !== '/') navigate('/');
        if(params.searchAdv === searchAdv){
            return;
        }
        dispatch(setParams({ searchAdv: searchAdv }));
        dispatch(setEventFlag({ eventName: 'ElkSearch', ready: true}));
    }
//для сброса значений поиска при щелчке на сброс фильтров
    useEffect(() => {
        setSearch(params.searchTerm);
        setSearchAdv(params.searchAdv);
    }, [params.searchTerm, params.searchAdv])

    return (
        <div className='flex'>
        <div className='flex items-center border-2 rounded-full py-2 shadow-sm w-[350px]'>
            <input
                type='text'
                placeholder='Поиск по точному совпадению'
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
        <div className='flex border-2 rounded-full py-2 shadow-sm w-[350px] ml-2'>
        <input
                type='text'
                placeholder='Расширенный поиск'
                className='input-custom text-sm text-gray-600'
                value={searchAdv}
                onChange={e => onAdvSearchChange(e)}
                onKeyDown={(e: any) => {
                    if (e.key === 'Enter') AdvSearch();
                }}
            />
        <button 
                onClick={() => AdvSearch()}
            >
                <FaSearch size={34} className='bg-blue-400 text-white rounded-full p-2 cursor-pointer mx-2' />
            </button>
        </div>
        </div>
    )
}
