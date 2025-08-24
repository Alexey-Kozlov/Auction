import { createSlice } from "@reduxjs/toolkit";
import { UrlCacheList } from "../types";

export const cacheState: UrlCacheList = {
	urlAuction: "",
	urlImage: {cache:false,id:''}
};

export const cacheSlice = createSlice({
	name: "cache",
	initialState: cacheState,
	reducers: {
		setCacheQuery: (state, action) => {
			if(action.payload.urlAuction) state.urlAuction = action.payload.urlAuction;
			if(action.payload.urlImage) state.urlImage = action.payload.urlImage;
		},
		clearCacheUrls: (state, action) => {
			state.urlAuction = "";
			state.urlImage = {cache:false, id:''}
		}
	},
});


export const { setCacheQuery, clearCacheUrls } = cacheSlice.actions;
export const cacheReducer = cacheSlice.reducer;
