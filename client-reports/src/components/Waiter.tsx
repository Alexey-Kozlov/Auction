import { ProgressSpinner } from "primereact/progressspinner";

export default function Waiter() {
  return (
    <div className="Absolute-Center">
      <ProgressSpinner style={{ width: "50px", height: "50px" }} />
    </div>
  );
}
