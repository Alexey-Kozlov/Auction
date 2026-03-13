import React, { useEffect, useState } from 'react';
import { FaSearch } from 'react-icons/fa';
import { useDispatch, useSelector } from 'react-redux';
import { setParams } from '../../../store/paramSlice';
import { useLocation, useNavigate } from 'react-router-dom';
import { setEventFlag } from '../../../store/processingSlice';
import { RootState } from '../../../store/store';
import api from '../../../api/AuctionApi';
import {
  ProcessingState,
  SignalREvents,
  ToastType,
  User,
} from '../../../types';
import { CheckEventReady } from '../../../utils/checkEvent';
import { setAuthUser } from '../../../store/authSlice';
import { InputText } from 'primereact/inputtext';
import { Tooltip } from 'primereact/tooltip';
import { Checkbox, CheckboxChangeEvent } from 'primereact/checkbox';
import { Toast } from 'primereact/toast';
import MessageToast from '../../signalRNotifications/MessageToast';

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
  const toastMessage: Toast | null = useSelector(
    (state: RootState) => state.serviceStore,
  ).toast;
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const [advSearchItems, setAdvSearchItems] = useState<string[]>([
    'Auction',
    'Comment',
    'Tag',
  ]);

  const onSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(event.target.value);
    setSearchAdv('');
  };

  //поиск в сервисе Search средствами PostgreSql БД
  const Search = () => {
    if (!search) {
      toastMessage!.show({
        severity: 'success',
        life: 5000,
        className: 'bg-white',
        content: (props) => (
          <MessageToast
            message="Необходимо указать строку поиска"
            toastType={ToastType.Error}
          />
        ),
      });
      return;
    }
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

  //поиск в сервисе Search с помощью елки
  const AdvSearch = () => {
    if (!searchAdv) {
      toastMessage!.show({
        severity: 'success',
        life: 5000,
        className: 'bg-white',
        content: (props) => (
          <MessageToast
            message="Необходимо указать строку расширенного поиска"
            toastType={ToastType.Error}
          />
        ),
      });
      return;
    }
    if (advSearchItems.length === 0) {
      toastMessage!.show({
        severity: 'success',
        life: 5000,
        className: 'bg-white',
        content: (props) => (
          <MessageToast
            message="Необходимо выбрать хотя бы одну область поиска - Аукционы, Обсуждения или Теги "
            toastType={ToastType.Error}
          />
        ),
      });
      return;
    }

    //функцией api.util.resetApiState() - полностью удаляем кеш RTK, иначе в пейджинге на других
    //страницах останутся старые данные
    dispatch(api.util.resetApiState());
    //выставляем параметр поиска - после этого срабатывает обновление отображаемых аукционов в Listings.tsx
    dispatch(
      setParams({
        searchAdv: searchAdv,
        advSearchParam: advSearchItems,
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

  //изменили значение параметров расширенного поиска (чек-боксы)
  const onAdvSearchChanged = (e: CheckboxChangeEvent) => {
    let _advSearchItems = [...advSearchItems];
    if (e.checked) {
      _advSearchItems.push(e.value);
    } else {
      _advSearchItems.splice(_advSearchItems.indexOf(e.value), 1);
    }
    setAdvSearchItems(_advSearchItems);
  };

  return (
    <>
      {showSearch && (
        <div className="SearchContainer">
          <div className="SearchHeader">
            <InputText
              id="sqlsearch"
              placeholder="Поиск по частичному совпадению"
              className="SearchInput"
              value={search}
              onChange={(e) => onSearchChange(e)}
              onKeyDown={(e: any) => {
                if (e.key === 'Enter') Search();
              }}
              data-pr-position="bottom"
              data-pr-at="center+24 bottom+10"
            />
            <button className="SearchButton" onClick={() => Search()}>
              <FaSearch size={40} className="SearchIcon" />
            </button>
            <Tooltip target="#sqlsearch" className="SearchToolTip" event="both">
              Будет проводится поиск по частичному совпадению в описании
              аукционов
            </Tooltip>
          </div>
          <div className="SearchHeader">
            <InputText
              id="advsearch"
              placeholder="Расширенный поиск"
              className="SearchInput"
              value={searchAdv}
              onChange={(e) => onAdvSearchChange(e)}
              onKeyDown={(e: any) => {
                if (e.key === 'Enter') AdvSearch();
              }}
              data-pr-position="bottom"
              data-pr-at="center+24 bottom+10"
            />
            <button className="SearchButton" onClick={() => AdvSearch()}>
              <FaSearch size={40} className="SearchIconAdv" />
            </button>
            <Tooltip target="#advsearch" className="SearchToolTip">
              Будет проводится расширенный поиск по указанным параметрам
            </Tooltip>
          </div>
          <div className="SearchHeaderAdv">
            <div className="SearchHeaderAdvItem">
              <Checkbox
                inputId="auctionAdvSearch"
                value="Auction"
                onChange={onAdvSearchChanged}
                checked={advSearchItems.includes('Auction')}
              />
              <label htmlFor="auctionAdvSearch" className="ml-2 text-4xl">
                Аукционы
              </label>
            </div>
            <div className="SearchHeaderAdvItem">
              <Checkbox
                inputId="commentAdvSearch"
                value="Comment"
                onChange={onAdvSearchChanged}
                checked={advSearchItems.includes('Comment')}
              />
              <label htmlFor="commentAdvSearch" className="ml-2 text-4xl">
                Обсуждения
              </label>
            </div>
            <div className="SearchHeaderAdvItem">
              <Checkbox
                inputId="tagAdvSearch"
                value="Tag"
                onChange={onAdvSearchChanged}
                checked={advSearchItems.includes('Tag')}
              />
              <label htmlFor="tagAdvSearch" className="ml-2 text-4xl">
                Теги
              </label>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
