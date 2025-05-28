import { createSlice } from "@reduxjs/toolkit";
import { ProcessingState } from "../types";

export const initEventsState: ProcessingState[] = [];

export const processingSlice = createSlice({
	name: "processing",
	initialState: initEventsState,
	reducers: {
		setEventFlag: (state, action) => {
			//удаляем флаг изменения у всех записей (если есть)
			if (state.length > 0) {
				state.forEach((item) => {
					item.lastChanged = false;
				});
			}
			let userState = state.find(
				(p) => p.eventName === action.payload.eventName
			);
			if (userState) {
				userState.ready = action.payload.ready;
				userState.param = action.payload.param ? action.payload.param : null;
				userState.itemId = action.payload.itemId;
				userState.lastChanged = true;
			} else {
				state.push({
					eventName: action.payload.eventName,
					ready: action.payload.ready,
					itemId: action.payload.itemId,
					param: action.payload.param ? action.payload.param : null,
					lastChanged: true,
				});
			}
		},
	},
});

export const { setEventFlag } = processingSlice.actions;
export const processingReducer = processingSlice.reducer;
