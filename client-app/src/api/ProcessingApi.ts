import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import {
  ApiResponseNet,
  AuctionDeleted,
  AuctionUpdated,
  CurrentSettings,
  FinanceCreate,
  NotifyUser,
  PlaceBidParams,
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

const processingApi = createApi({
  reducerPath: 'processingApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/processing`,
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
  tagTypes: ['processing'],
  endpoints: (builder) => ({
    placeBidForAuction: builder.mutation<any, PlaceBidParams>({
      query: (params) => ({
        url: '/placebid',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Bids],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['processing'],
    }),
    createAuction: builder.mutation<ApiResponseNet<{}>, AuctionUpdated>({
      query: (params) => ({
        url: '/createauction',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Create],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['processing'],
    }),
    updateAuction: builder.mutation<ApiResponseNet<{}>, AuctionUpdated>({
      query: (params) => ({
        url: '/updateauction',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Edit],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['processing'],
    }),
    deleteAuction: builder.mutation<ApiResponseNet<{}>, AuctionDeleted>({
      query: (params) => ({
        url: `/deleteauction`,
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Delete],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['processing'],
    }),
    financeCreate: builder.mutation<ApiResponseNet<{}>, FinanceCreate>({
      query: (params) => ({
        url: '/financecreate',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Finance],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['processing'],
    }),
    setNotifyUser: builder.mutation<ApiResponseNet<{}>, NotifyUser>({
      query: (params) => ({
        url: '/editnotification',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Notification],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['processing'],
    }),
    setCurrentSettings: builder.mutation<ApiResponseNet<{}>, CurrentSettings>({
      query: (params) => ({
        url: '/setcurrentsettings',
        method: 'post',
        headers: {
          RequestType: RequestType[RequestType.Notification],
        },
        body: JSON.stringify(params),
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ['processing'],
    }),
  }),
});

export const {
  usePlaceBidForAuctionMutation,
  useCreateAuctionMutation,
  useUpdateAuctionMutation,
  useDeleteAuctionMutation,
  useFinanceCreateMutation,
  useSetNotifyUserMutation,
  useSetCurrentSettingsMutation,
} = processingApi;
export default processingApi;
