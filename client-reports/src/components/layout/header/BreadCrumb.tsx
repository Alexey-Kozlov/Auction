import { Breadcrumb } from "flowbite-react";
import { HiHome } from "react-icons/hi";
import { NavLink, useParams } from "react-router-dom";
import ReportListData from "../../ReportListData";

export default function BreadCrumb() {
	const { root, id } = useParams();
	return (
		<>
			<Breadcrumb className="mt-3">
				<NavLink to={`/${root}`}>
					<Breadcrumb.Item icon={HiHome}>
						<span className="text-xl">Отчеты</span>
					</Breadcrumb.Item>
				</NavLink>
				{id && (
					<Breadcrumb.Item>
						<span className="text-xl">
							{id && ReportListData().find((p) => p.Id === id)!.Name}
						</span>
					</Breadcrumb.Item>
				)}
			</Breadcrumb>
		</>
	);
}
