type Props = {
	title: string;
	subtitle?: string;
	center?: boolean;
};

export default function Heading({ center, subtitle, title }: Props) {
	return (
		<div className="BidItem">
			<div className="DetailHeadingTitle">{title}</div>
			<div className="DetailHeadingSubTitle">{subtitle}</div>
		</div>
	);
}
