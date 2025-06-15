import { Auction, User } from "../../types";
import { TabPanel, TabView } from "primereact/tabview";
import TabDetailInfo from "./TabDetailInfo";
import TabChatTable from "./Chat/TabChatTable";

type Props = {
	auction: Auction;
	user: User;
};
export default function DetailedSpec({ auction, user }: Props) {
	return (
		<div>
			<TabView>
				<TabPanel header="Описание аукциона" leftIcon="pi pi-book mr-2">
					<TabDetailInfo auction={auction} />
				</TabPanel>
				<TabPanel header="Обсуждение" leftIcon="pi pi-send mr-2">
					<TabChatTable auction={auction} user={user} />
				</TabPanel>
			</TabView>
		</div>
	);
}
