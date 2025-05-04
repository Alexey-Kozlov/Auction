export type Auction = {
	id: string;
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
	finished: boolean;
};

export type Bid = {
	id: string;
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

export type LogoutUser = {
	login: string;
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
	id: string;
	auctionId: string;
	image: string;
};

export type NotifyUser = {
	auctionid: string;
	enable: boolean;
	sessionid: string;
};

export type FinanceItem = {
	id: string;
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
	id: string;
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
	id: string;
	auctionId: string;
	title: string;
	winner?: string;
	amount?: number;
};

export type AuctionDeleted = {
	id: string;
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
	show: boolean;
};

export enum RequestType {
	ReadDetail,
	ReadList,
	Edit,
	Register,
	Login,
	Logout,
	Create,
	Finance,
	NotFound,
	TraceId,
	Delete,
}

export type State = {
	pageNumber: number;
	pageSize: number;
	pageCount: number;
	orderBy: string;
	filterBy: string;
	seller?: string;
	winner?: string;
	searchTerm: string;
	searchAdv: string;
	sessionId: string;
};
