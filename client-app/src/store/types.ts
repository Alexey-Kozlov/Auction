export type Auction = {
	reservePrice: number;
	seller: string;
	winner?: string;
	soldAmount: number;
	currentHighBid: number;
	createAt: Date;
	updatedAt: Date;
	auctionEnd: Date;
	title: string;
	properties: string;
	description?: string;
	image?: string;
	auctionId: string;
	error?: string;
	usingImage?: boolean;
	show?: boolean;
};

export type Bid = {
	bidId: string;
	auctionId: string;
	bidder: string;
	bidTime: string;
	amount: number;
	show: boolean;
};

export type LoginResponse = {
	name: string;
	login: string;
	token: string;
	id: string;
};

export type CreateUser = {
	name: string;
	login: string;
	password: string;
};

export type LoginUser = {
	login: string;
	password: string;
};

export type User = {
	name: string;
	login: string;
	id?: string;
	isAdmin: boolean;
};

export type PagedResult<T> = {
	results: T[];
	pageCount: number;
	totalCount: number;
};

export type ApiResponseNet<T> = {
	statusCode: number;
	isSuccess: boolean;
	errorMessages: Array<string>;
	result: T;
};

export type ApiResponse<T> = {
	data?: ApiResponseNet<T>;
	error?: any;
};

export type ObjectResponse<T> = {
	data?: T;
	error?: any;
};

export type PlaceBidParams = {
	amount: number;
	auctionId: string;
	correlationId: string;
};

export type AuctionImage = {
	auctionId: string;
	image: string;
};

export type NotifyUser = {
	auctionid: string;
	enable: boolean;
	sessionid: string;
};

export type FinanceItem = {
	auctionId: string;
	itemId: string;
	value: number;
	actionDate: Date;
	status: number;
	show: boolean;
};

export type FinanceCreate = {
	amount: number;
	userlogin: string;
	sessionid: string;
};

export type ProcessingState = {
	eventName: string;
	ready: boolean;
	itemId?: string;
	param?: any;
};

export type AuctionUpdated = {
	auctionId: string;
	title: string;
	properties: string;
	description: string;
	image: string;
	reservePrice: number;
	auctionEnd: Date;
	correlationId: string;
	usingImage: boolean;
};

export type AuctionFinished = {
	auctionId: string;
	title: string;
	winner?: string;
	amount?: number;
};

export type AuctionDeleted = {
	auctionId: string;
	correlationId: string;
};

export type Message = {
	auctionId: string;
	message: string;
	messageType: number;
	userLogin?: string;
};

export type Session = {
	sessionid: string;
};

export type RestoreDb = {
	sessionid: string;
	restoreDate: Date;
	resetLog: boolean;
};

export type ModalParams = {
	confirmTitle: string;
	confirmText: string;
	handler?: string;
};

export type ProgressToast = {
	percent: number;
	duration: number;
};

export enum RequestType {
	ReadDetail,
	ReadList,
	Edit,
	Register,
	Login,
	Create,
	Finance,
	NotFound,
	TraceId,
}
