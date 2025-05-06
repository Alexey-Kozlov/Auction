import React, { useEffect } from "react";
import { Formik, Form, ErrorMessage } from "formik";
import * as Yup from "yup";
import TextInput from "../inputComponents/TextInput";
import { useRegisterUserMutation } from "../../api/AuthApi";
import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { ApiResponse, RequestType } from "../../store/types";
import { useCookies } from "react-cookie";
import { v4 as uuidv4 } from "uuid";
import { Button } from "semantic-ui-react";

export default function Register() {
	const [registerUser] = useRegisterUserMutation();
	// eslint-disable-next-line
	const [cookies, setCookie] = useCookies(["User", "RequestType", "RequestId"]);
	const navigate = useNavigate();
	useEffect(() => {
		setCookie("RequestType", RequestType[RequestType.Register]);
		setCookie("RequestId", uuidv4());
		// eslint-disable-next-line
	}, []);

	return (
		<div className="container">
			<Formik
				initialValues={{
					displayName: "",
					login: "",
					password: "",
					error: null,
				}}
				onSubmit={async (values, { setErrors }) => {
					const response: ApiResponse<object> = await registerUser({
						login: values.login,
						name: values.displayName,
						password: values.password,
					});
					if (response.data && response.data.isSuccess) {
						toast.success(
							`Пользователь ${values.displayName} успешно зарегистрирован! Войдите в систему для продолжения.`
						);
						navigate("/");
					}
				}}
				validationSchema={Yup.object({
					displayName: Yup.string().required(
						"Необходимо указать имя пользователя"
					),
					login: Yup.string().required("Необходимо указать логин пользователя"),
					password: Yup.string().required(
						"Необходимо указать пароль пользователя"
					),
				})}
			>
				{({ handleSubmit, isSubmitting, errors, isValid, dirty }) => (
					<Form onSubmit={handleSubmit} autoComplete="off">
						<div className="text-center">
							<h1 className="text-xl mt-5">Регистрация пользователя</h1>
							<div className="mt-5">
								<TextInput
									name="displayName"
									placeholder="Имя пользователя"
									label="Имя пользователя"
									labellWidth="w-40"
									inputWidth="w-52"
									onChange={() => {}}
									controlsAlign="justify-center"
									required
								/>
							</div>
							<div className="mt-5">
								<TextInput
									name="login"
									label="Логин"
									placeholder="Логин"
									labellWidth="w-40"
									inputWidth="w-52"
									onChange={() => {}}
									controlsAlign="justify-center"
									required
								/>
							</div>
							<div className="mt-5">
								<TextInput
									name="password"
									label="Пароль"
									placeholder="Пароль"
									type="password"
									labellWidth="w-40"
									inputWidth="w-52"
									onChange={() => {}}
									controlsAlign="justify-center"
									required
								/>
							</div>
							<ErrorMessage name="error" render={() => <p>{errors.error}</p>} />
							<div className="flex justify-around mt-5">
								<Button
									disabled={!isValid || !dirty || isSubmitting}
									isProcessing={isSubmitting}
									type="submit"
								>
									Регистрация
								</Button>
							</div>
						</div>
					</Form>
				)}
			</Formik>
		</div>
	);
}
