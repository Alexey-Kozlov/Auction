import React from "react";
import { Breadcrumb } from "flowbite-react";
import { HiHome } from "react-icons/hi";
import { NavLink, useParams } from "react-router-dom";

export default function BreadCrumb() {
	const { root, id } = useParams();

	return (
		<>
			<Breadcrumb aria-label="111">
				<NavLink to={`/${root}`}>
					<Breadcrumb.Item icon={HiHome}>Отчеты</Breadcrumb.Item>
				</NavLink>
				{id && <Breadcrumb.Item>{id}</Breadcrumb.Item>}
			</Breadcrumb>
		</>
	);
}
