import { jwtDecode } from "jwt-decode";

const AddTokenHeader = () => {
	let tokenData = localStorage.getItem("Auction");
	if (tokenData) {
		const token = JSON.parse(tokenData).token;
		const decode: { exp: number } = jwtDecode(token);
		//если токен просрочен - удаляем его из локального хранилища
		if (decode.exp * 1000 <= Date.now()) {
			localStorage.removeItem("Auction");
			return null;
		}
		return "Bearer " + token;
	}
	return null;
};

export default AddTokenHeader;
