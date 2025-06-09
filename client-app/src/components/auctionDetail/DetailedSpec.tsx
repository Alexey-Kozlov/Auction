import { Auction, User } from "../../types";
import { useGetUserNameQuery } from "../../api/AuthApi";
import { BiCommentDetail } from "react-icons/bi";
import { PiChats } from "react-icons/pi";
import ChatTable from "./Chat/ChatTable";
import { Menu } from "primereact/menu";

type Props = {
	auction: Auction;
	user: User;
};
export default function DetailedSpecs({ auction, user }: Props) {
	const { data, isLoading } = useGetUserNameQuery(auction.seller, {
		skip: !auction.seller,
	});

	return {};
}
