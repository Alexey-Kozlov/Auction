
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import toast from 'react-hot-toast';
import AuctionCreatedToast from '../components/signalRNotifications/AuctionCreatedToast';
import { Auction, AuctionFinished, Bid, User } from '../store/types';
import { useDispatch, useSelector } from 'react-redux';
import { setCurrentPrice } from '../store/auctionSlice';
import { addBid } from '../store/bidSlice';
import { useGetAuctionQuery } from '../api/SignalRApi';
import AuctionFinishedToast from '../components/signalRNotifications/AuctionFinishedToast';
import { RootState } from '../store/store';
import BidCreatedToast from '../components/signalRNotifications/BidCreatedToast';

export default function SignalRProvider() {
    const user: User = useSelector((state: RootState) => state.authStore);

    const [finishedAuction, setFinishedAuction] = useState<any>();
    const finishedAuctionId = finishedAuction?.auctionId ? finishedAuction.auctionId : 'empty';
    const auction = useGetAuctionQuery(finishedAuctionId, {
        skip: finishedAuctionId === 'empty'
    });
    const [auctionId, setAuctionId] = useState('');
    const dispatch = useDispatch();
    const [connection, setConnection] = useState<HubConnection | null>(null);

    const apiUrl = process.env.NODE_ENV === 'production'
        ? process.env.REACT_APP_PROD_NOTIFY_URL
        : process.env.REACT_APP_NOTIFY_URL

    useEffect(() => {
        const tokenData = localStorage.getItem("Auction");
        const token = JSON.parse(tokenData!).token;
        const newConnection = new HubConnectionBuilder()
            .withUrl(apiUrl!, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();
        setConnection(newConnection);
    }, [apiUrl]);

    useEffect(() => {
        if (!auction.isLoading && finishedAuction) {
            toast((p) => (
                <AuctionFinishedToast
                    finishedAuction={finishedAuction}
                    auction={auction.data!.result}
                />
            ),
                { duration: 10000 });
            setFinishedAuction(null);
        }
    }, [finishedAuction, auction.data, auction.data?.result, auction.isLoading]);

    useEffect(() => {
        if (auctionId) {
            toast((p) => (
                <BidCreatedToast auctionId={auctionId} toastId={p.id} />
            ), { duration: 10000 });
            setAuctionId('');
        }
    }, [auctionId]);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    console.log('Коннект установлен с хабом уведомлений');

                    connection.on('BidPlaced', (bid: Bid) => {
                        //задержка в 1 секунду - чтобы обновились данные
                        setTimeout(() => {
                            if (bid.bidStatus.includes('Принято') && bid.bidder !== user.login) {
                                dispatch(setCurrentPrice({ auctionId: bid.auctionId, amount: bid.amount }));
                                dispatch(addBid({ bid: bid }));
                                setAuctionId(bid.auctionId);
                            }
                        }, 1000);
                    })

                    connection.on('AuctionCreated', (auction: Auction) => {
                        //задержка в 1 секунду - чтобы обновились данные
                        setTimeout(() => {
                            if (user?.login !== auction.seller) {
                                return toast((p) => (
                                    <AuctionCreatedToast auction={auction} toastId={p.id} />
                                ),
                                    { duration: 10000 });
                            }
                        }, 1000);
                    })

                    connection.on('AuctionFinished', (finishedAuction: AuctionFinished) => {
                        setTimeout(() => {
                            setFinishedAuction(finishedAuction);
                        }, 1000);

                    })
                }).catch(err => console.log(err));
        }

        return () => {
            connection?.stop();
        }
    }, [connection, user.login, dispatch]);

    return (
        <></>
    )
}
