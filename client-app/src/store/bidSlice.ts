import { createSlice } from "@reduxjs/toolkit";
import { Bid } from "./types";

type State = {
  bids: Bid[];
  open: boolean;
};

const initialState: State = {
  open: false,
  bids: [],
};

export const bidSlice = createSlice({
  name: "Bid",
  initialState: initialState,
  reducers: {
    setBids: (state, action) => {
      state.bids = action.payload;
    },
    addBid: (state, action) => {
      state.bids = state.bids.find((p) => p.id === action.payload.id)
        ? [action.payload, ...state.bids]
        : [...state.bids];
    },
    setOpen: (state, action) => {
      state.open = action.payload;
    },
  },
});

export const { setBids, addBid, setOpen } = bidSlice.actions;

export const bidReducer = bidSlice.reducer;
