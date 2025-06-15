type Props = {
	title: string;
	subtitle?: string;
};

export default function Heading({ subtitle, title }: Props) {
	return (
		<div className="HeadingContainer">
			<div className="DetailHeadingTitle">{title}</div>
			<div className="DetailHeadingSubTitle">{subtitle}</div>
		</div>
	);
}
