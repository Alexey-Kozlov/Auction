import { configureStore } from "@reduxjs/toolkit";
import ReportApi from "../api/ReportApi";
import { reportReducer } from "./ReportSlice";
import { eventReducer } from "./EventSlice";
import { authReducer, refreshLinkReducer } from "./authSlice";
import { paramReducer } from "./paramSlice";
import authApi from "../api/AuthApi";

const Store = configureStore({
  reducer: {
    reportStore: reportReducer,
    eventStore: eventReducer,
    authStore: authReducer,
    refreshLink: refreshLinkReducer,
    paramStore: paramReducer,
    [ReportApi.reducerPath]: ReportApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    })
      .concat(ReportApi.middleware)
      .concat(authApi.middleware),
});

export type RootState = ReturnType<typeof Store.getState>;
export default Store;
