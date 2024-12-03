
import { Dropdown } from 'flowbite-react';
import { AiFillTrophy, AiOutlineLogout } from 'react-icons/ai';
import { RiAuctionFill } from "react-icons/ri";
import { HiUser } from 'react-icons/hi2';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import { Message, ModalParams, User } from '../../store/types';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../../store/store';
import { emptyUserState, setAuthUser } from '../../store/authSlice';
import { setParams } from '../../store/paramSlice';
import { GrMoney } from "react-icons/gr";
import { setEventFlag } from '../../store/processingSlice';
import { GoCodescanCheckmark, GoDatabase } from "react-icons/go";
import InfoMessageToast from '../signalRNotifications/InfoMessageToast';
import toast from 'react-hot-toast';
import ModalConfirm from '../modals/ModalConfirm';
import { useEffect, useState } from 'react';

export default function UserActions() {
    const user: User = useSelector((state: RootState) => state.authStore);
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const location = useLocation();
    const [confirmParam, setConfirmParam] = useState<ModalParams>({confirmText:'', confirmTitle:''});
    const [showConfirm, setShowConfirm] = useState(false);
    const [dateValue, setDateValue] = useState<Date>(new Date());
    const [confirmResult, setConfirmResult] = useState<boolean | undefined>(undefined);    
    const SetWinner = () => {
        dispatch(setParams({ winner: user.login, seller: undefined }));
        dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: true }));
        if (location.pathname !== '/') navigate('/');
    }

    const SetSeller = () => {
        dispatch(setParams({ seller: user.login, winner: undefined }));
        dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: true }));
        if (location.pathname !== '/') navigate('/');
    }

    const handleLogout = () => {
        localStorage.removeItem('Auction');
        dispatch(setAuthUser({ ...emptyUserState }));
        navigate('/');
    }

    const handlerElkReindex = () => {
        dispatch(setEventFlag({ eventName: 'ElkIndex', ready: true }));
        const message:Message = {message:'Старт переиндексации ELK...', auctionId:'', messageType:0};
        return toast((p) => (
            <InfoMessageToast message={message} toastId={p.id} />
        ),{ duration: 5000 });
    }

    const handlerSetSnapShotDb = () => {
        setConfirmParam({
            confirmText:'Произвести сохранение состояния БД?',
            confirmTitle:'Сохранение состояния БД'});
        dispatch(setEventFlag({ eventName: 'SetSnapShotDb', ready: true }));
        const message:Message = {message:'Старт создания снимка БД в ES...', auctionId:'', messageType:0};
        return toast((p) => (
            <InfoMessageToast message={message} toastId={p.id} />
        ),{ duration: 5000 });
    }

    const handlerRestoreSnapShotDb = () => {
        setDateValue(new Date());
        setConfirmParam({
            confirmText:'Произвести восстановление БД из лога?',
            confirmTitle:'Восстановление БД из лога'});
        setShowConfirm(true);

    }

    useEffect(() => {
        if(confirmResult){
            dispatch(setEventFlag({ eventName: 'RestoreSnapShot', ready: false, param: dateValue }));
            const message:Message = {message:'Старт восстановления БД из ES...', auctionId:'', messageType:0};
            toast((p) => (
                <InfoMessageToast message={message} toastId={p.id} />
            ),{ duration: 5000 });
        }
        setShowConfirm(false);
        setConfirmResult(undefined);
    },[confirmResult,dispatch]);

    const handleSetDate = (result:Date) => {
        setDateValue(result);
    }
    return (
        <>
            <Dropdown inline label={`Здравствуйте ${user.name}`}>
                <Dropdown.Item icon={HiUser} onClick={SetSeller}>
                    Мои аукционы
                </Dropdown.Item>
                <Dropdown.Item icon={AiFillTrophy} onClick={SetWinner}>
                    Аукционы выигранные
                </Dropdown.Item>
                <NavLink to='/auctions/create'>
                    <Dropdown.Item icon={RiAuctionFill}>
                        Создать аукцион
                    </Dropdown.Item>
                </NavLink>
                <NavLink to='/finance/list'>
                    <Dropdown.Item icon={GrMoney}>
                        Финансы
                    </Dropdown.Item>
                </NavLink>
                {user.isAdmin && (
                <>
                    <Dropdown.Divider />
                    <Dropdown.Item icon={GoCodescanCheckmark} onClick={handlerElkReindex}>
                        Elk индексация
                    </Dropdown.Item>
                    <Dropdown.Item icon={GoDatabase} onClick={handlerSetSnapShotDb}>
                        Выполнить SnapShot Db
                    </Dropdown.Item>
                    <Dropdown.Item icon={GoDatabase} onClick={handlerRestoreSnapShotDb}>
                        Восстановить Db из SnapShot
                    </Dropdown.Item>                
                </>
                )}
                <Dropdown.Divider />
                <Dropdown.Item icon={AiOutlineLogout} onClick={handleLogout}>
                    Выход
                </Dropdown.Item>
            </Dropdown>
            <ModalConfirm 
                openModal = {showConfirm} 
                text = {confirmParam.confirmText}
                title = {confirmParam.confirmTitle}
                setResult={setConfirmResult}
                returnData={handleSetDate}
                dateValue={dateValue}
            />
        </>                
    )
}