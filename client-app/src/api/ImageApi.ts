import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { AuctionImage } from "../store/types";

const imageApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: "imageApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/images`,
  }),
  tagTypes: ["images"],
  endpoints: (builder) => ({
    getImageForAuction: builder.query<AuctionImage, string>({
      query: (id) => ({
        url: `/${id}`,
      }),
      providesTags: ["images"],
    }),
  }),
});

export const { useGetImageForAuctionQuery } = imageApi;
export default imageApi;
