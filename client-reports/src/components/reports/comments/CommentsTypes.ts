import { TreeNode } from "primereact/treenode";

export interface AuctionTreeItem extends TreeNode {
  key: string;
  data: AuctionTreeItemData;
  children: CommentTreeItem[];
}

export type AuctionTreeItemData = {
  itemid: string;
  seller: string;
  title: string;
  createAt: Date;
  auctionEnd: Date;
};

export interface CommentTreeItem extends TreeNode {
  key: string;
  data: CommentItemData;
}

export type CommentItemData = {
  createAt: Date;
  author: string;
  comment: string;
};
