import { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../../store/store";
import Heading from "../auctionList/Heading";
import { Auction, Bid, User } from "../../types";
import EmptyFilter from "../auctionList/EmptyFilter";
import BidItem from "./BidItem";
import BidForm from "./BidForm";
import { useGetBidsForAuctionQuery } from "../../api/BidApi";
import { setBids, setOpen } from "../../store/bidSlice";
import NumberWithSpaces from "../../utils/NumberWithSpaces";
import { Segment } from "semantic-ui-react";

type Props = {
	user: User | null;
	auction: Auction;
};

export default function BidList({ user, auction }: Props) {
	const dispatch = useDispatch();
	const [lastBidId, setLastBidId] = useState("");
	const bidList = useGetBidsForAuctionQuery(auction?.auctionId);
	const bidStore = useSelector((state: RootState) => state.bidStore);
	const bids = bidStore.bids;
	const open = bidStore.open;
	const openForBids = new Date(auction?.auctionEnd) > new Date();

	//вычисляем самую большую ставку. Делать ставку меньше нельзя
	const bidRestriction = () => {
		let result = bids?.reduce((prev, current) => {
			return prev > current?.amount ? prev : current?.amount;
		}, 0);
		if (auction.reservePrice && auction.reservePrice > result) {
			result = auction.reservePrice;
		}
		return result;
	};

	const itemsRef = useRef<null | HTMLDivElement>(null);

	//при каждом обновлении заявок - вычисление последней заявки для прокрутки
	//списка заявок вверх (если заявок много)
	useEffect(() => {
		if (!bidList.isLoading && !bidList.isFetching && bids && bids.length > 0) {
			const maxBidId: Bid = Array.from(bids).sort((a: Bid, b: Bid) => {
				return Date.parse(b.bidTime) - Date.parse(a.bidTime);
			})[0];
			setLastBidId(maxBidId.bidId);
		}
		// eslint-disable-next-line
	}, [bidList, bids]);

	//первоначальное заполнение списка заявок
	useEffect(() => {
		if (!bidList.isLoading && !bidList.isFetching) {
			dispatch(setBids(bidList.data?.result));
		}
		// eslint-disable-next-line
	}, [bidList]);

	//закрытие аукциона
	useEffect(() => {
		dispatch(setOpen(openForBids));
		// eslint-disable-next-line
	}, [openForBids]);

	//перемотка списка завок - самые послеДние - в самом верху,
	//и потом перемотка всей странички вверх - чтобы были видны последние изменения
	useEffect(() => {
		if (itemsRef && itemsRef.current) {
			itemsRef.current.scrollIntoView({
				//behavior: 'smooth',
				behavior: "auto",
				block: "start",
				inline: "nearest",
			});
		}
		setTimeout(() => {
			window.scrollTo({ top: 0, behavior: "smooth" });
		}, 1000);
		// eslint-disable-next-line
	}, [lastBidId]);

	if (bidList.isLoading) return <span>Загрузка предложений...</span>;

	return (
		<div>
			<div className="mb-30">
				{bids?.length === 0 ? (
					<Heading
						title="Нет предложений для этого аукциона"
						subtitle="Сделайте предложение"
					/>
				) : (
					<Heading
						title={`Текущее лучшее предложение - ${NumberWithSpaces(
							bidRestriction()
						)} руб`}
					/>
				)}
			</div>
			<div>
				<div className="BidListHeight">
					{bids?.map((bid, index) => (
						<Segment
							key={bid?.bidId}
							secondary
							className="BidListItem"
							ref={bid?.bidId === lastBidId ? itemsRef : null}
						>
							<BidItem bid={bid} />
						</Segment>
					))}
				</div>
			</div>
			<div className="DetailNotifyText text-center">
				{!open ? (
					<div>Аукцион завершен</div>
				) : !user?.id ? (
					<div>Войдите в систему чтобы делать заявки</div>
				) : user && user.login === auction?.seller ? (
					<div>Невозможно сделать заявку для собственного аукциона</div>
				) : (
					<BidForm
						auctionId={auction?.auctionId}
						highBid={bidRestriction()}
						bidList={bidList}
					/>
				)}
			</div>
		</div>
	);
}
