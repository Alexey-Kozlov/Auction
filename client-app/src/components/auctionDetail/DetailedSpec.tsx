import { Auction, User } from '../../types';
import { TabPanel, TabView } from 'primereact/tabview';
import TabDetailInfo from './TabDetailInfo';
import TabChatTable from './Chat/TabChatTable';
import Tags from './Tags';

type Props = {
  auction: Auction;
  user: User;
};
export default function DetailedSpec({ auction, user }: Props) {
  return (
    <div>
      <TabView>
        <TabPanel
          header="Описание аукциона"
          leftIcon={
            <i className="pi pi-book mr-2" style={{ fontSize: '2rem' }} />
          }
          className="text-4xl"
        >
          <TabDetailInfo auction={auction} />
        </TabPanel>
        <TabPanel
          header="Обсуждение"
          leftIcon={
            <i className="pi pi-send mr-2" style={{ fontSize: '2rem' }} />
          }
          className="text-4xl"
        >
          <TabChatTable auction={auction} user={user} />
        </TabPanel>
        <TabPanel
          header="Теги"
          leftIcon={
            <i className="pi pi-tags mr-2" style={{ fontSize: '2rem' }} />
          }
          className="text-4xl"
        >
          <Tags auction={auction} />
        </TabPanel>
      </TabView>
    </div>
  );
}
