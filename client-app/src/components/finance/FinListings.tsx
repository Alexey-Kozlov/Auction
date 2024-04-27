import React, { useEffect, useState } from 'react'
import qs from 'query-string';
import { useDispatch, useSelector } from 'react-redux';
import { reset, setParams } from '../../store/paramSlice';
import { RootState } from '../../store/store';
import { FinanceItem, ProcessingState } from '../../store/types';
import { useAddCreditMutation, useGetFinanceItemQuery } from '../../api/FinanceApi';
import { setFinanceItems } from '../../store/financeSlice';
import AppPagination from '../auctionList/AddPagination';
import { Button } from 'flowbite-react';
import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../inputComponents/TextInput';
import FinTable from './FinTable';
import { setEventFlag } from '../../store/processingSlice';

export default function FinListings() {
    const dispatch = useDispatch();
    const [isWaiting, setIsWaiting] = useState(false);
    const [addCredit] = useAddCreditMutation();
    const params = useSelector((state: RootState) => state.paramStore);
    const financeItems: FinanceItem[] = useSelector((state: RootState) => state.financeStore).items;
    const url = qs.stringifyUrl({ url: '', query: { ...params, pageSize: 5 } });
    const financeData = useGetFinanceItemQuery(url);
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);

    useEffect(() => {
        if (!financeData.isLoading) {
            dispatch(setFinanceItems(financeData.data?.result));
        }
    }, [financeData.isLoading, financeData.data?.result, dispatch]);

    useEffect(() => {
        return () => {
            dispatch(reset(null));
        }
    }, [dispatch])

    useEffect(() => {
        const eventStateFinanceCreditAdd = procState.find(p => p.eventName === 'FinanceCreditAdd');
        if (eventStateFinanceCreditAdd && eventStateFinanceCreditAdd.ready) {
            dispatch(setEventFlag({ eventName: 'FinanceCreditAdd', ready: false }));
            financeData.refetch();
            setIsWaiting(false);
        }
    }, [procState, dispatch]);

    function setPageNumber(pageNumber: number) {
        dispatch(setParams({ pageNumber: pageNumber }));
    }
    const initialValue = { amount: 0 as number };
    if (financeData.isLoading) return <h3>Загрузка...</h3>

    return (
        <div>
            <Formik
                initialValues={initialValue}
                enableReinitialize
                onSubmit={async (values, { setErrors }) => {
                    dispatch(reset(null));
                    dispatch(setEventFlag({ eventName: 'FinanceCreditAdd', ready: false }));
                    setIsWaiting(true);
                    await addCredit(values.amount);

                    // if (response.data!.isSuccess) {
                    //     let attemptCounter = 10;
                    //     const refInterval = setInterval(() => {
                    //         if (attemptCounter === 0) {
                    //             //не удалось найти запись в Search после 10 попыток - ошибка
                    //             clearInterval(refInterval);
                    //             setIsWaiting(false);
                    //             values.amount = 0;
                    //             //toast.error(`Ошибка обновления аукциона "${values?.title}".`, { duration: 15000 });

                    //         }
                    //         attemptCounter--;
                    //         try {
                    //             financeData.refetch()
                    //                 .then(rez => {
                    //                     let currentTime = ConvertUTCDate(null);
                    //                     currentTime!.setSeconds(currentTime!.getSeconds() - 5);
                    //                     let updatedTime = new Date(rez.data!.result.results[0].actionDate);
                    //                     if (rez.data && rez.data.result &&
                    //                         updatedTime!.getTime() > currentTime!.getTime()
                    //                     ) {
                    //                         clearInterval(refInterval);
                    //                         setIsWaiting(false);
                    //                         values.amount = 0;
                    //                     }
                    //                 })
                    //         } catch (e) {
                    //             clearInterval(refInterval);
                    //         }
                    //     }, 500);
                    // }

                }
                }
                validationSchema={Yup.object({
                    amount: Yup.number()
                        .typeError('Сумма должна быть числом')
                        .required('Необходимо указать сумму для зачисления на счет')
                        .positive('Сумма должна быть больше 0!')

                })}
            >
                {({ handleSubmit, setFieldValue, isSubmitting, errors, isValid, dirty }) => (
                    <Form onSubmit={handleSubmit} autoComplete='off'>
                        <div className='w-[900px] m-auto flex items-center'>
                            <div className=''>
                                <TextInput
                                    name='amount'
                                    placeholder='Сумма'
                                    label='Сумма для зачисления'
                                    labellWidth='w-[250px]'
                                    inputWidth='w-[237px]'
                                    onChange={() => { }}
                                    required
                                    controlsAlign='w-[600px]'
                                />
                            </div>

                            <Button
                                className=''
                                disabled={!isValid || !dirty || isSubmitting}
                                isProcessing={isSubmitting || isWaiting}
                                type='submit'>Добавить сумму
                            </Button>
                        </div>
                    </Form>
                )}
            </Formik>


            <div className='mt-5'>
                {
                    financeItems.length === 0 ? (
                        <p>Записи не найдены</p>
                    ) : (<>
                        <FinTable items={financeItems} />
                        <div className='flex justify-center mt-4'>
                            <AppPagination pageChanged={setPageNumber}
                                currentPage={params.pageNumber} totalPages={financeData.data?.result.pageCount!} />
                        </div>
                    </>
                    )}

            </div>


        </div>
    )

}
