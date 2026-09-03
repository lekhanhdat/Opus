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
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.CommonUtil;
using ExchangeCommonWrapper;
using ExchangeUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph;
using Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.Folders.Item.Items.Delta;
using Microsoft.Graph.Beta.Models;
using Microsoft365.Graph.Extensions;
using Microsoft365.Graph.Service;
using Microsoft365.Graph.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using WellKnownFolderName = Microsoft365.Graph.Extensions.WellKnownFolderName;

using GraphV1MailboxItem = Microsoft.Graph.Models.MailboxItem;
using GraphV1ItemsDelta = Microsoft.Graph.Admin.Exchange.Mailboxes.Item.Folders.Item.Items.Delta;

namespace ExchangeBackupUtility.Graph;

public class ExchangeGraphFolder : IExchangeFolder
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(ExchangeGraphFolder));
    protected volatile bool IsOpen;
    private MailboxFolder currentFolder;
    private string mailboxId;
    private static readonly bool useImmutableId = false;
    private static readonly ItemDeltaComparer itemDeltaComparer = new();
    private string _folderRestId => FolderId.ToRestId();

    #region properties

    public ChangeStatus ChangeStatus { get; private set; }

    public int ChildFolderCount { get; private set; }

    public string DisplayFolderPath { get; set; }

    public string FolderId { get; private set; }

    public string FolderName { get; private set; }

    public string FolderSyncState { get; private set; }

    public string FolderType { get; private set; }
    
    public string MailBoxId => mailboxId;
    
    public bool IsRootFolder { get; set; }
    
    public string ImpersonateId {
        get
        {
            return GlobalExchangeSetting.GetImpersonateIdByMailbox(Mailbox.OriginalMailboxAddress);
        }
    }

    public bool IncludePermission => ExchangeGlobalConfig.IncludeFolderPermission;

    public string InternalFolderPath { get; set; }
    public bool IsExcluded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int ItemsCount { get; private set; }

    public string ItemSyncState { get; private set; }

    public ExchangeMailbox Mailbox { get; }

    public int NameEnumerator { get; set; }

    public string ParentFolderId { get; private set; }

    public GraphService Service { get; }
    public IAuthObject AuthObj { get; }
    public bool IsNestleCustomize { get; set; }

    #endregion

    public ExchangeGraphFolder(ExchangeMailbox mailbox, string folderId, GraphService service)
    {
        Mailbox = mailbox;
        FolderId = folderId.ToEwsId();
        Service = service;
    }

    public ExchangeGraphFolder(ExchangeMailbox mailbox, string folderId, IAuthObject authObj, GraphService service)
        : this(mailbox, folderId, service)
    {
        AuthObj = authObj;
    }

    private ExchangeGraphFolder(ExchangeMailbox mailbox, string mailboxId, MailboxFolder folder, ChangeStatus changeStatus, IAuthObject authObj, GraphService service, string parentFolderPath = "")
        : this(mailbox, folder.Id, authObj, service)
    {
        this.mailboxId = mailboxId;
        currentFolder = folder;
        GenerateFolderInfo(folder, changeStatus);

        DisplayFolderPath = string.IsNullOrEmpty(parentFolderPath) ? $"{ExchangeConstants.PathCombine}{folder.DisplayName}" : $"{parentFolderPath}{ExchangeConstants.PathCombine}{folder.DisplayName}";
        InternalFolderPath = $"{this.InternalFolderPath}{ExchangeConstants.PathParser}{EncodeFolderName(folder.DisplayName)}";
    }

    public void GenerateCurrentSyncState()
    {
        if (Mailbox.IsPublicFolder)
        {
            logger.Info("Skip GenerateCurrentSyncState for public folder.");
            this.FolderSyncState = string.Empty;
            return;
        }
        string currentSyncState = string.Empty;
        try
        {
            logger.Info("Generate current sync state start.");
            _ = Service.Mails.ExportImport.DeltaFoldersAsync(mailboxId, ExchangeGraphUtil.ToRestId(currentFolder.Id), currentSyncState, default, CallBackAsync, default)
                .ToListAsync()
                .ExecuteAsyncTask();

            logger.Info("Generate current sync state finish.");
        }
        catch (Exception ex)
        {
            logger.Warn("Get folder syncstate with exception, reason: {0}", ex);
        }
        this.FolderSyncState = currentSyncState;

        async Task CallBackAsync((string Nextlink, string Deltalink, PagingState State) info)
        {
            currentSyncState = info.Deltalink;
            await Task.CompletedTask;
        }
    }

    public void GenerateCurrentItemSyncState()
    {
        if (this.Mailbox.IsPublicFolder)
        {
            logger.Info("Skip GenerateCurrentItemSyncState for public folder.");
            this.ItemSyncState = string.Empty;
            return;
        }

        string currentSyncState = string.Empty;
        try
        {
            logger.Info("Generate current sync state start.");
            _ = Service.Mails.DeltaItemsAsync(this.mailboxId, _folderRestId, currentSyncState, CallBackAsync)
                .ToListAsync()
                .ExecuteAsyncTask();

            logger.Info("Generate current sync state finish.");
        }
        catch (Exception ex)
        {
            logger.Warn("Get folder syncstate with exception, reason: {0}", ex);
        }
        this.ItemSyncState = currentSyncState;

        async Task CallBackAsync((string Nextlink, string Deltalink, PagingState State) info)
        {
            currentSyncState = info.Deltalink;
            await Task.CompletedTask;
        }
    }

    public List<IExchangeFolder> GetAllSubFolders()
    {
        return GetAllSubFoldersAsync().ExecuteAsyncTask();
    }

    public async Task<List<IExchangeFolder>> GetAllSubFoldersAsync(CancellationToken cancellationToken = default)
    {
        if (IsOpen && ChildFolderCount == 0) return new List<IExchangeFolder>();

        var folders = new List<IExchangeFolder>();
        await foreach (var folder in Service.Mails.ExportImport
            .ListChildFoldersAsync(currentFolder.MailboxId(), currentFolder.Id, default, cancellationToken))
        {
            if (IsExcludeByFolderClass(folder))
            {
                continue;
            }

            folders.Add(AssemblyExchangeFolder(folder));
        }
        folders.Sort(new CompareFolderName());
        return folders;

        IExchangeFolder AssemblyExchangeFolder(MailboxFolder mailboxFolder)
        {
            return new ExchangeGraphFolder(Mailbox, mailboxId, mailboxFolder, ChangeStatus.Create, AuthObj, Service, this.DisplayFolderPath);
        }
    }

    public List<IExchangeFolder> GetAllSubFoldersDeep()
    {
        if (IsOpen && ChildFolderCount == 0) return new List<IExchangeFolder>();

        var result = new List<IExchangeFolder>();
        var subFolders = this.GetAllSubFoldersAsync().ExecuteAsyncTask();

        foreach (var folder in subFolders)
        {
            result.Add(folder);
            var childFolders = folder.GetAllSubFoldersDeep();
            result.AddRange(childFolders);
        }

        return result;
    }

    public IExchangeItem GetItemById(string itemId)
    {
        var item = Service.Mails.ExportImport.GetItemAsync(currentFolder.MailboxId(), _folderRestId, itemId.ToRestId()).ExecuteAsyncTask();
        return ConvertToExchangeItem(item);
    }

    public bool SyncDeleteItems(int pageSize, ref string syncState, HashSet<string> ignoredItemIds, out List<string> deleteItemIds)
    {
        deleteItemIds = new List<string>();
        var tempSyncState = syncState;//ref 参数不能再匿名委托中

        var changeCollection = Service.Mails.ExportImport.DeltaItemsAsync(currentFolder.MailboxId(), currentFolder.Id, tempSyncState, pageSize, useImmutableId, default).ExecuteAsyncTask();
        if (changeCollection == null) return false;

        deleteItemIds = changeCollection.Value.Where(item => item.IsDeleted()).Select(c => c.Id.ToEwsId()).ToList();

        syncState = changeCollection.OdataNextLink ?? changeCollection.OdataDeltaLink;
        this.ItemSyncState = syncState;
        return !string.IsNullOrEmpty(changeCollection.OdataNextLink);
    }

    private bool IsExcludeByFolderClass(MailboxFolder folder)
    {
        var lowerCaseFolderClass = (folder.Type ?? string.Empty).ToLowerInvariant();
        switch (lowerCaseFolderClass)
        {
            case "ipf.contact.galcontacts":
            case "ipf.contact.recipientcache":
            case "ipf.contact.moc.imcontactlist":
            case "ipf.contact.moc.quickcontacts":
            case "ipf.files"://Files folder
            case "ipf.webextension"://WebExtAddins
                return true;
            default:
                break;
        }
        if (lowerCaseFolderClass.StartsWith("ipf.configuration", StringComparison.Ordinal) ||
            lowerCaseFolderClass.StartsWith("ipf.note.socialconnector.feeditems", StringComparison.Ordinal))
        {
            return true;
        }
        //skip process contact type folder and contact folder file.
        if (folder.Type.EqualsIgnoreCase("IPF.Contact"))
        {
            //Add log here to output contact folder name if need.
            logger.Info($"Current folder:{folder.DisplayName} is ContactsFolder and skip process.");
            return true;
        }
        return false;
    }

    #region filter

    public List<IExchangeItem> FindItems(int pageSize, int offset, out bool moreAvailable, SearchFilter searchFilter = null)
    {
        logger.Info($"Items PageSize: {pageSize}, offset: {offset}.");
        if (pageSize < 1 || pageSize > 1000)
        {
            throw new ArgumentOutOfRangeException($"PageSize {pageSize} is invalid. It must be within the range of 1 and 1000.");
        }

        MailboxItemCollectionResponse collectionResponse = null;
        if (searchFilter == null)
        {
            collectionResponse = Service.Mails.GetItemsByFolderId(mailboxId, _folderRestId, pageSize, offset, select: ["id"]).ExecuteAsyncTask();
        }
        else
        {
            collectionResponse = Service.Mails.GetItemsByFolderId(mailboxId, _folderRestId, pageSize, offset, select: ["id"], filter: searchFilter.ToGraphFilter()).ExecuteAsyncTask();
        }

        logger.Info($"Get items count: {collectionResponse?.Value?.Count}.");
        var result = new List<IExchangeItem>();
        if (collectionResponse.Value.Count > 0)
        {
            var itemIds = collectionResponse.Value.Select(i => i.Id);

            // Get list item cannot get sensitive labels
            // So, we have to get each item data in batch
            var itemList = Service.Mails.BatchGetItemsInfo(mailboxId, _folderRestId, itemIds, GraphCommonUtil.CommonExtendedProperties.ToGraphSingleValueExpandString()).ExecuteAsyncTask();
            result.AddRange(itemList.Select(i => ConvertToExchangeItem(i)));
        }
        moreAvailable = collectionResponse.OdataNextLink != null;

        // https://learn.microsoft.com/en-us/office/vba/outlook/concepts/forms/item-types-and-message-classes
        if (IsNestleCustomize)
        {
            result = result
                .Where(i => i.ItemType == ExchangeConstants.ItemType.Message)
                .OrderBy(i => i, new CompareItemModifyTime())
                .ToList();
        }
        else
        {
            result = result
                .Where(i => i.ItemType != ExchangeConstants.ItemType.MeetingRequest || i.ItemType == ExchangeConstants.ItemType.Contact || i.ItemType == ExchangeConstants.ItemType.DistList)
                .OrderBy(i => i, new CompareItemModifyTime())
                .ToList();
        }

        return result;
    }
    //This method will not return sensitive label, need get sensitive label through item
    public async Task<List<IExchangeItem>> GetAllItemsUnderFolder()
    {
        var folders = new List<IExchangeItem>();
        await foreach (var item in Service.Mails
            .GetAllItemAsync(currentFolder.MailboxId(), _folderRestId, GraphCommonUtil.CommonExtendedProperties.ToGraphSingleValueExpandString()))
        {
            folders.Add(ConvertToExchangeItem(item));
        }

        logger.Info($"Get all items count: {folders.Count}.");
        return folders;
    }

    #endregion

    public IAuthObject GetCredential() => AuthObj;

    public (List<IExchangeItem>, List<FailedItemEntity>) GetItemsByIds(List<FailedItemEntity> failedItems)
    {
        return GetItemsByIdsAsync(failedItems).ExecuteAsyncTask();
    }

    public async Task<(List<IExchangeItem>, List<FailedItemEntity>)> GetItemsByIdsAsync(List<FailedItemEntity> failedItems)
    {
        var responseErrorItems = new List<FailedItemEntity>();
        var successfulItems = new List<IExchangeItem>();

        var failedItemsDic = failedItems.DistinctBy(i => i.Id).ToDictionary(i => i.Id.ToRestId());

        foreach (var itemBlock in failedItems.Batch(20))
        {
            try
            {

                var items = Service.Users.BatchGetItemsInfo(mailboxId, itemBlock.Select(i => i.Id), default);

                var folders = Service.Mails.ExportImport.DeltaFoldersAsync(mailboxId, default);

                var parentFolderLookup = new Dictionary<string, MailboxFolder>(StringComparer.Ordinal);
                await foreach (var folder in folders)
                {
                    if (!string.IsNullOrEmpty(folder?.Id))
                    {
                        parentFolderLookup[folder.Id] = folder;
                    }
                }

                var itemScope = new List<(string, MailboxFolder)>();

                await foreach (var item in items)
                {
                    if (item.Error != null ||
                        item.Item == null ||
                        string.IsNullOrEmpty(item.Item.ParentFolderId))
                    {
                        logger.Info("Failed item details when BatchGetItemsInfo, item id: {0}, item status: {1}.", item.Id, item.Error?.ResponseStatusCode);
                        continue;
                    }

                    if (parentFolderLookup.TryGetValue(item.Item.ParentFolderId, out var folder))
                    {
                        itemScope.Add((item.Id.ToRestId(), folder));
                    }
                }
                var collections = Service.Mails.ExportImport.BatchGetItemAsync(itemScope, default);
                await foreach (var i in collections)
                {
                    logger.Info("Failed item details, item id: {0}, item status: {1}.", i.Id, i.Error?.ResponseStatusCode);
                    if (i.Error != null)
                    {
                        if (i.Error.ResponseStatusCode != 404)
                        {
                            // Bind失败的不参与FailedCount计数，也不打Report
                            responseErrorItems.Add(failedItemsDic[i.Id]);
                        }
                    }
                    else
                    {
                        successfulItems.Add(ConvertToExchangeItem(i.Item, ChangeStatus.Create, failedItemsDic[i.Id].FailedCount));
                    }
                }
            }
            catch (Exception ex)
            {
                responseErrorItems.AddRange(itemBlock);
                logger.Warn("Get items exception, reason: {0}.", ex);
            }
        }

        return (successfulItems.OrderBy(itemArg => itemArg, new CompareItemModifyTime()).ToList(), responseErrorItems);
    }

    private IExchangeItem ConvertToExchangeItem(MailboxItem item, ChangeStatus status = ChangeStatus.Create, int failedCount = 0)
    {
        var exchangeItem = new ExchangeGraphItem(Service, currentFolder.MailboxId(), item, currentFolder);
        exchangeItem.ItemPath = $"{DisplayFolderPath}{ExchangeConstants.PathCombine}{exchangeItem.ItemName}";
        exchangeItem.ItemInternalPath = $"{InternalFolderPath}{ExchangeConstants.PathParser}{exchangeItem.ExchangeId}";
        exchangeItem.FailedCount = failedCount;
        exchangeItem.MailBoxObjectId = Mailbox.ObjectId;
        return exchangeItem;
    }

    public bool HasItemChange(string syncState)
    {
        return HasItemChangeAsync(syncState).ExecuteAsyncTask();
    }

    public async Task<bool> HasItemChangeAsync(string syncState)
    {
        var changes = await Service.Mails.ExportImport.DeltaItemsAsync(currentFolder.MailboxId(), currentFolder.Id, syncState, 1, useImmutableId: useImmutableId, default);
        if (changes == null) return false;

        // optimize: The "Get Channel Message Increment" api once had a situation where the first page was empty but the next page had a value. If the same problem occurs when using this method, please consider trying the following logic.
        if (changes.Value.Count == 0 && !String.IsNullOrEmpty(changes.OdataNextLink))
        {
            logger.Info("[Cheack item change] First page was empty.");
            return await HasItemChangeAsync(changes.OdataNextLink);
        }
        return changes.Value.Count > 0;
    }

    public void Open()
    {
        if (string.IsNullOrEmpty(FolderId))
        {
            var exchangeSetting = Service.Users.GetExchangeSettingsAsync(Mailbox.ObjectId).ExecuteAsyncTask();
            mailboxId = Mailbox.IsArchiveMailbox
                ? exchangeSetting.InPlaceArchiveMailboxId
                : exchangeSetting.PrimaryMailboxId;

            currentFolder = Service.Mails.GetRootFolderByPrimaryBoxId(mailboxId).ExecuteAsyncTask();
        }
        else
        {
            currentFolder = Service.Mails.ExportImport.GetFolderByIdAsync(mailboxId, _folderRestId, default).ExecuteAsyncTask();
        }
        GenerateFolderInfo(currentFolder, ChangeStatus.Create);
        this.IsOpen = true;
    }

    private void GenerateFolderInfo(MailboxFolder folder, ChangeStatus changeStatus)
    {
        FolderId = folder.Id.ToEwsId();
        FolderName = folder.DisplayName;
        ChildFolderCount = folder.ChildFolderCount ?? 0;
        ItemsCount = folder.TotalItemCount ?? 0;
        FolderType = folder.Type ?? "IPF.Note";
        ChangeStatus = changeStatus;
        if (folder.AdditionalData.TryGetValue("wellKnownName", out _))
            this.NameEnumerator = folder.WellKnownFolderName() != null
                ? (int?)folder.WellKnownFolderNameEnum() ?? -1
                : -1;
        else this.NameEnumerator = -1;
        SetParentFolderId();
    }

    protected virtual void SetParentFolderId()
    {
        this.ParentFolderId = currentFolder.ParentFolderId.ToString();
    }

    public List<string> SyncFolderHierarchy(string syncState, out List<string> deleteFolderIds, out List<string> updateFolderIds)
    {
        var isFristSync = String.IsNullOrEmpty(syncState);
        deleteFolderIds = new List<string>();
        updateFolderIds = new List<string>();
        List<string> findResults = new List<string>();
        try
        {
            var folderCollection = Service.Mails.ExportImport.DeltaFoldersAsync(currentFolder.MailboxId(), syncState, default, Callback, default);

            var folders = folderCollection.ToListAsync().ExecuteAsyncTask();
            foreach (MailboxFolder f in folders)
            {
                if (f.IsDeleted())
                {
                    deleteFolderIds.Add(f.Id);
                }
                else if (isFristSync)
                {
                    findResults.Add(f.Id);
                }
                else
                {
                    updateFolderIds.Add(f.Id);
                }
            }
            this.FolderSyncState = syncState;
        }
        catch (Exception e)
        {
            logger.Warn(string.Format("Sync subfolder with exception, reason: {0}", e.ToString()));
        }

        return findResults;

        async Task Callback((string Nextlink, string Deltalink, PagingState State) tuple)
        {
            syncState = tuple.Deltalink;

            await Task.CompletedTask;
        }
    }

    public bool SyncItems(int pageSize, ref string syncState, HashSet<string> ignoredItemIds, out List<IExchangeItem> items, out List<string> deleteItemIds, SyncItemsOptions options = null)
    {
        deleteItemIds = new List<string>();
        items = new List<IExchangeItem>();

        var tempSyncState = syncState;//ref 参数不能再匿名委托中
        var tempPageSize = pageSize;

        var watch = Stopwatch.StartNew();

        var changeCollection = Service.Mails.DeltaItemsAsync(currentFolder.MailboxId(), currentFolder.Id, tempSyncState, tempPageSize, useImmutableId, default).ExecuteAsyncTask();
        watch.Stop();
        logger.Info($"Diagnostic sync folder items time cost: {watch.Elapsed}.");

        if (changeCollection == null) return false;

        HandleSyncItemsResult(changeCollection, ignoredItemIds, items, out deleteItemIds);
        syncState = changeCollection.OdataNextLink ?? changeCollection.OdataDeltaLink;
        this.ItemSyncState = syncState;
        return !string.IsNullOrEmpty(changeCollection.OdataNextLink);
    }

    private void HandleSyncItemsResult(GraphV1ItemsDelta.DeltaGetResponse changeCollection, HashSet<string> ignoredItemIds, List<IExchangeItem> items, out List<string> deleteItemIds)
    {
        var watch = Stopwatch.StartNew();
        var stepOneTime = new TimeSpan();
        var stepTwoTime = new TimeSpan();
        var stepThreeTime = new TimeSpan();
        var stepFourTime = new TimeSpan();

        deleteItemIds = changeCollection.Value.Where(c => c.IsDeleted()).Select(c => c.Id.ToEwsId()).ToList();

        var changeItems = changeCollection.Value
            .Where(i => (!i.IsDeleted()) && (!ignoredItemIds.Contains(i.Id)) && (i.Type != "IPM.Schedule.Meeting.Request" || i.Type == "IPM.Contact" || i.Type == "IPM.DistList"))
            .Distinct(itemDeltaComparer);
        var changeItemsCount = changeItems.Count();
        if (changeCollection.Value.Count != changeItemsCount)
        {
            logger.Info("Sync items count, before remove: {0}, after remove: {1}", changeCollection.Value.Count, changeItemsCount);
        }
        stepOneTime = watch.Elapsed;
        watch.Restart();

        var needProcessItems = changeItems.Where(item => item.Type == ExchangeConstants.ItemType.Message).ToList();
        
        const int batchSize = 20;
        foreach (var itemBatch in needProcessItems.Batch(batchSize))
        {
            try
            {
                var itemScope = itemBatch.Select(ci => (ci.Id, currentFolder)).ToList();
                var batchResponses = Service.Mails.ExportImport.BatchGetItemAsync(itemScope, default)
                    .ToListAsync()
                    .ExecuteAsyncTask();

                foreach (var response in batchResponses)
                {
                    if (response.Error != null)
                    {
                        if (response.Error.ResponseStatusCode == 404)
                        {
                            logger.Warn($"Current item not found, id: {response.Id}");
                        }
                        else
                        {
                            logger.Error($"Get item failed, id: {response.Id}, error {response.Error}");
                        }
                        continue;
                    }

                    items.Add(ConvertToExchangeItem(response.Item));
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Batch get items failed, error {ex}");
            }
        }

        stepFourTime = watch.Elapsed;
        watch.Stop();
        logger.Info($"Diagnostic handle items time cost: {stepOneTime},{stepTwoTime},{stepThreeTime},{stepFourTime}.");
    }



    private class ItemDeltaComparer : IEqualityComparer<GraphV1MailboxItem>
    {
        public bool Equals(GraphV1MailboxItem x, GraphV1MailboxItem y)
        {
            return string.Equals(x?.Id, y?.Id, StringComparison.Ordinal);
        }

        public int GetHashCode(GraphV1MailboxItem obj)
        {
            return obj?.Id?.GetHashCode() ?? 0;
        }
    }

    #region Group/Teams mailbox only
    public List<IExchangeFolder> GetInboxAndCalendarFolder()
    {
        if (this.IsOpen && this.ChildFolderCount == 0) return new List<IExchangeFolder>();
        var result = new List<ExchangeGraphFolder>();

        try
        {
            var childGraphFolder = Service.Mails.ExportImport.ListAllFoldersByMailboxIdAsync(mailboxId).ExecuteAsyncTask().Value;
            if (childGraphFolder is null)
            {
                return [];
            }

            var exGraphFolders = childGraphFolder
                    .Where(f => f.WellKnownFolderName() == WellKnownFolderName.Inbox.ConvertToString() || f.WellKnownFolderName() == WellKnownFolderName.Calendar.ConvertToString())
                    .Select(f => new ExchangeGraphFolder(this.Mailbox, mailboxId, f, ChangeStatus.Create, this.AuthObj, this.Service, this.DisplayFolderPath));
            result.AddRange(exGraphFolders);
        }
        catch (Exception ex)
        {
            logger.Warn("Begin to use bind folder to get data and the error is {0}", ex);

            var inboxGraphFolder = Service.Mails.ExportImport.GetFolderByWellKnownName(mailboxId, WellKnownFolderName.Inbox.ConvertToString()).ExecuteAsyncTask();
            var calendarGraphFolder = Service.Mails.ExportImport.GetFolderByWellKnownName(mailboxId, WellKnownFolderName.Calendar.ConvertToString()).ExecuteAsyncTask();

            result.Add(new ExchangeGraphFolder(this.Mailbox, mailboxId, inboxGraphFolder, ChangeStatus.Create, this.AuthObj, this.Service, this.DisplayFolderPath));
            result.Add(new ExchangeGraphFolder(this.Mailbox, mailboxId, calendarGraphFolder, ChangeStatus.Create, this.AuthObj, this.Service, this.DisplayFolderPath));
        }

        result.Sort(new CompareFolderName());
        return result.ConvertAll<IExchangeFolder>(f => f);
    }

    private static string EncodeFolderName(string name)
    {
        return name;
    }
    #endregion

    #region Public folder metadata only
    public string ConvertHexEntryId()
    {
        var convertResultList = Service.Users.ConvertExchangeIds(Mailbox.ObjectId, ExchangeIdFormat.EwsId, ExchangeIdFormat.EntryId, _folderRestId).ExecuteAsyncTask().Value;

        if (convertResultList is null || convertResultList.Count == 0)
        {
            logger.Error("ConvertHexEntryId failed, folderId: {0}", FolderId);
            return string.Empty;
        }

        var convertResult = convertResultList.FirstOrDefault(r => r.SourceId == FolderId);
        if (convertResult is null || convertResult.ErrorDetails is not null)
        {
            logger.Error("ConvertHexEntryId failed, folderId: {0}", FolderId);
            if (convertResult?.ErrorDetails != null)
            {
                logger.Error("ConvertHexEntryId error details: {0}", convertResult.ErrorDetails.Message);
            }
            return string.Empty;
        }

        string idForPowerShell = convertResult.TargetId;
        return idForPowerShell;
    }
    #endregion

    #region public folder only
    public FolderPermissionCollectionM GetFolderPermissions()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region TagLabel
    public Dictionary<string, Guid> GetRetentionLabelDic()
    {
        var result = new Dictionary<string, Guid>();
        try
        {
            var response = Service.Security.GetRetentionLabelsAsync().ExecuteAsyncTask();
            result = response.Value.Where(r => r.IsInUse == true).ToDictionary(r => r.DisplayName, v => Guid.Parse(v.Id));
            return result;
        }
        catch (Exception ex)
        {
            logger.Error("Get Retention Label Dictionary failed. Exception: " + ex.ToString());
            throw;
        }
    }
    #endregion
}
