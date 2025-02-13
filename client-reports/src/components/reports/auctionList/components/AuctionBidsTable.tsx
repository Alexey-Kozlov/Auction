import React from "react";
import { AuctionListTypes } from "../AuctionListTypes";

type Props = {
	items: AuctionListTypes[] | undefined;
};

export default function AuctionBidsTable({ items }: Props) {
	return (
		<>
			{items!.map((item, index) => (
				<div className="grid grid-cols-4">
					<div key={index}>{item.Title}</div>
					<div key={index}>{item.Seller}</div>
					<div key={index}>{item.Bidder}</div>
					<div key={index}>{item.Amount}</div>
				</div>
			))}
		</>
	);
}
