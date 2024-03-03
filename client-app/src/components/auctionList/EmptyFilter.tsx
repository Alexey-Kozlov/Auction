import Heading from './Heading';
import { Button } from 'flowbite-react';

type Props = {
    title?: string;
    subtitle?: string;
    showReset?: boolean;
    showLogin?: boolean;
}

export default function EmptyFilter({
    title = 'Нет совпадений для указанного фильтра',
    subtitle = 'Попробуйте измененить или сбросить фильтр',
    showReset,
    showLogin
}: Props) {

    return (
        <div className='h-[40vh] flex flex-col gap-2 justify-center items-center shadow-lg'>
            <Heading title={title} subtitle={subtitle} center />
            <div className='mt-4'>
                {showReset && (
                    <Button outline onClick={() => { }}>Удалить фильтры</Button>
                )}
                {showLogin && (
                    <Button outline onClick={() => { }}>Логин</Button>
                )}
            </div>
        </div>
    )
}
