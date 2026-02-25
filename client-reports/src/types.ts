import { IconType } from "react-icons";

export interface ReportItem {
  Name: string;
  Description: string;
  Id: string;
  Icon: IconType;
}

export type ParameterItem = {
  Label: string;
  Type: ParameterType;
  Value: string | Date;
  Id: string;
};

export enum ParameterType {
  Text,
  Date,
  Number,
  Select,
  Bool,
}

export type ParameterSelect = {
  Label: string;
  Value: string;
  Default: boolean;
};

export type ApiResponseNet<T> = {
  statusCode: number;
  isSuccess: boolean;
  errorMessages: Array<string>;
  result: T;
};

export type ApiResponse<T> = {
  data?: ApiResponseNet<T>;
  error?: any;
};

export enum RequestType {
  ReadDetail,
  ReadList,
  Edit,
  Register,
  Login,
  Create,
  Finance,
  NotFound,
  TraceId,
  RefreshToken,
  CurrentSettings,
}

export enum ReportType {
  AuctionList,
  NotificationList,
}

export type User = {
  name: string;
  login: string;
  isAdmin: boolean;
  isGuest: boolean;
};

export enum SortDirection {
  descending,
  ascending,
}

export type RefreshLinkType = {
  value: any;
  setClear: boolean;
};

export type LogoutUser = {
  login: string;
};

export type LoginResponse = {
  name: string;
  login: string;
  token: string;
  isGuest: boolean;
};

export type State = {
  userLogin?: string;
};

export type LoginUser = {
  login: string;
  password: string;
  isGuest: boolean;
};

export type CurrentSettings = {
  adminMode: boolean;
};

export type ProcessingState = {
  eventName: string;
  ready: boolean;
  itemId?: string;
  param?: any;
  lastChanged: boolean;
};

export enum SignalREvents {
  BidPlaced,
  FinanceCreate,
  AuctionCreate,
  AuctionUpdate,
  AuctionDelete,
  ResetImageCache,
  ElkIndexReset,
  ElkSearch,
  SetSnapShot,
  OperationProgress,
  AuctionFinished,
  ErrorMessage,
  EditNotification,
  CommunicationCreate,
  CommunicationUpdate,
  CommunicationDelete,
  RestoreSnapShot,
  CommunicationChanged,
  CollectionChanged,
  SetCurrentSettings,
}

export type NotificationEvent = {
  show: boolean;
  data: string;
};

export enum ToastType {
  Info,
  Warning,
  Error,
}
