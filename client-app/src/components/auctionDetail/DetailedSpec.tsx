import { Auction, User } from '../../types';
import { TabPanel, TabView } from 'primereact/tabview';
import TabDetailInfo from './TabDetailInfo';
import TabChatTable from './Chat/TabChatTable';
import Tags from './Tags';
import History from './History';
import { useSelector } from 'react-redux';
import { RootState } from '../../store/store';

type Props = {
  auction: Auction;
};
export default function DetailedSpec({ auction }: Props) {
  const user: User = useSelector((state: RootState) => state.authStore);
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
          <TabChatTable auction={auction} />
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
        {user.isAdmin && (
          <TabPanel
            header="История"
            leftIcon={
              <i className="pi pi-history mr-2" style={{ fontSize: '2rem' }} />
            }
            className="text-4xl"
          >
            <History auction={auction} />
          </TabPanel>
        )}
      </TabView>
    </div>
  );
}
