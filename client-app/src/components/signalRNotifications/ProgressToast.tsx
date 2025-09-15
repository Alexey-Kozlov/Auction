import { ProgressBar } from 'primereact/progressbar';
import { Toast } from 'primereact/toast';
import { RefObject } from 'react';
import { Progress } from '../../types';
import { Button } from 'primereact/button';

type Props = {
  toast: RefObject<Toast | null>;
  data: Progress;
};

export default function ProgressToast({ toast, data }: Props) {
  const clear = () => {
    toast.current!.clear();
  };
  return (
    <div>
      <Toast
        ref={toast}
        // eslint-disable-next-line
        content={({}) => (
          <div className="ToastProgressContainer">
            <div className="CloseToastButtonContainer">
              <div className="CloseToastButton">
                <Button icon="pi pi-times" rounded text onClick={clear} />
              </div>
            </div>
            <p className="m-2 text-3xl text-center">{data.title}</p>
            <p className="m-2 text-3xl text-center">{data.message}</p>
            <div className="flex flex-column gap-2">
              <ProgressBar value={data.percent} showValue={false}></ProgressBar>
              <label className="text-xl text-center">
                {data.percent}% обработано...
              </label>
            </div>
          </div>
        )}
        position="bottom-right"
        onRemove={clear}
      ></Toast>
    </div>
  );
}
