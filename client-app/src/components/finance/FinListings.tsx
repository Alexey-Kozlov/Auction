import { useEffect, useState } from "react";
import qs from "query-string";
import { useDispatch, useSelector } from "react-redux";
import { reset } from "../../store/paramSlice";
import { RootState } from "../../store/store";
import uuid from "react-native-uuid";
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
import { useGetBalanceQuery, useGetSortItemsQuery } from "../../api/FinanceApi";
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
import Waiter from "../Waiter";
import { useNavigate } from "react-router-dom";

export default function FinListings() {
	const dispatch = useDispatch();
	const navigate = useNavigate();
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
		orderBy: "",
	});

	const [addCredit] = useFinanceCreateMutation();
	//const params = useSelector((state: RootState) => state.paramStore);
	const finance: FinanceStore = useSelector(
		(state: RootState) => state.financeStore
	);
	const financeItems: FinanceTableItem[] = finance.results;
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

	// инициируем получение всех записей по финансам данного пользователя
	useEffect(() => {
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
		setSortParam((prev) => {
			return {
				...prev,
				orderBy: "actionDateDesc",
				sessionId: sessionId,
			};
		});
		// eslint-disable-next-line
	}, [sessionId]);

	useEffect(() => {
		if (procState.find((p) => p.eventName === "FinanceCreate" && p.ready)) {
			dispatch(setEventFlag({ eventName: "FinanceCreate", ready: false }));
			//инициируем обновление таблицы финансов - обновляем состояние случайным значением
			setSortParam((prev) => {
				return {
					...prev,
					filterBy: uuid.v4().toString(),
				};
			});
			balance.refetch();
			setIsWaiting(false);
		}
		// eslint-disable-next-line
	}, [procState]);

	//если вышли из пользователя - переход на начало сайта
	useEffect(() => {
		if (!user.login) {
			navigate("/");
		}
		// eslint-disable-next-line
	}, [user]);

	function setPageNumber(pageNumber: number) {
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
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
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
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
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
		setIsWaiting(true);
		await addCredit({
			amount: amount as number,
			sessionid: sessionId,
			userlogin: user.login,
		});
		setAmount("");
	};

	const getSortedValue = (val: FinanceSortColumn) => {
		return sortState.column === val
			? sortState.direction === SortDirection.ascending
				? "ascending"
				: "descending"
			: undefined;
	};

	return (
		<>
			{procState.find((p) => p.eventName === "WaiterHide" && p.ready) ? (
				<div className="ListingContainer">
					<Form onSubmit={handleSubmit} error={editError !== null}>
						<div className="FinanceBalanceContainer">
							<div className="w-200"></div>
							<div>
								<div className="FinanceAmountContainer">
									<div className="FinanceAddLabel">
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
											sorted={getSortedValue(FinanceSortColumn.title)}
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
											sorted={getSortedValue(FinanceSortColumn.actionDate)}
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
											sorted={getSortedValue(FinanceSortColumn.seller)}
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
											sorted={getSortedValue(FinanceSortColumn.status)}
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
											sorted={getSortedValue(FinanceSortColumn.value)}
										>
											Финансы
										</TableHeaderCell>
									</TableRow>
								</TableHeader>
								<TableBody>
									{financeItems.length > 0 &&
										financeItems.map(
											(item: FinanceTableItem, index: number) => (
												<FinRow key={index} item={item} />
											)
										)}
								</TableBody>
							</Table>
							<div className="ListPagination">
								<AppPagination
									pageChanged={setPageNumber}
									currentPage={sortParam.pageNumber!}
									totalPages={finance?.pageCount!}
								/>
							</div>
						</>
					)}
				</div>
			) : (
				<Waiter color="rgb(156 163 175)" />
			)}
		</>
	);
}
