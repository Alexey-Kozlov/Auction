import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";

const NotFound = () => {
	const navigate = useNavigate();
	return (
		<div className="mt-10">
			<div className="flex justify-center mt-5">
				<h2>Страница не найдена</h2>
			</div>
			<div className="flex justify-center mt-10">
				<Button className="MainButton w-100" onClick={() => navigate("/")}>
					Домой
				</Button>
			</div>
		</div>
	);
};

export default NotFound;
