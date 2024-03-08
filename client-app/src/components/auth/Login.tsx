import React from 'react'
import { Formik, Form, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import TextInput from '../inputComponents/TextInput';
import { Button } from 'flowbite-react';
import { useLoginUserMutation } from '../../api/AuthApi';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useDispatch } from 'react-redux';
import { ApiResponse, LoginResponse, User } from '../../store/types';
import { setAuthUser } from '../../store/authSlice';

export default function Login() {
    const [loginUser] = useLoginUserMutation();
    const dispatch = useDispatch();
    const navigate = useNavigate();
    return (
        <div className='container'>
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
                        } else if (response.error) {
                            setErrors({ error: (response.error.data.errorMessages[0]) })
                            toast.error(response.error.data.errorMessages[0]);
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
                            <h2 className='text-xl mt-5'>Вход пользователя</h2>
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

                        </div>

                    </Form>
                )}
            </Formik>
        </div>

    )
}
