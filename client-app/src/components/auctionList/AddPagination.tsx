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
	return (
		<Pagination
			totalPages={totalPages}
			defaultActivePage={1}
			activePage={currentPage}
			onPageChange={(e, { activePage }) =>
				pageChanged(parseInt(activePage ? activePage.toString() : "0"))
			}
		/>
	);
}
