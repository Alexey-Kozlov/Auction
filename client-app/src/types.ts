export type Auction = {
	itemId: string;
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
	isGuest: boolean;
};

export type CreateUser = {
	name: string;
	login: string;
	password: string;
	isGuest: boolean;
};

export type LoginUser = {
	login: string;
	password: string;
	isGuest: boolean;
};

export type LogoutUser = {
	login: string;
};

export type User = {
	name: string;
	login: string;
	isAdmin: boolean;
	isGuest: boolean;
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
};

export type AuctionImage = {
	itemId: string;
	image: string;
};

export type NotifyUser = {
	itemId: string;
	enable: boolean;
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
};

export type ProcessingState = {
	eventName: string;
	ready: boolean;
	itemId?: string;
	param?: any;
	lastChanged: boolean;
};

export type AuctionUpdated = {
	itemId: string;
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
	itemId: string;
	title: string;
	winner?: string;
	soldAmount?: number;
};

export type AuctionDeleted = {
	itemId: string;
};

export type Message = {
	auctionId: string;
	message: string;
	messageType: number;
	userLogin?: string;
};

export type RestoreDb = {
	restoreDate: Date;
	resetLog: boolean;
};

export type ModalParams = {
	confirmTitle: string;
	confirmText: string;
	handler?: string;
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
	UsersCurrentPage
}

export type State = {
	pageNumber?: number;
	pageSize?: number;
	pageCount?: number;
	firstPage?: number;
	orderBy?: string;
	filterBy?: string;
	seller?: string;
	winner?: string;
	searchTerm?: string;
	searchAdv?: string;
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

export type Progress = {
	message: string;
	percent: number;
	title?: string;
}

export enum SignalREvents {
	BidPlaced,
	FinanceCreate,
	AuctionCreate,
	AuctionUpdate,
	AuctionDelete,	
	ResetImageCache,
	ElkIndexReset,
	ElkSearch,
	SetSnapShot,
	OperationProgress,
	AuctionFinished,
	ErrorMessage,
	EditNotification,
	CommunicationCreate,
	CommunicationUpdate,
	CommunicationDelete,
	RestoreSnapShot,
	CommunicationChanged,
	CollectionChanged
}

export type UrlCacheList = {
	urlAuction: string;
	urlImage: {
		id: string,
		cache: boolean
	}
}

