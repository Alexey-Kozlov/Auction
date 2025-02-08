import { configureStore } from "@reduxjs/toolkit";
import ReportApi from "../api/ReportApi";
import { reportReducer } from "./reportSlice";

const store = configureStore({
	reducer: {
		reportStore: reportReducer,
		[ReportApi.reducerPath]: ReportApi.reducer,
	},
	middleware: (getDefaultMiddleware) =>
		getDefaultMiddleware({
			serializableCheck: false,
		}).concat(ReportApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export default store;
