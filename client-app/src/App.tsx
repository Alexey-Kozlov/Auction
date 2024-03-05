import { Route, Routes } from "react-router-dom";
import Listings from "./components/auctionList/Listings";
import NavBar from "./components/nav/NavBar";
import ToasterProvider from "./providers/ToasterProvider";
import Register from "./components/auth/Register";
import Login from "./components/auth/Login";
import Detail from "./components/auctionDetail/Detail";
import AuctionForm from "./components/auctionEdit/AuctionForm";

function App() {
  return (
    <div>
      <ToasterProvider />
      <NavBar />
      <div className="container mx-auto px-5 pt-10">
        <Routes>
          <Route path="/" element={<Listings />}></Route>
          <Route path="/register" element={<Register />}></Route>
          <Route path="/login" element={<Login />}></Route>
          <Route path="/auctions/edit/:id" element={<AuctionForm />}></Route>
          <Route path="/auctions/:id" element={<Detail />}></Route>

        </Routes>

      </div>
    </div>
  );
}

export default App;
