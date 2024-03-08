import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import {
  Auction,
  PagedResult,
  CreateUpdateAuctionParams,
} from "../store/types";
import AddTokenHeader from "./AddTokenHeader";

const auctionApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: "auctionApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL,
    prepareHeaders: (headers: Headers, api) => {
      const token = AddTokenHeader();
      if (token) {
        headers.append("Authorization", token);
      }
    },
  }),
  tagTypes: ["auctions"],
  endpoints: (builder) => ({
    getDetailedViewData: builder.query<Auction, string>({
      query: (id) => ({
        url: `/auctions/${id}`,
      }),
      transformResponse: (auction: Auction, meta: any) => {
        if (auction.auctionEnd)
          auction.auctionEnd = new Date(auction.auctionEnd);
        if (auction.createAt) auction.createAt = new Date(auction.createAt);
        if (auction.updatedAt) auction.updatedAt = new Date(auction.updatedAt);
        return auction;
      },
      providesTags: ["auctions"],
    }),
    getAuctions: builder.query<PagedResult<Auction>, string>({
      query: (url) => ({
        url: "/search" + url,
      }),
      providesTags: ["auctions"],
    }),
    deleteAuction: builder.mutation<any, string>({
      query: (id) => ({
        url: `/auctions/${id}`,
        method: "delete",
        headers: {
          "content-type": "application/json",
        },
        body: "",
      }),
      invalidatesTags: ["auctions"],
    }),
    updateAuction: builder.mutation<Auction, CreateUpdateAuctionParams>({
      query: (params) => ({
        url: `/auctions/${params.id}`,
        method: "put",
        headers: {
          "content-type": "application/json",
        },
        body: params.data,
      }),
      invalidatesTags: ["auctions"],
    }),
    createAuction: builder.mutation<Auction, CreateUpdateAuctionParams>({
      query: (params) => ({
        url: "/auctions",
        method: "post",
        headers: {
          "content-type": "application/json",
        },
        body: params.data,
      }),
      invalidatesTags: ["auctions"],
    }),
  }),
});

export const {
  useGetAuctionsQuery,
  useGetDetailedViewDataQuery,
  useDeleteAuctionMutation,
  useUpdateAuctionMutation,
  useCreateAuctionMutation,
} = auctionApi;
export default auctionApi;
