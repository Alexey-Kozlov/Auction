import { SortDirection } from "../../../types";

export type AuctionListTypes = {
	AuctionId: string;
	Seller: string;
	Bidder: string;
	Amount: number;
	Title: string;
	StartDate: Date;
	EndDate: Date;
};

export type AuctionBidSortType = {
	column: AuctionListSortColumn;
	direction: SortDirection;
};

export enum AuctionListSortColumn {
	AuctionId,
	Seller,
	Bidder,
	Amount,
	Title,
	StartDate,
	EndDate,
}
