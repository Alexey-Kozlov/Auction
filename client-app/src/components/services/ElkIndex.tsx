import React, { useEffect } from 'react'
import { useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { ProcessingState, Session } from '../../store/types';
import { useElkIndexMutation } from '../../api/ElkApi';

export default function ElkIndex() {
    const elkIndexEvent: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const sessionId = useSelector((state: RootState) => state.paramStore).sessionId;
    const [elkIndex] = useElkIndexMutation();
    useEffect(() => {
        const doElkIndex = async (ses: Session) => {
            await elkIndex(ses);
        }
        if(elkIndexEvent.find(p => p.eventName === 'ElkIndex' && p.ready)){
            const sesion: Session = {sessionid: sessionId}
            doElkIndex(sesion);
        }
    },[elkIndexEvent,sessionId,elkIndex]);

  return (
    <></>
  )
}
