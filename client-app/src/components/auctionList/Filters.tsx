import { Button } from 'flowbite-react';
import { AiOutlineClockCircle, AiOutlineSortAscending } from 'react-icons/ai';
import { BsFillStopCircleFill, BsStopwatchFill } from 'react-icons/bs';
import { GiFinishLine, GiFlame } from 'react-icons/gi';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { setParams } from '../../store/paramSlice';

const pageSizeButtons = [4, 8, 16];
const orderButtons = [
    {
        label: 'Наименование',
        icon: AiOutlineSortAscending,
        value: 'title'
    },
    {
        label: 'Окончание',
        icon: AiOutlineClockCircle,
        value: 'other'
    },
    {
        label: 'Недавние',
        icon: BsFillStopCircleFill,
        value: 'new'
    }
];
const filterButtons = [
    {
        label: 'Текущие',
        icon: GiFlame,
        value: 'live'
    },
    {
        label: 'Заканчивающиеся',
        icon: GiFinishLine,
        value: 'endingSoon'
    },
    {
        label: 'Завершенные',
        icon: BsStopwatchFill,
        value: 'finished'
    }
];

export default function Filters() {
    const dispatch = useDispatch();
    const pageSize = useSelector((state: RootState) => state.paramStore).pageSize;
    const orderBy = useSelector((state: RootState) => state.paramStore).orderBy;
    const filterBy = useSelector((state: RootState) => state.paramStore).filterBy;

    return (
        <div className='flex justify-between items-center mb-4'>
            <div style={{ width: '455px' }}>
                <div style={{ textAlign: 'center' }}>
                    <span className='uppercase text-sm text-gray-500 mr-2'>Отбор по : </span>
                </div>

                <Button.Group>
                    {filterButtons.map(({ label, icon: Icon, value }) => {
                        return (
                            <Button
                                key={value}
                                onClick={() => dispatch(setParams({ filterBy: value }))}
                                color={`${filterBy === value ? 'blue' : 'gray'}`}

                                className='focus:ring-0'
                            >
                                <Icon className='mr-3 h-4 w-4' />
                                {label}
                            </Button>
                        )
                    })}
                </Button.Group>


            </div>
            <div style={{ maxWidth: '475px' }}>
                <div style={{ textAlign: 'center' }}>
                    <span className='uppercase text-sm text-gray-500 mr-2'>Сортировать по : </span>
                </div>

                <Button.Group>
                    {orderButtons.map(({ label, icon: Icon, value }) => {
                        return (
                            <Button
                                key={value}
                                onClick={() => dispatch(setParams({ orderBy: value }))}
                                color={`${orderBy === value ? 'blue' : 'gray'}`}
                                className='focus:ring-0'
                            >
                                <Icon className='mr-3 h-4 w-4' />
                                {label}
                            </Button>
                        )
                    })}
                </Button.Group>
            </div>
            <div style={{ maxWidth: '140px' }}>
                <div style={{ textAlign: 'center' }}>
                    <span className='uppercase text-sm text-gray-500 mr-2'>Размер страницы</span>
                </div>
                <Button.Group>
                    {pageSizeButtons.map((value: number, index: number) => {
                        return (
                            <Button key={index}
                                onClick={() => dispatch(setParams({ pageSize: value }))}
                                color={`${pageSize === value ? 'blue' : 'gray'}`}
                                className='focus:ring-0'
                            >
                                {value}
                            </Button>
                        )
                    })}
                </Button.Group>
            </div>
        </div>
    )
}
