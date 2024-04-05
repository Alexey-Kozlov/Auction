import toast from "react-hot-toast";
import { ApiResponseNet } from "../store/types";

export const PostApiProcess = (response: ApiResponseNet<any>) => {
  if (!response!.isSuccess) {
    console.log(response!.errorMessages.join(","));
    toast.error(response!.errorMessages[0]);
  }
};

export const PostErrorApiProcess = (response: any) => {
  if (!response!.data!.isSuccess) {
    console.log(response!.data!.errorMessages.join(","));
    toast.error(response!.data!.errorMessages[0]);
  }
};
