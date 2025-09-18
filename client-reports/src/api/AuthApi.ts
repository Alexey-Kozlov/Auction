import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, LoginUser, LogoutUser, RequestType } from "../types";
import uuid from "react-native-uuid";
import { GetCurrentUser } from "../utils";
import { PostApiProcess, PostErrorApiProcess } from "./PostResponse";

const authApi = createApi({
  reducerPath: "authApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_AUTH,
    prepareHeaders: (headers: Headers, api) => {
      headers.append(RequestType[RequestType.TraceId], uuid.v4() as string);
      headers.append("Content-type", "application/json");
      headers.append("User", GetCurrentUser());
      return headers;
    },
  }),
  endpoints: (builder) => ({
    refreshToken: builder.mutation<any, LogoutUser>({
      query: (userCredentials) => ({
        url: "refreshtoken",
        method: "POST",
        headers: {
          RequestType: RequestType[RequestType.RefreshToken],
        },
        body: userCredentials,
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
    }),
    loginUser: builder.mutation<any, LoginUser>({
      query: (userCredentials) => ({
        url: "login",
        method: "POST",
        headers: {
          RequestType: RequestType[RequestType.Login],
        },
        body: userCredentials,
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
    }),
  }),
});

export const { useRefreshTokenMutation, useLoginUserMutation } = authApi;
export default authApi;
