import { configureStore } from "@reduxjs/toolkit";
import ReportApi from "../api/ReportApi";
import { reportReducer } from "./ReportSlice";
import { eventReducer } from "./EventSlice";

const Store = configureStore({
	reducer: {
		reportStore: reportReducer,
		eventStore: eventReducer,
		[ReportApi.reducerPath]: ReportApi.reducer,
	},
	middleware: (getDefaultMiddleware) =>
		getDefaultMiddleware({
			serializableCheck: false,
		}).concat(ReportApi.middleware),
});

export type RootState = ReturnType<typeof Store.getState>;
export default Store;
