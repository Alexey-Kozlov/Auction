import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, ChatComment, RequestType } from "../types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import uuid from "react-native-uuid";

const communicationApi = createApi({
	//refetchOnMountOrArgChange: true,
	reducerPath: "communicationApi",
	baseQuery: fetchBaseQuery({
		baseUrl: process.env.REACT_APP_API_URL + `/api/communication`,
		prepareHeaders: (headers: Headers, api) => {
			headers.append(RequestType[RequestType.TraceId], uuid.v4() as string);
			headers.append("Content-type", "application/json");
			return headers;
		},
	}),
	tagTypes: ["communications"],
	endpoints: (builder) => ({
		getCommunicationItems: builder.query<ApiResponseNet<ChatComment[]>, string>(
			{
				query: (auctionId) => ({
					url: `/${auctionId}`,
					headers: {
						RequestType: RequestType[RequestType.Communications],
					},
				}),
				transformResponse: (
					response: ApiResponseNet<ChatComment[]>,
					meta: any
				) => {
					PostApiProcess(response);
					return response;
				},
				transformErrorResponse: (response: any, meta: any) => {
					PostErrorApiProcess(response);
				},
				providesTags: ["communications"],
			}
		),
	}),
});

export const { useGetCommunicationItemsQuery } = communicationApi;
export default communicationApi;
