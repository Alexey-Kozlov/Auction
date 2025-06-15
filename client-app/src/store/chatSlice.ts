import { createSlice } from "@reduxjs/toolkit";
import { ActionType, ChatComment, ChatResponse } from "../types";

const initialChatMessage: ChatComment = {
	itemId: "",
	message: "",
	parentId: "",
	userLogin: "",
	auctionId: "",
	updateAt: new Date(),
	sessionId: "",
	actionType: ActionType.read,
};

const initialResponse: ChatResponse = {
	itemId: "",
	message: "",
	parentId: "",
	userLogin: "",
	auctionId: "",
	updateAt: new Date(),
	actionType: null,
};

export const chatMessage = createSlice({
	name: "message",
	initialState: initialChatMessage,
	reducers: {
		setChatMessage: (state, action) => {
			if (action.payload?.itemId) state.itemId = action.payload.itemId;
			if (action.payload?.message) state.message = action.payload.message;
			if (action.payload?.parentId) state.parentId = action.payload.parentId;
			if (action.payload?.userLogin) state.userLogin = action.payload.userLogin;
			if (action.payload?.auctionId) state.auctionId = action.payload.auctionId;
			if (action.payload?.updateAt) state.updateAt = action.payload.updateAt;
			if (action.payload?.sessionId) state.sessionId = action.payload.sessionId;
			if (action.payload?.actionType !== null)
				state.actionType = action.payload.actionType;
		},
	},
});

export const chatResponse = createSlice({
	name: "response",
	initialState: initialResponse,
	reducers: {
		setChatResponse: (state, action) => {
			if (action.payload?.itemId) state.itemId = action.payload.itemId;
			if (action.payload?.message) state.message = action.payload.message;
			if (action.payload?.parentId) state.parentId = action.payload.parentId;
			if (action.payload?.userLogin) state.userLogin = action.payload.userLogin;
			if (action.payload?.auctionId) state.auctionId = action.payload.auctionId;
			if (action.payload?.updateAt) state.updateAt = action.payload.updateAt;
			if (action.payload?.actionType !== null)
				state.actionType = action.payload.actionType;
		},
	},
});

export const { setChatMessage } = chatMessage.actions;
export const { setChatResponse } = chatResponse.actions;

export const chatMessageReducer = chatMessage.reducer;
export const chatResponseReducer = chatResponse.reducer;
