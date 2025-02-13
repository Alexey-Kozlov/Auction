import { createSlice } from "@reduxjs/toolkit";
import { ParameterItem } from "../Types";

type State = {
	data: string;
	param: ParameterItem[];
};

const initialState: State = {
	data: "",
	param: [],
};

export const ReportSlice = createSlice({
	name: "report",
	initialState: initialState,
	reducers: {
		setData: (state, action) => {
			state.data = action.payload.data;
		},
		setParam: (state, action) => {
			state.param = action.payload.param;
			state.data = "";
		},
	},
});

export const { setData, setParam } = ReportSlice.actions;

export const reportReducer = ReportSlice.reducer;
