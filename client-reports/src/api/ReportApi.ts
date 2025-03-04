import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, ParameterItem } from "../Types";
import { PostApiProcess, PostErrorApiProcess } from "../api/PostResponse";
import { AuctionListTypes } from "../components/reports/auctionList/AuctionListTypes";
import { v4 as uuidv4 } from "uuid";

const ReportApi = createApi({
	//refetchOnMountOrArgChange: true,
	reducerPath: "reportApi",
	baseQuery: fetchBaseQuery({
		baseUrl: process.env.REACT_APP_API_URL + "/api/reports",
		prepareHeaders: (headers: Headers, api) => {
			headers.append("RequestId", uuidv4());
			return headers;
		},
	}),
	tagTypes: ["report"],
	endpoints: (builder) => ({
		runAuctionList: builder.mutation<any, ParameterItem[]>({
			query: (params) => ({
				url: "/auctionlist",
				method: "post",
				headers: {
					"Content-type": "application/json",
				},
				body: JSON.stringify(params),
			}),
			transformResponse: (
				response: ApiResponseNet<AuctionListTypes[]>,
				meta: any
			) => {
				PostApiProcess(response);
				return response;
			},
			transformErrorResponse: (response: any, meta: any) => {
				PostErrorApiProcess(response);
			},
			invalidatesTags: ["report"],
		}),
		runNotifyList: builder.mutation<any, ParameterItem[]>({
			query: (params) => ({
				url: "/notifylist",
				method: "post",
				headers: {
					"Content-type": "application/json",
				},
				body: JSON.stringify(params),
			}),
			transformResponse: (
				response: ApiResponseNet<AuctionListTypes[]>,
				meta: any
			) => {
				PostApiProcess(response);
				return response;
			},
			transformErrorResponse: (response: any, meta: any) => {
				PostErrorApiProcess(response);
			},
			invalidatesTags: ["report"],
		}),
	}),
});

export const { useRunAuctionListMutation, useRunNotifyListMutation } =
	ReportApi;
export default ReportApi;
