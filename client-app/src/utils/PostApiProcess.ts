import toast from "react-hot-toast";
import { ApiResponseNet } from "../store/types";

export const PostApiProcess = (response: ApiResponseNet<any>) => {
  if (response && !response.isSuccess) {
    console.log(response.errorMessages.join(","));
    toast.error(response.errorMessages[0]);
  }
};

export const PostErrorApiProcess = (response: any) => {
  if (response && response.data && !response.data.isSuccess) {
    console.log(response.data.errorMessages.join(","));
    toast.error(response.data.errorMessages[0]);
    return;
  }
  if (response && response.status === 403) {
    console.log("Ошибка доступа!");
    toast.error("Ошибка доступа к ресурсу!");
    return;
  }
  if (response && response.status === 404) {
    console.log("Страница не найдена!");
    toast.error("Страница не найдена!");
    return;
  }
  if (response) {
    console.log("Ошибка - " + response.status);
    toast.error("Ошибка - "  + response.status);
    return;
  }
};
