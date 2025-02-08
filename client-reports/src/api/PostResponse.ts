import { ApiResponseNet } from "../Types";

export const PostResponse = (response: ApiResponseNet<any>) => {
	if (response && !response.isSuccess && response.errorMessages) {
		console.log(response.errorMessages.join(","));
	}
};

export const PostApiProcess = (response: ApiResponseNet<any>) => {
	if (response && !response.isSuccess && response.errorMessages) {
		console.log(response.errorMessages.join(","));
	}
};

export const PostErrorApiProcess = (response: any) => {
	if (
		response &&
		response.data &&
		!response.data.isSuccess &&
		response.data.errorMessages
	) {
		console.log(response.data.errorMessages.join(","));
		return;
	}
	if (response && response.status === 403) {
		console.log("Ошибка доступа!");
		return;
	}
	if (response && response.status === 404) {
		console.log("Страница не найдена!");
		return;
	}
	if (response) {
		console.log("Ошибка - " + response.status);
		return;
	}
};
