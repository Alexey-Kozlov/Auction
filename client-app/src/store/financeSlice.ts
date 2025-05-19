import { createSlice } from "@reduxjs/toolkit";
import { FinanceStore } from "../types";

const initialState: FinanceStore = {
	pageCount: 0,
	results: [],
	totalCount: 0,
};

export const financeSlice = createSlice({
	name: "finance",
	initialState: initialState,
	reducers: {
		setFinanceItems: (state, action) => {
			if (action.payload?.results) state.results = action.payload.results;
			if (action.payload?.pageCount) state.pageCount = action.payload.pageCount;
			if (action.payload?.totalCount)
				state.totalCount = action.payload.totalCount;
		},
	},
});

export const { setFinanceItems } = financeSlice.actions;

export const financeReducer = financeSlice.reducer;
