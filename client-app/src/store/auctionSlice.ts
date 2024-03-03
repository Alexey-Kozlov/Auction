import { createSlice } from "@reduxjs/toolkit";
import { Auction, PagedResult } from "./types";

type State = {
  auctions: Auction[];
  totalCount: number;
  pageCount: number;
};

type Actions = {
  setData: (data: PagedResult<Auction>) => void;
  setCurrentPrice: (auctionId: string, amount: number) => void;
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
      state.auctions = action.payload.auctions;
      state.pageCount = action.payload.pageCount;
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
