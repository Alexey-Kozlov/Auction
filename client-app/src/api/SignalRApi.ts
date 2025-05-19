import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, Auction, RequestType } from "../types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import uuid from "react-native-uuid";
import { GetCurrentUser } from "../utils/GetCurrentUser";

const signalRApi = createApi({
	reducerPath: "signalRApi",
	baseQuery: fetchBaseQuery({
		baseUrl: process.env.REACT_APP_API_URL + "/api",
		prepareHeaders: (headers: Headers, api) => {
			headers.append(RequestType[RequestType.TraceId], uuid.v4() as string);
			headers.append("Content-type", "application/json");
			headers.append("User", GetCurrentUser());
			return headers;
		},
	}),
	tagTypes: ["signalR"],
	endpoints: (builder) => ({
		getAuction: builder.query<ApiResponseNet<Auction>, string>({
			query: (id) => ({
				url: `/auctions/${id}`,
				headers: {
					RequestType: RequestType[RequestType.SignalR],
				},
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
