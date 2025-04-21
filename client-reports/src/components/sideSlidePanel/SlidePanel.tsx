import React, { useEffect, useState } from "react";
import { ParameterItem, ParameterSelect, ParameterType } from "../../types";
import { Button } from "flowbite-react";
import { useDispatch, useSelector } from "react-redux";
import { setParamIsOpen, setReportLoading } from "../../store/ReportSlice";
import { RootState } from "../../store/Store";

type Props = {
	params: ParameterItem[];
	reportName: string;
};

export default function SlidePanel({ params, reportName }: Props) {
	const [paramValue, setParamValue] = useState<string[]>([]);
	const dispatch = useDispatch();
	const reportStore = useSelector((state: RootState) => state.reportStore);

	useEffect(() => {
		let paramVal: string[] = [];
		params.forEach((item, index) => {
			paramVal[index] = item.Value;
		});
		setParamValue(paramVal);
		document.getElementById("rt")?.focus();
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
					setParamValue((st) => {
						st[index] = p.target.value;
						return [...st];
					});
				}}
			/>
		);
	};

	const SetParams = (paramList: string[]): ParameterItem[] => {
		let tmp = structuredClone(params);
		paramList.forEach((item, index) => {
			tmp[index].Value = paramList[index];
		});
		return tmp;
	};

	const reportSubmit = async () => {
		const prm = SetParams(paramValue);
		dispatch(setReportLoading({ param: prm }));
	};
	const hitEnter = (e: string) => {
		if (e === "Enter") reportSubmit();
	};

	return (
		<div onKeyDown={(e) => hitEnter(e.key)}>
			<input
				type="checkbox"
				id="nav-toggle"
				hidden
				checked={reportStore.paramIsOpen}
				readOnly
			></input>
			<nav className="nav">
				{reportStore.paramIsOpen ? (
					<label
						className="nav-toggle"
						onClick={() => dispatch(setParamIsOpen({ isOpen: false }))}
					>
						&#x2715;
					</label>
				) : (
					<label
						className="nav-toggle"
						onClick={(e) => {
							e.stopPropagation();
							dispatch(setParamIsOpen({ isOpen: true }));
						}}
					>
						Параметры&nbsp;&nbsp;отчета
					</label>
				)}

				<h2>Параметры отчета "{reportName}"</h2>
				<div
					className="grid grid-cols-[2fr,1fr]"
					onClick={(e) => e.stopPropagation()}
				>
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
					<Button type="submit" onClick={reportSubmit} color="blue">
						Получить отчет
					</Button>
				</div>
			</nav>
		</div>
	);
}
