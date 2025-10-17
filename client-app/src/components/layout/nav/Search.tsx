import React, { useEffect, useState } from 'react';
import { FaSearch } from 'react-icons/fa';
import { useDispatch, useSelector } from 'react-redux';
import { setParams } from '../../../store/paramSlice';
import { useLocation, useNavigate } from 'react-router-dom';
import { setEventFlag } from '../../../store/processingSlice';
import { RootState } from '../../../store/store';
import api from '../../../api/AuctionApi';

export default function Search() {
  const [search, setSearch] = useState('');
  const [searchAdv, setSearchAdv] = useState('');
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();

  const params = useSelector((state: RootState) => state.paramStore);

  const onSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(event.target.value);
    setSearchAdv('');
  };

  //поиск в сервисе Search средствами PostgreSql БД
  const Search = () => {
    if (!search) return;
    //функцией api.util.resetApiState() - полностью удаляем кеш RTK для списка аукционов,чтобы были только свежие данные
    dispatch(api.util.resetApiState());
    //выставляем параметр поиска - после этого срабатывает обновление отображаемых аукционов в Listings.tsx
    dispatch(setParams({ searchTerm: search, searchAdv: '' }));
    if (location.pathname !== '/') {
      navigate('/');
    }
  };

  const onAdvSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchAdv(event.target.value);
    setSearch('');
  };

  //поиск в сервисе Search, потом поисковая строка уходит в елку
  const AdvSearch = () => {
    if (!searchAdv) return;
    dispatch(api.util.resetApiState());
    //выставляем параметр поиска - после этого срабатывает обновление отображаемых аукционов в Listings.tsx
    dispatch(setParams({ searchAdv: searchAdv, searchTerm: '' }));
    //выставляем флаг для скрытия страницы с аукционами и выставления иконки ожидания, т.к. поиск будет асинхронным
    dispatch(setEventFlag({ eventName: 'ElkSearch', ready: true }));
    if (location.pathname !== '/') {
      navigate('/');
    }
  };

  //для сброса значений поиска при щелчке на сброс фильтров
  useEffect(() => {
    dispatch(api.util.resetApiState());
    setSearch(params.searchTerm ? params.searchTerm : '');
    setSearchAdv(params.searchAdv ? params.searchAdv : '');
    // eslint-disable-next-line
  }, [params.searchTerm, params.searchAdv]);

  return (
    <div className="SearchContainer">
      <div className="SearchHeader">
        <input
          type="text"
          placeholder="Поиск по точному совпадению"
          className="SearchInput"
          value={search}
          onChange={(e) => onSearchChange(e)}
          onKeyDown={(e: any) => {
            if (e.key === 'Enter') Search();
          }}
        />
        <button className="SearchButton" onClick={() => Search()}>
          <FaSearch size={40} className="SearchIcon" />
        </button>
      </div>
      <div className="SearchHeader">
        <input
          type="text"
          placeholder="Расширенный поиск"
          className="SearchInput"
          value={searchAdv}
          onChange={(e) => onAdvSearchChange(e)}
          onKeyDown={(e: any) => {
            if (e.key === 'Enter') AdvSearch();
          }}
        />
        <button className="SearchButton" onClick={() => AdvSearch()}>
          <FaSearch size={40} className="SearchIconAdv" />
        </button>
      </div>
    </div>
  );
}
