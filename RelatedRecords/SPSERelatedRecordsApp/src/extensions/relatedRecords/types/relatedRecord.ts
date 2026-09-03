/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved.
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
import { EMessageType } from "../constants/relatedRecord";

interface IRelatedRecord {
  ID: string
  UniqueId: string;
  IsDocument: boolean;
  FileName?: string;
  Title?: string;
  Path?: string;
  FileExtension: string;
  ListId?: string;
  WebId?: string;
  WebUrl?: string;
  SiteId?: string;
  ListItemID: string;
  SPSiteUrl: string;
  SourceFlag: number;
  DocumentId?: string;
  NodeType?: number;
}

interface IRelatedRecordSearchResult {
  Datas: IRelatedRecord[],
  TotalPage: number,
  TokenIndex: string;
}

interface IRelatedRecordItem {
  key: string;
  name: string;
  path: string;
  iconExtension: string;
  listId: string;
  webId: string;
  uniqueId: string;
  listItemId: number;
  sourceFlag: number;
  siteUrl: string;
  siteId: string;
  nodeType?: number;
  needDelete?: boolean;
  isRelatedRecord: boolean;
  documentId?: string;
}

interface IRelatedInfo {
  Name: string;
  ListId: string;
  WebId: string;
  UniqueId: string;
  ListItemId: number;
  SiteUrl: string;
  SiteId: string;
  NeedDelete: boolean;
  RecordId?: string;  
}

interface ISaveRelatedRecord {
  CurrentInfo: IRelatedInfo;
  RelatedInfos: IRelatedInfo[];
}

interface IRelatedDetailArgs {
  ListId?: string;
  WebId?: string;
  UniqueId: string;
  ListItemId?: number;
  SiteUrl?: string;
  SiteId?: string;
  SourceFlag: number;
}

interface IRecordDetail {
  LeafName: string;
  FullPath: string;
  RecordId: string;
  Term: string;
  RuleName: string;
  DisposalAction: string;
  DisposalDate: string;
  HoldStatus: boolean;
  HoldSetting: {
    Name: string | null;
    Description: string | null;
  };
  HoldBy: string;
  HoldReleaseTime: string;
  DeclaredAsRecord: boolean;
}

interface IRecordSaved {
  ErrorMessage: string;
  MessageType: EMessageType;
}

export {
  IRelatedRecord,
  IRelatedRecordSearchResult,
  IRelatedRecordItem,
  IRelatedInfo,
  ISaveRelatedRecord,
  IRelatedDetailArgs,
  IRecordDetail,
  IRecordSaved,
};
