import { createSlice } from "@reduxjs/toolkit";
import { User } from "../types";
import { jwtDecode } from "jwt-decode";

export const emptyUserState: User = {
	name: "",
	login: "",
	itemId: undefined,
	isAdmin: false,
};

export const authSlice = createSlice({
	name: "userAuth",
	initialState: emptyUserState,
	reducers: {
		setAuthUser: (state, action) => {
			state.name = action.payload.name;
			state.itemId = action.payload.itemId;
			state.login = action.payload.login;
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
