import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { RootState } from "../../../store/store";
import { AuctionListTypes } from "./AuctionListTypes";
import { useRunAuctionListMutation } from "../../../api/ReportApi";
import { ApiResponseNet, ParameterItem } from "../../../Types";

type Props = {
	reportId: string;
};

export default function RenderReport({ reportId }: Props) {
	const dataStore = useSelector((state: RootState) => state.reportStore);
	const [data, setData] = useState<AuctionListTypes[]>();
	const [auctionListReport] = useRunAuctionListMutation();

	useEffect(() => {
		const getReport = async (param: ParameterItem[]) => {
			var rezult = await auctionListReport(param);
			setData(rezult.data);
		};
		if (dataStore && dataStore.param && dataStore.param.length > 0) {
			getReport(dataStore.param);
		}
	}, [dataStore.param]);

	return (
		<div>
			{data &&
				data.map((item, index) => {
					if (dataStore.param[1].Value.toLocaleLowerCase() === "true") {
						return (
							<div className="grid grid-cols-4">
								<div key={index}>{item.Title}</div>
								<div key={index}>{item.Seller}</div>
								<div key={index}>{item.Bidder}</div>
								<div key={index}>{item.Amount}</div>
							</div>
						);
					} else {
						return (
							<div className="grid grid-cols-2">
								<div key={index}>{item.Title}</div>
								<div key={index}>{item.Seller}</div>
							</div>
						);
					}
				})}
		</div>
	);
}
