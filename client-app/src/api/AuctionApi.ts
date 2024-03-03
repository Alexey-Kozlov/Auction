import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { Auction, PagedResult } from "../store/types";
import { jwtDecode } from "jwt-decode";

const auctionApi = createApi({
  reducerPath: "auctionApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + "/search",
    prepareHeaders: (headers: Headers, api) => {
      let token = localStorage.getItem("Auction");
      if (token) {
        const decode: { exp: number } = jwtDecode(token);
        if (decode.exp * 1000 <= Date.now()) {
          localStorage.removeItem("Auction");
          token = null;
        }
        token && headers.append("Authorization", "Bearer " + token);
      }
    },
  }),
  tagTypes: ["auctions"],
  endpoints: (builder) => ({
    getAuctions: builder.query<PagedResult<Auction>, string>({
      query: (url) => ({
        url: url,
      }),
      providesTags: ["auctions"],
    }),
    
  }),
});

export const { useGetAuctionsQuery } = auctionApi;
export default auctionApi;
