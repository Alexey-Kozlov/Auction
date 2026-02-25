import { createSlice } from '@reduxjs/toolkit';

type State = {
  adminMode: boolean;
};

const initialState: State = {
  adminMode: false,
};

export const settingsSlice = createSlice({
  name: 'settings',
  initialState: initialState,
  reducers: {
    setSettingsData: (state, action) => {
      state.adminMode = action.payload.adminMode;
    },
  },
});

export const { setSettingsData } = settingsSlice.actions;

export const settingsReducer = settingsSlice.reducer;
