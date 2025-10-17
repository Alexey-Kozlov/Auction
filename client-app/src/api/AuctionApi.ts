import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { Auction, PagedResult, ApiResponseNet, RequestType } from '../types';
import AddTokenHeader from './AddTokenHeader';
import { PostApiProcess, PostErrorApiProcess } from '../utils/PostApiProcess';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/GetCurrentUser';

const auctionApi = createApi({
  reducerPath: 'auctionApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + '/api',
    prepareHeaders: (headers: Headers, api) => {
      const token = AddTokenHeader();
      if (token) {
        headers.append('Authorization', token);
      }
      headers.append(RequestType[RequestType.TraceId], uuid.v4() as string);
      headers.append('Content-type', 'application/json');
      headers.append('User', GetCurrentUser());
      return headers;
    },
  }),
  tagTypes: ['auctions'],
  endpoints: (builder) => ({
    getDetailedViewData: builder.query<ApiResponseNet<Auction>, string>({
      query: (itemId) => ({
        url: `/search/${itemId}`,
        headers: {
          RequestType: RequestType[RequestType.ReadDetail],
        },
      }),
      transformResponse: (response: ApiResponseNet<Auction>, meta: any) => {
        PostApiProcess(response);
        if (response.isSuccess) {
          if (response.result) {
            if (response.result.auctionEnd)
              response.result.auctionEnd = new Date(response.result.auctionEnd);
            if (response.result.createAt)
              response.result.createAt = new Date(response.result.createAt);
            if (response.result.updatedAt)
              response.result.updatedAt = new Date(response.result.updatedAt);
          }
        }
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ['auctions'],
    }),
    getAuctions: builder.query<ApiResponseNet<PagedResult<Auction>>, string>({
      query: (url) => ({
        url: '/search' + url,
        headers: {
          RequestType: RequestType[RequestType.ReadList],
        },
      }),
      transformResponse: (
        response: ApiResponseNet<PagedResult<Auction>>,
        meta: any,
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ['auctions'],
    }),
  }),
});

export const { useGetAuctionsQuery, useGetDetailedViewDataQuery } = auctionApi;
export default auctionApi;
