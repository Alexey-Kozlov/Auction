import { createSlice } from "@reduxjs/toolkit";
import { ParameterItem } from "../Types";

type State = {
	data: string;
	param: ParameterItem[];
	isOpen: boolean;
};

const initialState: State = {
	data: "",
	param: [],
	isOpen: false,
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
		setIsOpen: (state, action) => {
			state.isOpen = action.payload.isOpen;
		},
	},
});

export const { setData, setParam, setIsOpen } = ReportSlice.actions;

export const reportReducer = ReportSlice.reducer;
