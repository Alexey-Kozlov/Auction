import { useEffect } from "react";
import { useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { ProcessingState, RestoreDb, Session } from "../../types";
import {
  useElkIndexMutation,
  useSetSnapShotMutation,
  useRestoreSnapShotMutation,
  useResetImageCacheMutation,
} from "../../api/ServiceApi";
import { CheckEventLastChangedReady } from "../../utils/CheckEvent";

export default function HandleServiceEvents() {
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore
  );
  const sessionId = useSelector(
    (state: RootState) => state.paramStore
  ).sessionId;
  const [elkIndex] = useElkIndexMutation();
  const [snapShotDb] = useSetSnapShotMutation();
  const [restoreSnapShot] = useRestoreSnapShotMutation();
  const [resetImageCache] = useResetImageCacheMutation();
  useEffect(() => {
    //запускаем переиндексацию
    if (CheckEventLastChangedReady(procState, "ElkIndex")) {
      const sesion: Session = { sessionid: sessionId };
      elkIndex(sesion);
    }

    //запускаем создание снапшота
    if (CheckEventLastChangedReady(procState, "SetSnapShot")) {
      const session: Session = { sessionid: sessionId };
      snapShotDb(session);
    }

    //запускаем восстановление снапшота
    if (CheckEventLastChangedReady(procState, "RestoreSnapShot")) {
      let restoreData = procState.find(
        (p) => p.eventName === "RestoreSnapShot" && p.ready && p.lastChanged
      )?.param;
      const data: RestoreDb = {
        sessionid: sessionId,
        resetLog: restoreData.resetLog,
        restoreDate: restoreData.dateValue,
      };
      restoreSnapShot(data);
    }

    //запускаем сброс кеша изображений
    if (CheckEventLastChangedReady(procState, "ResetImageCache")) {
      const session: Session = { sessionid: sessionId };
      resetImageCache(session);
    }
    // eslint-disable-next-line
  }, [procState, sessionId]);

  return <></>;
}
