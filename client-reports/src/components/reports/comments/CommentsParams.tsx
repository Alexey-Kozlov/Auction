import SlidePanel from "../../SlidePanel";
import { ParameterItem, ParameterType } from "../../../types";
import RenderReport from "./RenderReport";

export default function CommentsParams() {
  return (
    <div>
      <RenderReport reportId="CommentsList" />
      <SlidePanel
        params={[
          {
            Label: "Автор аукциона (логин)",
            Type: ParameterType.Text,
            Value: "",
            Id: "Seller",
          } as ParameterItem,
          {
            Label: "Комментарий (часть)",
            Type: ParameterType.Text,
            Value: "",
            Id: "Comment",
          } as ParameterItem,
        ]}
        reportName="Аукционы с комментариями"
      />
    </div>
  );
}
