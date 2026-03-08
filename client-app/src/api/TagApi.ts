import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import {
  ApiResponseNet,
  CurrentSettings,
  RequestType,
  TagItem,
  TagList,
} from '../types';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/postApiProcess';
import AddTokenHeader from './AddTokenHeader';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/getCurrentUser';

const tagApi = createApi({
  reducerPath: 'tagApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/tag`,
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
  tagTypes: ['tag'],
  endpoints: (builder) => ({
    getTagList: builder.query<ApiResponseNet<TagList[]>, {}>({
      query: () => ({
        url: '/GetTagList',
        headers: {
          RequestType: RequestType[RequestType.Tag],
        },
      }),
      transformResponse: (response: ApiResponseNet<TagList[]>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['tag'],
    }),
    getAuctionTags: builder.query<ApiResponseNet<TagList[]>, string>({
      query: (auctionid) => ({
        url: `/GetAuctionTags/${auctionid}`,
        headers: {
          RequestType: RequestType[RequestType.Tag],
        },
      }),
      transformResponse: (response: ApiResponseNet<TagList[]>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['tag'],
    }),
  }),
});

export const { useGetTagListQuery, useGetAuctionTagsQuery } = tagApi;
export default tagApi;
