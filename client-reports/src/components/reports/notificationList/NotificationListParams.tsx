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
            Label: "Пользователь",
            Type: ParameterType.Text,
            Value: "",
            Id: "UserLogin",
          } as ParameterItem,
        ]}
        reportName="Уведомления пользователя"
      />
    </div>
  );
}
