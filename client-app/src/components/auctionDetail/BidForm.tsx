import { Formik, Form, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import TextInput from '../inputComponents/TextInput';
import NumberWithSpaces from '../../utils/NumberWithSpaces';
import { usePlaceBidForAuctionMutation } from '../../api/ProcessingApi';
import uuid from 'react-native-uuid';
import { ProcessingState, User } from '../../store/types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { useEffect } from 'react';
import { setEventFlag } from '../../store/processingSlice';
import Waiter from '../Waiter';

type Props = {
    auctionId: string;
    highBid: number;
    bidList: any;
}

export default function BidForm({ auctionId, highBid, bidList }: Props) {
    const [placeBid] = usePlaceBidForAuctionMutation();
    const dispatch = useDispatch();
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const user: User = useSelector((state: RootState) => state.authStore);
    useEffect(() => {       
        const eventState = procState.find(p => p.eventName === 'BidPlaced');
        if (eventState && eventState.ready) {
            bidList.refetch();
            dispatch(setEventFlag({ eventName: 'BidPlaced', ready: false }));
        }
    }, [procState, user, dispatch, bidList]);

    return (
        <>
            { (procState.find(p => p.eventName === 'WaiterHide') &&
                !procState.find(p => p.eventName === 'WaiterHide')!.ready) ? <
                    Waiter color='rgb(156 163 175)' /> 
                    :
                    <Formik
                    initialValues={{ amount: 0, error: null }}
                    onSubmit={
                        async (values, { setErrors }) => {
                            dispatch(setEventFlag({ eventName: 'BidPlaced', ready: false}));
                            dispatch(setEventFlag({ eventName: 'WaiterHide', ready: false}));
                            dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: false}));
                            await placeBid({
                                amount: values.amount,
                                id: auctionId,
                                correlationId: uuid.v4() as string
                            });
                            values.amount = 0;
                        }
                    }
                    validationSchema={Yup.object({
                        amount: Yup.number()
                            .required('Необходимо указать предложение')
                            .positive('Предложение должно быть больше 0')
                            .min(highBid + 1, 'Предложение должно быть не меньше ' + (highBid + 1) + ' руб.')
                    })}
                >
                    {({ handleSubmit, errors }) => (
                        <Form onSubmit={handleSubmit} autoComplete='off'>
                            <div className='text-center'>
                                <div className='mt-5'>
                                    <TextInput
                                        type='number'
                                        name='amount'
                                        label={`Ваше предложение (мин. - ${NumberWithSpaces(highBid + 1)} руб)`}
                                        placeholder={`Укажите ваше предложение (минимально - ${highBid + 1}) руб`}
                                        labellWidth='w-96'
                                        inputWidth='w-20'
                                        inputDescr='руб.'
                                        onChange={() => { }} />

                                </div>
                                <ErrorMessage name='error' render={() =>
                                    <p className='text-red-500'>{errors.error}</p>
                                } />
                            </div>
                        </Form>
                    )}
                </Formik>
            }
        </>

    )
}
