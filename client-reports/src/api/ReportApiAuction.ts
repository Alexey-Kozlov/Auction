import { ApiResponseNet, ParameterItem } from "../types";
import {
  CustomError,
  PostApiProcess,
  PostErrorApiProcess,
} from "../utils/postApiProcess";
import { AuctionListTypes } from "../components/reports/auctionList/AuctionListTypes";
import { ReportApi } from "./ReportApi";
import { AuctionTreeItem } from "../components/reports/auctionListTree/AuctionListTypes";
import { CommentTreeItem } from "../components/reports/comments/CommentsTypes";

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
      transformErrorResponse: (response: any, meta: any): CustomError => {
        return PostErrorApiProcess(response, meta);
      },
      invalidatesTags: ["report"],
    }),
    CommentsList: builder.mutation<CommentTreeItem[], ParameterItem[]>({
      query: (params) => ({
        url: "/comments",
        method: "post",
        body: JSON.stringify(params),
      }),
      transformResponse: (response: CommentTreeItem[], meta: any) => {
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

export const {
  useAuctionListMutation,
  useAuctionListTreeMutation,
  useCommentsListMutation,
} = ReportApiAuction;
export default ReportApiAuction;
