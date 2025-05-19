export interface ReportItem {
	Name: string;
	Description: string;
	Id: string;
	Icon: string;
}

export type ParameterItem = {
	Label: string;
	Type: ParameterType;
	Value: string;
	Id: string;
};

export enum ParameterType {
	Text,
	Date,
	Number,
	Select,
	Bool,
}

export type ParameterSelect = {
	Label: string;
	Value: string;
	Default: boolean;
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

export enum ReportType {
	AuctionList,
	NotificationList,
}

export type User = {
	name: string;
	login: string;
	id?: string;
	isAdmin: boolean;
};
