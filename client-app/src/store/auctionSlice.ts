import { createSlice } from "@reduxjs/toolkit";
import { Auction } from "./types";

type State = {
  auctions: Auction[];
  totalCount: number;
  pageCount: number;
};

const initialState: State = {
  pageCount: 0,
  auctions: [],
  totalCount: 0,
};

export const auctionSlice = createSlice({
  name: "auction",
  initialState: initialState,
  reducers: {
    setData: (state, action) => {
      if (action.payload?.results) state.auctions = action.payload.results;
      if (action.payload?.pageCount) state.pageCount = action.payload.pageCount;
      if (action.payload?.totalCount)
        state.totalCount = action.payload.totalCount;
    },
    setCurrentPrice: (state, action) => {
      state.auctions = state.auctions.map((auction) =>
        auction.id === action.payload.auctionid
          ? { ...auction, currentHighBid: action.payload.amount }
          : auction
      );
    },
  },
});

export const { setData, setCurrentPrice } = auctionSlice.actions;

export const auctionReducer = auctionSlice.reducer;
