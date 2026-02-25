import { InputSwitch } from 'primereact/inputswitch';
import React, { useEffect, useState } from 'react';
import NumberWithSpaces from '../../utils/numberWithSpaces';

type Props = {
  value?: string;
  name: string;
  inputWidth?: string;
  inputDescr?: string;
  controlsAlign?: string;
  onChange: (imageData: string) => void;
  usingImage: (usingImage: boolean) => void;
};

export default function ImageFileInput({
  inputWidth,
  inputDescr,
  value,
  controlsAlign,
  onChange,
  usingImage,
  ...rest
}: Props) {
  const [imageDisplay, setImageDisplay] = useState('');
  const [usingImg, setUsingImg] = useState(false);
  const [imageSize, setImageSize] = useState<number>(0);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files && e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.readAsDataURL(file);

      reader.onload = (e) => {
        setImageDisplay(e.target?.result as string);
        setImageSize(Math.round(e.loaded / 1000));
        onChange(e.target?.result as string);
      };
    }
  };

  useEffect(() => {
    if (value) {
      setImageDisplay(`${value}`);
      setImageSize(
        Math.round(
          atob(
            value
              .replace('data:image/png;base64,', '')
              .replace('data:image/jpg;base64,', '')
              .replace('data:image/jpeg;base64,', '')
              .replace('data:image/bmp;base64,', ''),
          ).length / 1000,
        ),
      );
      setUsingImg((prev) => true);
    } else {
      setUsingImg((prev) => false);
    }
    // eslint-disable-next-line
  }, [value]);

  const handleImageUsing = (checked: boolean) => {
    setUsingImg((prev) => {
      usingImage(!prev);
      return !prev;
    });
  };

  return (
    <>
      <div className="flex-column">
        <div className="flex-column">
          <div>
            <input type="file" onChange={handleFileChange} />
            <span className="ml-6">Размер :</span>
            <span className="ml-2">{NumberWithSpaces(imageSize) + ' Kb.'}</span>
          </div>
          <div className="flex mt-10 align-items-center">
            <p className="mr-2 mr-10">Требуется изображение</p>
            <InputSwitch
              className="ImageUsing"
              checked={usingImg}
              onChange={(e) => handleImageUsing(e.value)}
            />
          </div>
        </div>
        <div>
          {usingImg && (
            <img alt="" className="ImageEditPreview" src={imageDisplay} />
          )}
        </div>
      </div>
    </>
  );
}
