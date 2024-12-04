import { createSlice } from "@reduxjs/toolkit";
import { ProcessingState } from "./types";
import { useSyncExternalStore } from "react";

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
        userState.param = action.payload.param ? action.payload.param : null;
        userState.itemId = action.payload.itemId;
      } else {
        state.push({
          eventName: action.payload.eventName,
          ready: action.payload.ready,
          itemId: action.payload.itemId,
          param: action.payload.param ? action.payload.param : null
        });
      }
    },
  },
});

export const { setEventFlag } = processingSlice.actions;
export const processingReducer = processingSlice.reducer;
