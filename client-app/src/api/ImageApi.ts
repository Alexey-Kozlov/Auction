import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, AuctionImage, RequestType } from "../types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import { v4 as uuidv4 } from "uuid";

const imageApi = createApi({
	refetchOnMountOrArgChange: true,
	reducerPath: "imageApi",
	baseQuery: fetchBaseQuery({
		baseUrl: process.env.REACT_APP_API_URL + `/api/images`,
		prepareHeaders: (headers: Headers, api) => {
			headers.append(RequestType[RequestType.TraceId], uuidv4());
			return headers;
		},
	}),
	tagTypes: ["images"],
	endpoints: (builder) => ({
		getImageForAuction: builder.query<
			ApiResponseNet<AuctionImage>,
			{ id: string; cache: boolean }
		>({
			query: (arg) => ({
				url: `/`,
				params: { id: arg.id, cache: arg.cache },
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
