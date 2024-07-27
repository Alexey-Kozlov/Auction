import { Button, Modal } from 'flowbite-react'
type Props = {
    openModal: boolean;
    setResult: (rezult?: boolean) => void;    
    title: string;
    text: string;
}

export default function ModalConfirm({openModal, setResult, title, text}:Props) {
  return (
    <Modal show={openModal} onClose={() => setResult(false)}>
    <Modal.Header>{title}</Modal.Header>
    <Modal.Body>
      <div className="space-y-6">
        <p className="text-base leading-relaxed text-gray-500 dark:text-gray-400">
          {text}
        </p>
      </div>
    </Modal.Body>
    <Modal.Footer>
      <Button color='success' onClick={() => setResult(true)}>Да</Button>
      <Button color="failure" onClick={() => setResult(false)}>Нет</Button>
    </Modal.Footer>
  </Modal>
  )
}
