import { FormEvent, useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { reset } from "../../store/paramSlice";
import { RootState } from "../../store/store";
import {
  FinanceTableItem,
  FormErrors,
  PagedResult,
  ProcessingState,
  State,
  User,
} from "../../types";
import {
  useGetBalanceQuery,
  useGetFinanceItemQuery,
} from "../../api/FinanceApi";
import { setEventFlag } from "../../store/processingSlice";
import { useFinanceCreateMutation } from "../../api/ProcessingApi";

import { useNavigate } from "react-router-dom";
import { InputNumber } from "primereact/inputnumber";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import FinRow from "./FinRow";
import qs from "query-string";
import { Paginator, PaginatorPageChangeEvent } from "primereact/paginator";
import { SelectButton, SelectButtonChangeEvent } from "primereact/selectbutton";
import { SelectItem } from "primereact/selectitem";
import Waiter from "../Waiter";
import Footer from "../layout/Footer";
import { CheckEventReady } from "../../utils/CheckEvent";
import { useSetUsersCurrentPageMutation } from "../../api/ServiceApi";

export default function FinListings() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const [isWaiting, setIsWaiting] = useState(false);
  const [amount, setAmount] = useState<number | null>();
  const [editError, setEditError] = useState<FormErrors | null>(null);
  const editErrorList: FormErrors[] = [
    {
      name: "NegativeAmount",
      message: "Нужно указать платеж больше 0",
    },
  ];
  const [firstRecord, setFirstRecord] = useState(0);
  const [sortParam, setSortParam] = useState<State>({
    pageSize: 5,
    pageNumber: 1,
    orderBy: "actionDateDesc",
  });

  const [addCredit] = useFinanceCreateMutation();
  const [setCurrentPage] = useSetUsersCurrentPageMutation();
  const sortUrl = qs.stringifyUrl({
    url: "",
    query: { ...sortParam },
  });
  const financeQuery = useGetFinanceItemQuery(sortUrl);
  const balance = useGetBalanceQuery(null);
  const user: User = useSelector((state: RootState) => state.authStore);
  const [financeItems, setFinanceItems] =
    useState<PagedResult<FinanceTableItem>>();
  const procState: ProcessingState[] = useSelector(
    (state: RootState) => state.processingStore
  );

  // первоначальное получение всех записей по финансам данного пользователя
  useEffect(() => {
    if (
      !financeQuery.isFetching &&
      !financeQuery.isLoading &&
      financeQuery.data
    ) {
      setFinanceItems(financeQuery.data.result);
      //посылаем вызов в апи процессинга - для записи в кеш редиса страницы, где находится пользователь
      setCurrentPage("/edit/");
    }
    // eslint-disable-next-line
  }, [financeQuery]);

  //при добавлении нового значения - обновляем таблицу
  useEffect(() => {
    if (!CheckEventReady(procState, "FinanceCreate") && isWaiting) {
      financeQuery.refetch();
      balance.refetch();
      setIsWaiting(() => false);
    }
    // eslint-disable-next-line
  }, [procState]);

  //если вышли из пользователя - переход на начало сайта
  useEffect(() => {
    if (!balance.isLoading && !balance.isFetching && (!user || user.isGuest)) {
      navigate("/");
    }
    // eslint-disable-next-line
  }, [user]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsWaiting(() => true);
    dispatch(reset(null));
    dispatch(setEventFlag({ eventName: "FinanceCreate", ready: true }));
    await addCredit({
      amount: amount as number,
      userlogin: user.login,
    });
    setAmount(0);
  };

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

  function setPageNumber(e: PaginatorPageChangeEvent) {
    setSortParam((prev) => {
      return {
        ...prev,
        pageNumber: e.page + 1,
        pageSize: e.rows,
      };
    });
    setFirstRecord(e.first);
  }
  // #region OrderItems
  // eslint-disable-next-line
  const [orderItem, setOrderItem] = useState<SelectItem[]>([
    {
      label: "Наименование",
      icon: (
        <i
          className="pi pi-sort"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "title",
    },
    {
      label: "Наименование",
      icon: (
        <i
          className="pi pi-sort-alpha-down"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "titleAsc",
    },
    {
      label: "Наименование",
      icon: (
        <i
          className="pi pi-sort-alpha-up"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "titleDesc",
    },
    {
      label: "Автор аукциона",
      icon: (
        <i
          className="pi pi-sort"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "seller",
    },
    {
      label: "Автор аукциона",
      icon: (
        <i
          className="pi pi-sort-alpha-down"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "sellerAsc",
    },
    {
      label: "Автор аукциона",
      icon: (
        <i
          className="pi pi-sort-alpha-up"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "sellerDesc",
    },
    {
      label: "Дата операции",
      icon: (
        <i
          className="pi pi-sort"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "actionDate",
    },
    {
      label: "Дата операции",
      icon: (
        <i
          className="pi pi-sort-amount-down"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "actionDateAsc",
    },
    {
      label: "Дата операции",
      icon: (
        <i
          className="pi pi-sort-amount-up"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "actionDateDesc",
    },
    {
      label: "Тип операции",
      icon: (
        <i
          className="pi pi-sort"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "status",
    },
    {
      label: "Тип операции",
      icon: (
        <i
          className="pi pi-sort-amount-down"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "statusAsc",
    },
    {
      label: "Тип операции",
      icon: (
        <i
          className="pi pi-sort-amount-up"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "statusDesc",
    },
    {
      label: "Значение",
      icon: (
        <i
          className="pi pi-sort"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "value",
    },
    {
      label: "Значение",
      icon: (
        <i
          className="pi pi-sort-amount-down"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "valueAsc",
    },
    {
      label: "Значение",
      icon: (
        <i
          className="pi pi-sort-amount-up"
          style={{ fontSize: "2rem" }}
        />
      ),
      value: "valueDesc",
    },
  ]);
  // #endregion

  const handleMenuClick = (e: SelectButtonChangeEvent) => {
    //если этот столбец был ранее выбран - меняем направление сортировки на противоположное
    let order = "";
    const previousColumn = sortParam.orderBy
      ?.replace("Asc", "")
      .replace("Desc", "");
    if (!e.value) {
      if (sortParam.orderBy!.indexOf("Asc") === -1) {
        order = previousColumn + "Asc";
      } else {
        order = previousColumn + "Desc";
      }
    } else {
      //если это другой столбец сортировки
      const columnValue = e.value.toString();
      const currentColumn = columnValue.replace("Asc", "").replace("Desc", "");
      order = currentColumn + "Asc";
    }

    setSortParam((prev) => {
      return {
        ...prev,
        orderBy: order,
      };
    });
  };

  const filterTemplate = (option: any) => {
    return (
      <>
        {option.icon}
        <label className="ml-2 cursor-pointer text-4xl">{option.label}</label>
      </>
    );
  };

  return (
    <>
      <div>
        <div>
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
                    {CheckEventReady(procState, "FinanceCreate") ? (
                      <Waiter />
                    ) : (
                      <Button
                        text
                        raised
                        rounded
                        className="CustomButton w-20rem"
                      >
                        Добавить сумму
                      </Button>
                    )}
                  </div>
                </div>
                <div className="text-center">
                  <Message
                    className="mt-2"
                    severity="error"
                    text={editError?.message}
                    pt={{
                      root: {
                        className:
                          editError !== null &&
                          editError.name === "NegativeAmount"
                            ? ""
                            : "hidden",
                      },
                    }}
                  />
                </div>
              </div>

              <div className="FinanceBalance CenterItem">
                <div className="mr-4">Баланс :</div>
                <div className="FinanceBalanceValue">
                  {balance.data?.result ?? 0} р.
                </div>
              </div>
            </div>
          </form>
          {financeItems &&
          financeItems.results &&
          financeItems.results.length === 0 ? (
            <p className="CenterItem text-4xl">Записи не найдены</p>
          ) : (
            <div>
              <div>
                <div className="grid FinanceListText FinanceTableHeader">
                  <div className="col-2 CenterItem FinanceTableCell">
                    Изображение
                  </div>
                  <div className="col-4 CenterItem FinanceTableCell">
                    <SelectButton
                      className="BackgroundTransparent"
                      options={orderItem.filter(
                        (p) =>
                          p.value ===
                          (sortParam.orderBy!.indexOf("title") === -1
                            ? "title"
                            : sortParam.orderBy)
                      )}
                      onChange={(e) => handleMenuClick(e)}
                      itemTemplate={filterTemplate}
                      value={sortParam.orderBy}
                    />
                  </div>
                  <div className="col-2 CenterItem FinanceTableCell">
                    <SelectButton
                      className="BackgroundTransparent"
                      options={orderItem.filter(
                        (p) =>
                          p.value ===
                          (sortParam?.orderBy!.indexOf("seller") === -1
                            ? "seller"
                            : sortParam.orderBy)
                      )}
                      onChange={(e) => handleMenuClick(e)}
                      itemTemplate={filterTemplate}
                      value={sortParam.orderBy}
                    />
                  </div>
                  <div className="col-2 CenterItem FinanceTableCell">
                    <SelectButton
                      className="BackgroundTransparent"
                      options={orderItem.filter(
                        (p) =>
                          p.value ===
                          (sortParam?.orderBy!.indexOf("actionDate") === -1
                            ? "actionDate"
                            : sortParam.orderBy)
                      )}
                      onChange={(e) => handleMenuClick(e)}
                      itemTemplate={filterTemplate}
                      value={sortParam.orderBy}
                    />
                  </div>
                  <div className="col-1 CenterItem FinanceTableCell">
                    <SelectButton
                      className="BackgroundTransparent"
                      options={orderItem.filter(
                        (p) =>
                          p.value ===
                          (sortParam?.orderBy!.indexOf("status") === -1
                            ? "status"
                            : sortParam.orderBy)
                      )}
                      onChange={(e) => handleMenuClick(e)}
                      itemTemplate={filterTemplate}
                      value={sortParam.orderBy}
                    />
                  </div>
                  <div className="col-1 CenterItem FinanceTableCell">
                    <SelectButton
                      className="BackgroundTransparent"
                      options={orderItem.filter(
                        (p) =>
                          p.value ===
                          (sortParam?.orderBy!.indexOf("value") === -1
                            ? "value"
                            : sortParam.orderBy)
                      )}
                      onChange={(e) => handleMenuClick(e)}
                      itemTemplate={filterTemplate}
                      value={sortParam.orderBy}
                    />
                  </div>
                </div>
                {financeItems &&
                  financeItems.results &&
                  financeItems.results.map(
                    (item: FinanceTableItem, index: number) => (
                      <FinRow
                        key={index}
                        item={item}
                      />
                    )
                  )}
              </div>
              <div className="ListPagination">
                <Paginator
                  onPageChange={setPageNumber}
                  first={firstRecord}
                  rows={sortParam.pageSize}
                  totalRecords={financeItems?.totalCount}
                  rowsPerPageOptions={[5, 10, 50]}
                />
              </div>
            </div>
          )}
        </div>

        <Footer />
      </div>
    </>
  );
}
