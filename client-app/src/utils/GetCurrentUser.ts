export const GetCurrentUser = (): string => {
	const tokenData = localStorage.getItem("Auction");
	if (tokenData) {
		return JSON.parse(tokenData).login;
	}
	return "";
};
