
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'
import { ReactNode, useEffect, useState } from 'react'
import toast from 'react-hot-toast';
import AuctionCreatedToast from '../components/signalRNotifications/AuctionCreatedToast';
import { Auction, AuctionFinished, Bid, User } from '../store/types';
import { useDispatch } from 'react-redux';
import { setCurrentPrice } from '../store/auctionSlice';
import { addBid } from '../store/bidSlice';
import { useGetAuctionQuery } from '../api/SignalRApi';
import AuctionFinishedToast from '../components/signalRNotifications/AuctionFinishedToast';

type Props = {
    children: ReactNode;
    user: User | null;
}

export default function SignalRProvider({ children, user }: Props) {
    const [finishedAuction, setFinishedAuction] = useState<any>();
    const auction = useGetAuctionQuery(finishedAuction?.auctionId ? finishedAuction.auctionId : 'empty');
    const dispatch = useDispatch();
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const apiUrl = process.env.NODE_ENV === 'production'
        ? 'http://api.carsties.com/notifications'
        : process.env.REACT_APP_NOTIFY_URL

    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl(apiUrl!)
            .withAutomaticReconnect()
            .build();
        setConnection(newConnection);
    }, [apiUrl]);

    useEffect(() => {
        if (!auction.isLoading && finishedAuction) {
            toast((p) => (
                <AuctionFinishedToast
                    finishedAuction={finishedAuction}
                    auction={auction.data!}
                />
            ),
                { duration: 10000 });
            setFinishedAuction(null);
        }
    }, [finishedAuction, auction.data, auction.isLoading]);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    console.log('Коннект установлен с хабом уведомлений');
                    connection.on('BidPlaced', (bid: Bid) => {
                        if (bid.bidStatus.includes('Принято')) {
                            dispatch(setCurrentPrice({ auctionId: bid.auctionId, amount: bid.amount }));
                        }
                        dispatch(addBid({ bid: bid }));
                    })

                    connection.on('AuctionCreated', (auction: Auction) => {
                        if (user?.login !== auction.seller) {
                            return toast((p) => (
                                <AuctionCreatedToast auction={auction} toastId={p.id} />
                            ),
                                { duration: 10000 });
                        }
                    })

                    connection.on('AuctionFinished', (finishedAuction: AuctionFinished) => {
                        setFinishedAuction(finishedAuction);
                    })

                }).catch(err => console.log(err));
        }

        return () => {
            connection?.stop();
        }
    }, [connection, setCurrentPrice, addBid, user?.login]);

    return (
        children
    )
}
