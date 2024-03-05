import React from 'react'
import Heading from '../auctionList/Heading'
import { useLocation, useParams } from 'react-router-dom'

export default function AuctionForm() {

    const { id } = useParams();
    const location = useLocation();

    return (
        <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
            <Heading title='Редактирование аукциона' subtitle='Отредактируйте данные ниже' />

        </div>
    )
}
