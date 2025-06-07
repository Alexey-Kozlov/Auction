import { createSlice } from "@reduxjs/toolkit";
import { ChatComment, ChatResponse } from "../types";

const initialChatMessage: ChatComment = {
	id: "",
	message: "",
	parentId: "",
	userLogin: "",
	auctionId: "",
	updateAt: new Date(),
	sessionId: "",
};

const initialResponse: ChatResponse = {
	id: "",
	message: "",
	parentId: "",
	userLogin: "",
	auctionId: "",
	updateAt: new Date(),
	action: null,
};

export const chatMessage = createSlice({
	name: "message",
	initialState: initialChatMessage,
	reducers: {
		setChatMessage: (state, action) => {
			if (action.payload?.id) state.id = action.payload.id;
			if (action.payload?.message) state.message = action.payload.message;
			if (action.payload?.parentId) state.parentId = action.payload.parentId;
			if (action.payload?.userLogin) state.userLogin = action.payload.userLogin;
			if (action.payload?.auctionId) state.auctionId = action.payload.auctionId;
			if (action.payload?.updateAt) state.updateAt = action.payload.updateAt;
			if (action.payload?.sessionId) state.sessionId = action.payload.sessionId;
		},
	},
});

export const chatResponse = createSlice({
	name: "response",
	initialState: initialResponse,
	reducers: {
		setChatResponse: (state, action) => {
			if (action.payload?.id) state.id = action.payload.id;
			if (action.payload?.message) state.message = action.payload.message;
			if (action.payload?.parentId) state.parentId = action.payload.parentId;
			if (action.payload?.userLogin) state.userLogin = action.payload.userLogin;
			if (action.payload?.auctionId) state.auctionId = action.payload.auctionId;
			if (action.payload?.updateAt) state.updateAt = action.payload.updateAt;
			if (action.payload?.action !== null) state.action = action.payload.action;
		},
	},
});

export const { setChatMessage } = chatMessage.actions;
export const { setChatResponse } = chatResponse.actions;

export const chatMessageReducer = chatMessage.reducer;
export const chatResponseReducer = chatResponse.reducer;
