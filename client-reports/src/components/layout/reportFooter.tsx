import React from "react";
import { Footer } from "flowbite-react";

export default function ReportFooter() {
	return (
		<div className="flex justify-center">
			<Footer container>
				<Footer.Copyright href="#" by="Аукцион" year={2025} />
			</Footer>
		</div>
	);
}
