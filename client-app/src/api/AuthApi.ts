import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import {
	ApiResponseNet,
	CreateUser,
	LoginUser,
	LogoutUser,
	RequestType,
} from "../types";
import { PostApiProcess, PostErrorApiProcess } from "../utils/PostApiProcess";
import uuid from "react-native-uuid";
import { GetCurrentUser } from "../utils/GetCurrentUser";

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
		registerUser: builder.mutation<any, CreateUser>({
			query: (userData) => ({
				url: "register",
				method: "POST",
				headers: {
					RequestType: RequestType[RequestType.Register],
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
		logoutUser: builder.mutation<any, LogoutUser>({
			query: (userCredentials) => ({
				url: "logout",
				method: "POST",
				headers: {
					RequestType: RequestType[RequestType.Logout],
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
					RequestType: RequestType[RequestType.UpdatePassword],
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
	}),
});

export const {
	useRegisterUserMutation,
	useLoginUserMutation,
	useLogoutUserMutation,
	useGetUserNameQuery,
	useSetNewPasswordMutation,
} = authApi;
export default authApi;
