import { useState } from "react";
import { useRegisterUserMutation } from "../../api/AuthApi";
import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { ApiResponse, CreateUser, FormErrors } from "../../types";
import Heading from "../auctionList/Heading";
import { Button } from "primereact/button";

export default function Register() {
	const [registerUser] = useRegisterUserMutation();
	// eslint-disable-next-line
	const navigate = useNavigate();
	const [loginUserModel, setLoginUserModel] = useState<CreateUser>({
		name: "",
		login: "",
		password: "",
	});
	const [editError, setEditError] = useState<FormErrors | null>(null);
	const [submittingCreate, setSubmittingCreate] = useState(false);
	const editErrorList: FormErrors[] = [
		{
			name: "EmptyName",
			topic: "Ошибка - пустое наименование пользователя!",
			detail: "Нужно указать наименование пользователя",
		},
		{
			name: "EmptyLogin",
			topic: "Ошибка - пустое значение логина!",
			detail: "Нужно указать логин пользователя",
		},
		{
			name: "EmptyPassword",
			topic: "Ошибка - пустое значение пароля!",
			detail: `Нужно указать пароль пользователя`,
		},
	];

	const handleNameChanged = (value: string) => {
		setEditError(() => null);
		setLoginUserModel((prev) => {
			return { ...prev, name: value };
		});
	};

	const handleLoginChanged = (value: string) => {
		setEditError(() => null);
		setLoginUserModel((prev) => {
			return { ...prev, login: value };
		});
	};

	const handlePasswordChanged = (value: string) => {
		setEditError(() => null);
		setLoginUserModel((prev) => {
			return { ...prev, password: value };
		});
	};

	const handleSubmit = async () => {
		if (!loginUserModel.name) {
			setEditError(() => editErrorList.find((p) => p.name === "EmptyName")!);
			return;
		}
		if (!loginUserModel.login) {
			setEditError(() => editErrorList.find((p) => p.name === "EmptyLogin")!);
			return;
		}
		if (!loginUserModel.password) {
			setEditError(
				() => editErrorList.find((p) => p.name === "EmptyPassword")!
			);
			return;
		}
		setSubmittingCreate(true);
		const response: ApiResponse<object> = await registerUser({
			login: loginUserModel.login,
			name: loginUserModel.name,
			password: loginUserModel.password,
		});
		if (response.data && response.data.isSuccess) {
			toast.success(
				`Пользователь ${loginUserModel.name} успешно зарегистрирован! Войдите в систему для продолжения.`
			);
			navigate("/");
		}
		setSubmittingCreate(false);
	};

	return (
		<div className="mt-50">
			<Heading
				title="Регистрация пользователя"
				subtitle="Введите наименование пользователя, его логин и пароль для регистрации в системе"
			/>
			<form onSubmit={handleSubmit}>
				<div id="LoginButton">
					<Button
						type="submit"
						loading={submittingCreate}
						disabled={submittingCreate}
						className="MainButton w-120"
					>
						Регистрация
					</Button>
				</div>
			</form>
		</div>
	);
}
