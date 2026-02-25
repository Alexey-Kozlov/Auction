import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, ParameterItem, RequestType } from "../types";
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from "../utils/postApiProcess";
import { AuctionListTypes } from "../components/reports/auctionList/AuctionListTypes";
import { DiagramTypes } from "../components/reports/diagrams/DiagramTypes";
import uuid from "react-native-uuid";

export const ReportApi = createApi({
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
    NotifyList: builder.mutation<any, ParameterItem[]>({
      query: (params) => ({
        url: "/notifylist",
        method: "post",
        body: JSON.stringify(params),
      }),
      transformResponse: (
        response: ApiResponseNet<AuctionListTypes[]>,
        meta: any,
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ["report"],
    }),
    Diagrams: builder.mutation<any, ParameterItem[]>({
      query: (params) => ({
        url: "/diagrams",
        method: "post",
        body: JSON.stringify(params),
      }),
      transformResponse: (
        response: ApiResponseNet<DiagramTypes[]>,
        meta: any,
      ) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ["report"],
    }),
  }),
});

export const { useNotifyListMutation, useDiagramsMutation } = ReportApi;
export default ReportApi;
