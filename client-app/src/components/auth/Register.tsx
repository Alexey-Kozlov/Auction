import React, { useEffect, useState } from "react";
import { useRegisterUserMutation } from "../../api/AuthApi";
import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { ApiResponse, CreateUser, FormErrors, RequestType } from "../../types";
import { useCookies } from "react-cookie";
import { v4 as uuidv4 } from "uuid";
import {
	Button,
	Form,
	FormInput,
	Grid,
	GridColumn,
	GridRow,
	Message,
} from "semantic-ui-react";
import Heading from "../auctionList/Heading";

export default function Register() {
	const [registerUser] = useRegisterUserMutation();
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
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
	useEffect(() => {
		setCookie("RequestType", RequestType[RequestType.Register]);
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

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
				title="Вход пользователя"
				subtitle="Введите логин и пароль для входа в систему"
			/>
			<Form onSubmit={handleSubmit} error={editError !== null}>
				<Grid columns={3} className="FormLoginTable">
					<GridRow>
						<GridColumn width={5} verticalAlign="middle">
							Имя пользователя<span>*</span>
						</GridColumn>
						<GridColumn width={8}>
							<FormInput
								className="InputLoginText"
								placeholder="Имя пользователя"
								value={loginUserModel.name}
								onChange={(e, data) => handleNameChanged(data.value)}
							/>
						</GridColumn>
					</GridRow>
					<GridRow columns={1}>
						<GridColumn verticalAlign="middle">
							<Message
								size="tiny"
								error
								hidden={editError !== null && editError.name !== "EmptyName"}
								header={editError?.topic}
								content={editError?.detail}
							/>
						</GridColumn>
					</GridRow>
					<GridRow>
						<GridColumn width={5} verticalAlign="middle">
							Логин<span>*</span>
						</GridColumn>
						<GridColumn width={8}>
							<FormInput
								className="InputLoginText"
								placeholder="Логин"
								value={loginUserModel.login}
								onChange={(e, data) => handleLoginChanged(data.value)}
							/>
						</GridColumn>
					</GridRow>
					<GridRow columns={1}>
						<GridColumn verticalAlign="middle">
							<Message
								size="tiny"
								error
								hidden={editError !== null && editError.name !== "EmptyLogin"}
								header={editError?.topic}
								content={editError?.detail}
							/>
						</GridColumn>
					</GridRow>
					<GridRow>
						<GridColumn width={5} verticalAlign="middle">
							Пароль<span>*</span>
						</GridColumn>
						<GridColumn width={8}>
							<FormInput
								className="InputLoginText"
								placeholder="Пароль"
								value={loginUserModel.password}
								onChange={(e, data) => handlePasswordChanged(data.value)}
							/>
						</GridColumn>
					</GridRow>
					<GridRow columns={1}>
						<GridColumn verticalAlign="middle">
							<Message
								size="tiny"
								error
								hidden={
									editError !== null && editError.name !== "EmptyPassword"
								}
								header={editError?.topic}
								content={editError?.detail}
							/>
						</GridColumn>
					</GridRow>
				</Grid>
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
			</Form>
		</div>
	);
}
