import { ApiResponseNet } from "../types";

export const PostApiProcess = (response: ApiResponseNet<any>) => {
	if (response && response.errorMessages && !response.isSuccess) {
		console.log(response.errorMessages.join(","));
	}
};

export const PostErrorApiProcess = (response: any) => {
	if (
		response &&
		response.data &&
		response.data.errorMessages &&
		!response.data.isSuccess
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
		console.log(
			"Ошибка, код  - " + response.status + ", " + response.data.errors["$"]
		);
		return;
	}
};
