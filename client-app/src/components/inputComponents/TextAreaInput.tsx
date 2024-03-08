import { useField } from "formik";

interface TI {
    placeholder: string;
    name: string;
    label?: string;
    rows: number;
    labellWidth?: string;
    inputWidth?: string;
    inputDescr?: string;
    required?: boolean;
    controlsAlign?: string;
}

export default function TextAreaInput({ labellWidth, inputWidth, inputDescr, required,
    controlsAlign, ...rest }: TI) {

    const [field, meta] = useField(rest.name);

    return (
        <>
            <div className={`flex col-sm-6 offset-sm-3 col-xs-12 mt-4 ${controlsAlign}`}>
                <label className={labellWidth ? labellWidth : 'w-6/12'}>
                    {rest.label}
                    {required && (<span>*</span>)}
                </label>
                <textarea
                    className={`border-gray-400 border ml-2 pl-2
                ${inputWidth ? inputWidth : 'w-6/12'}`}
                    {...field}
                    {...rest}
                />
                {inputDescr && (
                    <label className="ml-3">{inputDescr}</label>
                )}
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