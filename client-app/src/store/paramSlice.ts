import { createSlice } from "@reduxjs/toolkit";

type State = {
  pageNumber: number;
  pageSize: number;
  pageCount: number;
  orderBy: string;
  filterBy: string;
  seller?: string;
  winner?: string;
  searchTerm: string;
};

const initialState: State = {
  pageNumber: 1,
  pageSize: 4,
  pageCount: 1,
  orderBy: "newDesc",
  filterBy: "live",
  seller: undefined,
  winner: undefined,
  searchTerm: "",
};

export const paramSlice = createSlice({
  name: "Param",
  initialState: initialState,
  reducers: {
    setParams: (state, action) => {
      if (action.payload.pageNumber)
        state.pageNumber = action.payload.pageNumber;
      if (action.payload.pageSize) state.pageSize = action.payload.pageSize;
      if (action.payload.pageCount) state.pageCount = action.payload.pageCount;
      if (action.payload.orderBy) state.orderBy = action.payload.orderBy;
      if (action.payload.filterBy) {
        state.filterBy = action.payload.filterBy;
        state.seller = undefined;
        state.winner = undefined;
      }
      if (action.payload.seller) {
        state.seller = action.payload.seller;
        state.filterBy = "";
        state.searchTerm = "";
        state.winner = undefined;
      }
      if (action.payload.winner) {
        state.winner = action.payload.winner;
        state.filterBy = "";
        state.searchTerm = "";
        state.seller = undefined;
      }
      if (action.payload.searchTerm || action.payload?.searchTerm === "") {
        state.searchTerm = action.payload.searchTerm;
        state.seller = undefined;
        state.winner = undefined;
      }

      if (!action.payload.pageNumber) {
        state.pageNumber = 1;
      }
    },
    reset: (state, action) => {
      state.pageNumber = 1;
      state.pageSize = 4;
      state.pageCount = 1;
      state.orderBy = "newDesc";
      state.filterBy = "live";
      state.seller = undefined;
      state.winner = undefined;
      state.searchTerm = "";
    },
  },
});

export const { setParams, reset } = paramSlice.actions;

export const paramReducer = paramSlice.reducer;
