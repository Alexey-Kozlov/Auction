import { configureStore } from '@reduxjs/toolkit';
import auctionApi from '../api/AuctionApi';
import authApi from '../api/AuthApi';
import { authReducer, refreshLinkReducer } from './authSlice';
import { bidReducer } from './bidSlice';
import { auctionReducer } from './auctionSlice';
import { paramReducer } from './paramSlice';
import bidApi from '../api/BidApi';
import imageApi from '../api/ImageApi';
import notificationApi from '../api/NotificationApi';
import financeApi from '../api/FinanceApi';
import processingApi from '../api/ProcessingApi';
import { processingReducer } from './processingSlice';
import serviceApi from '../api/ServiceApi';
import { chatMessageReducer, chatResponseReducer } from './chatSlice';
import communicationApi from '../api/CommunicationApi';
import { serviceReducer } from './serviceSlice';
import { cacheReducer } from './cacheSlice';
import settingsApi from '../api/SettingsApi';
import { settingsReducer } from './settingsSlice';
import { rtkQueryErrorLogger } from '../api/test';

const store = configureStore({
  reducer: {
    authStore: authReducer,
    bidStore: bidReducer,
    auctionStore: auctionReducer,
    paramStore: paramReducer,
    processingStore: processingReducer,
    chatMessageStore: chatMessageReducer,
    chatResponseStore: chatResponseReducer,
    serviceStore: serviceReducer,
    cacheStore: cacheReducer,
    refreshLink: refreshLinkReducer,
    settingsStore: settingsReducer,
    [auctionApi.reducerPath]: auctionApi.reducer,
    [authApi.reducerPath]: authApi.reducer,
    [bidApi.reducerPath]: bidApi.reducer,
    [imageApi.reducerPath]: imageApi.reducer,
    [notificationApi.reducerPath]: notificationApi.reducer,
    [financeApi.reducerPath]: financeApi.reducer,
    [processingApi.reducerPath]: processingApi.reducer,
    [serviceApi.reducerPath]: serviceApi.reducer,
    [communicationApi.reducerPath]: communicationApi.reducer,
    [settingsApi.reducerPath]: settingsApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    })
      .concat(auctionApi.middleware)
      .concat(authApi.middleware)
      .concat(bidApi.middleware)
      .concat(imageApi.middleware)
      .concat(notificationApi.middleware)
      .concat(financeApi.middleware)
      .concat(processingApi.middleware)
      .concat(serviceApi.middleware)
      .concat(communicationApi.middleware)
      .concat(settingsApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export default store;
