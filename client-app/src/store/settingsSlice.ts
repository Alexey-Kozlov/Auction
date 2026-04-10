import { createSlice } from '@reduxjs/toolkit';
import { CurrentSettings } from '../types';

const initialState: CurrentSettings = {
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
