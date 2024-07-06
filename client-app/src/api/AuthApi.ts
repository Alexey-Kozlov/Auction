import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { ApiResponseNet, CreateUser, LoginUser } from "../store/types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";

const authApi = createApi({
  reducerPath: "authApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_AUTH,
  }),
  endpoints: (builder) => ({
    registerUser: builder.mutation<any, CreateUser>({
      query: (userData) => ({
        url: "register",
        method: "POST",
        headers: {
          "Content-type": "application/json",
        },
        body: userData,
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
          "Content-type": "application/json",
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
    getUserName: builder.query<ApiResponseNet<string>, string>({
      query: (login) => ({
        url: "",
        method: "POST",
        headers: {
          "content-type": "application/json",
        },
        body: { login: login },
      }),
      transformResponse: (response: ApiResponseNet<string>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
    }),
    setNewPassword: builder.mutation<any, CreateUser>({
      query: (userData) => ({
        url: "setnewpassword",
        method: "POST",
        headers: {
          "Content-type": "application/json",
        },
        body: userData,
      }),
      transformResponse: (response: ApiResponseNet<{}>, meta: any) => {
        PostApiProcess(response);
        return response;
      },
      transformErrorResponse: (response: any, meta: any) => {
        PostErrorApiProcess(response);
      },
    })
  }),
});

export const {
  useRegisterUserMutation,
  useLoginUserMutation,
  useGetUserNameQuery,
  useSetNewPasswordMutation
} = authApi;
export default authApi;
