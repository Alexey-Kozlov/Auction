import { useGetImageForAuctionQuery } from '../../api/ImageApi';
const empty = require('../../assets/Empty.png');

type Props = {
    id?: string;
    dopStyle?: string;
}

export default function ImageCard({ id, dopStyle }: Props) {
    const { isLoading, data } = useGetImageForAuctionQuery(id!);

    if (isLoading) return;
    return (
        <img src={data?.result?.image ? `data:image/png;base64 , ${data.result.image}` : empty}
            alt=''
            className={`object-cover group-hover:drop-shadow-2xl duration-700 ease-in-out  w-auto h-auto
            ${isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'} 
            ${dopStyle}`}

        />
    )
}
