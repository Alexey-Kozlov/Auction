import { useEffect, useState } from "react";
import { NotificationListTypes } from "./NotificationListTypes";
import { DataTable, DataTableFilterMeta } from "primereact/datatable";
import { Column } from "primereact/column";
import { FilterMatchMode } from "primereact/api";

type Props = {
  items: NotificationListTypes[];
};

export default function NotificationTable({ items }: Props) {
  const [repItems, setRepItems] = useState<NotificationListTypes[] | null>();
  // eslint-disable-next-line
  const [filters, setFilters] = useState<DataTableFilterMeta>({
    Title: { value: null, matchMode: FilterMatchMode.CONTAINS },
    UserLogin: { value: null, matchMode: FilterMatchMode.CONTAINS },
    ItemId: { value: null, matchMode: FilterMatchMode.CONTAINS },
  });

  useEffect(() => {
    setRepItems(() => items);
    // eslint-disable-next-line
  }, []);
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
          field="UserLogin"
          header="Пользователь"
          sortable
          filter
          filterPlaceholder="Поиск по автору"
        ></Column>
        <Column
          field="ItemId"
          header="ID аукциона"
          sortable
          filter
          filterPlaceholder="Поиск по ID"
        ></Column>
        <Column
          field="Title"
          header="Наименование аукциона"
          sortable
          filter
          filterPlaceholder="Поиск по наименованию"
        ></Column>
      </DataTable>
    </>
  );
}
