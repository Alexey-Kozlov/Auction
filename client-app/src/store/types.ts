export type Auction = {
  reservePrice: number;
  seller: string;
  winner?: string;
  soldAmount: number;
  currentHighBid: number;
  createAt: Date;
  updatedAt: Date;
  auctionEnd: Date;
  title: string;
  properties: string;
  description?: string;
  image?: string;
  id: string;
  error?: string;
};

export type Bid = {
  bidId: string;
  id: string;
  bidder: string;
  bidTime: string;
  amount: number;
};

export type LoginResponse = {
  name: string;
  login: string;
  token: string;
  id: string;
};

export type CreateUser = {
  name: string;
  login: string;
  password: string;
};

export type LoginUser = {
  login: string;
  password: string;
};

export type User = {
  name: string;
  login: string;
  id?: string;
};

export type PagedResult<T> = {
  results: T[];
  pageCount: number;
  totalCount: number;
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

export type ObjectResponse<T> = {
  data?: T;
  error?: any;
};

export type PlaceBidParams = {
  amount: number;
  id: string;
  correlationId: string;
};

export type AuctionImage = {
  id: string;
  image: string;
};

export type NotifyUser = {
  id: string;
  enable: boolean;
};

export type FinanceItem = {
  id: string;
  itemId: string;
  debit: number;
  credit: number;
  actionDate: Date;
  status: number;
  balance: number;
};

export type ProcessingState = {
  eventName: string;
  ready: boolean;
  itemId?: string;
};

export type AuctionUpdated = {
  id: string;
  title: string;
  properties: string;
  description: string;
  image: string;
  reservePrice: number;
  auctionEnd: Date;
  correlationId: string;
};

export type AuctionFinished = {
  itemSold: boolean;
  id: string;
  winner?: string;
  amount?: number;
};

export type AuctionDeleted = {
  id: string;
  correlationId: string;
};

export type Message = {
  id: string;
  message: string;
  messageType: number;
  correlationId: string;
};

export type Session = {
  sessionid: string;
}
