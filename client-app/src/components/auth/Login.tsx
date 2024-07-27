import React, { MouseEventHandler, useEffect, useRef, useState } from 'react'
import { Formik, Form, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import TextInput from '../inputComponents/TextInput';
import { Button } from 'flowbite-react';
import { useLoginUserMutation, useSetNewPasswordMutation } from '../../api/AuthApi';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useDispatch } from 'react-redux';
import { ApiResponse, CreateUser, LoginResponse } from '../../store/types';
import { setAuthUser } from '../../store/authSlice';
import ModalConfirm from '../modals/ModalConfirm';

export default function Login() {
    const [loginUser] = useLoginUserMutation();
    const [setPassword] = useSetNewPasswordMutation();
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const loginRef = useRef(null);
    const passwordRef = useRef(null);
    const [showConfirm, setShowConfirm] = useState(false);
    const [updatePassword, setUpdatePassword] = useState<boolean | undefined>(undefined);

    const handleSetNewPassword = async () => {
        if(!(loginRef.current! as HTMLInputElement).value){
            toast.success(`Необходимо указать логин`);
            return;
        }
        if(!(passwordRef.current! as HTMLInputElement).value){
            toast.success(`Необходимо указать пароль`);
            return;
        }
        setShowConfirm(true);
        return;
    }

    useEffect(() => {
        if(updatePassword){
            const setPasswordFunc = async () => await setPassword({
                login: (loginRef.current! as HTMLInputElement).value,
                name:'',
                password: (passwordRef.current! as HTMLInputElement).value
            });
            setPasswordFunc()
            .then((rez:ApiResponse<object>) =>{
                if(rez.data!.isSuccess){
                    toast.success(`Пароль успешно изменен. Можно войти в систему под новым паролем`);
                }
            })
            .catch(e => {
                console.log(`Ошибка установки пароля - ${e.message}`);
                toast.error(`Ошибка установки пароля - ${e.message}`);
            });
           
        }
        setUpdatePassword(undefined);
        setShowConfirm(false);
    }, [updatePassword]);

    return (
        <div className='container'>
            <ModalConfirm 
                openModal={showConfirm} 
                text='Подтверждение обновления пароля. Обновить пароль?'
                title='Обновление пароля'
                setResult={setUpdatePassword}
            />
            <Formik
                initialValues={{ login: '', password: '', error: null }}
                onSubmit={
                    async (values, { setErrors }) => {
                        const response: ApiResponse<LoginResponse> = await loginUser({
                            login: values.login,
                            password: values.password
                        });
                        if (response.data && response.data.isSuccess) {
                            const userData: LoginResponse = {
                                token: response.data.result.token,
                                id: response.data.result.id,
                                login: response.data.result.login,
                                name: response.data.result.name
                            };
                            localStorage.setItem('Auction', JSON.stringify(userData));
                            dispatch(setAuthUser({
                                name: response.data.result.name,
                                login: response.data.result.login,
                                id: response.data.result.id
                            }))
                            toast.success(`Успешный вход в систему пользователя ${response.data.result.name}`);
                            navigate('/');
                        }
                    }
                }
                validationSchema={Yup.object({
                    login: Yup.string().required('Необходимо указать логин пользователя'),
                    password: Yup.string().required('Необходимо указать пароль пользователя')
                })}
            >
                {({ handleSubmit, isSubmitting, errors, isValid, dirty }) => (
                    <Form onSubmit={handleSubmit} autoComplete='off'>
                        <div className='text-center'>
                            <h1 className='text-xl mt-5'>Вход пользователя</h1>
                            <div className='mt-5'>
                                <TextInput
                                    name='login'
                                    label='Логин'
                                    placeholder='Логин'
                                    labellWidth='w-40'
                                    inputWidth='w-52'
                                    onChange={() => { }}
                                    controlsAlign='justify-center'
                                    required
                                    ref={loginRef}
                                />
                            </div>
                            <div className='mt-5'>
                                <TextInput
                                    name='password'
                                    label='Пароль'
                                    placeholder='Пароль'
                                    type='password'
                                    labellWidth='w-40'
                                    inputWidth='w-52'
                                    onChange={() => { }}
                                    controlsAlign='justify-center'
                                    required
                                    ref = {passwordRef}
                                />
                            </div>
                            <ErrorMessage name='error' render={() =>
                                <p className='text-red-500'>{errors.error}</p>
                            } />
                            <div className='flex justify-around mt-5'>
                                <Button
                                    disabled={!isValid || !dirty || isSubmitting}
                                    isProcessing={isSubmitting}
                                    type='submit'>Вход</Button>
                            </div>
                            <hr className='mt-4' />
                            <div className='flex justify-around mt-5'>
                                <Button
                                    onClick={handleSetNewPassword}
                                >Я забыл пароль. Установить новый.</Button>
                            </div>

                        </div>

                    </Form>
                )}
            </Formik>
        </div>

    )
}
