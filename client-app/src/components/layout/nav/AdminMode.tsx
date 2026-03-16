import { useEffect, useState } from 'react';
import { useGetCurrentSettingsQuery } from '../../../api/SettingsApi';
import { ProcessingState, SignalREvents } from '../../../types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../../store/store';
import { CheckEventReady } from '../../../utils/checkEvent';
import { setSettingsData } from '../../../store/settingsSlice';

export default function AdminMode() {
  const [adminMode, setAdminMode] = useState(false);
  const [reloadCounter, setReloadCounter] = useState(3);
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
    if (
      !settingsData.isLoading &&
      !settingsData.isFetching &&
      settingsData.isError
    ) {
      if (reloadCounter > 0) {
        setReloadCounter((prev) => {
          return prev - 1;
        });
        window.location.reload();
      }
      setAdminMode(true);
      dispatch(
        setSettingsData({
          adminMode: true,
        }),
      );
    }
    // eslint-disable-next-line
  }, [settingsData]);

  //обновление видимости контрола
  useEffect(() => {
    if (
      !CheckEventReady(
        procState,
        SignalREvents[SignalREvents.SetCurrentSettings],
      )
    ) {
      settingsData.refetch();
    }
    // eslint-disable-next-line
  }, [procState]);
  return (
    <div>
      {adminMode && <div className="AdminMode">Административный режим</div>}
    </div>
  );
}
