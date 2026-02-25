import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ApiResponseNet, RequestType } from '../types';
import AddTokenHeader from './AddTokenHeader';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/postApiProcess';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/getCurrentUser';

const notificationApi = createApi({
  reducerPath: 'notificationApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_NOTIFY_URL,
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
  tagTypes: ['notifications'],
  endpoints: (builder) => ({
    isNotifyUser: builder.query<ApiResponseNet<boolean>, string>({
      query: (id) => ({
        url: `/items/${id}`,
        headers: {
          RequestType: RequestType[RequestType.Notification],
        },
      }),
      transformResponse: (response: ApiResponseNet<boolean>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['notifications'],
    }),
  }),
});

export const { useIsNotifyUserQuery } = notificationApi;
export default notificationApi;
