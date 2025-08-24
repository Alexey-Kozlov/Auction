import { useEffect } from "react";
import { useSelector } from "react-redux";
import { RootState } from "../../store/store";
import { ProcessingState, RestoreDb } from "../../types";
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
  const [elkIndex] = useElkIndexMutation();
  const [snapShotDb] = useSetSnapShotMutation();
  const [restoreSnapShot] = useRestoreSnapShotMutation();
  const [resetImageCache] = useResetImageCacheMutation();
  useEffect(() => {
    //запускаем переиндексацию
    if (CheckEventLastChangedReady(procState, "ElkIndex")) {
      elkIndex(null);
    }

    //запускаем создание снапшота
    if (CheckEventLastChangedReady(procState, "SetSnapShot")) {
      snapShotDb(null);
    }

    //запускаем восстановление снапшота
    if (CheckEventLastChangedReady(procState, "RestoreSnapShot")) {
      let restoreData = procState.find(
        (p) => p.eventName === "RestoreSnapShot" && p.ready && p.lastChanged
      )?.param;
      const data: RestoreDb = {
        resetLog: restoreData.resetLog,
        restoreDate: restoreData.dateValue,
      };
      restoreSnapShot(data);
    }

    //запускаем сброс кеша изображений
    if (CheckEventLastChangedReady(procState, "ResetImageCache")) {
      resetImageCache(null);
    }
    // eslint-disable-next-line
  }, [procState]);

  return <></>;
}
