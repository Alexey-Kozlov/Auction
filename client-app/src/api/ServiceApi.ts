import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import AddTokenHeader from './AddTokenHeader';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/PostApiProcess';
import { ApiResponseNet, RequestType, RestoreDb } from '../types';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/GetCurrentUser';

const serviceApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: 'serviceApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + '/api/processing',
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
  tagTypes: ['service'],
  endpoints: (builder) => ({
    elkIndex: builder.mutation<ApiResponseNet<number>, null>({
      query: () => ({
        url: '/elkindex',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.ELK],
        },
        body: {},
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['service'],
    }),
    setSnapShot: builder.mutation<ApiResponseNet<number>, null>({
      query: () => ({
        url: '/setsnapshot',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.SnapShot],
        },
        body: {},
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['service'],
    }),
    RestoreSnapShot: builder.mutation<ApiResponseNet<number>, RestoreDb>({
      query: (params) => ({
        url: '/restoresnapshot',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.SnapShot],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['service'],
    }),
    resetImageCache: builder.mutation<ApiResponseNet<{}>, null>({
      query: () => ({
        url: `/resetimagecache`,
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Cache],
        },
        body: {},
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['service'],
    }),
    setUsersCurrentPage: builder.mutation<ApiResponseNet<{}>, string>({
      query: (url) => ({
        url: `/setuserscurrentpage`,
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.UsersCurrentPage],
        },
        body: JSON.stringify(url),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['service'],
    }),
  }),
});

export const {
  useElkIndexMutation,
  useSetSnapShotMutation,
  useRestoreSnapShotMutation,
  useResetImageCacheMutation,
  useSetUsersCurrentPageMutation,
} = serviceApi;
export default serviceApi;
