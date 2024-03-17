import { useGetImageForAuctionQuery } from '../../api/ImageApi';
const empty = require('../../assets/Empty.png');

type Props = {
    id?: string;
    style?: {}
}

export default function CarImage({ id, style }: Props) {
    const { isLoading, data } = useGetImageForAuctionQuery(id!);

    if (isLoading) return;
    return (
        <img src={data?.image ? `data:image/png;base64 , ${data.image}` : empty}
            alt=''
            className={`object-cover group-hover:opacity-75 duration-700 ease-in-out
            ${isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'}`}
            sizes='(max-width:768px) 100vw, (max-width:1200px) 50vw, 25 vw'
        />
    )
}
