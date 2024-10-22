import React, { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { ProcessingState, Session } from '../../store/types';
import { useElkIndexMutation, useSetSnapShotDbMutation } from '../../api/ServiceApi';
import { setEventFlag } from '../../store/processingSlice';

export default function ShowEventsPopUp() {
  const dispatch = useDispatch();
  const events: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
  const sessionId = useSelector((state: RootState) => state.paramStore).sessionId;
  const [elkIndex] = useElkIndexMutation();
  const [snapShotDb] = useSetSnapShotDbMutation();
    useEffect(() => {
        const doElkIndex = async (ses: Session) => {
            await elkIndex(ses);
        }
        const doSnapShotDb = async (ses: Session) => {
          await snapShotDb(ses);
      }
        if(events.find(p => p.eventName === 'ElkIndex' && p.ready)){
          const sesion: Session = {sessionid: sessionId}
          doElkIndex(sesion);
          //сбрасываем состояние эвента
          dispatch(setEventFlag({ eventName: 'ElkIndex', ready: false }));
        }
        if(events.find(p => p.eventName === 'SetSnapShotDb' && p.ready)){
          const sesion: Session = {sessionid: sessionId}
          doSnapShotDb(sesion);
          //сбрасываем состояние эвента
          dispatch(setEventFlag({ eventName: 'SetSnapShotDb', ready: false }));
      }
    },[events, sessionId, elkIndex, snapShotDb]);

  return (
    <></>
  )
}
