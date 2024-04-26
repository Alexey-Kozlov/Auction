import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, FinanceItem, PagedResult } from "../store/types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import AddTokenHeader from "./AddTokenHeader";

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
    addCredit: builder.mutation<ApiResponseNet<number>, number>({
      query: (params) => ({
        url: "/addcredit",
        method: "post",
        headers: {
          "content-type": "application/json",
        },
        body: JSON.stringify({ amount: params }),
      }),
      transformResponse: (response: ApiResponseNet<number>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
      invalidatesTags: ["finance"],
    }),
  }),
});

export const {
  useGetFinanceItemQuery,
  useGetBalanceQuery,
  useAddCreditMutation,
} = financeApi;
export default financeApi;
