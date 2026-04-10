import { useEffect, useState } from 'react';
import { CurrentSettings } from '../../../types';
import { RootState } from '../../../store/store';
import { useSelector } from 'react-redux';

export default function AdminMode() {
  const [adminMode, setAdminMode] = useState(false);
  const settingsState: CurrentSettings = useSelector(
    (state: RootState) => state.settingsStore,
  );

  //обновление видимости контрола
  useEffect(() => {
    setAdminMode(settingsState.adminMode);
    // eslint-disable-next-line
  }, [settingsState]);
  return (
    <div>
      {adminMode && <div className="AdminMode">Административный режим</div>}
    </div>
  );
}
