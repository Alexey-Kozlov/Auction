import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { Auction } from "../store/types";

const signalRApi = createApi({
  reducerPath: "signalRApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL,
  }),
  tagTypes: ["signalR"],
  endpoints: (builder) => ({
    getAuction: builder.query<Auction, string>({
      query: (id) => ({
        url: `/auctions/${id}`,
      }),
      providesTags: ["signalR"],
    }),
  }),
});

export const { useGetAuctionQuery } = signalRApi;
export default signalRApi;
