import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ApiResponseNet, ChatComment, RequestType } from '../types';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/PostApiProcess';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/GetCurrentUser';
import AddTokenHeader from './AddTokenHeader';

const communicationApi = createApi({
  reducerPath: 'communicationApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/communication`,
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
  tagTypes: ['communications'],
  endpoints: (builder) => ({
    getCommunicationItems: builder.query<ApiResponseNet<ChatComment[]>, string>(
      {
        query: (itemId) => ({
          url: `/${itemId}`,
          headers: {
            RequestType: RequestType[RequestType.Communications],
          },
        }),
        transformResponse: (
          response: ApiResponseNet<ChatComment[]>,
          meta: any,
        ) => {
          PostApiProcess(response);
          return response;
        },
        transformErrorResponse: (response: any, meta: any): CustomError => {
          return PostErrorApiProcess(response, meta);
        },
        providesTags: ['communications'],
      },
    ),
  }),
});

export const { useGetCommunicationItemsQuery } = communicationApi;
export default communicationApi;
