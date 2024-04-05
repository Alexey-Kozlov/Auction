import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet } from "../store/types";
import AddTokenHeader from "./AddTokenHeader";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";

const notificationApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: "notificationApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_NOTIFY_API + "/notifications",
    prepareHeaders: (headers: Headers, api) => {
      const token = AddTokenHeader();
      if (token) {
        headers.append("Authorization", token);
      }
    },
  }),
  tagTypes: ["notifications"],
  endpoints: (builder) => ({
    isNotifyUser: builder.query<ApiResponseNet<boolean>, string>({
      query: (id) => ({
        url: `/items/${id}`,
      }),
      transformResponse: (response: ApiResponseNet<boolean>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ["notifications"],
    }),
    setNotifyUser: builder.mutation<ApiResponseNet<boolean>, string>({
      query: (params) => ({
        url: "/items",
        method: "post",
        headers: {
          "content-type": "application/json",
        },
        body: params,
      }),
      transformResponse: (response: ApiResponseNet<boolean>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      invalidatesTags: ["notifications"],
    }),
  }),
});

export const { useIsNotifyUserQuery, useSetNotifyUserMutation } =
  notificationApi;
export default notificationApi;
