import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, Auction } from "../store/types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";

const signalRApi = createApi({
  reducerPath: "signalRApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + "/api",
  }),
  tagTypes: ["signalR"],
  endpoints: (builder) => ({
    getAuction: builder.query<ApiResponseNet<Auction>, string>({
      query: (id) => ({
        url: `/auctions/${id}`,
      }),
      transformResponse: (response: ApiResponseNet<Auction>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ["signalR"],
    }),
  }),
});

export const { useGetAuctionQuery } = signalRApi;
export default signalRApi;
