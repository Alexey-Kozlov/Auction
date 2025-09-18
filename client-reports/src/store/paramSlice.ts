import { createSlice } from "@reduxjs/toolkit";
import { State } from "../types";

const initialState: State = {
  userLogin: "",
};

export const paramSlice = createSlice({
  name: "Param",
  initialState: initialState,
  reducers: {
    setParams: (state, action) => {
      if (action.payload.userLogin) state.userLogin = action.payload.userLogin;
    },
  },
});

export const { setParams } = paramSlice.actions;

export const paramReducer = paramSlice.reducer;
