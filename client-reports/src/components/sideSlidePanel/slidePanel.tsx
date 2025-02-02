import React, { useEffect, useState } from "react";
import { ParameterItem, ParameterSelect, ParameterType } from "../../types";
import { Button } from "flowbite-react";

type Props = {
	params: ParameterItem[];
};

export default function SlidePanel({ params }: Props) {
	const [isOpen, setIsOpen] = useState(true);
	const [paramValue, setParamValue] = useState<string[]>([]);

	useEffect(() => {
		let paramObj: string[] = [];
		params.forEach((item, index) => {
			paramObj[index] = item.Value;
		});
		setParamValue(paramObj);
		// eslint-disable-next-line
	}, []);

	const GetParamControl = (item: ParameterItem, index: number) => {
		if (item.Type === ParameterType.Select) {
			return (
				<select
					name={item.Id}
					defaultValue={
						(JSON.parse(item.Value) as ParameterSelect[]).find((p) => p.Default)
							?.Value
					}
					onChange={(p) => {
						if (!p.target.value) return;
						setParamValue((st) => {
							let _item: ParameterSelect[] = JSON.parse(st[index]);
							_item.forEach((st) => {
								if (st.Default) st.Default = false;
								if (st.Value === p.target.value) st.Default = true;
							});
							st[index] = JSON.stringify(_item);
							return [...st];
						});
					}}
				>
					{(JSON.parse(item.Value) as ParameterSelect[]).map((p) => {
						return (
							<option value={p.Value} key={p.Value}>
								{p.Label}
							</option>
						);
					})}
				</select>
			);
		}
		if (item.Type === ParameterType.Bool) {
			return (
				<input
					type="checkbox"
					name={item.Id}
					checked={paramValue![index].toLowerCase() === "true"}
					onChange={(p) => {
						if (!p.target.value) return;
						setParamValue((st) => {
							st[index] = p.target.checked ? "true" : "false";
							return [...st];
						});
					}}
				/>
			);
		}
		return (
			<input
				type={
					item.Type === ParameterType.Text
						? "text"
						: item.Type === ParameterType.Number
						? "number"
						: item.Type === ParameterType.Date
						? "datetime-local"
						: ""
				}
				name={item.Id}
				value={paramValue![index]}
				onChange={(p) => {
					if (!p.target.value) return;
					setParamValue((st) => {
						st[index] = p.target.value;
						return [...st];
					});
				}}
			/>
		);
	};
	const reportSubmit = () => {
		alert(paramValue);
		setIsOpen(false);
	};
	return (
		<div>
			<input
				type="checkbox"
				id="nav-toggle"
				hidden
				checked={isOpen}
				readOnly
			></input>
			<nav className="nav">
				<label
					className="nav-toggle"
					onClick={() => setIsOpen((p) => !p)}
				></label>
				<h2>Параметры</h2>
				<div className="grid grid-cols-2">
					{paramValue &&
						paramValue!.length !== 0 &&
						params.map((p, index) => {
							return (
								<React.Fragment key={index}>
									<div className="reportItem">{p.Label}</div>
									<div className="reportItem">{GetParamControl(p, index)}</div>
								</React.Fragment>
							);
						})}
				</div>
				<div className="reportSubmit">
					<Button onClick={reportSubmit} color="blue">
						Получить отчет
					</Button>
				</div>
			</nav>
		</div>
	);
}
