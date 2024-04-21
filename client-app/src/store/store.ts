import { configureStore } from "@reduxjs/toolkit";
import auctionApi from "../api/AuctionApi";
import authApi from "../api/AuthApi";
import { authReducer } from "./authSlice";
import { bidReducer } from "./bidSlice";
import { auctionReducer } from "./auctionSlice";
import { paramReducer } from "./paramSlice";
import bidApi from "../api/BidApi";
import signalRApi from "../api/SignalRApi";
import imageApi from "../api/ImageApi";
import notificationApi from "../api/NotificationApi";
import financeApi from "../api/FinanceApi";
import { financeReducer } from "./financeSlice";

const store = configureStore({
  reducer: {
    authStore: authReducer,
    bidStore: bidReducer,
    auctionStore: auctionReducer,
    paramStore: paramReducer,
    financeStore: financeReducer,
    [auctionApi.reducerPath]: auctionApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
    [bidApi.reducerPath]: bidApi.reducer,
    [signalRApi.reducerPath]: signalRApi.reducer,
    [imageApi.reducerPath]: imageApi.reducer,
    [notificationApi.reducerPath]: notificationApi.reducer,
    [financeApi.reducerPath]: financeApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    })
      .concat(auctionApi.middleware)
      .concat(authApi.middleware)
      .concat(bidApi.middleware)
      .concat(signalRApi.middleware)
      .concat(imageApi.middleware)
      .concat(notificationApi.middleware)
      .concat(financeApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export default store;
