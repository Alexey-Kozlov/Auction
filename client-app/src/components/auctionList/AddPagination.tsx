import { Pagination } from 'flowbite-react'

type Props = {
    currentPage: number;
    totalPages: number;
    pageChanged: (page: number) => void;
}

export default function AppPagination({ currentPage, totalPages, pageChanged }: Props) {
    return (
        <Pagination
            currentPage={currentPage}
            onPageChange={e => pageChanged(e)}
            totalPages={totalPages}
            layout='pagination'
            previousLabel='Назад'
            nextLabel='Вперед'
            showIcons={true}
            className='text-blue-500 mb-5'
        />
    )
}
