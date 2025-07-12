import { FormEvent, useState } from "react";
import { useRegisterUserMutation } from "../../api/AuthApi";
import { useNavigate } from "react-router-dom";
import { ApiResponse, CreateUser, FormErrors } from "../../types";
import { Button } from "primereact/button";
import { Panel } from "primereact/panel";
import { FloatLabel } from "primereact/floatlabel";
import { InputText } from "primereact/inputtext";
import { Message } from "primereact/message";
import { Password } from "primereact/password";
import { Toast } from "primereact/toast";
import { useSelector } from "react-redux";
import { RootState } from "../../store/store";

export default function Register() {
  const [registerUser] = useRegisterUserMutation();
  const navigate = useNavigate();
  const toastMessage: Toast | null = useSelector(
    (state: RootState) => state.serviceStore
  ).toast;
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
      message: "Нужно указать наименование пользователя",
    },
    {
      name: "EmptyLogin",
      message: "Нужно указать логин пользователя",
    },
    {
      name: "EmptyPassword",
      message: `Нужно указать пароль пользователя`,
    },
    {
      name: "ErrorCreate",
      message: `Ошибка создания нового пользователя`,
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

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
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
      toastMessage!.show({
        severity: "success",
        summary: "Успешное действие",
        detail: `Пользователь ${loginUserModel.name} успешно зарегистрирован! 
					Войдите в систему для продолжения.`,
        life: 4000,
      });
      navigate("/");
    } else {
      //ошибка создания
      setEditError(() => editErrorList.find((p) => p.name === "ErrorCreate")!);
      toastMessage!.show({
        severity: "error",
        summary: "Ошибка действия",
        detail: `Ошибка создания нового пользователя - ${response.data?.errorMessages[0]}`,
        life: 4000,
      });
      setSubmittingCreate(false);
    }
  };

  return (
    <div className="w-full mt-8">
      <form onSubmit={(e) => handleSubmit(e)}>
        <Panel className="LoginPanel">
          <div className="LoginPanelTitle">Регистрация пользователя</div>
          <div className="LoginPanelDescription">
            Введите наименование пользователя, его логин и пароль для
            регистрации в системе
          </div>
          <div className="mt-4">
            <div className="field grid">
              <label className="col-fixed w-20rem text-4xl">
                Имя пользователя<span>*</span>
              </label>
              <div className="col">
                <FloatLabel>
                  <InputText
                    id="InputName"
                    type="text"
                    className="w-full InputControl"
                    value={loginUserModel.name}
                    onChange={(e) => handleNameChanged(e.target.value)}
                    invalid={
                      editError !== null && editError.name === "EmptyName"
                    }
                  />
                  <label htmlFor="InputName">Имя пользователя</label>
                  <Message
                    className="ErrorMessage mt-2"
                    severity="error"
                    text={editError?.message}
                    pt={{
                      root: {
                        className:
                          editError !== null && editError.name === "EmptyName"
                            ? ""
                            : "hidden",
                      },
                    }}
                  />
                </FloatLabel>
              </div>
            </div>
            <div className="field grid">
              <label className="col-fixed w-20rem text-4xl">
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
                      (editError.name === "EmptyLogin" ||
                        editError.name === "ErrorCreate")
                    }
                    onChange={(e) => handleLoginChanged(e.target.value)}
                  />
                  <label htmlFor="InputLogin">Логин</label>
                  <Message
                    className="ErrorMessage mt-2"
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
              <label className="col-fixed w-20rem text-4xl">
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
                      editError !== null && editError.name === "EmptyPassword"
                    }
                  />
                  <label htmlFor="InputPassword">Пароль</label>
                  <Message
                    className="ErrorMessage mt-3"
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
              loading={submittingCreate}
              disabled={submittingCreate}
              type="submit"
              className="CustomButton w-16rem justify-content-center"
            >
              Регистрация
            </Button>
          </div>
        </Panel>
      </form>
    </div>
  );
}
