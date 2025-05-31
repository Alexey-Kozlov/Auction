import {
	BreadcrumbSection,
	BreadcrumbDivider,
	Breadcrumb,
} from "semantic-ui-react";
import { useNavigate, useParams } from "react-router-dom";
import ReportListData from "../ListData";

export default function BreadCrumb() {
	const { root, id } = useParams();
	const navigate = useNavigate();
	return (
		<>
			<Breadcrumb size="big" id="BreadCrumbStyle">
				<BreadcrumbSection link onClick={() => navigate(`/${root}`)}>
					Отчеты
				</BreadcrumbSection>
				<BreadcrumbDivider icon="right chevron" />
				{id && (
					<BreadcrumbSection>
						{id && ReportListData().find((p) => p.Id === id)!.Name}
					</BreadcrumbSection>
				)}
			</Breadcrumb>
		</>
	);
}
