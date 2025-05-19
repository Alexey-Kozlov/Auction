import { useEffect, useState } from "react";
import qs from "query-string";
import { useDispatch, useSelector } from "react-redux";
import { reset } from "../../store/paramSlice";
import { RootState } from "../../store/store";
import {
	FinanceSortColumn,
	FinanceSortType,
	FinanceStore,
	FinanceTableItem,
	FormErrors,
	ProcessingState,
	SortDirection,
	State,
	User,
} from "../../types";
import {
	useGetBalanceQuery,
	useGetFinanceItemQuery,
	useGetSortItemsQuery,
} from "../../api/FinanceApi";
import { setFinanceItems } from "../../store/financeSlice";
import AppPagination from "../auctionList/AddPagination";
import { setEventFlag } from "../../store/processingSlice";
import { useFinanceCreateMutation } from "../../api/ProcessingApi";
import {
	Button,
	Form,
	FormInput,
	Message,
	Table,
	TableBody,
	TableHeader,
	TableHeaderCell,
	TableRow,
} from "semantic-ui-react";
import FinRow from "./FinRow";
import { useGetAuctionsArrayMutation } from "../../api/AuctionApi";

export default function FinListings() {
	const dispatch = useDispatch();
	const [isWaiting, setIsWaiting] = useState(false);
	const [amount, setAmount] = useState<number | string | null>("");
	const [editError, setEditError] = useState<FormErrors | null>(null);
	const editErrorList: FormErrors[] = [
		{
			name: "NegativeAmount",
			topic: "Ошибка ввода платежа!",
			detail: "Нужно указать платеж больше 0",
		},
	];
	const [sortState, setSortState] = useState<FinanceSortType>({
		column: FinanceSortColumn.actionDate,
		direction: SortDirection.ascending,
	});
	const [sortParam, setSortParam] = useState<State>({
		pageSize: 5,
		pageNumber: 1,
		orderBy: "actionDateDesc",
	});

	const [addCredit] = useFinanceCreateMutation();
	const [getAuctions] = useGetAuctionsArrayMutation();
	const params = useSelector((state: RootState) => state.paramStore);
	const financeItems: FinanceTableItem[] = useSelector(
		(state: RootState) => state.financeStore
	).results;
	const url = qs.stringifyUrl({ url: "", query: { ...params, pageSize: 5 } });
	const financeData = useGetFinanceItemQuery(url, {
		skip: url.endsWith("sessionId="),
	});
	const sortUrl = qs.stringifyUrl({
		url: "",
		query: { ...sortParam },
	});
	useGetSortItemsQuery(sortUrl, { skip: !sortParam.sessionId });
	const balance = useGetBalanceQuery(null);
	const procState: ProcessingState[] = useSelector(
		(state: RootState) => state.processingStore
	);
	const sessionId = useSelector(
		(state: RootState) => state.paramStore
	).sessionId;
	const user: User = useSelector((state: RootState) => state.authStore);

	//получаем все записи по финансам, также получаем список аукционов, которые указаны в платежах финансов
	useEffect(() => {
		if (financeData.data && !financeData.isLoading && !financeData.isFetching) {
			getAuctions({
				//параметр - список auctionid, исключая null
				auctionIds: financeData.data.result.results
					.filter((p) => p.auctionId)
					.map((p) => p.auctionId),
			}).then((rezult) => {
				const auctionArray = rezult.data?.result;
				let _items: FinanceStore = { results: [], pageCount: 0, totalCount: 0 };
				financeData.data?.result.results.forEach((item) => {
					_items.results.push({
						actionDate: item.actionDate,
						auctionId: item.auctionId,
						auctionSeller: auctionArray?.find(
							(p) => p.auctionId === item.auctionId
						)?.seller,
						auctionTitle: auctionArray?.find(
							(p) => p.auctionId === item.auctionId
						)?.title,
						id: item.id,
						itemId: item.itemId,
						show: item.show,
						status: item.status,
						value: item.value,
					} as FinanceTableItem);
				});
				_items.pageCount = financeData.data?.result.pageCount!;
				_items.totalCount = financeData.data?.result.totalCount!;
				dispatch(setFinanceItems(_items));
			});
		}
		// eslint-disable-next-line
	}, [financeData]);

	//сбрасываем пежинацию - чтобы исключить наследование пежинации от списка аукционов
	useEffect(() => {
		dispatch(reset(null));
		return () => {
			dispatch(reset(null));
		};
		// eslint-disable-next-line
	}, []);

	useEffect(() => {
		const eventStateFinanceCreditAdd = procState.find(
			(p) => p.eventName === "FinanceCreate" && p.ready
		);
		if (eventStateFinanceCreditAdd) {
			dispatch(setEventFlag({ eventName: "FinanceCreate", ready: false }));
			financeData.refetch();
			balance.refetch();
			setIsWaiting(false);
		}
		// eslint-disable-next-line
	}, [procState]);

	function setPageNumber(pageNumber: number) {
		setSortParam((prev) => {
			return {
				...prev,
				sessionId: sessionId,
				pageNumber: pageNumber,
			};
		});
	}

	const handleSetAmount = (value: number | "" | null) => {
		if (value !== "" && value !== null && value <= 0) {
			setEditError(
				() => editErrorList.find((p) => p.name === "NegativeAmount")!
			);
			return;
		} else {
			setEditError(() => null);
		}
		setAmount(value);
	};

	const handleSetSort = (value: FinanceSortType) => {
		//направление сортировки
		setSortState((prev) => {
			return {
				...value,
				direction:
					prev.direction === SortDirection.ascending
						? SortDirection.descending
						: SortDirection.ascending,
			};
		});
		//посылаем запрос на возврат отсортированных данных
		setSortParam((prev) => {
			return {
				...prev,
				orderBy:
					FinanceSortColumn[value.column] +
					(sortState.direction === SortDirection.ascending ? "Asc" : "Desc"),
				sessionId: sessionId,
			};
		});
	};

	const handleSubmit = async () => {
		dispatch(reset(null));
		dispatch(setEventFlag({ eventName: "FinanceCreate", ready: false }));
		setIsWaiting(true);
		await addCredit({
			amount: amount as number,
			sessionid: sessionId,
			userlogin: user.login,
		});
		setAmount("");
	};

	if (financeData.isLoading) return <h3>Загрузка...</h3>;

	return (
		<div className="ListingContainer">
			<Form onSubmit={handleSubmit} error={editError !== null}>
				<div className="FinanceBalanceContainer">
					<div className="w-200"></div>
					<div>
						<div className="FinanceAmountContainer">
							<div>
								Сумма для зачисления<span>*</span>
							</div>
							<div>
								<FormInput
									type="number"
									className="FinanceAmount"
									placeholder="Сумма"
									value={amount}
									onChange={(e, data) =>
										handleSetAmount(
											data.value === "" ? "" : parseInt(data.value)
										)
									}
								/>
							</div>
							<div>
								<Button
									className="MainButton w-150"
									type="submit"
									loading={isWaiting}
								>
									Добавить сумму
								</Button>
							</div>
						</div>
						<div>
							<Message
								error
								hidden={
									editError !== null && editError.name !== "NegativeAmount"
								}
								header={editError?.topic}
								content={editError?.detail}
							/>
						</div>
					</div>

					<div className="flex w-200">
						<div className="FinanceBalanceLabel">Баланс :</div>
						<div className="FinanceBalanceValue">
							{balance.data?.result ?? 0} р.
						</div>
					</div>
				</div>
			</Form>
			{financeItems.length === 0 ? (
				<p className="mx-center">Записи не найдены</p>
			) : (
				<>
					<Table sortable celled striped>
						<TableHeader>
							<TableRow>
								<TableHeaderCell textAlign="center">
									Изображение
								</TableHeaderCell>
								<TableHeaderCell
									textAlign="center"
									onClick={() =>
										handleSetSort({
											column: FinanceSortColumn.title,
											direction: sortState.direction,
										})
									}
									sorted={
										sortState.column === FinanceSortColumn.title
											? sortState.direction === SortDirection.ascending
												? "ascending"
												: "descending"
											: undefined
									}
								>
									Наименование
								</TableHeaderCell>
								<TableHeaderCell
									textAlign="center"
									onClick={() =>
										handleSetSort({
											column: FinanceSortColumn.actionDate,
											direction: sortState.direction,
										})
									}
									sorted={
										sortState.column === FinanceSortColumn.actionDate
											? sortState.direction === SortDirection.ascending
												? "ascending"
												: "descending"
											: undefined
									}
								>
									Дата
								</TableHeaderCell>
								<TableHeaderCell
									textAlign="center"
									onClick={() =>
										handleSetSort({
											column: FinanceSortColumn.seller,
											direction: sortState.direction,
										})
									}
									sorted={
										sortState.column === FinanceSortColumn.seller
											? sortState.direction === SortDirection.ascending
												? "ascending"
												: "descending"
											: undefined
									}
								>
									Продавец
								</TableHeaderCell>
								<TableHeaderCell
									textAlign="center"
									onClick={() =>
										handleSetSort({
											column: FinanceSortColumn.status,
											direction: sortState.direction,
										})
									}
									sorted={
										sortState.column === FinanceSortColumn.status
											? sortState.direction === SortDirection.ascending
												? "ascending"
												: "descending"
											: undefined
									}
								>
									Приход / Расход
								</TableHeaderCell>
								<TableHeaderCell
									textAlign="center"
									onClick={() =>
										handleSetSort({
											column: FinanceSortColumn.value,
											direction: sortState.direction,
										})
									}
									sorted={
										sortState.column === FinanceSortColumn.value
											? sortState.direction === SortDirection.ascending
												? "ascending"
												: "descending"
											: undefined
									}
								>
									Финансы
								</TableHeaderCell>
							</TableRow>
						</TableHeader>
						<TableBody>
							{financeItems.length > 0 &&
								financeItems.map((item: FinanceTableItem, index: number) => (
									<FinRow key={index} item={item} />
								))}
						</TableBody>
					</Table>
					<div className="ListPagination">
						<AppPagination
							pageChanged={setPageNumber}
							currentPage={sortParam.pageNumber!}
							totalPages={financeData.data?.result.pageCount!}
						/>
					</div>
				</>
			)}
		</div>
	);
}
