import { createSlice } from "@reduxjs/toolkit";
import { ParameterItem, ReportItem } from "../types";

type State = {
  param: ParameterItem[];
  paramIsOpen: boolean;
  reportLoading: boolean;
  currentReport: ReportItem | null;
};

const initialState: State = {
  param: [],
  paramIsOpen: false,
  reportLoading: false,
  currentReport: null,
};

export const reportSlice = createSlice({
  name: "report",
  initialState: initialState,
  reducers: {
    setParam: (state, action) => {
      state.param = action.payload.param;
    },
    setParamIsOpen: (state, action) => {
      state.paramIsOpen = action.payload.isOpen;
    },
    setReportLoading: (state, action) => {
      state.reportLoading = true;
      state.paramIsOpen = false;
      state.param = action.payload.param;
    },
    setReportLoaded: (state) => {
      state.paramIsOpen = false;
      state.reportLoading = false;
      state.param = [];
    },
    setCurrentReport: (state, action) => {
      state.currentReport = action.payload.currentReport;
    },
  },
});

export const {
  setParam,
  setParamIsOpen,
  setReportLoading,
  setReportLoaded,
  setCurrentReport,
} = reportSlice.actions;

export const reportReducer = reportSlice.reducer;
