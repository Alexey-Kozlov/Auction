import { createSlice } from "@reduxjs/toolkit";
import { ParameterItem } from "../Types";

type State = {
	param: ParameterItem[];
	paramIsOpen: boolean;
	reportLoading: boolean;
};

const initialState: State = {
	param: [],
	paramIsOpen: false,
	reportLoading: false,
};

export const ReportSlice = createSlice({
	name: "report",
	initialState: initialState,
	reducers: {
		setParam: (state, action) => {
			state.param = action.payload.param;
		},
		setParamIsOpen: (state, action) => {
			state.paramIsOpen = action.payload.isOpen;
		},
		setReportLoading: (state, action) => {
			state.reportLoading = true;
			state.paramIsOpen = false;
			state.param = action.payload.param;
		},
		setReportLoaded: (state) => {
			state.paramIsOpen = false;
			state.reportLoading = false;
			state.param = [];
		},
	},
});

export const { setParam, setParamIsOpen, setReportLoading, setReportLoaded } =
	ReportSlice.actions;

export const reportReducer = ReportSlice.reducer;
