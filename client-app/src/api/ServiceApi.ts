import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import AddTokenHeader from "./AddTokenHeader";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import { ApiResponseNet, RestoreDb, Session } from "../store/types";

const serviceApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: "serviceApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + "/api/processing",
    prepareHeaders: (headers: Headers, api) => {
      const token = AddTokenHeader();
      if (token) {
        headers.append("Authorization", token);
      }
    },
  }),
  tagTypes: ["elk"],
  endpoints: (builder) => ({
    elkIndex: builder.mutation<ApiResponseNet<number>, Session>({
      query: (params) => ({
        url: "/elkindex",
        method: "post",
        headers: {
          "content-type": "application/json",
        },
        body: JSON.stringify(params)
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      invalidatesTags: ["elk"],
    }),
    setSnapShot: builder.mutation<ApiResponseNet<number>, Session>({
      query: (params) => ({
        url: "/setsnapshot",
        method: "post",
        headers: {
          "content-type": "application/json",
        },
        body: JSON.stringify(params)
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      invalidatesTags: ["elk"],
    }),
    RestoreSnapShot: builder.mutation<ApiResponseNet<number>, RestoreDb>({
      query: (params) => ({
        url: "/restoresnapshot",
        method: "post",
        headers: {
          "content-type": "application/json",
        },
        body: JSON.stringify(params)
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      invalidatesTags: ["elk"],
    })
  }),
});

export const {
  useElkIndexMutation,
  useSetSnapShotMutation,
  useRestoreSnapShotMutation
} = serviceApi;
export default serviceApi;
