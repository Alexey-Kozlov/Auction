import { useEffect } from "react";
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
import { Auction, ProcessingState, RequestType } from "../../store/types";
import { setEventFlag } from "../../store/processingSlice";
import Waiter from "../Waiter";
import { useCookies } from "react-cookie";
import { v4 as uuidv4 } from "uuid";

export default function Listings() {
	const dispatch = useDispatch();
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
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

	//Обновляем набор записей при поступлении новых данных из апи - пишем в локальное хранилище
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

	//первоначальая загрузка - выставляем куки для логирования
	useEffect(() => {
		setCookie("RequestType", RequestType[RequestType.ReadList]);
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

	function setPageNumber(pageNumber: number) {
		dispatch(setParams({ pageNumber: pageNumber }));
	}

	if (auctionsData.isLoading && auctionsData.isFetching)
		return <h3>Загрузка...</h3>;

	return (
		<div>
			<Filters />
			{!elkSearch ? (
				auctions.length === 0 ? (
					<EmptyFilter showReset />
				) : (
					<div>
						<div className="grid grid-cols-4 gap-6 items-center">
							{auctions.map((auction: Auction) => {
								return (
									<AuctionCard auction={auction} key={auction.auctionId} />
								);
							})}
						</div>
						<div className="flex justify-center mt-4">
							<AppPagination
								pageChanged={setPageNumber}
								currentPage={params.pageNumber}
								totalPages={
									auctionsData.data?.result
										? auctionsData.data?.result.pageCount!
										: data.pageCount
								}
							/>
						</div>
					</div>
				)
			) : (
				<Waiter color="rgb(156 163 175)" />
			)}
		</div>
	);
}
