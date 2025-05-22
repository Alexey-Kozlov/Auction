import { useEffect, useRef } from "react";
import qs from "query-string";
import { useGetAuctionsQuery } from "../../api/AuctionApi";
import EmptyFilter from "./EmptyFilter";
import AuctionCard from "./AuctionCard";
import AppPagination from "./AddPagination";
import { useDispatch, useSelector } from "react-redux";
import { setParams } from "../../store/paramSlice";
import { RootState } from "../../store/store";
import { setData } from "../../store/auctionSlice";
import Filters from "./Filters";
import { Auction, ProcessingState } from "../../types";
import { setEventFlag } from "../../store/processingSlice";
import Waiter from "../Waiter";
import { Sticky } from "semantic-ui-react";

export default function Listings() {
	const dispatch = useDispatch();
	// eslint-disable-next-line
	const params = useSelector((state: RootState) => state.paramStore);
	const data = useSelector((state: RootState) => state.auctionStore);
	const auctions: Auction[] = data.auctions;
	const url = qs.stringifyUrl({ url: "", query: params });
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const elkSearch =
		procState.find((p) => p.eventName === "ElkSearch" && p.ready) &&
		params.searchAdv;
	//автоматически запускается при изменении url
	let auctionsData = useGetAuctionsQuery(url, {
		skip: !params.sessionId,
		refetchOnMountOrArgChange: true,
	});

	// Обновляем набор записей при поступлении новых данных из апи - пишем в локальное хранилище
	// auctionStore -> auctionSlice
	useEffect(() => {
		if (
			!auctionsData.isLoading &&
			!auctionsData.isFetching &&
			auctionsData.data
		) {
			dispatch(setData(auctionsData.data.result));
		}
		// eslint-disable-next-line
	}, [auctionsData]);

	//запрос на обновление данных при поступлении сообщения об изменении коллекции - нужно
	//принудительно обновить все записи.
	useEffect(() => {
		const eventStateChanged = procState.find(
			(p) => p.eventName === "CollectionChanged" && p.ready
		);
		if (eventStateChanged) {
			auctionsData.refetch();
			dispatch(setEventFlag({ eventName: "CollectionChanged", ready: false }));
		}
		// eslint-disable-next-line
	}, [procState]);

	function setPageNumber(pageNumber: number) {
		dispatch(setParams({ pageNumber: pageNumber }));
	}

	const stickDiv = useRef<HTMLDivElement>(null);

	if (auctionsData.isLoading && auctionsData.isFetching)
		return <h3>Загрузка...</h3>;

	return (
		<div ref={stickDiv}>
			<Sticky offset={65} context={stickDiv}>
				<div className="ListingFilter">
					<Filters />
				</div>
			</Sticky>

			<div className="ListingContainer">
				{!elkSearch ? (
					auctions.length === 0 ? (
						<EmptyFilter showReset />
					) : (
						<div>
							<div className="ListItem">
								{auctions.map((auction: Auction) => {
									return (
										<AuctionCard auction={auction} key={auction.auctionId} />
									);
								})}
							</div>
							<div className="ListPagination">
								<AppPagination
									pageChanged={setPageNumber}
									currentPage={params.pageNumber!}
									totalPages={data.pageCount}
								/>
							</div>
						</div>
					)
				) : (
					<Waiter color="rgb(156 163 175)" />
				)}
			</div>
		</div>
	);
}
