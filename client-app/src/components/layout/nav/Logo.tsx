import { RiAuctionLine } from 'react-icons/ri';
import { useDispatch } from 'react-redux';
import { NavLink } from 'react-router-dom';
import { setParams } from '../../../store/paramSlice';

export default function Logo() {
  const dispatch = useDispatch();
  const handleClickHome = () => {
    dispatch(
      setParams({
        searchTerm: '',
        searchAdv: '',
        pageNumber: 1,
        firstPage: 0,
        tag: '',
      }),
    );
  };

  return (
    <NavLink id="NavBarLogo" onClick={handleClickHome} to="/">
      <RiAuctionLine size={34} />
      <div>Аукцион</div>
    </NavLink>
  );
}
