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
      if (!action.payload.bid) state.bids = [...state.bids, action.payload];
    },
    setOpen: (state, action) => {
      state.open = action.payload;
    },
  },
});

export const { setBids, addBid, setOpen } = bidSlice.actions;

export const bidReducer = bidSlice.reducer;
