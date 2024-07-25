import { useDispatch, useSelector } from 'react-redux';
import { useGetImageForAuctionQuery } from '../../api/ImageApi';
import { ProcessingState } from '../../store/types';
import { RootState } from '../../store/store';
import { useEffect } from 'react';
import { setEventFlag } from '../../store/processingSlice';
import { TransformWrapper, TransformComponent } from "react-zoom-pan-pinch";

const empty = require('../../assets/Empty.png');

type Props = {
    id?: string;
    dopStyle?: string;
    zooming: boolean;
}

export default function ImageCard({ id, dopStyle, zooming }: Props) {
    const imageQuery = useGetImageForAuctionQuery(id ? id : '', {
        skip: !id
    });
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const dispatch = useDispatch();

    useEffect(() => {
        const eventState = procState.find(p => p.eventName === 'ImageChanged');
        if (eventState && eventState.ready && eventState.itemId && id && eventState.itemId === id) {
            imageQuery.refetch();
            dispatch(setEventFlag({ eventName: 'ImageChanged', ready: false }));
        }
    }, [procState, dispatch, imageQuery, id]);

    if (imageQuery.isLoading) return;
    return (
        <>
            {zooming ?
                <TransformWrapper centerOnInit={true} doubleClick={{mode:"reset"}}   >
                    <TransformComponent>
                        <img src={imageQuery.data?.result?.image ? `data:image/png;base64 , ${imageQuery.data.result.image}` : empty}
                            alt=''
                            className={`object-cover ease-in-out  max-w-[750px] max-h-[512px]
            ${imageQuery.isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'} 
            `}
                        />
                    </TransformComponent>
                </TransformWrapper>
                :
                <img src={imageQuery.data?.result?.image ? `data:image/png;base64 , ${imageQuery.data.result.image}` : empty}
                    alt=''
                    className={`object-cover ease-in-out  w-auto h-auto
            ${imageQuery.isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'} 
            ${dopStyle}`}
                />
            }
        </>
    )
}
