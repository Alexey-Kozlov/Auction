import { createSlice } from '@reduxjs/toolkit';
import { TagItem } from '../types';

type State = {
  TagList: TagItem[];
};

const initialState: State = {
  TagList: [],
};

export const tagSlice = createSlice({
  name: 'tag',
  initialState: initialState,
  reducers: {
    addTag: (state, action) => {
      state.TagList.push({
        label: action.payload.tag,
        value: action.payload.tag,
      });
    },
    setTagList: (state, action) => {
      state.TagList = action.payload.tagList;
    },
    deleteTag: (state, action) => {
      state.TagList = state.TagList.filter((p) => p !== action.payload.tag);
    },
  },
});

export const { addTag, deleteTag, setTagList } = tagSlice.actions;

export const tagReducer = tagSlice.reducer;
