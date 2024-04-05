import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, AuctionImage } from "../store/types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";

const imageApi = createApi({
  refetchOnMountOrArgChange: true,
  reducerPath: "imageApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + `/api/images`,
  }),
  tagTypes: ["images"],
  endpoints: (builder) => ({
    getImageForAuction: builder.query<ApiResponseNet<AuctionImage>, string>({
      query: (id) => ({
        url: `/${id}`,
      }),
      transformResponse: (
        response: ApiResponseNet<AuctionImage>,
        meta: any
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      providesTags: ["images"],
    }),
  }),
});

export const { useGetImageForAuctionQuery } = imageApi;
export default imageApi;
