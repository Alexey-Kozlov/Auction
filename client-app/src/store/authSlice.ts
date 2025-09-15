import { createSlice } from '@reduxjs/toolkit';
import { RefreshLinkType, User } from '../types';
import { jwtDecode } from 'jwt-decode';

export const emptyUserState: User = {
  name: '',
  login: '',
  isAdmin: false,
  isGuest: true,
};

const refreshLink: RefreshLinkType = {
  value: 0,
  setClear: false,
};

export const authSlice = createSlice({
  name: 'userAuth',
  initialState: emptyUserState,
  reducers: {
    setAuthUser: (state, action) => {
      state.name = action.payload.name;
      state.login = action.payload.login;
      state.isAdmin = getIsAdmin();
      state.isGuest = action.payload.isGuest;
    },
  },
});

export const setRefreshTokenLink = createSlice({
  name: 'refresh',
  initialState: refreshLink,
  reducers: {
    setRefreshLink: (state, action) => {
      state.value = action.payload.value;
      state.setClear = false;
    },
    clearRefreshLink: (state, action) => {
      state.setClear = true;
    },
  },
});

const getIsAdmin = (): boolean => {
  const tokenData = localStorage.getItem('Auction');
  if (tokenData) {
    const token = JSON.parse(tokenData).token;
    if (token) {
      const decode: { role: string } = jwtDecode(token);
      if (decode.role === 'Admin') return true;
    }
  }
  return false;
};

export const { setAuthUser } = authSlice.actions;
export const { setRefreshLink, clearRefreshLink } = setRefreshTokenLink.actions;
export const authReducer = authSlice.reducer;
export const refreshLinkReducer = setRefreshTokenLink.reducer;
