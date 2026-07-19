import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ApiResponseNet, HistoryItem, RequestType } from '../types';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/postApiProcess';
import AddTokenHeader from './AddTokenHeader';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/getCurrentUser';

const historyApi = createApi({
  reducerPath: 'historyApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/history`,
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
  tagTypes: ['history'],
  endpoints: (builder) => ({
    getHistoryList: builder.query<ApiResponseNet<HistoryItem[]>, {}>({
      query: (id) => ({
        url: `/${id}`,
        headers: {
          RequestType: RequestType[RequestType.History],
        },
      }),
      transformResponse: (
        response: ApiResponseNet<HistoryItem[]>,
        meta: any,
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['history'],
    }),
  }),
});

export const { useGetHistoryListQuery } = historyApi;
export default historyApi;
