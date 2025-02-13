import { createSlice } from "@reduxjs/toolkit";

type State = {
	exportPdfEnable: boolean;
	exportPdfClicked: boolean;
};

const initialState: State = {
	exportPdfEnable: false,
	exportPdfClicked: false,
};

export const EventSlice = createSlice({
	name: "Param",
	initialState: initialState,
	reducers: {
		setEvent: (state, action) => {
			if (state.exportPdfEnable !== action.payload.exportPdfEnable) {
				state.exportPdfEnable = action.payload.exportPdfEnable;
			}
			if (state.exportPdfClicked !== action.payload.exportPdfClicked) {
				state.exportPdfClicked = action.payload.exportPdfClicked;
			}
		},
	},
});

export const { setEvent } = EventSlice.actions;

export const eventReducer = EventSlice.reducer;
