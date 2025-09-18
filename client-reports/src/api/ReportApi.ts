import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, ParameterItem, RequestType } from "../types";
import { PostApiProcess, PostErrorApiProcess } from "../api/PostResponse";
import { AuctionListTypes } from "../components/reports/auctionList/AuctionListTypes";
import { DiagramTypes } from "../components/reports/diagrams/DiagramTypes";
import uuid from "react-native-uuid";

const ReportApi = createApi({
  reducerPath: "reportApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_URL + "/api/reports",
    prepareHeaders: (headers: Headers, api) => {
      headers.append(RequestType[RequestType.TraceId], uuid.v4() as string);
      headers.append("Content-type", "application/json");
      const tokenData = localStorage.getItem("Auction");
      if (tokenData) {
        const token = "Bearer " + JSON.parse(tokenData).token;
        headers.append("Authorization", token);
      }
      return headers;
    },
  }),
  tagTypes: ["report"],
  endpoints: (builder) => ({
    runAuctionList: builder.mutation<any, ParameterItem[]>({
      query: (params) => ({
        url: "/auctionlist",
        method: "post",
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
    runDiagrams: builder.mutation<any, ParameterItem[]>({
      query: (params) => ({
        url: "/diagrams",
        method: "post",
        body: JSON.stringify(params),
      }),
      transformResponse: (
        response: ApiResponseNet<DiagramTypes[]>,
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

export const {
  useRunAuctionListMutation,
  useRunNotifyListMutation,
  useRunDiagramsMutation,
} = ReportApi;
export default ReportApi;
