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
import { setEventFlag } from "../../store/processingSlice";
import { useFinanceCreateMutation } from "../../api/ProcessingApi";
import FinRow from "./FinRow";
import Waiter from "../Waiter";
import { useNavigate } from "react-router-dom";
import { InputNumber } from "primereact/inputnumber";
import { Button } from "primereact/button";
import { Message } from "primereact/message";

export default function FinListings() {
	const dispatch = useDispatch();
	const navigate = useNavigate();
	const [isWaiting, setIsWaiting] = useState(false);
	const [amount, setAmount] = useState<number | string | null>("");
	const [editError, setEditError] = useState<FormErrors | null>(null);
	const editErrorList: FormErrors[] = [
		{
			name: "NegativeAmount",
			message: "Нужно указать платеж больше 0",
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
		if (!balance.isLoading && !balance.isFetching && (!user || !user.login)) {
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
					<form onSubmit={handleSubmit}>
						<div className="FinanceBalanceContainer">
							<div className="w-200"></div>
							<div>
								<div className="FinanceAmountContainer">
									<div className="FinanceAddLabel">
										Сумма для зачисления<span>*</span>
									</div>
									<div>
										<InputNumber
											type="number"
											className="FinanceAmount"
											placeholder="Сумма"
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
										hidden={
											editError !== null && editError.name !== "NegativeAmount"
										}
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
					</form>
					{/* {financeItems.length === 0 ? (
						<p className="mx-center">Записи не найдены</p>
					) : (
						<>
							<div className="ListPagination">
								<AppPagination
									pageChanged={setPageNumber}
									currentPage={sortParam.pageNumber!}
									totalPages={finance?.pageCount!}
								/>
							</div>
						</>
					)} */}
				</div>
			) : (
				<Waiter />
			)}
		</>
	);
}
