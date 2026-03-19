import { createSlice } from '@reduxjs/toolkit';
import { State } from '../types';
import { GetCurrentUser } from '../utils/getCurrentUser';

const initialState: State = {
  pageNumber: 1,
  pageSize: 16,
  pageCount: 1,
  firstPage: 0,
  orderBy: 'newDesc',
  filterBy: 'live',
  seller: '',
  winner: '',
  searchTerm: '',
  searchAdv: '',
  userLogin: '',
  tag: '',
  advSearchParam: ['Auction', 'Comment', 'Tag'],
};

export const paramSlice = createSlice({
  name: 'Param',
  initialState: initialState,
  reducers: {
    setParams: (state, action) => {
      if (action.payload.pageNumber)
        state.pageNumber = action.payload.pageNumber;
      if (action.payload.firstPage || action.payload.firstPage === 0)
        state.firstPage = action.payload.firstPage;
      if (action.payload.pageSize) state.pageSize = action.payload.pageSize;
      if (action.payload.pageCount || action.payload.pageCount === 0)
        state.pageCount = action.payload.pageCount;
      if (action.payload.orderBy) state.orderBy = action.payload.orderBy;
      if (action.payload.filterBy) {
        state.filterBy = action.payload.filterBy;
        state.seller = '';
        state.winner = '';
        state.tag = '';
      }

      if (action.payload.tag) {
        state.tag = action.payload.tag;
        state.searchTerm = '';
        state.searchAdv = '';
        state.filterBy = '';
        state.winner = '';
        state.seller = '';
      }

      if (action.payload.advSearchParam) {
        state.advSearchParam = action.payload.advSearchParam;
      }

      if (action.payload.seller) {
        state.seller = action.payload.seller;
        state.filterBy = '';
        state.searchTerm = '';
        state.searchAdv = '';
        state.tag = '';
        state.winner = '';
      }

      if (action.payload.winner) {
        state.winner = action.payload.winner;
        state.filterBy = '';
        state.searchTerm = '';
        state.searchAdv = '';
        state.tag = '';
        state.seller = '';
      }

      if (action.payload.searchTerm || action.payload.searchTerm === '') {
        state.searchTerm = action.payload.searchTerm;
        state.seller = '';
        state.winner = '';
        if (action.payload.searchTerm) state.tag = '';
      }

      if (action.payload.searchAdv || action.payload.searchAdv === '') {
        state.searchAdv = action.payload.searchAdv;
        state.seller = '';
        state.winner = '';
        if (action.payload.searchAdv) state.tag = '';
      }

      state.userLogin = GetCurrentUser();
    },
    reset: (state, action) => {
      state.pageNumber = 1;
      state.pageSize = 16;
      state.pageCount = 1;
      state.orderBy = 'newDesc';
      state.filterBy = 'live';
      state.seller = '';
      state.winner = '';
      state.searchTerm = '';
      state.searchAdv = '';
      state.tag = '';
      state.advSearchParam = ['Auction', 'Comment', 'Tag'];
    },
  },
});

export const { setParams, reset } = paramSlice.actions;

export const paramReducer = paramSlice.reducer;
