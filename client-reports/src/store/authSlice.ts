import { createSlice } from "@reduxjs/toolkit";
import { jwtDecode } from "jwt-decode";
import { User } from "../types";

export const emptyUserState: User = {
	name: "",
	login: "",
	id: undefined,
	isAdmin: false,
};

export const authSlice = createSlice({
	name: "userAuth",
	initialState: emptyUserState,
	reducers: {
		setAuthUser: (state, action) => {
			state.name = action.payload.user.name;
			state.id = action.payload.user.id;
			state.login = action.payload.user.login;
			state.isAdmin = getIsAdmin();
		},
	},
});

const getIsAdmin = (): boolean => {
	const tokenData = localStorage.getItem("Auction");
	if (tokenData) {
		const token = JSON.parse(tokenData).token;
		const decode: { role: string } = jwtDecode(token);
		if (decode.role === "Admin") return true;
	}
	return false;
};

export const { setAuthUser } = authSlice.actions;
export const authReducer = authSlice.reducer;
