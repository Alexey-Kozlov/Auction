import React, { useEffect, useState } from "react";
import { Checkbox, Input } from "semantic-ui-react";

type Props = {
	value?: string;
	name: string;
	inputWidth?: string;
	inputDescr?: string;
	controlsAlign?: string;
	onChange: (imageData: string) => void;
	usingImage: (usingImage: boolean) => void;
};

export default function ImageFileInput({
	inputWidth,
	inputDescr,
	value,
	controlsAlign,
	onChange,
	usingImage,
	...rest
}: Props) {
	const [imageDisplay, setImageDisplay] = useState("");
	const [usingImg, setUsingImg] = useState(false);

	const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		const file = e.target.files && e.target.files[0];
		if (file) {
			const reader = new FileReader();
			reader.readAsDataURL(file);

			reader.onload = (e) => {
				setImageDisplay(e.target?.result as string);
				onChange(e.target?.result as string);
			};
		}
	};

	useEffect(() => {
		if (value) {
			setImageDisplay(`${value}`);
			setUsingImg((prev) => true);
		} else {
			setUsingImg((prev) => false);
		}
		// eslint-disable-next-line
	}, [value]);

	const handleImageUsing = (checked: boolean) => {
		setUsingImg((prev) => {
			usingImage(!prev);
			return !prev;
		});
	};

	return (
		<>
			<div className="flex-column">
				<div className="flex-column">
					<div>
						<Input className="w-100P" type="file" onChange={handleFileChange} />
					</div>
					<div className="flex mt-10">
						<p className="mr-2 mr-10">Требуется изображение</p>
						<Checkbox
							className="ImageUsing"
							toggle
							checked={usingImg}
							onChange={(e, data) => handleImageUsing(data.checked!)}
						/>
					</div>
				</div>

				<div>
					{usingImg && (
						<img alt="" className="ImageEditPreview" src={imageDisplay} />
					)}
				</div>
			</div>
		</>
	);
}
