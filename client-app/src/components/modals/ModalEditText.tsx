import { Button } from "primereact/button";
import { confirmDialog, ConfirmDialog } from "primereact/confirmdialog";
import { InputTextarea } from "primereact/inputtextarea";

type Props = {
	accept: () => void;
	reject: () => void;
	editValue: (val: string) => void;
	header: string;
	label: string;
	message: string;
	visible: boolean;
	group: string;
};

export default function ModalEditText({
	accept,
	reject,
	message,
	header,
	label,
	visible,
	editValue,
	group,
}: Props) {
	if (visible) {
		confirmDialog({
			defaultFocus: "accept",
			accept,
			reject,
			onHide: () => reject(),
			group: group,
		});
	}

	return (
		<ConfirmDialog
			group={group}
			visible={visible}
			className="w-6 "
			content={({ hide }) => (
				<div className="flex flex-column align-items-center p-5 border-round-lg bg-white w-full">
					<span className="font-bold text-4xl block mb-2 mt-4">{header}</span>
					<p className="mb-1rem text-4xl">{label}</p>
					<InputTextarea
						name="EditMessage"
						variant="filled"
						autoResize
						value={message}
						placeholder="Новое сообщение (для перевода строки нажмите Shift-Enter)"
						onChange={(e) => editValue(e.currentTarget.value)}
						onKeyDown={(e) => {
							if (e.key === "Enter" && e.shiftKey) return;
							if (e.key === "Enter") {
								hide(e);
								accept();
							}
						}}
						rows={3}
						className="w-full text-4xl"
					/>
					<div className="flex align-items-center gap-2 mt-4">
						<Button
							label="Отмена"
							text
							raised
							rounded
							onClick={(event) => {
								hide(event);
								reject();
							}}
							className="w-16rem CustomButton mr-2"
						></Button>
						<Button
							label="Ок"
							text
							raised
							rounded
							onClick={(event) => {
								hide(event);
								accept();
							}}
							className="w-16rem CustomButton"
						></Button>
					</div>
				</div>
			)}
		/>
	);
}
