import React, { useEffect } from 'react'
import { useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { ProcessingState, Session } from '../../store/types';
import { useSetSnapShotDbMutation } from '../../api/ElkApi';

export default function SnapSotDb() {
    const snapShotDbEvent: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const sessionId = useSelector((state: RootState) => state.paramStore).sessionId;
    const [snapShotDb] = useSetSnapShotDbMutation();
    useEffect(() => {
        const doElkIndex = async (ses: Session) => {
            await snapShotDb(ses);
        }
        if(snapShotDbEvent.find(p => p.eventName === 'SetSnapShotDb' && p.ready)){
            const sesion: Session = {sessionid: sessionId}
            doElkIndex(sesion);
        }
    },[snapShotDbEvent,sessionId,snapShotDb]);

  return (
    <></>
  )
}
