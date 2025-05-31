import { SortDirection } from "../../../types";

export type NotificationListTypes = {
	AuctionId: string;
	Title: string;
	UserLogin: string;
};

export type NotificationSortType = {
	column: NotificationSortColumn;
	direction: SortDirection;
};

export enum NotificationSortColumn {
	AuctionId,
	Title,
	UserLogin,
}
