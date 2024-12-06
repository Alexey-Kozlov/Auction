import toast from 'react-hot-toast';
import { Message } from '../../store/types';
import { BsInfoCircle } from "react-icons/bs";
import { Progress } from "flowbite-react";

type Props = {
    message: Message;
    toastId: string;
}

export default function ProgressMessageToast({ message, toastId }: Props) {
    return (
        <div>
            <>
                <div className='flex flex-row-reverse' >
                    <button onClick={() => toast.dismiss(toastId)}>X</button>
                </div>
                <div className='flex flex-col items-center w-80'>
                    <div className='flex flex-row  gap-2'>
                        <div>
                            <BsInfoCircle size={36} />
                        </div>

                        <div className='text-center'>
                            {message.message !== "-1" ? 
                                (message.message + ", завершено - " + message.messageType + "%") :
                                ("Записи не найдены")
                            }

                        </div>

                    </div>
                    <div className='flex flex-row'>
                        <div>
                            {message.message !== "-1" &&
                                <Progress progress={message.messageType} textLabel="Выполнено" 
                                    size="lg" labelProgress labelText color='blue' className='w-80' />
                            }
                        </div>
                    </div>
                </div>
            </>
        </div>
    )
}
