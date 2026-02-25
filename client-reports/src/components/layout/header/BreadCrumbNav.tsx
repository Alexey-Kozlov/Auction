import { Link, useParams } from "react-router-dom";
import { BreadCrumb } from "primereact/breadcrumb";
import { useSelector } from "react-redux";
import { RootState } from "../../../store/store";

export default function BreadCrumbNav() {
  const reportStore = useSelector((state: RootState) => state.reportStore);
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
        <Link className="BreadCrumbItem" to={`/reports/${id}`}>
          {id && reportStore.currentReport?.Name}
        </Link>
      ),
    },
  ];

  return (
    <>
      <BreadCrumb model={items} home={home} style={{ border: "none" }} />
    </>
  );
}
