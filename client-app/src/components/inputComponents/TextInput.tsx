import { useField } from "formik";

interface TI {
    placeholder: string;
    name: string;
    label?: string;
    type?: string;
    labellWidth?: string;
    inputWidth?: string;
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export default function TextInput(props: TI) {

    const [field, meta, helpers] = useField(props.name);
    const { setValue } = helpers;
    return (
        <>
            <div className='flex justify-center col-sm-6 offset-sm-3 col-xs-12 mt-4'>
                <label className={props.labellWidth ? props.labellWidth : 'w-6/12'}>{props.label}</label>
                <input
                    className={`border-gray-400 border ml-2 pl-2
                ${props.inputWidth ? props.inputWidth : 'w-6/12'}`}
                    {...field}
                    {...props}
                    onChange={(e) => { setValue(e.currentTarget.value); props.onChange!(e) }} />
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