import { TreeNode } from "primereact/treenode";

export interface AuctionTreeItem extends TreeNode {
  key: string;
  data: AuctionTreeItemData;
  children: BidTreeItem[];
}

export type AuctionTreeItemData = {
  itemid: string;
  seller: string;
  title: string;
  createAt: Date;
  auctionEnd: Date;
};

export interface BidTreeItem extends TreeNode {
  key: string;
  data: BidTreeItemData;
}

export type BidTreeItemData = {
  createAt: Date;
  bidder: string;
  amount: number;
};
