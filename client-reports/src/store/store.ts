import { configureStore } from "@reduxjs/toolkit";
import ReportApi from "../api/ReportApi";
import { reportReducer } from "./reportSlice";
import { eventReducer } from "./eventSlice";
import { authReducer, refreshLinkReducer } from "./authSlice";
import { paramReducer } from "./paramSlice";
import authApi from "../api/AuthApi";
import { settingsReducer } from "./settingsSlice";
import settingsApi from "../api/SettingsApi";
import { processingReducer } from "./processingSlice";
import { serviceReducer } from "./serviceSlice";

const store = configureStore({
  reducer: {
    reportStore: reportReducer,
    eventStore: eventReducer,
    authStore: authReducer,
    refreshLink: refreshLinkReducer,
    paramStore: paramReducer,
    processingStore: processingReducer,
    settingsStore: settingsReducer,
    serviceStore: serviceReducer,
    [ReportApi.reducerPath]: ReportApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
    [settingsApi.reducerPath]: settingsApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    })
      .concat(ReportApi.middleware)
      .concat(authApi.middleware)
      .concat(settingsApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export default store;
