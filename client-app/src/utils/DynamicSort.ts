import { SortDirection } from "../types";

export const DynamicSort = (column: string, direction: SortDirection) => {
	var sortOrder = 1;
	if (direction == SortDirection.ascending) sortOrder = -1;
	return function (a: any, b: any) {
		var result = a[column] > b[column] ? -1 : a[column] < b[column] ? 1 : 0;
		return result * sortOrder;
	};
};
