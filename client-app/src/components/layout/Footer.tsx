import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";

export default function Footer() {
  const navigate = useNavigate();
  return (
    <div className="StickyFooter">
      <div className="NavBarFooter2"></div>
      <div className="CenterItem pt-2 bg-white">
        <Button
          text
          raised
          rounded
          className="StickyFooter CustomButton"
          onClick={() => navigate(-1)}
        >
          Назад
        </Button>
      </div>
      <div className="NavBarFooter1"></div>
    </div>
  );
}
