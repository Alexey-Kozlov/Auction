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
