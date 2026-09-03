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

using ExchangeCommonWrapper;
using ExchangeUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExchangeBackupUtility.Graph;

public interface IExchangeFolder
{
    ChangeStatus ChangeStatus { get; }
    int ChildFolderCount { get; }
    string DisplayFolderPath { get; set; }
    string FolderId { get; }
    string FolderName { get; }
    string FolderSyncState { get; }
    string FolderType { get; }
    bool IncludePermission { get; }
    string InternalFolderPath { get; set; }
    bool IsExcluded { get; set; }
    int ItemsCount { get; }
    string ItemSyncState { get; }
    ExchangeMailbox Mailbox { get; }
    int NameEnumerator { get; set; }
    string ParentFolderId { get; }
    bool IsNestleCustomize { get; set; }
    string MailBoxId { get; }
    bool IsRootFolder { get; }
    string ImpersonateId { get; }

    string ConvertHexEntryId();
    void GenerateCurrentSyncState();
    void GenerateCurrentItemSyncState();
    List<IExchangeFolder> GetAllSubFolders();
    List<IExchangeFolder> GetAllSubFoldersDeep();
    IExchangeItem GetItemById(string itemId);
    bool SyncDeleteItems(int pageSize, ref string syncState, HashSet<string> ignoredItemIds, out List<string> deleteItemIds);
    FolderPermissionCollectionM GetFolderPermissions();
    List<IExchangeFolder> GetInboxAndCalendarFolder();
    (List<IExchangeItem>, List<FailedItemEntity>) GetItemsByIds(List<FailedItemEntity> failedItems);
    bool HasItemChange(string syncState);
    void Open();
    List<string> SyncFolderHierarchy(string syncState, out List<string> deleteFolderIds, out List<string> updateFolderIds);
    bool SyncItems(int pageSize, ref string syncState, HashSet<string> ignoredItemIds, out List<IExchangeItem> items, out List<string> deleteItemIds, SyncItemsOptions options = null);
    IAuthObject GetCredential();

    List<IExchangeItem> FindItems(int pageSize, int offset, out bool moreAvailable, SearchFilter searchFilter = null); 
    Task<List<IExchangeItem>> GetAllItemsUnderFolder();

    Dictionary<string, Guid> GetRetentionLabelDic();
}