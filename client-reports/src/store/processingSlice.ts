import { createSlice } from '@reduxjs/toolkit';
import { ProcessingState, SignalREvents } from '../types';

export const initEventsState: ProcessingState[] = [];

export const processingSlice = createSlice({
  name: 'processing',
  initialState: initEventsState,
  reducers: {
    setEventFlag: (state, action) => {
      //удаляем флаг изменения у всех записей (если есть)
      if (state.length > 0) {
        state.forEach((item) => {
          item.lastChanged = false;
        });
      }
      //сбрасываем флаг ошибки (если пришло сообщение не об ошибке, а других типов)
      if (
        action.payload.eventName !== SignalREvents[SignalREvents.ErrorMessage]
      ) {
        let errorState = state.find(
          (p) => p.eventName === SignalREvents[SignalREvents.ErrorMessage],
        );
        if (errorState) {
          errorState.ready = !action.payload.ready;
        } else {
          state.push({
            eventName: SignalREvents[SignalREvents.ErrorMessage],
            ready: !action.payload.ready,
            lastChanged: true,
          });
        }
      }
      //далее основной функционал по назначению сообщения
      let userState = state.find(
        (p) => p.eventName === action.payload.eventName,
      );
      if (userState) {
        userState.ready = action.payload.ready;
        userState.param = action.payload.param ? action.payload.param : null;
        userState.itemId = action.payload.itemId;
        userState.lastChanged = true;
      } else {
        state.push({
          eventName: action.payload.eventName,
          ready: action.payload.ready,
          itemId: action.payload.itemId,
          param: action.payload.param ? action.payload.param : null,
          lastChanged: true,
        });
      }
    },
  },
});

export const { setEventFlag } = processingSlice.actions;
export const processingReducer = processingSlice.reducer;
