import { useEffect, useState } from "react";
import { AuctionListTypes } from "../AuctionListTypes";
import { DataTable, DataTableFilterMeta } from "primereact/datatable";
import { Column } from "primereact/column";
import { FilterMatchMode } from "primereact/api";
import { NavLink } from "react-router-dom";

type Props = {
  items: AuctionListTypes[] | undefined;
};

export default function AuctionBidsTable({ items }: Props) {
  const [repItems, setRepItems] = useState<AuctionListTypes[] | null>();
  // eslint-disable-next-line
  const [filters, setFilters] = useState<DataTableFilterMeta>({
    Seller: { value: null, matchMode: FilterMatchMode.CONTAINS },
    Title: { value: null, matchMode: FilterMatchMode.CONTAINS },
    Bidder: { value: null, matchMode: FilterMatchMode.CONTAINS },
    Amount: { value: null, matchMode: FilterMatchMode.EQUALS },
    StartDate: { value: null, matchMode: FilterMatchMode.CONTAINS },
    EndDate: { value: null, matchMode: FilterMatchMode.CONTAINS },
  });

  useEffect(() => {
    setRepItems(() => items);
    // eslint-disable-next-line
  }, []);

  const StartDateTemplate = (val: AuctionListTypes) => {
    return (
      new Date(val.StartDate).toLocaleDateString() +
      " " +
      new Date(val.StartDate).toLocaleTimeString()
    );
  };

  const EndDateTemplate = (val: AuctionListTypes) => {
    return (
      new Date(val.EndDate).toLocaleDateString() +
      " " +
      new Date(val.EndDate).toLocaleTimeString()
    );
  };

  const AmountTemplate = (val: AuctionListTypes) => {
    return val.Amount === 0 ? "" : val.Amount;
  };

  const AuctionIdTemplate = (val: AuctionListTypes) => {
    return (
      <NavLink
        to={process.env.REACT_APP_API_URL! + "/auctions/" + val.ItemId}
        target="_blank"
        className="no-underline text-color"
      >
        <div>{val.ItemId}</div>
      </NavLink>
    );
  };

  return (
    <>
      <DataTable
        value={repItems!}
        size="large"
        stripedRows
        paginator
        rows={25}
        rowsPerPageOptions={[25, 50, 100]}
        emptyMessage="Данных не найдено"
        filters={filters}
        filterDisplay="row"
      >
        <Column
          field="AuctionId"
          header="ID аукциона"
          body={AuctionIdTemplate}
        ></Column>
        <Column
          field="Seller"
          header="Продавец"
          sortable
          filter
          filterPlaceholder="Поиск по автору"
        ></Column>
        <Column
          field="Title"
          header="Наименование"
          sortable
          filter
          filterPlaceholder="Поиск по наименованию"
        ></Column>
        <Column
          field="StartDate"
          header="Дата начала"
          body={StartDateTemplate}
          sortable
          filter
          filterPlaceholder="Поиск по дате"
        ></Column>
        <Column
          field="EndDate"
          header="Дата завершения"
          body={EndDateTemplate}
          sortable
          filter
          filterPlaceholder="Поиск по дате"
        ></Column>
        <Column
          field="Bidder"
          header="Автор ставки"
          sortable
          filter
          filterPlaceholder="Поиск по автору ставки"
        ></Column>
        <Column
          field="Amount"
          header="Размер ставки"
          body={AmountTemplate}
          sortable
          filter
          filterPlaceholder="Поиск по размеру ставки"
        ></Column>
      </DataTable>
    </>
  );
}
