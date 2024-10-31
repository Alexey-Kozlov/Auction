import { useEffect, useState } from 'react'
import qs from 'query-string';
import { useDispatch, useSelector } from 'react-redux';
import { reset, setParams } from '../../store/paramSlice';
import { RootState } from '../../store/store';
import { FinanceItem, ProcessingState, User } from '../../store/types';
import { useGetBalanceQuery, useGetFinanceItemQuery } from '../../api/FinanceApi';
import { setFinanceItems } from '../../store/financeSlice';
import AppPagination from '../auctionList/AddPagination';
import { Button } from 'flowbite-react';
import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../inputComponents/TextInput';
import FinTable from './FinTable';
import { setEventFlag } from '../../store/processingSlice';
import { useFinanceCreateMutation } from '../../api/ProcessingApi';

export default function FinListings() {
    const dispatch = useDispatch();
    const [isWaiting, setIsWaiting] = useState(false);
    const [addCredit] = useFinanceCreateMutation();
    const params = useSelector((state: RootState) => state.paramStore);
    const financeItems: FinanceItem[] = useSelector((state: RootState) => state.financeStore).items;
    const url = qs.stringifyUrl({ url: '', query: { ...params, pageSize: 5 } });
    const financeData = useGetFinanceItemQuery(url);
    const balance = useGetBalanceQuery(null);
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const sessionId = useSelector((state: RootState) => state.paramStore).sessionId;
    const user: User = useSelector((state: RootState) => state.authStore);

    useEffect(() => {
        if (financeData.data && !financeData.isLoading) {
            dispatch(setFinanceItems(financeData.data.result));
        }
    }, [financeData, dispatch]);

    useEffect(() => {
        dispatch(reset(null));
        return () => {
            dispatch(reset(null));
        }
    }, [dispatch])

    useEffect(() => {
        const eventStateFinanceCreditAdd = procState.find(p => p.eventName === 'FinanceCreate' && p.ready);
        if (eventStateFinanceCreditAdd) {
            dispatch(setEventFlag({ eventName: 'FinanceCreate', ready: false }));
            financeData.refetch();
            balance.refetch();
            setIsWaiting(false);
        }
    }, [procState, dispatch, financeData]);

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
                    dispatch(setEventFlag({ eventName: 'FinanceCreate', ready: false }));
                    setIsWaiting(true);
                    await addCredit({
                        amount: values.amount,
                        sessionid:sessionId,
                        userlogin:user.login
                    });
                    values.amount = 0;
                }
                }
                validationSchema={Yup.object({
                    amount: Yup.number()
                        .typeError('Сумма должна быть числом')
                        .required('Необходимо указать сумму для зачисления на счет')
                        .positive('Сумма должна быть больше 0!')

                })}
            >
                {({ handleSubmit, isSubmitting, errors, isValid, dirty }) => (
                    <Form onSubmit={handleSubmit} autoComplete='off'>
                        <div className='flex justify-between  items-center'>
                            <div className='w-24'></div>
                            <div className='flex items-center content-center'>
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
                            <div className='w-40'>
                                <div className='mx-auto text-center font-bold'>Баланс :</div>
                                <div className='mx-auto text-center font-bold text-2xl text-blue-700'>
                                    {balance.data?.result ?? 0} р.
                                </div>
                            </div>
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
