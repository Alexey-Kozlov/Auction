import { useEffect, useState } from "react";
import { useGetCurrentSettingsQuery } from "../../../api/SettingsApi";
import { ProcessingState, SignalREvents } from "../../../types";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../../store/store";
import { checkEventReady } from "../../../utils/checkEvent";
import { setSettingsData } from "../../../store/settingsSlice";

export default function AdminMode() {
  const [adminMode, setAdminMode] = useState(false);
  const settingsData = useGetCurrentSettingsQuery({});
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );
  const dispatch = useDispatch();

  useEffect(() => {
    if (
      !settingsData.isLoading &&
      !settingsData.isFetching &&
      settingsData.data?.result
    ) {
      setAdminMode(settingsData.data!.result.adminMode);
      dispatch(
        setSettingsData({
          adminMode: settingsData.data!.result.adminMode,
        }),
      );
    }
  }, [settingsData]);

  //обновление видимости контрола
  useEffect(() => {
    if (
      !checkEventReady(
        procState,
        SignalREvents[SignalREvents.SetCurrentSettings],
      )
    ) {
      settingsData.refetch();
    }
  }, [procState]);
  return (
    <div>
      {adminMode && <div className="AdminMode">Административный режим</div>}
    </div>
  );
}
