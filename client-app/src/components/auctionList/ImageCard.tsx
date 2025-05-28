import { useDispatch, useSelector } from "react-redux";
import { useGetImageForAuctionQuery } from "../../api/ImageApi";
import { ProcessingState } from "../../types";
import { RootState } from "../../store/store";
import { useEffect } from "react";
import { setEventFlag } from "../../store/processingSlice";
import { TransformWrapper, TransformComponent } from "react-zoom-pan-pinch";

const empty = require("../../assets/Empty.png");

type Props = {
	id?: string;
	dopStyle?: string;
	zooming: boolean;
	cache: boolean;
};

export default function ImageCard({ id, dopStyle, zooming, cache }: Props) {
	const imageQuery = useGetImageForAuctionQuery(
		{ id: id ? id : "", cache: cache },
		{
			skip: !id,
		}
	);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const dispatch = useDispatch();

	useEffect(() => {
		const eventState = procState.find(
			(p) => p.eventName === "ImageChanged" && p.ready && p.lastChanged
		);
		if (eventState && eventState.itemId && id && eventState.itemId === id) {
			imageQuery.refetch();
			dispatch(setEventFlag({ eventName: "ImageChanged", ready: false }));
		}
		// eslint-disable-next-line
	}, [procState, id]);

	if (imageQuery.isLoading) return;
	return (
		<>
			{zooming ? (
				<TransformWrapper centerOnInit={true} doubleClick={{ mode: "reset" }}>
					<TransformComponent>
						<img
							src={
								imageQuery.data?.result?.image
									? `data:image/jpeg;base64 , ${imageQuery.data.result.image}`
									: empty
							}
							alt=""
							className="AuctionImageCardZoom"
						/>
					</TransformComponent>
				</TransformWrapper>
			) : (
				<img
					src={
						imageQuery.data?.result?.image
							? `data:image/jpeg;base64 , ${imageQuery.data.result.image}`
							: empty
					}
					alt=""
					className={"AuctionImageCardNoZoom " + dopStyle}
				/>
			)}
		</>
	);
}
