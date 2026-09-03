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
import { IFileTypeIconOptions } from "@fluentui/react-file-type-icons"
import { MessageType } from "../constants/related-record";

export interface IRelatedRecordItem {
    key: string;
    name: string;
    path: string;
    iconOptions: IFileTypeIconOptions;
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
};

export interface IRelatedInfo {
    Name: string;
    ListId: string;
    WebId: string;
    UniqueId: string;
    ListItemId: number;
    SiteUrl: string;
    SiteId: string;
    NeedDelete: boolean;
};

export interface ISaveRelatedRecord {
    CurrentInfo: IRelatedInfo;
    RelatedInfos: IRelatedInfo[];
};

export interface IRelatedDetailArgs {
    ListId?: string;
    WebId?: string;
    UniqueId: string;
    ListItemId?: number;
    SiteUrl?: string;
    SiteId?: string;
    SourceFlag: number;
};

export interface IRecordDetail {
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
};

export interface IRecordSaved {
    ErrorMessage: string;
    MessageType: MessageType;
}