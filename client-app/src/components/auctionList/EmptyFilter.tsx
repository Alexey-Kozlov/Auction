import { useDispatch } from 'react-redux';
import Heading from './Heading';
import { Button } from 'flowbite-react';
import { reset } from '../../store/paramSlice';

type Props = {
    title?: string;
    subtitle?: string;
    showReset?: boolean;
}

export default function EmptyFilter({
    title = 'Нет совпадений для указанного фильтра',
    subtitle = 'Попробуйте измененить или сбросить фильтр',
    showReset
}: Props) {
    const dispather = useDispatch();
    const clearFilters = () => {
        dispather(reset(null));
    }
    return (
        <div className='h-[40vh] flex flex-col gap-2 justify-center items-center shadow-lg'>
            <Heading title={title} subtitle={subtitle} center />
            <div className='mt-4'>
                {showReset && (
                    <Button outline onClick={clearFilters}>Удалить фильтры</Button>
                )}
            </div>
        </div>
    )
}
