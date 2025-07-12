import { FinanceTableItem } from "../../types";
import NumberWithSpaces from "../../utils/NumberWithSpaces";
import { GrMoney } from "react-icons/gr";
import ImageCard from "../auctionList/ImageCard";
import { NavLink } from "react-router-dom";

type Props = {
  item: FinanceTableItem;
};

export default function FinRow({ item }: Props) {
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
          ""
        )}
      </div>
      <div className="col-2 CenterItem FinanceTableCell">
        <div className="FinanceListText">{item.auctionSeller}</div>
      </div>
      <div className="col-2 CenterItem FinanceTableCell">
        <div className="FinanceListText">
          {`${new Date(item.actionDate).toLocaleDateString("ru-RU")} 
				${new Date(item?.actionDate).toLocaleTimeString("ru-RU")}`}
        </div>
      </div>

      <div className="col-1 CenterItem FinanceTableCell">
        <div className="FinanceListText">
          {item.status === 0 ? "Приход" : "Расход"}
        </div>
      </div>
      <div className="col-1 CenterItem FinanceTableCell">
        <div className="FinanceListText">
          {item.value === 0 ? "0" : NumberWithSpaces(item.value)}
        </div>
      </div>
    </div>
  );
}
