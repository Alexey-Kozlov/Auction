import SlidePanel from "../../SlidePanel";
import { ParameterItem, ParameterType } from "../../../types";
import RenderReport from "./RenderReport";

export default function NotificationListParams() {
  return (
    <div>
      <RenderReport reportId="NotificationList" />
      <SlidePanel
        params={[
          {
            Label: "Пользователь (логин)",
            Type: ParameterType.Text,
            Value: "",
            Id: "UserLogin",
          } as ParameterItem,
          {
            Label: "Наименование аукциона",
            Type: ParameterType.Text,
            Value: "",
            Id: "Auction",
          } as ParameterItem,
        ]}
        reportName="Уведомления пользователя"
      />
    </div>
  );
}
