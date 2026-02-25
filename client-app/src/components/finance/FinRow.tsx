import { FinanceTableItem } from '../../types';
import NumberWithSpaces from '../../utils/numberWithSpaces';
import { GrMoney } from 'react-icons/gr';
import ImageCard from '../auctionList/ImageCard';
import { NavLink } from 'react-router-dom';

type Props = {
  item: FinanceTableItem;
};

export default function FinRow({ item }: Props) {
  const getLocalTime = (_date: Date, selector: string): string => {
    let dt = new Date(_date);
    dt.setHours(dt.getHours() - 3);
    return selector === 'd'
      ? dt.toLocaleDateString('RU-ru')
      : dt.toLocaleTimeString('RU-ru');
  };
  return (
    <div className="grid m-0">
      <div className="col-2 CenterItem FinanceTableCell">
        {item.auctionId ? (
          <NavLink to={`/auctions/${item.auctionId}`}>
            <ImageCard
              id={item.auctionId}
              dopStyle="max-w-7rem max-h-5rem"
              detail={false}
              cache={true}
            />
          </NavLink>
        ) : (
          <GrMoney size={30} />
        )}
      </div>
      <div className="col-4 CenterItem FinanceTableCell">
        {item.auctionTitle ? (
          <NavLink
            to={`/auctions/${item.auctionId}`}
            className="FinanceListText"
          >
            {item.auctionTitle}
          </NavLink>
        ) : (
          ''
        )}
      </div>
      <div className="col-2 CenterItem FinanceTableCell">
        <div className="FinanceListText">{item.auctionSeller}</div>
      </div>
      <div className="col-2 CenterItem FinanceTableCell">
        <div className="FinanceListText">
          {`${getLocalTime(item.actionDate, 'd')} 
				  ${getLocalTime(item.actionDate, 't')}`}
        </div>
      </div>

      <div className="col-1 CenterItem FinanceTableCell">
        <div className="FinanceListText">
          {item.status === 0 ? 'Приход' : 'Расход'}
        </div>
      </div>
      <div className="col-1 CenterItem FinanceTableCell">
        <div className="FinanceListText">
          {item.value === 0 ? '0' : NumberWithSpaces(item.value)}
        </div>
      </div>
    </div>
  );
}
