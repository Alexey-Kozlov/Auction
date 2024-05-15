import React from 'react'
import { RotatingLines } from 'react-loader-spinner'
type Props = {
    color: string;
}

export default function Waiter({ color }: Props) {
    return (
        <div className='flex items-center justify-center'>
            <RotatingLines
                visible={true}
                width="50"
                strokeWidth="5"
                strokeColor={color}
                animationDuration="0.75"
                ariaLabel="rotating-lines-loading"

            />
        </div>
    )
}
