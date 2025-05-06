import { Pagination } from "semantic-ui-react";

type Props = {
	currentPage: number;
	totalPages: number;
	pageChanged: (page: number) => void;
};

export default function AppPagination({
	currentPage,
	totalPages,
	pageChanged,
}: Props) {
	return <Pagination totalPages={totalPages} layout="pagination" />;
}
