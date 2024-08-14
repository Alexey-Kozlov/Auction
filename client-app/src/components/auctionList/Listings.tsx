import { useEffect } from 'react'
import qs from 'query-string';
import { useGetAuctionsQuery } from '../../api/AuctionApi';
import EmptyFilter from './EmptyFilter';
import AuctionCard from './AuctionCard';
import AppPagination from './AddPagination';
import { useDispatch, useSelector } from 'react-redux';
import { setParams } from '../../store/paramSlice';
import { RootState } from '../../store/store';
import { setData } from '../../store/auctionSlice';
import Filters from './Filters';
import { Auction, ProcessingState } from '../../store/types';
import { setEventFlag } from '../../store/processingSlice';
import Waiter from '../Waiter';

export default function Listings() {
    const dispatch = useDispatch();
    const params = useSelector((state: RootState) => state.paramStore);
    const data = useSelector((state: RootState) => state.auctionStore);
    const auctions: Auction[] = data.auctions;
    const url = qs.stringifyUrl({ url: '', query: params });
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const elkSearch = procState.find(p => p.eventName === 'ElkSearch' && p.ready) && params.searchAdv;
    let auctionsData = useGetAuctionsQuery(url, {
        skip: !params.sessionId, 
        //если делаем поиск в ELK - отключаем кеширование, чтобы всегда был запрос 
        //и соответственно ответ. Иначе нарушится UI
        refetchOnMountOrArgChange: !!params.searchAdv
    });

    useEffect(() => {
        dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: false }));
    }, [dispatch])

    useEffect(() => {
        if (!auctionsData.isLoading && auctionsData.data) {
            dispatch(setData(auctionsData.data.result));
        }
    }, [auctionsData, dispatch]);

    useEffect(() => {
        const eventStateChanged = procState.find(p => p.eventName === 'CollectionChanged' && p.ready);
        if (eventStateChanged) {
            auctionsData.refetch();
            dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: false }));
        }
    }, [procState, auctionsData, dispatch]);

    function setPageNumber(pageNumber: number) {
        dispatch(setParams({ pageNumber: pageNumber }));
    }

    if (auctionsData.isLoading) return <h3>Загрузка...</h3>

    return (
        <div>
            <Filters />
            {
                !elkSearch ? (
                auctions.length === 0 ? (
                    <EmptyFilter showReset />
                ) : (<div>
                    <div className='grid grid-cols-4 gap-6 items-center'>
                        {auctions.map((auction: Auction, index: number) => {
                            return (
                                <AuctionCard auction={auction} key={index} />
                            )
                        })}
                    </div>
                    <div className='flex justify-center mt-4'>

                        <AppPagination pageChanged={setPageNumber}
                            currentPage={params.pageNumber} 
                            totalPages={auctionsData.data?.result ? 
                                auctionsData.data?.result.pageCount!
                                : data.pageCount
                            } />                                                
                    </div>
                </div>
                )
                ) : (
                    <Waiter color='rgb(156 163 175)' /> 
                )
            }
        </div>
    )

}
