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

export default function Listings() {
    const dispatch = useDispatch();
    const params = useSelector((state: RootState) => state.paramStore);
    const url = qs.stringifyUrl({ url: '', query: params });
    const { data, isLoading } = useGetAuctionsQuery(url);
    useEffect(() => {
        if (!isLoading) {
            dispatch(setData(data));
        }
    }, [isLoading, url]);

    function setPageNumber(pageNumber: number) {
        dispatch(setParams({ pageNumber: pageNumber }));
    }

    if (isLoading) return <h3>Загрузка...</h3>

    return (
        <div>
            <Filters />
            {
                data?.totalCount == 0 ? (
                    <EmptyFilter showReset />
                ) : (<>
                    <div className='grid grid-cols-4 gap-6'>
                        {data?.results.map((auction, index: number) => {
                            return (
                                <AuctionCard auction={auction} key={index} />
                            )
                        })}
                    </div>
                    <div className='flex justify-center mt-4'>
                        <AppPagination pageChanged={setPageNumber}
                            currentPage={params.pageNumber} totalPages={data?.pageCount!} />
                    </div>
                </>
                )}

        </div>
    )

}
