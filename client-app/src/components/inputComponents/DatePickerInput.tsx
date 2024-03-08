import { useField } from "formik";
import DatePicker, { ReactDatePickerProps, registerLocale } from 'react-datepicker';
import { ru } from 'date-fns/locale'

interface Props extends ReactDatePickerProps {
    required?: boolean;
    labellWidth?: string;
    label?: string;
    controlsAlign?: string;
}

registerLocale('ru', ru);

export default function DatePickerInput({ required, label, labellWidth,
    controlsAlign, ...rest }: Partial<Props>) {

    const [field, meta, helpers] = useField(rest.name!);

    return (
        <>
            <div className={`flex col-sm-6 offset-sm-3 col-xs-12 mt-4 ${controlsAlign}`}>
                <label className={labellWidth ? labellWidth : 'w-6/12'}>
                    {label}
                    {required && (<span>*</span>)}
                </label>
                <DatePicker
                    {...field}
                    {...rest}
                    wrapperClassName="datepicker"
                    selected={(field.value && new Date(field.value)) || null}
                    onChange={value => helpers.setValue(value)}
                />
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