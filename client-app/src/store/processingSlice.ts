import { createSlice } from "@reduxjs/toolkit";
import { ProcessingState } from "./types";

export const initEventsState: ProcessingState[] = [];

export const processingSlice = createSlice({
  name: "processing",
  initialState: initEventsState,
  reducers: {
    setEventFlag: (state, action) => {
      let userState = state.find(
        (p) => p.eventName === action.payload.eventName
      );
      if (userState) {
        userState.ready = action.payload.ready;
      } else {
        state.push({
          eventName: action.payload.eventName,
          ready: action.payload.ready,
          itemId: action.payload.itemId
        });
      }
    },
  },
});

export const { setEventFlag } = processingSlice.actions;
export const processinhReducer = processingSlice.reducer;
