import { AiOutlineCar } from 'react-icons/ai';

export default function Logo() {
    return (
        <div
            className='flex items-center gap-2 text-3xl font-semibold text-red-500 cursor-pointer'
        >
            <AiOutlineCar size={34} />
            <div>Аукцион</div>
        </div>
    )
}
