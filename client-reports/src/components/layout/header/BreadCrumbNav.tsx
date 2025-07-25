import { Link, useNavigate, useParams } from "react-router-dom";
import ReportListData from "../ListData";
import { BreadCrumb } from "primereact/breadcrumb";

export default function BreadCrumbNav() {
  const { root, id } = useParams();
  const home = { icon: "pi pi-home", url: `/${root}` };

  const items = [
    {
      label: id && ReportListData().find((p) => p.Id === id)!.Name,
      template: () => <Link to="/inputtext">InputText</Link>,
    },
  ];

  const navigate = useNavigate();
  return (
    <>
      <BreadCrumb
        model={items}
        home={home}
      />
      {/* <Breadcrumb size="big" id="BreadCrumbStyle">
				<BreadcrumbSection link onClick={() => navigate(`/${root}`)}>
					Отчеты
				</BreadcrumbSection>
				<BreadcrumbDivider icon="right chevron" />
				{id && (
					<BreadcrumbSection>
						{id && ReportListData().find((p) => p.Id === id)!.Name}
					</BreadcrumbSection>
				)}
			</Breadcrumb> */}
    </>
  );
}
