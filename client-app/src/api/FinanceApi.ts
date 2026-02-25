import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import {
  ApiResponseNet,
  FinanceTableItem,
  PagedResult,
  RequestType,
} from '../types';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/postApiProcess';
import AddTokenHeader from './AddTokenHeader';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/getCurrentUser';

const financeApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: 'financeApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/finance`,
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
  tagTypes: ['finance'],
  endpoints: (builder) => ({
    getFinanceItem: builder.query<
      ApiResponseNet<PagedResult<FinanceTableItem>>,
      string
    >({
      query: (url) => ({
        url: '/gethistory' + url,
        headers: {
          RequestType: RequestType[RequestType.Finance],
        },
      }),
      transformResponse: (
        response: ApiResponseNet<PagedResult<FinanceTableItem>>,
        meta: any,
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['finance'],
    }),
    getBalance: builder.query<ApiResponseNet<number>, null>({
      query: () => ({
        url: '/getbalance',
        headers: {
          RequestType: RequestType[RequestType.Balance],
        },
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['finance'],
    }),
  }),
});

export const { useGetFinanceItemQuery, useGetBalanceQuery } = financeApi;
export default financeApi;
