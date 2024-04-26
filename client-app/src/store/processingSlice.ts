import { createSlice } from "@reduxjs/toolkit";
import { ProcessingState } from "./types";

export const initProcessingState: ProcessingState[] = [];

export const processingSlice = createSlice({
  name: "processing",
  initialState: initProcessingState,
  reducers: {
    setProcessFlag: (state, action) => {
      let userState = state.find(
        (p) => p.userLogin === action.payload.userLogin
      );
      if (userState) {
        userState.ready = action.payload.ready;
      } else {
        state.push({
          userLogin: action.payload.userLogin,
          ready: action.payload.ready,
        });
      }
    },
  },
});

export const { setProcessFlag } = processingSlice.actions;
export const processinhReducer = processingSlice.reducer;
