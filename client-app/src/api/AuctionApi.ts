import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { Auction, PagedResult, ApiResponseNet } from "../store/types";
import AddTokenHeader from "./AddTokenHeader";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";

const auctionApi = createApi({
  //refetchOnMountOrArgChange: true,
  reducerPath: "auctionApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + "/api",
    prepareHeaders: (headers: Headers, api) => {
      const token = AddTokenHeader();
      if (token) {
        headers.append("Authorization", token);
      }
    },
  }),
  tagTypes: ["auctions"],
  endpoints: (builder) => ({
    getDetailedViewData: builder.query<ApiResponseNet<Auction>, string>({
      query: (id) => ({
        url: `/auctions/${id}`,
      }),
      transformResponse: (response: ApiResponseNet<Auction>, meta: any) => {
        PostApiProcess(response);
        if (response.isSuccess) {
          if (response.result.auctionEnd)
            response.result.auctionEnd = new Date(response.result.auctionEnd);
          if (response.result.createAt)
            response.result.createAt = new Date(response.result.createAt);
          if (response.result.updatedAt)
            response.result.updatedAt = new Date(response.result.updatedAt);
        }
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ["auctions"],
    }),
    getAuctions: builder.query<ApiResponseNet<PagedResult<Auction>>, string>({
      query: (url) => ({
        url: "/search" + url,
      }),
      transformResponse: (
        response: ApiResponseNet<PagedResult<Auction>>,
        meta: any
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ["auctions"],
    }),
    getAuctionById: builder.query<ApiResponseNet<Auction>, string>({
      query: (id) => ({
        url: "/search/" + id,
      }),
      transformResponse: (response: ApiResponseNet<Auction>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ["auctions"],
    }),
  }),
});

export const {
  useGetAuctionsQuery,
  useGetAuctionByIdQuery,
  useGetDetailedViewDataQuery,
} = auctionApi;
export default auctionApi;
