export type Auction = {
	auctionId: string;
	reservePrice: number;
	seller: string;
	sellerName?: string;
	winner?: string;
	winnerName: string;
	soldAmount: number;
	currentHighBid: number;
	createAt: Date;
	updatedAt: Date;
	auctionEnd: Date;
	title: string;
	properties: string;
	description?: string;
	image?: string;
	error?: string;
	usingImage?: boolean;
	show?: boolean;
	finished: boolean;
};

export type Bid = {
	itemId: string;
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
	itemId: string;
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
	itemId: string;
	auctionId: string;
	image: string;
};

export type NotifyUser = {
	auctionid: string;
	enable: boolean;
	sessionid?: string;
};

export type FinanceStore = {
	results: FinanceTableItem[];
	totalCount: number;
	pageCount: number;
};

export type FinanceItem = {
	auctionId: string;
	itemId: string;
	value: number;
	actionDate: Date;
	status: number;
	show: boolean;
};

export type FinanceTableItem = {
	auctionId: string;
	itemId: string;
	value: number;
	actionDate: Date;
	status: number;
	show: boolean;
	auctionTitle: string;
	auctionSeller: string;
};

export type FinanceCreate = {
	amount: number;
	userlogin: string;
	sessionid?: string;
};

export type ProcessingState = {
	eventName: string;
	ready: boolean;
	itemId?: string;
	param?: any;
	lastChanged: boolean;
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
	sessionid?: string;
};

export type RestoreDb = {
	sessionid?: string;
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
	message: string;
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
	UpdatePassword,
	Bids,
	Balance,
	Image,
	Notification,
	ELK,
	SnapShot,
	Cache,
	SignalR,
	Communications,
}

export type State = {
	pageNumber?: number;
	pageSize?: number;
	pageCount?: number;
	orderBy?: string;
	filterBy?: string;
	seller?: string;
	winner?: string;
	searchTerm?: string;
	searchAdv?: string;
	sessionId?: string;
};

export type FormErrors = {
	name: string;
	message: string;
};

export enum SortDirection {
	descending,
	ascending,
}

export enum FinanceSortColumn {
	title,
	actionDate,
	status,
	value,
	seller,
}

export type FinanceSortType = {
	column: FinanceSortColumn;
	direction: SortDirection;
};

export type RequestAuctionsArray = {
	auctionIds: string[];
};

export enum ToastType {
	Info,
	Warning,
	Error,
}

export type ChatComment = {
	itemId: string;
	parentId: string | null;
	message: string;
	userLogin: string;
	auctionId: string;
	updateAt: Date;
	sessionId: SessionType | string;
	actionType: ActionType;
};

export enum ActionType {
	create,
	read,
	update,
	delete,
}

export type ChatResponse = {
	itemId: string;
	parentId: string | null;
	message: string;
	userLogin: string;
	auctionId: string;
	updateAt: Date;
	actionType: ActionType | null;
};

export type NotificationEvent = {
	show: boolean;
	data: string;
};

export type DataRow = {
	field: string;
	value: {};
};

export enum SessionType {
	all,
	auctionGroup,
}

export enum ModalTypes {
	warning,
	info,
}
