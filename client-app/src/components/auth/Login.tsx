import { useEffect, useState } from "react";
import {
	useLoginUserMutation,
	useSetNewPasswordMutation,
} from "../../api/AuthApi";
import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { useDispatch } from "react-redux";
import {
	ApiResponse,
	FormErrors,
	LoginResponse,
	LoginUser,
	RequestType,
} from "../../types";
import { setAuthUser } from "../../store/authSlice";
import ModalConfirm from "../modals/ModalConfirm";
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

export default function Login() {
	const [loginUser] = useLoginUserMutation();
	const [setPassword] = useSetNewPasswordMutation();
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
	const dispatch = useDispatch();
	const navigate = useNavigate();
	const [showConfirm, setShowConfirm] = useState(false);
	const [updatePassword, setUpdatePassword] = useState<boolean | null>(null);
	const [loginUserModel, setLoginUserModel] = useState<LoginUser>({
		login: "",
		password: "",
	});
	const [editError, setEditError] = useState<FormErrors | null>(null);
	const editErrorList: FormErrors[] = [
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
	const [submittingLogin, setSubmittingLogin] = useState(false);
	const [submittingPassword, setSubmittingPassword] = useState(false);

	useEffect(() => {
		if (updatePassword) {
			setSubmittingPassword(true);
			const setPasswordFunc = async () =>
				await setPassword({
					login: loginUserModel.login,
					name: "",
					password: loginUserModel.password,
				});
			setPasswordFunc()
				.then((rez: ApiResponse<object>) => {
					if (rez.data!.isSuccess) {
						toast.success(
							`Пароль успешно изменен. Можно войти в систему под новым паролем`
						);
					}
					setSubmittingPassword(false);
				})
				.catch((e) => {
					toast.error(`Ошибка установки пароля - ${e.message}`);
				});
		}
		setUpdatePassword(null);
		setShowConfirm(false);
		// eslint-disable-next-line
	}, [updatePassword]);

	useEffect(() => {
		setCookie("RequestType", RequestType[RequestType.Login]);
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

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

	const handleSetNewPassword = () => {
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
		setShowConfirm(true);
		return;
	};

	const handleSubmit = async () => {
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
		setSubmittingLogin(true);
		const response: ApiResponse<LoginResponse> = await loginUser({
			login: loginUserModel.login,
			password: loginUserModel.password,
		});
		if (response.data && response.data.isSuccess) {
			const userData: LoginResponse = {
				token: response.data.result.token,
				id: response.data.result.id,
				login: response.data.result.login,
				name: response.data.result.name,
			};
			localStorage.setItem("Auction", JSON.stringify(userData));
			dispatch(
				setAuthUser({
					name: response.data.result.name,
					login: response.data.result.login,
					id: response.data.result.id,
				})
			);
			toast.success(
				`Успешный вход в систему пользователя ${response.data.result.name}`
			);
			//возврат на предыдущую страничку
			navigate(-1);
		} else {
			setSubmittingLogin(false);
		}
	};

	return (
		<div className="mt-50">
			<ModalConfirm
				openModal={showConfirm}
				text={
					"Подтверждение обновления пароля для пользователя '" +
					loginUserModel.login +
					"'. Обновить пароль?"
				}
				title="Обновление пароля"
				setResult={setUpdatePassword}
			/>

			<Heading
				title="Вход пользователя"
				subtitle="Введите логин и пароль для входа в систему"
			/>
			<Form onSubmit={handleSubmit} error={editError !== null}>
				<Grid columns={3} className="FormLoginTable">
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
						loading={submittingLogin}
						disabled={submittingLogin}
						type="submit"
						className="MainButton w-100"
					>
						Вход
					</Button>
				</div>
				<div id="ForgotPasswordButton">
					<Button
						type="button"
						loading={submittingPassword}
						disabled={submittingPassword}
						onClick={handleSetNewPassword}
						className="MainButton w-200"
					>
						Я забыл пароль. Установить новый.
					</Button>
				</div>
			</Form>
		</div>
	);
}
