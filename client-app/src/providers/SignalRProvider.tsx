
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import toast from 'react-hot-toast';
import AuctionCreatedToast from '../components/signalRNotifications/AuctionCreatedToast';
import { Auction, AuctionFinished, Bid, Message, PagedResult, User } from '../store/types';
import { useDispatch, useSelector } from 'react-redux';
import { useGetAuctionQuery } from '../api/SignalRApi';
import AuctionFinishedToast from '../components/signalRNotifications/AuctionFinishedToast';
import { RootState } from '../store/store';
import BidCreatedToast from '../components/signalRNotifications/BidCreatedToast';
import { setEventFlag } from '../store/processingSlice';
import ErrorMessageToast from '../components/signalRNotifications/ErrorMessageToast';
import WarningMessageToast from '../components/signalRNotifications/WarningMessageToast';
import InfoMessageToast from '../components/signalRNotifications/InfoMessageToast';
import FinanceUnsufficientToast from '../components/signalRNotifications/FinanceUnsufficientToast';
import { setData } from '../store/auctionSlice';
import { setParams } from '../store/paramSlice';

export default function SignalRProvider() {
    const user: User = useSelector((state: RootState) => state.authStore);

    const [finishedAuction, setFinishedAuction] = useState<AuctionFinished>();
    const finishedAuctionId = finishedAuction?.id ? finishedAuction.id : 'empty';
    const auction = useGetAuctionQuery(finishedAuctionId, {
        skip: finishedAuctionId === 'empty'
    });
    const dispatch = useDispatch();
    const [connection, setConnection] = useState<HubConnection | null>(null);

    const apiUrl = process.env.NODE_ENV === 'production'
        ? process.env.REACT_APP_PROD_NOTIFY_URL
        : process.env.REACT_APP_NOTIFY_URL

    const tokenData = localStorage.getItem("Auction");

    useEffect(() => {
        if(tokenData){
            const token = JSON.parse(tokenData!).token;
            const newConnection = new HubConnectionBuilder()
                .withUrl(apiUrl!, {
                    accessTokenFactory: () => token
                })
                .withAutomaticReconnect()
                .build();
            setConnection(newConnection);
        } else {
            const newConnection = new HubConnectionBuilder()
                .withUrl(apiUrl!)
                .withAutomaticReconnect()
                .build();
            setConnection(newConnection);
        }

    }, [apiUrl, tokenData]);

    useEffect(() => {
        if (!auction.isLoading && finishedAuction) {
            toast((p) => (
                <AuctionFinishedToast
                    finishedAuction={finishedAuction}
                    auction={auction.data!.result}
                />
            ),
                { duration: 10000 });
            setFinishedAuction(undefined);
        }
    }, [finishedAuction, auction.data, auction.data?.result, auction.isLoading]);

    useEffect(() => {
        const con_execute = async () => {
            if(connection){
                if(connection.state === HubConnectionState.Disconnected){
                    await connection.start();
                    console.log('Коннект установлен с хабом уведомлений');
                }
                connection.on('BidPlaced', (bid: Bid) => {
                    //устанавливаем флаг что данные для данного пользователя готовы и нужно обновить запрос
                    dispatch(setEventFlag({ eventName: 'BidPlaced', ready: true, itemId: bid.id}));
                    dispatch(setEventFlag({ eventName: 'WaiterHide', ready: true }));
                    //для обновления плашки ставки на страничке аукциона в списке аукционов
                    dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: true, itemId: bid.id}));
                    if (user?.login !== bid.bidder) {
                        return toast((p) => (
                            <BidCreatedToast id={bid.id} toastId={p.id} />
                        ),
                            { duration: 5000 });
                    }
                })

                connection.on('AuctionCreated', (auction: Auction) => {
                    dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: true, itemId: auction.id}));
                    dispatch(setEventFlag({ eventName: 'ImageChanged', ready: true, itemId: auction.id}));
                    if (user?.login !== auction.seller) {
                        return toast((p) => (
                            <AuctionCreatedToast auction={auction} toastId={p.id} />
                        ),
                            { duration: 5000 });
                    }
                })

                connection.on('AuctionUpdated', (auction: Auction) => {
                    dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: true, itemId: auction.id}));
                    dispatch(setEventFlag({ eventName: 'ImageChanged', ready: true, itemId: auction.id}));
                })

                connection.on('AuctionFinished', (finishedAuction: AuctionFinished) => {
                    setFinishedAuction(finishedAuction);
                })

                connection.on('AuctionDeleted', (auction: any) => {
                    dispatch(setEventFlag({ eventName: 'CollectionChanged', ready: true, itemId: auction.id }));
                })

                connection.on('FinanceCreditAdd', (finance: any) => {
                    dispatch(setEventFlag({ eventName: 'FinanceCreditAdd', ready: true}));
                })

                connection.on('SessionId', (id: any) => {
                    dispatch(setParams({ sessionId: id}));
                })

                connection.on('ElkSearch', (elk: any) => {
                    const elkData = elk.result as PagedResult<Auction>;
                     dispatch(setData(elkData));
                     dispatch(setEventFlag({ eventName: 'ElkSearch', ready: false}));
                })

                connection.on('ElkIndex', (result: number) => {
                    dispatch(setEventFlag({ eventName: 'ElkIndex', ready: false }));
                    const mes:Message = {message:result.toString(),correlationId:'',id:'',messageType:0};
                    return toast((p) => (
                        <InfoMessageToast message={mes} toastId={p.id} />
                    ), { duration: 5000 });
                })

                connection.on('SetSnapShotDb', (result: number) => {
                    dispatch(setEventFlag({ eventName: 'SetSnapShotDb', ready: false }));
                    const mes:Message = {message:result.toString(),correlationId:'',id:'',messageType:0};
                    return toast((p) => (
                        <InfoMessageToast message={mes} toastId={p.id} />
                    ), { duration: 5000 });
                })

                connection.on('ErrorMessage', (message: Message) => {
                    dispatch(setEventFlag({ eventName: 'WaiterHide', ready: true}));
                    switch (message.messageType) {
                        case 0:
                            return toast((p) => (
                                <ErrorMessageToast message={message} toastId={p.id} />
                            ),
                                { duration: 5000 });
                        case 1:
                            return toast((p) => (
                                <WarningMessageToast message={message} toastId={p.id} />
                            ),
                                { duration: 5000 });

                        case 2:
                            return toast((p) => (
                                <InfoMessageToast message={message} toastId={p.id} />
                            ),
                                { duration: 5000 });

                        case 3:
                            return toast((p) => (
                                <FinanceUnsufficientToast message={message} toastId={p.id} />
                            ),
                                { duration: 5000 });

                    }
                    
                })
            }
            
        }
        con_execute();
        return () => {
            connection?.stop();
        }
    }, [connection, user.login, dispatch]);

    return (
        <></>
    )
}
