import { useGetUserNameQuery } from "../../../api/AuthApi";

type Props = {
	userLogin: string;
};
export default function ChatUser({ userLogin }: Props) {
	const userName = useGetUserNameQuery(userLogin, { skip: !userLogin });
	return <div className="text-4xl">{userName.data?.result}</div>;
}
