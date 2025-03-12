import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import {
	ApiResponseNet,
	FinanceItem,
	PagedResult,
	RequestType,
} from "../store/types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import AddTokenHeader from "./AddTokenHeader";
import { v4 as uuidv4 } from "uuid";

const financeApi = createApi({
	refetchOnMountOrArgChange: true,
	reducerPath: "financeApi",
	baseQuery: fetchBaseQuery({
		baseUrl: process.env.REACT_APP_API_URL + `/api/finance`,
		prepareHeaders: (headers: Headers, api) => {
			const token = AddTokenHeader();
			if (token) {
				headers.append("Authorization", token);
			}
			headers.append(RequestType[RequestType.TraceId], uuidv4());
			return headers;
		},
	}),
	tagTypes: ["finance"],
	endpoints: (builder) => ({
		getFinanceItem: builder.query<
			ApiResponseNet<PagedResult<FinanceItem>>,
			string
		>({
			query: (url) => ({
				url: "/gethistory" + url,
			}),
			transformResponse: (
				response: ApiResponseNet<PagedResult<FinanceItem>>,
				meta: any
			) => {
				PostApiProcess(response);
				return response;
			},
			transformErrorResponse: (response: any, meta: any) => {
				PostErrorApiProcess(response);
			},
			providesTags: ["finance"],
		}),
		getBalance: builder.query<ApiResponseNet<number>, null>({
			query: () => ({
				url: "/getbalance",
			}),
			transformResponse: (response: ApiResponseNet<number>, meta: any) => {
				PostApiProcess(response);
				return response;
			},
			transformErrorResponse: (response: any, meta: any) => {
				PostErrorApiProcess(response);
			},
			providesTags: ["finance"],
		}),
	}),
});

export const { useGetFinanceItemQuery, useGetBalanceQuery } = financeApi;
export default financeApi;
