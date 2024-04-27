import { useEffect, useState } from 'react'
import Heading from '../auctionList/Heading'
import { useNavigate, useParams } from 'react-router-dom'
import { Formik, Form, ErrorMessage } from 'formik';
import TextInput from '../inputComponents/TextInput';
import * as Yup from 'yup';
import { Auction, ProcessingState } from '../../store/types';
import DatePickerInput from '../inputComponents/DatePickerInput';
import ImageFileInput from '../inputComponents/ImageFileInput';
import TextAreaInput from '../inputComponents/TextAreaInput';
import { Button } from 'flowbite-react';
import { useCreateAuctionMutation, useGetDetailedViewDataQuery, useUpdateAuctionMutation } from '../../api/AuctionApi';
import { useGetImageForAuctionQuery } from '../../api/ImageApi';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { setEventFlag } from '../../store/processingSlice';

export default function AuctionForm() {
    let { id } = useParams();
    if (!id) id = 'empty';
    const { data, isLoading } = useGetDetailedViewDataQuery(id!, {
        skip: id === 'empty'
    });

    const [createAuction] = useCreateAuctionMutation();
    const [updateAuction] = useUpdateAuctionMutation();
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const procState: ProcessingState[] = useSelector((state: RootState) => state.processingStore);
    const [image, setImage] = useState('');
    const [isWaiting, setIsWaiting] = useState(false);
    const [newAuction, setNewAuction] = useState<Auction>(
        {
            title: '',
            properties: '',
            auctionEnd: new Date(),
            image: '',
            reservePrice: 0,
            description: '',
            error: ''
        } as Auction
    )
    const auctionImage = useGetImageForAuctionQuery(newAuction.id, {
        skip: newAuction.id === undefined
    });

    useEffect(() => {
        if (!isLoading && data) {
            setNewAuction(data!.result);
        }
    }, [data, isLoading])

    useEffect(() => {
        if (!auctionImage.isLoading && auctionImage.data?.result?.image) {
            setImage('data:image/png;base64, ' + auctionImage?.data?.result?.image);
        }
    }, [auctionImage.isLoading, auctionImage.data?.result?.image])


    useEffect(() => {
        if (id !== 'empty' && !isLoading && data?.isSuccess && data?.result && !data?.result.title) {
            navigate('/not-found');
        }
    }, [isLoading, data, id, navigate]);

    useEffect(() => {
        const eventStateAuctionUpdated = procState.find(p => p.eventName === 'AuctionUpdated');
        const eventStateAuctionCreated = procState.find(p => p.eventName === 'AuctionCreated');
        if ((eventStateAuctionUpdated && eventStateAuctionUpdated.ready) ||
            (eventStateAuctionCreated && eventStateAuctionCreated.ready)) {
            dispatch(setEventFlag({ eventName: 'AuctionUpdated', ready: false }));
            dispatch(setEventFlag({ eventName: 'AuctionCreated', ready: false }));
            navigate('/');
        }
    }, [procState, dispatch, navigate]);

    if (isLoading) return 'Загрузка...';

    return (
        <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
            <>
                <Heading title='Редактирование аукциона' subtitle='Отредактируйте данные ниже' />
                <Formik
                    initialValues={newAuction}
                    enableReinitialize
                    onSubmit={async (values, { setErrors }) => {
                        setIsWaiting(true);
                        if (id !== 'empty') {
                            //обновление аукциона
                            dispatch(setEventFlag({ eventName: 'AuctionUpdated', ready: false }));
                            await updateAuction({
                                id: id,
                                data: JSON.stringify(values)
                            });
                        } else {
                            //создание аукциона
                            dispatch(setEventFlag({ eventName: 'AuctionCreated', ready: false }));
                            await createAuction({
                                data: JSON.stringify(values)
                            });
                        }
                    }
                    }
                    validationSchema={Yup.object({
                        title: Yup.string().required('Необходимо указать наименование товара'),
                        auctionEnd: Yup.date().required('Необходимо указать дату и время окончания акциона')
                    })}
                >
                    {({ handleSubmit, setFieldValue, isSubmitting, errors, isValid, dirty }) => (
                        <Form onSubmit={handleSubmit} autoComplete='off'>
                            <div className='mt-5'>
                                <TextInput
                                    name='title'
                                    placeholder='Наименование'
                                    label='Наименование'
                                    labellWidth='w-[250px]'
                                    inputWidth='w-[237px]'
                                    onChange={() => { }}
                                    required
                                />
                            </div>
                            <div className='mt-5'>
                                <TextAreaInput
                                    name='properties'
                                    label='Описание'
                                    placeholder='Описание'
                                    rows={5}
                                    labellWidth='w-[250px]'
                                    inputWidth='w-[400px]'
                                />
                            </div>
                            <div className='mt-5'>
                                <DatePickerInput
                                    name='auctionEnd'
                                    label="Дата окончания аукциона"
                                    labellWidth='w-[230px]'
                                    showTimeSelect
                                    showMonthDropdown
                                    showYearDropdown
                                    todayButton="Сегодня"
                                    closeOnScroll={true}
                                    timeCaption='time'
                                    locale='ru'
                                    dateFormat='dd.MM.yyyy HH:mm'
                                    timeIntervals={60}
                                    required
                                />
                            </div>
                            <div className='mt-5'>
                                <ImageFileInput
                                    name='image'
                                    label='Изображение'
                                    value={image}
                                    labellWidth='w-56'
                                    onChange={(imageData: string) => {
                                        setFieldValue('image', imageData);
                                        setImage(imageData);
                                    }}
                                />
                            </div>

                            {id === 'empty' &&
                                <div className='mt-5'>
                                    <TextInput
                                        name='reservePrice'
                                        label='Начальная цена'
                                        type='number'
                                        placeholder='Начальная цена'
                                        labellWidth='w-[250px]'
                                        inputWidth='w-[237px]'
                                        onChange={() => { }}
                                    />
                                </div>
                            }
                            <div className='mt-5'>
                                <TextAreaInput
                                    name='description'
                                    label='Примечание'
                                    placeholder='Примечание'
                                    rows={3}
                                    labellWidth='w-[250px]'
                                    inputWidth='w-[400px]'
                                />
                            </div>
                            <ErrorMessage name='error' render={() =>
                                <p>{errors.error}</p>
                            } />
                            <div className='flex justify-center m-5'>
                                <Button disabled={!isValid || !dirty || isSubmitting}
                                    isProcessing={isSubmitting || isWaiting}
                                    type='submit'>
                                    {id ? 'Сохранить' : 'Создать'}
                                </Button>
                                <Button className='ml-5'
                                    onClick={() => { navigate(-1) }}>
                                    Отмена
                                </Button>
                            </div>
                        </Form>
                    )}
                </Formik>
            </>

        </div>

    )
}
