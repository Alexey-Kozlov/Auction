import { Button, Modal } from 'flowbite-react'
import DatePicker, { registerLocale } from 'react-datepicker';
import { ru } from 'date-fns/locale'
import SwitchInput from '../inputComponents/SwitchInput';
import { useState } from 'react';
registerLocale('ru', ru);

type Props = {
    openModal: boolean;
    setResult: (rezult?: boolean) => void;    
    title: string;
    text: string;
    returnData: (rezult: Date) => void;
    resetLog: (rezult: boolean) => void;
    dateValue: Date | null;
}

export default function ModalConfirm({openModal, setResult, title, text, 
    returnData, resetLog, dateValue}:Props) {
      const [resLog, setResetLog] = useState(false);
      const handleResetLog = (event: React.FormEvent<HTMLInputElement>) => {
        setResetLog(event.currentTarget.checked);
        resetLog(event.currentTarget.checked);
      }

  return (
    <Modal show={openModal} onClose={() => {setResult(false);setResetLog(prev =>false);}}>
    <Modal.Header>{title}</Modal.Header>
    <Modal.Body>
      <div className="space-y-6">
      {dateValue && (
        <>
          <div>
            Укажите дату восстановления: 
              <DatePicker
                wrapperClassName="datepicker"
                locale='ru'
                showTimeSelect
                dateFormat='dd.MM.yyyy HH:mm'
                onChange={value => returnData(value!)}
                selected={dateValue}
              />
          </div>
          <div>            
            <label className="inline-flex items-center mb-5">
              <p className='font-semibold mr-3'>Удаление событий после даты восстановления:</p>
                <SwitchInput checked={resLog} handleImageUsing={handleResetLog} />
            </label>
          </div>
        </>
        )}
        <div className="text-base leading-relaxed text-gray-500 dark:text-gray-400">
          {text}
        </div>
      </div>
    </Modal.Body>
    <Modal.Footer>
      <Button color='success' onClick={() => {setResult(true);setResetLog(prev =>false);}}>Да</Button>
      <Button color="failure" onClick={() => {setResult(false);setResetLog(prev =>false);}}>Нет</Button>
    </Modal.Footer>
  </Modal>
  )
}
