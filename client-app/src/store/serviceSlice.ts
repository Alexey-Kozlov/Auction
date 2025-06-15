import { createSlice } from "@reduxjs/toolkit";
import { Toast } from "primereact/toast";

type ServiceState = {
	toast: Toast | null;
};

const initialState: ServiceState = {
	toast: null,
};

export const serviceSlice = createSlice({
	name: "service",
	initialState: initialState,
	reducers: {
		setServiceData: (state, action) => {
			if (action.payload?.toast) state.toast = action.payload.toast;
		},
	},
});

export const { setServiceData } = serviceSlice.actions;

export const serviceReducer = serviceSlice.reducer;
