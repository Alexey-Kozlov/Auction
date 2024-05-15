import toast from 'react-hot-toast';
import { ImWarning } from "react-icons/im";
import { Message } from '../../store/types';

type Props = {
    message: Message;
    toastId: string;
}

export default function WarningMessageToast({ message, toastId }: Props) {
    return (
        <div>
            <>
                <div className='flex flex-row-reverse' >
                    <button onClick={() => toast.dismiss(toastId)}>X</button>
                </div>
                <div className='flex flex-col items-center'>
                    <div className='flex flex-row  gap-2'>
                        <div>
                            <ImWarning size={36} />
                        </div>
                        <span>{message.message}</span>
                    </div>
                </div>
            </>
        </div>
    )
}
