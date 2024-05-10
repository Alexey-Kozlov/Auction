import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import {
  ApiResponseNet,
  AuctionDeleted,
  AuctionUpdated,
  PlaceBidParams,
} from "../store/types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import AddTokenHeader from "./AddTokenHeader";

const processingApi = createApi({
  //refetchOnMountOrArgChange: true,
  reducerPath: "processingApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/processing`,
    prepareHeaders: (headers: Headers, api) => {
      const token = AddTokenHeader();
      if (token) {
        headers.append("Authorization", token);
      }
    },
  }),
  tagTypes: ["processing"],
  endpoints: (builder) => ({
    placeBidForAuction: builder.mutation<any, PlaceBidParams>({
      query: (params) => ({
        url: "/placebid",
        method: "post",
        headers: {
          "Content-type": "application/json",
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
      invalidatesTags: ["processing"],
    }),
    updateAuction: builder.mutation<ApiResponseNet<{}>, AuctionUpdated>({
      query: (params) => ({
        url: "/updateauction",
        method: "post",
        headers: {
          "Content-type": "application/json",
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
      invalidatesTags: ["processing"],
    }),
    deleteAuction: builder.mutation<ApiResponseNet<{}>, AuctionDeleted>({
      query: (params) => ({
        url: `/deleteauction`,
        method: "post",
        headers: {
          "content-type": "application/json",
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
      invalidatesTags: ["processing"],
    }),
  }),
});

export const {
  usePlaceBidForAuctionMutation,
  useUpdateAuctionMutation,
  useDeleteAuctionMutation,
} = processingApi;
export default processingApi;
