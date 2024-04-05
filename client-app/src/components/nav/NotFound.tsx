import { Button } from 'flowbite-react';
import { useNavigate } from 'react-router-dom'

const NotFound = () => {
    const navigate = useNavigate();
    return (
        <>
            <div className='flex justify-center mt-5'>
                <h1 className='font-bold'>Запись не найдена</h1>
            </div>
            <div className='flex justify-center mt-2'>
                <Button onClick={() => navigate('/')}>Домой</Button>
            </div>
        </>
    )
}

export default NotFound;