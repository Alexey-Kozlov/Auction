import { Formik, Form, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import TextInput from '../inputComponents/TextInput';
import NumberWithSpaces from '../../utils/NumberWithSpaces';
import { usePlaceBidForAuctionMutation } from '../../api/ProcessingApi';
import uuid from 'react-uuid';
import { ProcessingState, User } from '../../store/types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { setProcessFlag } from '../../store/processingSlice';
import { useEffect } from 'react';

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
        const userState = procState.find(p => p.userLogin === user.login);
        if (userState && userState.ready) {
            bidList.refetch();
            dispatch(setProcessFlag({ userLogin: user.login, ready: false }));
        }
    }, [procState, user, dispatch, bidList]);

    return (
        <Formik
            initialValues={{ amount: 0, error: null }}
            onSubmit={
                async (values, { setErrors }) => {
                    dispatch(setProcessFlag({ userLogin: user.login, ready: false }));
                    await placeBid({
                        amount: values.amount,
                        auctionId: auctionId,
                        correlationId: uuid()
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
    )
}
