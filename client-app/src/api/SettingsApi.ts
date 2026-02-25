import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ApiResponseNet, CurrentSettings, RequestType } from '../types';
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from '../utils/postApiProcess';
import AddTokenHeader from './AddTokenHeader';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/getCurrentUser';

const settingsApi = createApi({
  reducerPath: 'settingsApi',
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/settings`,
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
  tagTypes: ['settings'],
  endpoints: (builder) => ({
    getCurrentSettings: builder.query<ApiResponseNet<CurrentSettings>, {}>({
      query: () => ({
        url: '/GetCurrentSettings',
        headers: {
          RequestType: RequestType[RequestType.CurrentSettings],
        },
      }),
      transformResponse: (
        response: ApiResponseNet<CurrentSettings>,
        meta: any,
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      providesTags: ['settings'],
    }),
  }),
});

export const { useGetCurrentSettingsQuery } = settingsApi;
export default settingsApi;
