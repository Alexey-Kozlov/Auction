import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, Bid, RequestType } from "../store/types";
import AddTokenHeader from "./AddTokenHeader";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import { v4 as uuidv4 } from "uuid";

const bidApi = createApi({
	//refetchOnMountOrArgChange: true,
	reducerPath: "bidApi",
	baseQuery: fetchBaseQuery({
		baseUrl: process.env.REACT_APP_API_URL + `/api/bids`,
		prepareHeaders: (headers: Headers, api) => {
			const token = AddTokenHeader();
			if (token) {
				headers.append("Authorization", token);
			}
			headers.append(RequestType[RequestType.TraceId], uuidv4());
			return headers;
		},
	}),
	tagTypes: ["bids"],
	endpoints: (builder) => ({
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

export const { useGetBidsForAuctionQuery } = bidApi;
export default bidApi;
