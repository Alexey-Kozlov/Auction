import React, { useEffect } from 'react'
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

export default function Listings() {
    const dispatch = useDispatch();
    const params = useSelector((state: RootState) => state.paramStore);
    const auctions: Auction[] = useSelector((state: RootState) => state.auctionStore).auctions;
    const url = qs.stringifyUrl({ url: '', query: params });
    const auctionsData = useGetAuctionsQuery(url);
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);

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
        }
    }, [procState, auctionsData]);

    function setPageNumber(pageNumber: number) {
        dispatch(setParams({ pageNumber: pageNumber }));
    }

    if (auctionsData.isLoading) return <h3>Загрузка...</h3>

    return (
        <div>
            <Filters />
            {
                auctions.length === 0 ? (
                    <EmptyFilter showReset />
                ) : (<>
                    <div className='grid grid-cols-4 gap-6'>
                        {auctions.map((auction: Auction, index: number) => {
                            return (
                                <AuctionCard auction={auction} key={index} />
                            )
                        })}
                    </div>
                    <div className='flex justify-center mt-4'>
                        <AppPagination pageChanged={setPageNumber}
                            currentPage={params.pageNumber} totalPages={auctionsData.data?.result.pageCount!} />
                    </div>
                </>
                )}

        </div>
    )

}
