import { ProgressSpinner } from 'primereact/progressspinner';
import { CheckEventNotReady } from '../utils/checkEvent';
import { ProcessingState, SignalREvents } from '../types';
import { RootState } from '../store/store';
import { useSelector } from 'react-redux';

export default function Waiter() {
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore,
  );
  return (
    <>
      {CheckEventNotReady(
        procState,
        SignalREvents[SignalREvents.ErrorMessage],
      ) ? (
        <div className="Absolute-Center">
          <ProgressSpinner style={{ width: '50px', height: '50px' }} />
        </div>
      ) : (
        <div></div>
      )}
    </>
  );
}
