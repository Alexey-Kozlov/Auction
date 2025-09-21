import { ApiResponseNet, ParameterItem } from "../types";
import { PostApiProcess, PostErrorApiProcess } from "./PostResponse";
import { AuctionListTypes } from "../components/reports/auctionList/AuctionListTypes";
import { ReportApi } from "./ReportApi";
import { AuctionTreeItem } from "../components/reports/auctionListTree/AuctionListTypes";

const ReportApiAuction = ReportApi.injectEndpoints({
  endpoints: (builder) => ({
    AuctionList: builder.mutation<any, ParameterItem[]>({
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
    AuctionListTree: builder.mutation<AuctionTreeItem[], ParameterItem[]>({
      query: (params) => ({
        url: "/auctionlisttree",
        method: "post",
        body: JSON.stringify(params),
      }),
      transformResponse: (response: AuctionTreeItem[], meta: any) => {
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

export const { useAuctionListMutation, useAuctionListTreeMutation } =
  ReportApiAuction;
export default ReportApiAuction;
