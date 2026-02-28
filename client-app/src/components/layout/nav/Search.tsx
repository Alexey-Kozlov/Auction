import React, { useEffect, useState } from 'react';
import { FaSearch } from 'react-icons/fa';
import { useDispatch, useSelector } from 'react-redux';
import { setParams } from '../../../store/paramSlice';
import { useLocation, useNavigate } from 'react-router-dom';
import { setEventFlag } from '../../../store/processingSlice';
import { RootState } from '../../../store/store';
import api from '../../../api/AuctionApi';
import { ProcessingState, SignalREvents, User } from '../../../types';
import { CheckEventReady } from '../../../utils/checkEvent';
import { setAuthUser } from '../../../store/authSlice';

export default function Search() {
  const [search, setSearch] = useState('');
  const [searchAdv, setSearchAdv] = useState('');
  const [showSearch, setShowSearch] = useState(true);
  const settings = useSelector((state: RootState) => state.settingsStore);
  const user: User = useSelector((state: RootState) => state.authStore);
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );
  const params = useSelector((state: RootState) => state.paramStore);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();

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
    dispatch(
      setParams({
        searchTerm: search,
        searchAdv: '',
        pageNumber: 1,
        firstPage: 0,
      }),
    );
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
    dispatch(
      setParams({
        searchAdv: searchAdv,
        searchTerm: '',
        pageNumber: 1,
        firstPage: 0,
      }),
    );
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

  //отключение панели поиска для админского режима (кроме администраторов)
  useEffect(() => {
    setShowSearch(() => !(settings.adminMode && !user.isAdmin));
    // eslint-disable-next-line
  }, [settings, user]);

  //обновление видимости контрола
  useEffect(() => {
    if (
      !CheckEventReady(
        procState,
        SignalREvents[SignalREvents.SetCurrentSettings],
      )
    ) {
      dispatch(
        setAuthUser({
          name: user.name,
          login: user.login,
          isGuest: user.isGuest,
          isAdmin: user.isAdmin,
        }),
      );
    }
    // eslint-disable-next-line
  }, [procState]);

  return (
    <>
      {showSearch && (
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
      )}
    </>
  );
}
