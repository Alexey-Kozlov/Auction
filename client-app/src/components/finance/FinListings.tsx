import { FormEvent, FormEventHandler, useEffect, useState } from "react";
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
import { NavLink, useNavigate } from "react-router-dom";
import { InputNumber } from "primereact/inputnumber";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { DataTable } from "primereact/datatable";
import { Column, ColumnSortEvent, ColumnSortMetaData } from "primereact/column";
import ImageCard from "../auctionList/ImageCard";
import { GrMoney } from "react-icons/gr";
import NumberWithSpaces from "../../utils/NumberWithSpaces";

export default function FinListings() {
	const dispatch = useDispatch();
	const navigate = useNavigate();
	const [amount, setAmount] = useState<number | null>();
	const [editError, setEditError] = useState<FormErrors | null>(null);
	const editErrorList: FormErrors[] = [
		{
			name: "NegativeAmount",
			message: "Нужно указать платеж больше 0",
		},
	];
	const [sortState, setSortState] = useState<ColumnSortMetaData>({
		field: FinanceSortColumn[FinanceSortColumn.actionDate],
		order: 1,
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

	const handleSetSort = (e: ColumnSortEvent) => {
		//dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));

		//направление сортировки
		setSortState((prev) => {
			return {
				field: e.field,
				order: e.order,
			};
		});
		return 1;
		// //посылаем запрос на возврат отсортированных данных
		// setSortParam((prev) => {
		// 	return {
		// 		...prev,
		// 		orderBy:
		// 			FinanceSortColumn[value.column] +
		// 			(sortState.direction === SortDirection.ascending ? "Asc" : "Desc"),
		// 		sessionId: sessionId,
		// 	};
		// });
	};

	const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		dispatch(reset(null));
		dispatch(setEventFlag({ eventName: "FinanceCreate", ready: false }));
		dispatch(setEventFlag({ eventName: "WaiterHide", ready: false }));
		await addCredit({
			amount: amount as number,
			sessionid: sessionId,
			userlogin: user.login,
		});
		setAmount(0);
	};

	// const getSortedValue = (val: FinanceSortColumn) => {
	// 	return sortState.column === val
	// 		? sortState.direction === SortDirection.ascending
	// 			? "ascending"
	// 			: "descending"
	// 		: undefined;
	// };

	const handleAmountChanged = (amount: number | null) => {
		if ((amount !== null && amount <= 0) || !Number.isInteger(amount)) {
			setEditError(
				() => editErrorList.find((p) => p.name === "NegativeAmount")!
			);
			return;
		}
		//сбрасываем ошибки валидации ставки
		setEditError(() => null);
		setAmount(amount!);
	};

	const ImageRowTemplate = (item: FinanceTableItem) => {
		return item.auctionId ? (
			<NavLink to={`/auctions/${item.auctionId}`}>
				<ImageCard
					id={item.auctionId}
					dopStyle="FinanceListImage"
					detail={false}
					cache={true}
				/>
			</NavLink>
		) : (
			<GrMoney size={30} />
		);
	};

	const TitleRowTemplate = (item: FinanceTableItem) => {
		return item.auctionTitle ? (
			<NavLink to={`/auctions/${item.auctionId}`} className="FinanceListText">
				{item.auctionTitle}
			</NavLink>
		) : (
			""
		);
	};

	const DateRowTemplate = (item: FinanceTableItem) => {
		return (
			<div className="FinanceListText">
				{`${new Date(item.actionDate).toLocaleDateString("ru-RU")} 
				${new Date(item?.actionDate).toLocaleTimeString("ru-RU")}`}
			</div>
		);
	};

	const SellerRowTemplate = (item: FinanceTableItem) => {
		return <div className="FinanceListText">{item.auctionSeller}</div>;
	};

	const TypeRowTemplate = (item: FinanceTableItem) => {
		return (
			<div className="FinanceListText">
				{item.status === 0 ? "Приход" : "Расход"}
			</div>
		);
	};

	const AmountRowTemplate = (item: FinanceTableItem) => {
		return (
			<div className="FinanceListText">
				{item.value === 0 ? "0" : NumberWithSpaces(item.value)}
			</div>
		);
	};

	return (
		<>
			<div className="ListingContainer">
				<form onSubmit={handleSubmit}>
					<div className="FinanceBalanceContainer">
						<div className="w-10rem"></div>
						<div>
							<div className="CenterItem">
								<div className="FinanceAddLabel">
									Сумма для зачисления<span>*</span>
								</div>
								<div className="CenterItem">
									<InputNumber
										name="amount"
										step={5}
										variant="filled"
										className="AmountInput"
										placeholder={`Укажите сумму`}
										tooltip="Стрелки вверх/вниз - шаг 5 руб."
										tooltipOptions={{ position: "bottom" }}
										onChange={(e) => handleAmountChanged(e.value)}
										value={amount}
									/>
								</div>
								<div>
									<Button text raised rounded className="CustomButton w-20rem">
										Добавить сумму
									</Button>
								</div>
							</div>
							<div></div>
						</div>

						<div className="FinanceBalance CenterItem">
							<div className="mr-4">Баланс :</div>
							<div className="FinanceBalanceValue">
								{balance.data?.result ?? 0} р.
							</div>
						</div>
					</div>
				</form>
				{financeItems.length === 0 ? (
					<p className="CenterItem text-4xl">Записи не найдены</p>
				) : (
					<>
						<div className="w-full">
							<DataTable
								value={financeItems}
								tableStyle={{ minWidth: "50rem" }}
								size={"large"}
								className="mt-6"
							>
								<Column body={ImageRowTemplate} header="Изображение"></Column>
								<Column
									sortable
									body={TitleRowTemplate}
									header="Наименование"
									sortField="title"
								></Column>
								<Column
									sortable
									sortField="actionDate"
									body={DateRowTemplate}
									header="Дата"
								></Column>
								<Column
									sortable
									body={SellerRowTemplate}
									header="Продавец"
									sortField="seller"
								></Column>
								<Column
									sortable
									body={TypeRowTemplate}
									header="Приход / Расход"
									sortField="status"
								></Column>
								<Column
									sortable
									body={AmountRowTemplate}
									header="Сумма"
									sortField="value"
									sortFunction={(e) => {
										e.order;
									}}
								></Column>
							</DataTable>
						</div>
					</>
				)}
			</div>
		</>
	);
}
