import { Route, Routes } from "react-router-dom";
import Listings from "./components/auctionList/Listings";
import NavBar from "./components/nav/NavBar";
import ToasterProvider from "./providers/ToasterProvider";
import Register from "./components/auth/Register";
import Login from "./components/auth/Login";
import Detail from "./components/auctionDetail/Detail";
import AuctionForm from "./components/auctionEdit/AuctionForm";
import SignalRProvider from "./providers/SignalRProvider";
import { User } from "./store/types";
import { useSelector } from "react-redux";
import { RootState } from "./store/store";
import NotFound from "./components/nav/NotFound";
import FinListings from "./components/finance/FinListings";
import { useEffect, useState } from "react";
import ShowEventsPopUp from "./components/services/ShowEventsPopUp";

function App() {
  const user: User = useSelector((state: RootState) => state.authStore);

  const [userChange, setUserChange] = useState<User>();

  useEffect(() =>{
    setUserChange(user);
  },[user]);

  return (
    <div>
      <ToasterProvider />
      <NavBar />
      <div className="container mx-auto px-5 pt-10">
        <Routes>
          <Route path="/" element={<Listings />}></Route>
          <Route path="/register" element={<Register />}></Route>
          <Route path="/login" element={<Login />}></Route>
          <Route path="/auctions/create" element={<AuctionForm />}></Route>
          <Route path="/auctions/edit/:id" element={<AuctionForm />}></Route>
          <Route path="/auctions/:id" element={<Detail />}></Route>
          <Route path="/finance/list" element={<FinListings />}></Route>
          <Route path="/not-found" element={<NotFound />}></Route>
        </Routes>
        <SignalRProvider />
        <ShowEventsPopUp />

      </div>
    </div>
  );
}

export default App;
