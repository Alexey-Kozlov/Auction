import { createSlice } from "@reduxjs/toolkit";

type State = {
  exportPdfEnable: boolean;
  exportPdfClicked: boolean;
  exportExcelEnable: boolean;
  exportExcelClicked: boolean;
};

const initialState: State = {
  exportPdfEnable: false,
  exportPdfClicked: false,
  exportExcelEnable: false,
  exportExcelClicked: false,
};

export const eventSlice = createSlice({
  name: "Param",
  initialState: initialState,
  reducers: {
    setEvent: (state, action) => {
      if (state.exportPdfEnable !== action.payload.exportPdfEnable) {
        state.exportPdfEnable = action.payload.exportPdfEnable;
      }
      if (state.exportPdfClicked !== action.payload.exportPdfClicked) {
        state.exportPdfClicked = action.payload.exportPdfClicked;
      }
      if (state.exportExcelEnable !== action.payload.exportExcelEnable) {
        state.exportExcelEnable = action.payload.exportExcelEnable;
      }
      if (state.exportExcelClicked !== action.payload.exportExcelClicked) {
        state.exportExcelClicked = action.payload.exportExcelClicked;
      }
    },
  },
});

export const { setEvent } = eventSlice.actions;

export const eventReducer = eventSlice.reducer;
