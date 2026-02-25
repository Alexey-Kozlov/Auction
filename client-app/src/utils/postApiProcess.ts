import { ApiResponseNet } from '../types';

export interface CustomError {
  message: string;
  statusCode: number;
}

export const PostApiProcess = (response: ApiResponseNet<any>) => {
  if (response && response.errorMessages && !response.isSuccess) {
    console.log(response.errorMessages.join(','));
  }
};

export const PostErrorApiProcess = (response: any, meta: any): CustomError => {
  if (
    response &&
    response.status === 400 &&
    response.data.errorMessages[0] === 'SystemMessage'
  ) {
    //это системное сообщение, нужно отобразить страничку SystemMessage.tsx
    console.log(response.data.errorMessages[1]);
    return {
      message: response.data.errorMessages[1],
      statusCode: response.status,
    };
  }

  if (response && response.status === 403) {
    console.log('Ошибка доступа!');
    return { message: meta.response.statusText, statusCode: response.status };
  }

  if (response && response.status === 404) {
    console.log('Страница не найдена!');
    return { message: meta.response.statusText, statusCode: response.status };
  }

  if (
    response &&
    response.data &&
    response.data.errorMessages &&
    !response.data.isSuccess
  ) {
    console.log(response.data.errorMessages.join(','));
    return { message: meta.response.statusText, statusCode: response.status };
  }
  return { message: meta.response.statusText, statusCode: response.status };
};
