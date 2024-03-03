import { createSlice } from "@reduxjs/toolkit";

type State = {
  pageNumber: number;
  pageSize: number;
  pageCount: number;
  orderBy: string;
  filterBy: string;
  seller?: string;
  winner?: string;
  imageValue?: any;
};

type Action = {
  setParams: (params: Partial<State>) => void;
  reset: () => void;
};

const initialState: State = {
  pageNumber: 1,
  pageSize: 4,
  pageCount: 1,
  orderBy: "new",
  filterBy: "live",
  seller: undefined,
  winner: undefined,
  imageValue: undefined,
};

export const paramSlice = createSlice({
  name: "Param",
  initialState: initialState,
  reducers: {
    setParams: (state, action) => {
      if (action.payload.pageNumber) {
        state = { ...state, pageNumber: action.payload.pageNumber };
      } else {
        state = { ...state, ...action.payload, pageNumber: 1 };
      }
    },
    reset: (state, action) => {
      state = { ...initialState };
    },
  },
});

export const { setParams, reset } = paramSlice.actions;

export const paramReducer = paramSlice.reducer;
