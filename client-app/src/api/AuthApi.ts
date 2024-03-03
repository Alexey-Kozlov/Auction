import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import {
  ApiResponse,
  CreateUser,
  LoginResponse,
  LoginUser,
  PagedResult,
} from "../store/types";

const authApi = createApi({
  reducerPath: "authApi",
  baseQuery: fetchBaseQuery({
    baseUrl: process.env.REACT_APP_API_AUTH + "/auth/",
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
    }),
  }),
});

export const { useRegisterUserMutation, useLoginUserMutation } = authApi;
export default authApi;
