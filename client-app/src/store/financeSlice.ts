import { createSlice } from "@reduxjs/toolkit";
import { FinanceItem } from "./types";

type State = {
  items: FinanceItem[];
  totalCount: number;
  pageCount: number;
};

const initialState: State = {
  pageCount: 0,
  items: [],
  totalCount: 0,
};

export const financeSlice = createSlice({
  name: "finance",
  initialState: initialState,
  reducers: {
    setFinanceItems: (state, action) => {
      if (action.payload?.results) state.items = action.payload.results;
      if (action.payload?.pageCount) state.pageCount = action.payload.pageCount;
      if (action.payload?.totalCount)
        state.totalCount = action.payload.totalCount;
    },
  },
});

export const { setFinanceItems } = financeSlice.actions;

export const financeReducer = financeSlice.reducer;
