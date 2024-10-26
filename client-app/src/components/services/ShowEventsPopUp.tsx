import React, { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { ProcessingState, RestoreDb, Session } from '../../store/types';
import { useElkIndexMutation, useSetSnapShotDbMutation, useRestoreSnapShotDbMutation } from '../../api/ServiceApi';
import { setEventFlag } from '../../store/processingSlice';

export default function ShowEventsPopUp() {
  const dispatch = useDispatch();
  const events: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
  const sessionId = useSelector((state: RootState) => state.paramStore).sessionId;
  const [elkIndex] = useElkIndexMutation();
  const [snapShotDb] = useSetSnapShotDbMutation();
  const [restoreSnapShot] = useRestoreSnapShotDbMutation();
  useEffect(() => {
    const doElkIndex = async (ses: Session) => {
      await elkIndex(ses);
    }
    const doSnapShotDb = async (ses: Session) => {
      await snapShotDb(ses);
    }
    const doRestoreSnapShot = async (ses: RestoreDb) => {
      await restoreSnapShot(ses);
    }
    if(events.find(p => p.eventName === 'ElkIndex' && p.ready)){
      const sesion: Session = {sessionid: sessionId};
      doElkIndex(sesion);
      //сбрасываем состояние эвента
      dispatch(setEventFlag({ eventName: 'ElkIndex', ready: false }));
    }
    if(events.find(p => p.eventName === 'SetSnapShotDb' && p.ready)){
      const sesion: Session = {sessionid: sessionId};
      doSnapShotDb(sesion);
      //сбрасываем состояние эвента
      dispatch(setEventFlag({ eventName: 'SetSnapShotDb', ready: false }));
    }
    if(events.find(p => p.eventName === 'RestoreSnapShotDb' && p.ready)){
      const data: RestoreDb = {sessionid: sessionId,snapShotId:'658b35a0-e0bd-4fac-a843-4b3782c1832d'};      
      doRestoreSnapShot(data);
      //сбрасываем состояние эвента
      dispatch(setEventFlag({ eventName: 'RestoreSnapShotDb', ready: false }));
    }    
    },[events, sessionId, elkIndex, snapShotDb, restoreSnapShot]);

  return (
    <></>
  )
}
