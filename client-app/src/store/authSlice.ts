import { createSlice } from "@reduxjs/toolkit";
import { User } from "./types";

export const emptyUserState: User = {
  name: "",
  login: "",
  id: "",
};

export const authSlice = createSlice({
  name: "userAuth",
  initialState: emptyUserState,
  reducers: {
    setAuthUser: (state, action) => {
      state.name = action.payload.name;
      state.id = action.payload.id;
      state.login = action.payload.login;
    },
  },
});

export const { setAuthUser } = authSlice.actions;
export const authReducer = authSlice.reducer;
