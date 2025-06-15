import { FormEvent, useEffect, useState } from "react";
import {
	useLoginUserMutation,
	useSetNewPasswordMutation,
} from "../../api/AuthApi";
import { useNavigate } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { ApiResponse, FormErrors, LoginResponse, LoginUser } from "../../types";
import { setAuthUser } from "../../store/authSlice";
import { Button } from "primereact/button";
import { Panel } from "primereact/panel";
import { InputText } from "primereact/inputtext";
import { Divider } from "primereact/divider";
import { FloatLabel } from "primereact/floatlabel";
import { Message } from "primereact/message";
import { Password } from "primereact/password";
import { confirmDialog, ConfirmDialog } from "primereact/confirmdialog";
import { RootState } from "../../store/store";
import { Toast } from "primereact/toast";

export default function Login() {
	const [loginUser] = useLoginUserMutation();
	const [setPassword] = useSetNewPasswordMutation();
	const toastMessage: Toast | null = useSelector(
		(state: RootState) => state.serviceStore
	).toast;
	const dispatch = useDispatch();
	const navigate = useNavigate();
	const [updatePassword, setUpdatePassword] = useState<boolean | null>(null);
	const [loginUserModel, setLoginUserModel] = useState<LoginUser>({
		login: "",
		password: "",
	});
	const [editError, setEditError] = useState<FormErrors | null>(null);
	const editErrorList: FormErrors[] = [
		{
			name: "EmptyLogin",
			message: "Нужно указать логин пользователя",
		},
		{
			name: "EmptyPassword",
			message: `Нужно указать пароль пользователя`,
		},
		{
			name: "ErrorLogin",
			message: `Ошибка пользователя или пароля`,
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
						toastMessage!.show({
							severity: "success",
							summary: "Успешное действие",
							detail: `Пароль успешно изменен. Можно войти в систему под новым паролем`,
							life: 4000,
						});
					}
					setSubmittingPassword(false);
				})
				.catch((e) => {
					toastMessage!.show({
						severity: "error",
						summary: "Ошибка действия",
						detail: `Ошибка установки пароля - ${e.message}`,
						life: 4000,
					});
				});
		}
		setUpdatePassword(null);
		// eslint-disable-next-line
	}, [updatePassword]);

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

		confirmDialog({
			group: "templating",
			header: (
				<div className="text-center font-bold">Подтверждение действия</div>
			),
			message: (
				<div className="flex flex-column align-items-center text-lg">
					<div>
						{`Подтверждение обновления пароля для пользователя 
				"${loginUserModel.login}".`}
					</div>
					<div>Обновить пароль?</div>
				</div>
			),
			icon: "pi pi-exclamation-circle",
			rejectLabel: "Нет",
			acceptLabel: "Да",
			acceptClassName: "p-button-danger",
			defaultFocus: "reject",
			accept,
			reject,
		});
		return;
	};

	const accept = () => setUpdatePassword(true);

	const reject = () => setUpdatePassword(false);

	const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
		e.preventDefault();
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
				itemId: response.data.result.itemId,
				login: response.data.result.login,
				name: response.data.result.name,
			};
			localStorage.setItem("Auction", JSON.stringify(userData));
			dispatch(
				setAuthUser({
					name: response.data.result.name,
					login: response.data.result.login,
					itemId: response.data.result.itemId,
				})
			);
			toastMessage!.show({
				severity: "success",
				summary: "Успешный вход",
				detail: `Успешный вход в систему пользователя ${response.data.result.name}!`,
				life: 4000,
			});
			//возврат на предыдущую страничку
			navigate(-1);
		} else {
			//ошибка входа
			setEditError(() => editErrorList.find((p) => p.name === "ErrorLogin")!);
			toastMessage!.show({
				severity: "error",
				summary: "Ошибка входа",
				detail: "Ошибка логина или пароля",
				life: 4000,
			});
			setSubmittingLogin(false);
		}
	};

	return (
		<div className="w-full mt-8">
			<form onSubmit={(e) => handleSubmit(e)}>
				<Panel className="LoginPanel">
					<div className="LoginPanelTitle">Вход пользователя</div>
					<div className="LoginPanelDescription">
						Введите логин и пароль для входа в систему
					</div>
					<div className="mt-4">
						<div className="field grid">
							<label className="col-fixed w-6rem">
								Логин<span>*</span>
							</label>
							<div className="col">
								<FloatLabel>
									<InputText
										id="InputLogin"
										type="text"
										className="w-full InputControl"
										value={loginUserModel.login}
										invalid={
											editError !== null &&
											(editError.name === "ErrorLogin" ||
												editError.name === "EmptyLogin")
										}
										onChange={(e) => handleLoginChanged(e.target.value)}
									/>
									<label htmlFor="InputLogin">Логин</label>
									<Message
										className="mt-2"
										severity="error"
										text={editError?.message}
										pt={{
											root: {
												className:
													editError !== null && editError.name === "EmptyLogin"
														? ""
														: "hidden",
											},
										}}
									/>
								</FloatLabel>
							</div>
						</div>
						<div className="field grid">
							<label className="col-fixed w-6rem">
								Пароль<span>*</span>
							</label>
							<div className="col">
								<FloatLabel>
									<Password
										id="InputPassword"
										toggleMask
										feedback={false}
										className="w-full InputControl"
										value={loginUserModel.password}
										onChange={(e) => handlePasswordChanged(e.target.value)}
										invalid={
											editError !== null &&
											(editError.name === "ErrorLogin" ||
												editError.name === "EmptyPassword")
										}
									/>
									<label htmlFor="InputPassword">Пароль</label>
									<Message
										className="mt-3"
										severity="error"
										text={editError?.message}
										pt={{
											root: {
												className:
													editError !== null &&
													editError.name === "EmptyPassword"
														? ""
														: "hidden",
											},
										}}
									/>
								</FloatLabel>
							</div>
						</div>
					</div>

					<div className="LoginButtons">
						<Button
							text
							raised
							rounded
							loading={submittingLogin}
							disabled={submittingLogin}
							type="submit"
							className="CustomButton w-9rem"
						>
							Вход
						</Button>
						<Divider />
						<Button
							text
							raised
							rounded
							severity="danger"
							type="button"
							loading={submittingPassword}
							disabled={submittingPassword}
							onClick={handleSetNewPassword}
							className="CustomButton"
						>
							Я забыл пароль. Установить новый.
						</Button>
					</div>
				</Panel>
			</form>
			<ConfirmDialog group="templating" />
		</div>
	);
}
