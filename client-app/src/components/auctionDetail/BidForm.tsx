
import { Formik, Form, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import TextInput from '../inputComponents/TextInput';
import { usePlaceBidForAuctionMutation } from '../../api/BidApi';
import NumberWithSpaces from '../../utils/NumberWithSpaces';

type Props = {
    auctionId: string;
    highBid: number;
}


export default function BidForm({ auctionId, highBid }: Props) {
    const [placeBid] = usePlaceBidForAuctionMutation();

    return (
        <Formik
            initialValues={{ amount: 0, error: null }}
            onSubmit={
                async (values, { setErrors }) => {
                    await placeBid({ amount: values.amount, auctionId: auctionId });
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
