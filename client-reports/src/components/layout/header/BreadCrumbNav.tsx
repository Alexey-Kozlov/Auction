import { Link, useParams } from "react-router-dom";
import ReportListData from "../ListData";
import { BreadCrumb } from "primereact/breadcrumb";

export default function BreadCrumbNav() {
  const { root, id } = useParams();
  const home = {
    icon: (
      <i
        className="pi pi-home"
        style={{ fontSize: "2rem", color: "rgba(0, 0, 0, 0.87)" }}
      />
    ),
    url: `/${root}`,
  };

  const items = [
    {
      template: () => (
        <Link
          className="BreadCrumbItem"
          to={`/reports/${id}`}
        >
          {id && ReportListData().find((p) => p.Id === id)!.Name}
        </Link>
      ),
    },
  ];

  return (
    <>
      <BreadCrumb
        model={items}
        home={home}
        style={{ border: "none" }}
      />
    </>
  );
}
