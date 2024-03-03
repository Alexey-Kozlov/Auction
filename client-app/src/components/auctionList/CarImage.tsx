import React, { useState } from 'react'
const empty = require('../../assets/Empty.png');

type Props = {
    image?: string;
    style?: {}
}

export default function CarImage({ image, style }: Props) {
    const [isLoading, setIsLoading] = useState(true);
    return (
        <img src={image ? `data:image/png;base64 , ${image}` : empty}
            alt=''
            className={`object-cover group-hover:opacity-75 duration-700 ease-in-out
            ${isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'}`}
            sizes='(max-width:768px) 100vw, (max-width:1200px) 50vw, 25 vw'
            onLoad={() => setIsLoading(false)}
        />
    )
}
