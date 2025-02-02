import React, { useState } from "react";
import SlidePanel from "../../sideSlidePanel/slidePanel";
import { ParameterItem, ParameterSelect, ParameterType } from "../../../types";
import { json } from "stream/consumers";

export default function AuctionList() {
	return (
		<div className="mt-16">
			<div>
				report
				<SlidePanel
					params={[
						{
							Label: "Наименование1",
							Type: ParameterType.Text,
							Value: "Тест1",
							Id: "Text1",
						} as ParameterItem,
						{
							Label: "Наименование2",
							Type: ParameterType.Text,
							Value: "Тест2",
							Id: "Text2",
						} as ParameterItem,
						{
							Label: "Дата начала",
							Type: ParameterType.Date,
							Value: new Date().toISOString().substring(0, 16),
							Id: "DateBegin",
						} as ParameterItem,
						{
							Label: "Дата окончания",
							Type: ParameterType.Date,
							Value: new Date().toISOString().substring(0, 16),
							Id: "DateEnd",
						} as ParameterItem,
						{
							Label: "Выбор",
							Type: ParameterType.Select,
							Value: JSON.stringify([
								{
									Label: "Значение 1",
									Value: "Val1",
									Default: false,
								} as ParameterSelect,
								{
									Label: "Значение 2",
									Value: "Val2",
									Default: false,
								} as ParameterSelect,
								{
									Label: "Значение 3",
									Value: "Val3",
									Default: true,
								} as ParameterSelect,
							]),
							Id: "Select1",
						} as ParameterItem,
						{
							Label: "Да / Нет",
							Type: ParameterType.Bool,
							Value: "false",
							Id: "Checked1",
						} as ParameterItem,
					]}
				/>
			</div>
		</div>
	);
}
