import { configureStore } from "@reduxjs/toolkit";
import auctionApi from "../api/AuctionApi";
import authApi from "../api/AuthApi";
import { authReducer } from "./authSlice";
import { bidReducer } from "./bidSlice";
import { auctionReducer } from "./auctionSlice";
import { paramReducer } from "./paramSlice";
import bidApi from "../api/BidApi";

const store = configureStore({
  reducer: {
    authStore: authReducer,
    bidStore: bidReducer,
    auctionStore: auctionReducer,
    paramStore: paramReducer,
    [auctionApi.reducerPath]: auctionApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
    [bidApi.reducerPath]: bidApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware()
      .concat(auctionApi.middleware)
      .concat(authApi.middleware)
      .concat(bidApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export default store;
