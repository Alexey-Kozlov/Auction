import { Button, Modal } from 'flowbite-react'
import DatePicker, { registerLocale } from 'react-datepicker';
import { ru } from 'date-fns/locale'
registerLocale('ru', ru);

type Props = {
    openModal: boolean;
    setResult: (rezult?: boolean) => void;    
    title: string;
    text: string;
    returnData: (rezult: Date) => void;
    dateValue: Date | null;
}

export default function ModalConfirm({openModal, setResult, title, text, 
    returnData, dateValue}:Props) {
  return (
    <Modal show={openModal} onClose={() => setResult(false)}>
    <Modal.Header>{title}</Modal.Header>
    <Modal.Body>
      <div className="space-y-6">
      {dateValue && (
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
        )}
        <div className="text-base leading-relaxed text-gray-500 dark:text-gray-400">
          {text}
        </div>
      </div>
    </Modal.Body>
    <Modal.Footer>
      <Button color='success' onClick={() => setResult(true)}>Да</Button>
      <Button color="failure" onClick={() => setResult(false)}>Нет</Button>
    </Modal.Footer>
  </Modal>
  )
}
