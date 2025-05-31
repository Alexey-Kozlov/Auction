import { Loader } from "semantic-ui-react";
type Props = {
	color: string;
};

export default function Waiter({ color }: Props) {
	return (
		<div className="flex items-center justify-center">
			<Loader active inline="centered" />
		</div>
	);
}
