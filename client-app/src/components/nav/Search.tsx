import React from 'react';
import { FaSearch } from 'react-icons/fa';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { setSearchValue } from '../../store/searchSlice';

export default function Search() {
    const dispatch = useDispatch();
    const searchValue = useSelector((state: RootState) => state.searchStore.searchValue);

    const onSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        dispatch(setSearchValue(event.target.value));
    };

    const Search = () => {
        console.log(searchValue);
    }
    return (
        <div className='flex items-center border-2 rounded-full py-2 shadow-sm' style={{ width: '600px' }}>
            <input
                type='text'
                placeholder='Поиск предложения по производителю, модели или цвету'
                className='input-custom text-sm text-gray-600'
                value={searchValue}
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
