import { useEffect, useState } from "react";
import { CommentTreeItem } from "./CommentsTypes";
import {
  TreeTable,
  TreeTableTogglerTemplateOptions,
} from "primereact/treetable";
import { TreeNode } from "primereact/treenode";
import { Column } from "primereact/column";
import { NavLink } from "react-router-dom";
import { classNames } from "primereact/utils";

type Props = {
  items: CommentTreeItem[] | undefined;
};

export default function Comments({ items }: Props) {
  const [repItems, setRepItems] = useState<TreeNode[]>([]);

  useEffect(() => {
    if (items) {
      //нужно глубокое копирование, можно использовать JSON.parse(JSON.stringify(items)),
      //а можно более эффективное решение - structuredClone(items)
      setRepItems(structuredClone(items));
    }
    // eslint-disable-next-line
  }, [items]);

  const StartDateTemplate = (val: CommentTreeItem) => {
    return val.data.createAt
      ? new Date(val.data.createAt).toLocaleDateString() +
          " " +
          new Date(val.data.createAt).toLocaleTimeString()
      : "";
  };

  const TitleTemplate = (
    node: TreeNode,
    options: TreeTableTogglerTemplateOptions
  ) => {
    if (!node) {
      return;
    }
    const expanded = options.expanded;
    const iconClassName = classNames("p-treetable-toggler-icon pi pi-fw", {
      "pi-caret-right": !expanded,
      "pi-caret-down": expanded,
    });

    return (
      <div className="flex">
        <button
          type="button"
          className="p-treetable-toggler p-link"
          style={options.buttonStyle}
          tabIndex={-1}
          onClick={options.onClick}
        >
          <span
            className={iconClassName}
            aria-hidden="true"
          ></span>
        </button>
        <NavLink
          to={process.env.REACT_APP_API_URL! + "/auctions/" + node.data.itemid}
          target="_blank"
          className="no-underline text-color flex flex-column justify-content-center"
        >
          <span>{node.data.title}</span>
        </NavLink>
      </div>
    );
  };

  const TitleBodyTemplate = (val: CommentTreeItem) => {
    return <></>;
  };

  return (
    <>
      <TreeTable
        stripedRows
        value={repItems}
        tableStyle={{ minWidth: "50rem" }}
        paginator
        rows={25}
        rowsPerPageOptions={[25, 50, 100]}
        emptyMessage="Данных не найдено"
        filterMode="lenient"
        togglerTemplate={TitleTemplate}
      >
        <Column
          field="title"
          header="Наименование"
          expander
          filter
          filterPlaceholder="Поиск по наименованию"
          filterMatchMode="contains"
          sortable
          body={TitleBodyTemplate}
        ></Column>
        <Column
          field="seller"
          header="Автор"
          filter
          filterPlaceholder="Поиск по автору аукциона"
          filterMatchMode="contains"
          sortable
        ></Column>
        <Column
          field="createAt"
          header="Дата создания"
          body={StartDateTemplate}
          sortable
        ></Column>
        <Column
          field="author"
          header="Автор комментария"
          filter
          filterPlaceholder="Поиск по автору комментария"
          filterMatchMode="contains"
        ></Column>
        <Column
          field="comment"
          header="Комментарий"
          filter
          filterPlaceholder="Поиск по комментарию"
          filterMatchMode="contains"
        ></Column>
      </TreeTable>
    </>
  );
}
