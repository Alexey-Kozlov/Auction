import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ApiResponseNet, CurrentSettings, RequestType } from '../types';
import { PostApiProcess, PostErrorApiProcess } from '../utils/PostApiProcess';
import AddTokenHeader from './AddTokenHeader';
import uuid from 'react-native-uuid';
import { GetCurrentUser } from '../utils/GetCurrentUser';

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
    setCurrentSettings: builder.mutation<ApiResponseNet<{}>, CurrentSettings>({
      query: (params) => ({
        url: '/editcurrent',
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
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      invalidatesTags: ['settings'],
    }),
  }),
});

export const { useSetCurrentSettingsMutation } = settingsApi;
export default settingsApi;
