import React, { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { ProcessingState, RestoreDb, Session } from '../../store/types';
import { useElkIndexMutation, useSetSnapShotMutation, useRestoreSnapShotMutation } from '../../api/ServiceApi';
import { setEventFlag } from '../../store/processingSlice';

export default function ShowEventsPopUp() {
  const dispatch = useDispatch();
  const events: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
  const sessionId = useSelector((state: RootState) => state.paramStore).sessionId;
  const [elkIndex] = useElkIndexMutation();
  const [snapShotDb] = useSetSnapShotMutation();
  const [restoreSnapShot] = useRestoreSnapShotMutation();
  useEffect(() => {
    const doElkIndex = async (ses: Session) => {
      await elkIndex(ses);
    }
    const doSetSnapShot = async (ses: Session) => {
      await snapShotDb(ses);
    }
    const doRestoreSnapShot = async (ses: RestoreDb) => {
      await restoreSnapShot(ses);
    }
    if(events.find(p => p.eventName === 'ElkIndex' && p.ready)){
      const sesion: Session = {sessionid: sessionId};
      doElkIndex(sesion);
      dispatch(setEventFlag({ eventName: 'ElkIndex', ready: false }));
    }
    if(events.find(p => p.eventName === 'SetSnapShot' && !p.ready)){
      const sesion: Session = {sessionid: sessionId};
      doSetSnapShot(sesion);
    }
    if(events.find(p => p.eventName === 'RestoreSnapShot' && !p.ready)){
      const data: RestoreDb = {sessionid: sessionId, 
        restoreDate: events.find(p => p.eventName === 'RestoreSnapShot' && !p.ready)?.param};      
      doRestoreSnapShot(data);
    }    
    },[events, sessionId, elkIndex, snapShotDb, restoreSnapShot, dispatch]);

  return (
    <></>
  )
}
