import { format } from "date-fns";
import { Bid } from "../../types";
import NumberWithSpaces from "../../utils/NumberWithSpaces";
import { useGetUserNameQuery } from "../../api/AuthApi";

type Props = {
	bid: Bid;
};

export default function BidItem({ bid }: Props) {
	const { data, isLoading } = useGetUserNameQuery(bid.bidder, {
		skip: !bid.bidder,
	});

	return (
		<div className="BidItemContainer">
			<div className="BidItem">
				<span className="BidItemText">
					Покупатель: {!isLoading && data?.result}
				</span>
				<span className="BidItemText">
					Время: {format(new Date(bid?.bidTime), "dd.MM.yyyy HH:mm")}
				</span>
			</div>
			<div className="BidItemText">{NumberWithSpaces(bid?.amount)} руб</div>
		</div>
	);
}
