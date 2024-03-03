import { createSlice } from "@reduxjs/toolkit";

const initialState = {
  searchTerm: "",
  searchValue: "",
};

export const searchSlice = createSlice({
  name: "Search",
  initialState: initialState,
  reducers: {
    setSearchTerm: (state, action) => {
      state.searchTerm = action.payload;
    },
    setSearchValue: (state, action) => {
      state.searchValue = action.payload;
    },
  },
});

export const { setSearchTerm, setSearchValue } = searchSlice.actions;

export const searchReducer = searchSlice.reducer;
