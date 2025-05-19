import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, Bid, RequestType } from "../types";
import AddTokenHeader from "./AddTokenHeader";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import uuid from "react-native-uuid";
import { GetCurrentUser } from "../utils/GetCurrentUser";

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
			headers.append(RequestType[RequestType.TraceId], uuid.v4() as string);
			headers.append("Content-type", "application/json");
			headers.append("User", GetCurrentUser());
			return headers;
		},
	}),
	tagTypes: ["bids"],
	endpoints: (builder) => ({
		getBidsForAuction: builder.query<ApiResponseNet<Bid[]>, string>({
			query: (id) => ({
				url: `/${id}`,
				headers: {
					RequestType: RequestType[RequestType.Bids],
				},
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
