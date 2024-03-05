import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { Bid, PlaceBidParams } from "../store/types";
import AddTokenHeader from "./AddTokenHeader";

const bidApi = createApi({
  reducerPath: "bidApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/bids`,
    prepareHeaders: (headers: Headers, api) => {
      const token = AddTokenHeader();
      if (token) {
        headers.append("Authorization", token);
      }
    },
  }),
  tagTypes: ["bids"],
  endpoints: (builder) => ({
    placeBidForAuction: builder.mutation<any, PlaceBidParams>({
      query: (params) => ({
        url: "",
        method: "post",
        headers: {
          "Content-type": "application/json",
        },
        body: JSON.stringify(params),
      }),
      invalidatesTags: ["bids"],
    }),
    getBidsForAuction: builder.query<Bid[], string>({
      query: (id) => ({
        url: `/${id}`,
      }),
      providesTags: ["bids"],
    }),
  }),
});

export const { useGetBidsForAuctionQuery, usePlaceBidForAuctionMutation } =
  bidApi;
export default bidApi;
