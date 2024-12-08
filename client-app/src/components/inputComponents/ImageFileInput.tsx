import { FileInput } from 'flowbite-react';
import React, { useEffect, useState } from 'react'
import { useField } from "formik";
import SwitchInput from './SwitchInput';

type Props = {
    label: string;
    value?: string;
    name: string;
    labellWidth?: string;
    inputWidth?: string;
    inputDescr?: string;
    required?: boolean;
    controlsAlign?: string;
    onChange: (imageData: string) => void;
    usingImage: (usingImage: boolean) => void;
}

export default function ImageFileInput({ labellWidth, inputWidth, inputDescr, required, value,
    controlsAlign, onChange, usingImage, ...rest }: Props) {
    const [imageDisplay, setImageDisplay] = useState('');
    const [usingImg, setUsingImg] = useState(false);
    const [field, meta] = useField(rest.name);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files && e.target.files[0];
        console.log(field.name);
        if (file) {
            const reader = new FileReader();
            reader.readAsDataURL(file);

            reader.onload = (e) => {
                setImageDisplay(e.target?.result as string);
                onChange(e.target?.result as string);
            }
        }
    }

    useEffect(() => {
        if (value) {
            setImageDisplay(`${value}`);
            setUsingImg(prev => true);
            usingImage(true);
        } else {
            setUsingImg(prev => false);
            usingImage(usingImg);
        }
    }, [value])

    const handleImageUsing = async (event: React.FormEvent<HTMLInputElement>) => {
        setUsingImg(prev => {
            usingImage(!prev);
            return !prev;
        });

    }

    return (
        <>
            <div className={`flex col-sm-6 offset-sm-3 col-xs-12 mt-4 ${controlsAlign}`}>
                <div>
                    <div>
                        <label className={labellWidth ? labellWidth : 'w-6/12'}>
                            {rest.label}
                            {required && (<span>*</span>)}
                        </label>
                        <FileInput
                            className='mt-2'
                            name={rest.name}
                            placeholder={rest.label}
                            onChange={handleFileChange}
                        />
                    </div>
                    <div className='mt-4'>
                        <label className="inline-flex items-center">
                            <p className='mr-2'>Требуется изображение</p>
                            <SwitchInput
                                checked={usingImg}
                                handleImageUsing={handleImageUsing}
                            />
                        </label>
                    </div>
                </div>
                <div className='h-52'>
                    {usingImg && <img alt='' className='max-h-full max-w-full' src={imageDisplay} />}
                </div>
            </div>
            <div className="text-left">
                {
                    meta.touched && meta.error ?
                        <p className="text-red-500">{meta.error}</p>
                        : null
                }
            </div>
        </>
    )
}
