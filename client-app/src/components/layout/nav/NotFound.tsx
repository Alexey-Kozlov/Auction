import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";

const NotFound = () => {
  const navigate = useNavigate();
  return (
    <div className="CenterItem mt-10">
      <div className="flex-column">
        <div className="mt-5 text-4xl">
          <h2>Страница не найдена</h2>
        </div>
        <div className="mt-10 text-center">
          <Button
            text
            raised
            rounded
            className="CustomButton w-20rem"
            onClick={() => navigate("/")}
          >
            Домой
          </Button>
        </div>
      </div>
    </div>
  );
};

export default NotFound;
