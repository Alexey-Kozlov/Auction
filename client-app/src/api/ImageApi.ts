import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ApiResponseNet, AuctionImage, RequestType } from '../types';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/postApiProcess';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/getCurrentUser';

const imageApi = createApi({
  reducerPath: 'imageApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/images`,
    prepareHeaders: (headers: Headers, api) => {
      headers.append(RequestType[RequestType.TraceId], uuid.v4() as string);
      headers.append('Content-type', 'application/json');
      headers.append('User', GetCurrentUser());
      return headers;
    },
  }),
  tagTypes: ['images'],
  endpoints: (builder) => ({
    getImageForAuction: builder.query<
      ApiResponseNet<AuctionImage>,
      { id: string; cache: boolean }
    >({
      query: (arg) => ({
        url: `/`,
        params: { id: arg.id, cache: arg.cache },
        headers: {
          RequestType: RequestType[RequestType.Image],
        },
      }),
      transformResponse: (
        response: ApiResponseNet<AuctionImage>,
        meta: any,
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['images'],
    }),
  }),
});

export const { useGetImageForAuctionQuery } = imageApi;
export default imageApi;
