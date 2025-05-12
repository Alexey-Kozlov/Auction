import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import AddTokenHeader from "./AddTokenHeader";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import { ApiResponseNet, RequestType, RestoreDb, Session } from "../types";
import { v4 as uuidv4 } from "uuid";

const serviceApi = createApi({
	refetchOnMountOrArgChange: true,
	reducerPath: "serviceApi",
	baseQuery: fetchBaseQuery({
		baseUrl: process.env.REACT_APP_API_URL + "/api/processing",
		prepareHeaders: (headers: Headers, api) => {
			const token = AddTokenHeader();
			if (token) {
				headers.append("Authorization", token);
			}
			headers.append(RequestType[RequestType.TraceId], uuidv4());
			return headers;
		},
	}),
	tagTypes: ["service"],
	endpoints: (builder) => ({
		elkIndex: builder.mutation<ApiResponseNet<number>, Session>({
			query: (params) => ({
				url: "/elkindex",
				method: "post",
				headers: {
					"content-type": "application/json",
				},
				body: JSON.stringify(params),
			}),
			transformResponse: (response: ApiResponseNet<number>, meta: any) => {
				PostApiProcess(response);
				return response;
			},
			transformErrorResponse: (response: any, meta: any) => {
				PostErrorApiProcess(response);
			},
			invalidatesTags: ["service"],
		}),
		setSnapShot: builder.mutation<ApiResponseNet<number>, Session>({
			query: (params) => ({
				url: "/setsnapshot",
				method: "post",
				headers: {
					"content-type": "application/json",
				},
				body: JSON.stringify(params),
			}),
			transformResponse: (response: ApiResponseNet<number>, meta: any) => {
				PostApiProcess(response);
				return response;
			},
			transformErrorResponse: (response: any, meta: any) => {
				PostErrorApiProcess(response);
			},
			invalidatesTags: ["service"],
		}),
		RestoreSnapShot: builder.mutation<ApiResponseNet<number>, RestoreDb>({
			query: (params) => ({
				url: "/restoresnapshot",
				method: "post",
				headers: {
					"content-type": "application/json",
				},
				body: JSON.stringify(params),
			}),
			transformResponse: (response: ApiResponseNet<number>, meta: any) => {
				PostApiProcess(response);
				return response;
			},
			transformErrorResponse: (response: any, meta: any) => {
				PostErrorApiProcess(response);
			},
			invalidatesTags: ["service"],
		}),
		resetImageCache: builder.mutation<ApiResponseNet<{}>, Session>({
			query: (params) => ({
				url: `/resetimagecache`,
				method: "post",
				headers: {
					"content-type": "application/json",
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
			invalidatesTags: ["service"],
		}),
	}),
});

export const {
	useElkIndexMutation,
	useSetSnapShotMutation,
	useRestoreSnapShotMutation,
	useResetImageCacheMutation,
} = serviceApi;
export default serviceApi;
