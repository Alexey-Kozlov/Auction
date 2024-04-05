import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, Bid, PlaceBidParams } from "../store/types";
import AddTokenHeader from "./AddTokenHeader";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";

const bidApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: "bidApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/bids`,
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
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      invalidatesTags: ["bids"],
    }),
    getBidsForAuction: builder.query<ApiResponseNet<Bid[]>, string>({
      query: (id) => ({
        url: `/${id}`,
      }),
      transformResponse: (response: ApiResponseNet<Bid[]>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ["bids"],
    }),
  }),
});

export const { useGetBidsForAuctionQuery, usePlaceBidForAuctionMutation } =
  bidApi;
export default bidApi;
