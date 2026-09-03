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



namespace ExchangeBackupUtility
{
    using AvePoint.RA.Common.Util;
    #region directory
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Exceptions;
    using ExchangeBackupUtility.Graph;
    using ExchangeCommonWrapper;
    using ExchangeUtility;
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using GraphChangeStatus = ExchangeUtility.Graph.ChangeStatus;
    using GraphUtility = ExchangeUtility.Graph;
    #endregion

    public class ExchangeFolder : ExchangeObjectBase, IExchangeFolder
    {
        protected static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private volatile bool isOpen;
        private Folder currentFolder;
        public bool IsNestleCustomize;
        internal ExchangeService service;
        protected FolderId inputFolderId;
        protected bool isRootFolder;
        protected ExchangeMailbox mailbox;
        public ExchangeMailbox Mailbox { get { return mailbox; } }
        public bool IsRootFolder { get { return isRootFolder; } }
        private Dictionary<Guid, string> mLabelIdNameMapping;

        #region Properties
        public string FolderName { get; private set; }
        public string FolderId { get; private set; }
        public string FolderType { get; private set; }
        public string DisplayFolderPath { get; set; }
        public string InternalFolderPath { get; set; }
        public int ChildFolderCount { get; private set; }
        public int ItemsCount { get; private set; }
        public string ItemSyncState { get; private set; }
        public string FolderSyncState { get; private set; }
        public ChangeStatus ChangeStatus { get; private set; }
        public string ParentFolderId { get; private set; }
        public Dictionary<Guid, string> LabelIdNameMapping
        {
            get
            {
                if (mLabelIdNameMapping != null)
                {
                    return mLabelIdNameMapping;
                }
                else
                {
                    var result = new Dictionary<Guid, string>();
                    try
                    {
                        result = GetRetentionTags(service);
                        mLabelIdNameMapping = result;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Get Retention Label Id Name Mapping failed. Exception: " + ex.ToString());
                        throw;
                    }
                }
            }
        }

        private Dictionary<Guid, string> GetRetentionTags(ExchangeService service, int retryCount = 0)
        {
            try
            {
                var tags = service.GetUserRetentionPolicyTags().GetAwaiter().GetResult();
                return tags.RetentionPolicyTags.Where(r => !r.IsArchive && r.IsVisible).ToDictionary(r => r.RetentionId, v => v.DisplayName);
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while GetUserRetentionPolicyTags. ERROR:{e.ToString()}, retry count {retryCount}");
                if (retryCount < 3)
                {
                    return GetRetentionTags(service, ++retryCount);
                }
                else
                {
                    throw e;
                }
            }
        }

        private DateTime modified;
        public DateTime Modified
        {
            get
            {
                GetModifyData();
                return modified;
            }
        }

        private void GetModifyData()
        {
            try
            {
                PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties);
                ExtendedPropertyDefinition modifiedTime = new ExtendedPropertyDefinition(0x3008, MapiPropertyType.SystemTime);
                set.Add(modifiedTime);
                Folder tempFolder = Folder.Bind(service, this.FolderId, set).GetAwaiter().GetResult();
                if (tempFolder.ExtendedProperties != null)
                {
                    foreach (ExtendedProperty extend in tempFolder.ExtendedProperties)
                    {
                        if (extend.PropertyDefinition.Tag == 0x3008)
                        {
                            modified = (DateTime)extend.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Get folder modified date with exception : {0}", ex.ToString());
                modified = DateTime.Now;
            }
        }
        /// <summary>
        /// 表示该Folder是否是被ExchangeOnline Restore回来的
        /// </summary>
        public bool IsRestored { get; private set; }

        public bool NeedFullBackup { get; set; }
        public string ImpersonateId
        {
            get
            {
                return GlobalExchangeSetting.GetImpersonateIdByMailbox(this.mailbox.OriginalMailboxAddress);
            }
        }

        GraphChangeStatus IExchangeFolder.ChangeStatus => Enum.Parse<GraphChangeStatus>(this.ChangeStatus.ToString());

        public bool IncludePermission { get { return ExchangeGlobalConfig.IncludeFolderPermission; } }

        public bool IsExcluded { get; set; }

        GraphUtility.ExchangeMailbox IExchangeFolder.Mailbox => new(this.Mailbox.MailboxAddress, Enum.Parse<GraphUtility.ExchangeMailboxType>(this.Mailbox.MailboxType.ToString()));

        public int NameEnumerator { get; set; }
        bool IExchangeFolder.IsNestleCustomize { get => this.IsNestleCustomize; set => this.IsNestleCustomize = value; }

        public string MailBoxId { get; set; }

        #endregion

        #region Constructor
        public ExchangeFolder(ExchangeMailbox mailbox, string folderId, AuthObject authObj)
            : base(authObj)
        {
            this.mailbox = mailbox;
            this.service = CreateExchangeService();
            SetServiceUrl(this.service, mailbox.OriginalMailboxAddress);
            SetFolderId(mailbox, folderId);
            AddImpersonationHeader(mailbox);
            SetImpersonateId(ImpersonateId);
        }

        public ExchangeFolder(ExchangeMailbox mailbox, string folderId, ExchangeFolder parentFolder)
            : this(mailbox, folderId, parentFolder.AuthObject)
        { }

        private ExchangeFolder(ExchangeMailbox mailbox, Folder folder, ChangeType changeType, AuthObject authObj)
            : base(authObj)
        {
            this.mailbox = mailbox;
            this.service = folder.Service;
            this.currentFolder = folder;
            GenerateFolderInfo(folder, changeType.ToChangeStatus());
            this.isOpen = true;
        }
        #endregion

        #region Public Method
        public ExchangeService GetService()
        {
            return this.service;
        }
        /// <summary>
        /// Get all sub folders under current folder
        /// </summary>
        /// <returns></returns>
        public List<ExchangeFolder> GetAllSubFolders()
        {
            if (this.isOpen && this.ChildFolderCount == 0) return new List<ExchangeFolder>();//return if no sub folder to improve performance

            const int pageSize = 50;
            int offset = 0;
            var findResults = new List<ExchangeFolder>();
            FindFoldersResults result;
            do
            {
                //TODO: research GRAPH API to replace
                result = currentFolder.FindFolders(new FolderView(pageSize, offset) { Traversal = FolderTraversal.Shallow, }).GetAwaiter().GetResult();
                findResults.AddRange(AssemblyExchangeFolder(result));
                offset = result.NextPageOffset.HasValue ? result.NextPageOffset.Value : offset;
            }
            while (result.MoreAvailable);
            findResults.Sort(new CompareFolderName());
            return findResults;
        }

        public List<ExchangeFolder> GetAllSubFoldersDeep()
        {
            if (this.isOpen && this.ChildFolderCount == 0) return new List<ExchangeFolder>();//return if no sub folder to improve performance

            const int pageSize = 50;
            int offset = 0;
            var findResults = new List<ExchangeFolder>();
            FindFoldersResults result;
            do
            {
                result = currentFolder.FindFolders(new FolderView(pageSize, offset) { Traversal = FolderTraversal.Deep, }).GetAwaiter().GetResult();
                findResults.AddRange(AssemblyExchangeFolder(result));
                offset = result.NextPageOffset.HasValue ? result.NextPageOffset.Value : offset;
            }
            while (result.MoreAvailable);
            findResults.Sort(new CompareFolderName());
            return findResults;
        }


        public Tuple<List<ExchangeItem>, List<string>> GetItemsByIds(List<string> itemIds)
        {
            List<ExchangeItem> items = new List<ExchangeItem>();
            List<string> notExistItemIds = new List<string>();
            foreach (var id in itemIds)
            {
                try
                {
                    Item item = Item.Bind(this.service, new ItemId(id)).GetAwaiter().GetResult();
                    items.Add(ConvertToExchangeItem(item, ChangeType.Create));
                }
                catch (Microsoft.Exchange.WebServices.Data.ServiceResponseException e)
                {
                    if (e.ErrorCode == ServiceError.ErrorItemNotFound)
                    {
                        notExistItemIds.Add(id);
                    }
                    else
                    {
                        logger.Error("Faild to get fail item with ServiceResponseException, id:{0} error:{1}", id, e.ToString());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Faild to get fail item, id:{0} error:{1}", id, ex.ToString());
                }
            }
            return new Tuple<List<ExchangeItem>, List<string>>(items, notExistItemIds);
        }
        /// <summary>
        /// Get sub folders under current folder from the specified offset.
        /// </summary>
        /// <param name="pageSize">for internal paging use, always return all folders from offset</param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ExchangeFolderFindResults GetSubFolders(int pageSize, int offset)
        {
            logger.Info("SubFolders pageSize: {0}, offset: {1}.", pageSize, offset);
            var searchFilter = BuildSearchFilter();
            var result = currentFolder.FindFolders(searchFilter, new FolderView(pageSize, offset) { Traversal = FolderTraversal.Shallow, }).GetAwaiter().GetResult();
            return new ExchangeFolderFindResults(result, this);
        }

        private SearchFilter BuildSearchFilter()
        {
            var paramters = new SearchFilter[]
            {
                new SearchFilter.IsNotEqualTo(FolderSchema.FolderClass,"IPF.Contact.GalContacts"),
                new SearchFilter.IsNotEqualTo(FolderSchema.FolderClass,"IPF.Contact.RecipientCache"),
                new SearchFilter.IsNotEqualTo(FolderSchema.FolderClass,"IPF.Contact.MOC.ImContactList"),
                new SearchFilter.IsNotEqualTo(FolderSchema.FolderClass,"IPF.Contact.MOC.QuickContacts"),
                new SearchFilter.Not(new SearchFilter.ContainsSubstring(FolderSchema.FolderClass,"IPF.Configuration")),
                new SearchFilter.Not(new SearchFilter.ContainsSubstring(FolderSchema.FolderClass,"IPF.Note.SocialConnector.FeedItems")),
            };
            return new SearchFilter.SearchFilterCollection(LogicalOperator.And, paramters);
        }
        public List<ACLEntry> GetFolderACL()
        {
            var propertySet = new PropertySet(BasePropertySet.IdOnly, FolderSchema.Permissions);
            var folder = Folder.Bind(service, this.FolderId, propertySet).GetAwaiter().GetResult();
            return folder.Permissions.Select(pArg => new ACLEntry()
            {
                DisplayName = pArg.UserId.DisplayName,
                UserId = pArg.UserId.PrimarySmtpAddress,
                ObjectSid = pArg.UserId.SID,
                Permissions = pArg.ToPermissionList(),
            }).ToList();
            //if (permission.UserId.DisplayName == null)
            //{
            //    entry.DisplayName = permission.UserId.StandardUser.ToString();
            //    if ("Default".Equals(entry.DisplayName))
            //    {
            //        entry.DisplayName = "Everyone";
            //        entry.UserId = @"NT AUTHORITY\Authenticated Users";
            //    }
            //    else if ("Anonymous".Equals(entry.DisplayName))
            //    {
            //        entry.UserId = @"NT AUTHORITY\Anonymous Logon";
            //    }
            //    continue;
            //}
        }
        /// <summary>
        /// Get all items under current folder.
        /// </summary>
        /// <returns></returns>
        public List<ExchangeItem> GetAllItems()
        {
            const int pageSize = 100;
            int offset = 0;
            var findResults = new List<ExchangeItem>();
            FindItemsResults<Item> result;
            do
            {
                logger.Info(string.Format("Items PageSize: {0}", pageSize));
                result = currentFolder.FindItems(new ItemView(pageSize, offset) { Traversal = ItemTraversal.Shallow, }).GetAwaiter().GetResult();
                logger.Info($"Get items count : {result.Items.Count}.");
                if (result.Items.Count > 0)
                {
                    service.LoadPropertiesForItems(result, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments)).GetAwaiter().GetResult();
                }
                findResults.AddRange(result.Select(itemArg => ConvertToExchangeItem(itemArg, ChangeType.Create)));
                if (result.NextPageOffset.HasValue)
                {
                    offset = result.NextPageOffset.Value;
                }
            }
            while (result.MoreAvailable);
            findResults.Sort(new CompareItemModifyTime());
            return findResults;
        }
        
        public Task<List<IExchangeItem>> GetAllItemsUnderFolder()
        {
            throw new NotImplementedException();
        }

        public List<ExchangeItem> FindAllItems(SearchFilter searchFilter)
        {
            const int pageSize = 100;
            int offset = 0;
            var findResults = new List<ExchangeItem>();
            FindItemsResults<Item> result;
            do
            {
                ItemView view = new ItemView(pageSize, offset);
                logger.Info(string.Format("Items PageSize: {0}", pageSize));
                result = currentFolder.FindItems(searchFilter, view).GetAwaiter().GetResult();
                logger.Info($"Get items count : {result.Items.Count}.");
                if (result.Items.Count > 0)
                {
                    service.LoadPropertiesForItems(result, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments)).GetAwaiter().GetResult();
                }
                findResults.AddRange(result.Select(itemArg => ConvertToExchangeItem(itemArg, ChangeType.Create)));
                if (result.NextPageOffset.HasValue)
                {
                    offset = result.NextPageOffset.Value;
                }
            }
            while (result.MoreAvailable);
            findResults.Sort(new CompareItemModifyTime());
            return findResults;
        }

        /// <summary>
        /// Get items under current folder, with paging.
        /// </summary>
        /// <param name="pageSize">the maximum number of element the operation should return.</param>
        /// <param name="offset">offset</param>
        /// <param name="moreAvailable">whether more items are available</param>
        /// <returns></returns>
        public List<ExchangeItem> FindItems(int pageSize, int offset, out bool moreAvailable, SearchFilter searchFilter = null)
        {
            logger.Info($"Items PageSize: {pageSize}, offset : {offset}.");
            FindItemsResults<Item> result = null;
            if (searchFilter == null)
            {
                result = currentFolder.FindItems(new ItemView(pageSize, offset) { Traversal = ItemTraversal.Shallow }).GetAwaiter().GetResult();
            }
            else
            {
                result = currentFolder.FindItems(searchFilter, new ItemView(pageSize, offset) { Traversal = ItemTraversal.Shallow }).GetAwaiter().GetResult();
            }

            logger.Info($"Get items count : {result.Items.Count}, total items count : {result.TotalCount}.");
            logger.Info($"Meeting request item count is {result.Where(itemArgs => itemArgs is MeetingRequest).Count()}.");
            if (result.Items.Count > 0)
            {
                var sensitivityLabelDef = new ExtendedPropertyDefinition(DefaultExtendedPropertySet.InternetHeaders, "msip_labels", MapiPropertyType.String);
                service.LoadPropertiesForItems(result, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments,sensitivityLabelDef)).GetAwaiter().GetResult();
            }
            moreAvailable = result.MoreAvailable;
            if (IsNestleCustomize)
            {
                return result.
                    Where(itemArgs => itemArgs is EmailMessage).
                    Select(itemArg => ConvertToExchangeItem(itemArg, ChangeType.Create)).
                    OrderBy(itemArg => itemArg, new CompareItemModifyTime()).
                    ToList();
            }
            else
            {
                return result.
                    Where(itemArgs => !(itemArgs is MeetingRequest) || itemArgs is Contact || itemArgs is ContactGroup).
                    Select(itemArg => ConvertToExchangeItem(itemArg, ChangeType.Create)).
                    OrderBy(itemArg => itemArg, new CompareItemModifyTime()).
                    ToList();
            }
        }

        /// <summary>
        /// Get item by item id
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <returns></returns>
        public ExchangeItem GetItemById(string itemId)
        {
            try
            {
                ItemId id = new ItemId(itemId);
                Item item = Item.Bind(service, id, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments)).GetAwaiter().GetResult();
                return new ExchangeItem(item, ChangeType.Create, this);
            }
            catch (Exception ex)
            {
                logger.Warn("Item {0} does not exist in the store. Reason : {1}", itemId, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Generate sync state for current folder
        /// </summary>
        public void GenerateCurrentSyncState()
        {
            if (this.mailbox.IsPublicFolder)
            {
                logger.Info("Skip GenerateCurrentSyncState for public folder.");
                this.FolderSyncState = string.Empty;
                return;
            }
            string currentSyncState = string.Empty;
            try
            {
                logger.Info("Generate current sync state start.");
                ChangeCollection<FolderChange> changeCollection;
                do
                {
                    //TODO: research GRAPH API to replace
                    changeCollection = service.SyncFolderHierarchy(currentFolder.Id, PropertySet.IdOnly, currentSyncState).GetAwaiter().GetResult();
                    currentSyncState = changeCollection.SyncState;
                }
                while (changeCollection.MoreChangesAvailable);
                logger.Info("Generate current sync state finish.");
            }
            catch (Exception ex)
            {
                logger.Warn("Get folder syncstate with exception, reason: {0}.", ex.ToString());
            }
            this.FolderSyncState = currentSyncState;
        }

        /// <summary>
        /// Generate sync state for current folder
        /// </summary>
        public void GenerateCurrentItemSyncState()
        {
            if (this.mailbox.IsPublicFolder)
            {
                logger.Info("Skip GenerateCurrentItemSyncState for public folder.");
                this.ItemSyncState = string.Empty;
                return;
            }
            string currentSyncState = string.Empty;
            var tempService = CloneExchangeService(this.service, 5);
            try
            {
                logger.Info("Generate item sync state start.");
                ChangeCollection<ItemChange> changeCollection;
                do
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        changeCollection = tempService.SyncFolderItems(currentFolder.Id, PropertySet.IdOnly, null, 512, SyncFolderItemsScope.NormalItems, currentSyncState).GetAwaiter().GetResult();
                        currentSyncState = changeCollection.SyncState;
                    }       
                }
                while (changeCollection.MoreChangesAvailable);
                logger.Info("Generate item sync state finish."); 
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                logger.Warn("Get item syncstate with exception, reason: {0}.", ex.ToString());
            }
            this.ItemSyncState = currentSyncState;
        }

        /// <summary>
        /// Open current folder which set basic info on this class.
        /// </summary>
        public void Open()
        {
            if (this.isOpen) return;
            this.currentFolder = InternalOpen();
            if (this.currentFolder == null)
            {
                logger.Error("Not find folder with uniqueId property. FieldValue :{0}. UserMailAddress: {1}. ", this.inputFolderId.UniqueId, this.mailbox.OriginalMailboxAddress);
                throw new Exception("Not find folder with uniqueId property. ");
            }
            GenerateFolderInfo(this.currentFolder, ChangeStatus.Create);
            this.isOpen = true;
        }

        protected virtual void SetParentFolderId()
        {
            this.ParentFolderId = currentFolder.ParentFolderId.ToString();
        }

        /// <summary>
        /// Synchronizes the sub-folders of a specific folder. Calling this method results in a call to EWS.
        /// </summary>
        /// <param name="syncState">The optional sync state representing the point in time when to start the synchronization.</param>
        /// <param name="deleteFolderIds">out param, list of deleted folder ids.</param>
        /// <param name="updateFolderIds">out param, list of updated folder ids.</param>
        /// <returns>list of all folder ids</returns>
        public List<string> SyncFolderHierarchy(string syncState, out List<string> deleteFolderIds, out List<string> updateFolderIds)
        {
            deleteFolderIds = new List<string>();
            updateFolderIds = new List<string>();
            List<string> findResults = new List<string>();
            try
            {
                ChangeCollection<FolderChange> changeCollection;
                do
                {
                    changeCollection = service.SyncFolderHierarchy(currentFolder.Id, PropertySet.IdOnly, syncState).GetAwaiter().GetResult();
                    foreach (FolderChange change in changeCollection)
                    {
                        if (change.ChangeType == ChangeType.Delete)
                        {
                            deleteFolderIds.Add(change.FolderId.ToString());
                        }
                        if (change.ChangeType == ChangeType.Update)
                        {
                            updateFolderIds.Add(change.FolderId.ToString());
                        }
                        findResults.Add(change.FolderId.ToString());
                    }
                    syncState = changeCollection.SyncState;
                }
                while (changeCollection.MoreChangesAvailable);
                this.FolderSyncState = syncState;
            }
            catch (Exception e)
            {
                logger.Warn(string.Format("Sync subfolder with exception, reason: {0}", e.ToString()));
            }
            return findResults;
        }
        /// <summary>
        /// Incremental并分页获取folder下的Item
        /// </summary>
        /// <param name="pageSize">每页获取Item数量上限</param>
        /// <param name="syncState">sync state token, 为null时会获取全部Item</param>
        /// <param name="items">除了delete之外的全部Item集合</param>
        /// <param name="deleteItemIds">被delete的item集合</param>
        /// <returns></returns>
        public bool SyncItems(int pageSize, ref string syncState, HashSet<string> ignoredItemIds, out List<ExchangeItem> items, out List<string> deleteItemIds)
        {
            deleteItemIds = new List<string>();
            items = new List<ExchangeItem>();

            var tempSyncState = syncState;//ref 参数不能再匿名委托中
            var sensitivityLabelDef = new ExtendedPropertyDefinition(DefaultExtendedPropertySet.InternetHeaders, "msip_labels", MapiPropertyType.String);
            var changeCollection = service.SyncFolderItems(currentFolder.Id, new PropertySet(BasePropertySet.FirstClassProperties, sensitivityLabelDef),//只获取需要的属性会提升一些性能
                                                        null, pageSize, SyncFolderItemsScope.NormalItems, tempSyncState).GetAwaiter().GetResult();
            if (changeCollection == null) return false;

            deleteItemIds = changeCollection.Where(c => c.ChangeType == ChangeType.Delete).Select(c => c.ItemId.ToString()).ToList();

            var changeItems = RemoveIgnoredItemsAndDeletedItems(changeCollection, ignoredItemIds);
            //LoadAdditionalProperties(changeItems);
            var itemIds = changeItems.Where(change => change.Item is EmailMessage).Select(change => change.ItemId).ToList();
            if (itemIds.Count > 0)
            {
                Dictionary<string, Item> mapping = new Dictionary<string, Item>();
                try
                {
                    var tempService = this.CloneExchangeService(this.service, 5);
                    var itemResponse = tempService.BindToItems(itemIds, new PropertySet(EmailMessageSchema.ToRecipients, EmailMessageSchema.Sender, ItemSchema.Attachments)).GetAwaiter().GetResult();
                    //var mapping = itemResponse.Where(r => r.Result == ServiceResult.Success).ToDictionary(r => r.Item.Id.UniqueId, r => r.Item, StringComparer.InvariantCultureIgnoreCase);
                    mapping = GetRecipientsMapping(itemResponse);
                }
                catch (Exception ex)
                {
                    //log and ingore this error.
                    logger.Warn("Failed to load additional properties while sync items, error: {0}", ex);
                }
                foreach (var change in changeItems)
                {
                    try
                    {
                        var exchangeItem = ConvertToExchangeItem(change);
                        Item item2;
                        if (mapping.TryGetValue(change.ItemId.UniqueId, out item2))
                        {
                            //LoadEmailMessageProperties(change.Item, item2);
                            var emailMessage1 = change.Item as EmailMessage;
                            var emailMessage2 = item2 as EmailMessage;
                            if (emailMessage1 != null && emailMessage2 != null)
                            {
                                Contract.Assert(emailMessage1.ToRecipients != null);
                                Contract.Assert(emailMessage2.ToRecipients != null);
                                string displayTo = string.Empty;
                                if (emailMessage2 != null && emailMessage2.ToRecipients != null && emailMessage2.ToRecipients.Count > 0)
                                {
                                    displayTo = string.Join(";", emailMessage2.ToRecipients.Select(address => address.ToFormatString()));
                                }
                                exchangeItem.DisplayTo = displayTo;
                                exchangeItem.Attachments = emailMessage2.Attachments;
                            }
                        }
                        items.Add(exchangeItem);
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Item is deleted, and no need to backup. Exception: {0}. ", ex.ToString());
                    }
                }
            }

            //foreach (ItemChange change in changeItems)
            //{
            //    try
            //    {
            //        items.Add(ConvertToExchangeItem(change));
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Info("Item is deleted, and no need to backup. Exception: {0}. ", ex.ToString());
            //    }
            //}
            syncState = changeCollection.SyncState;
            this.ItemSyncState = syncState;
            return changeCollection.MoreChangesAvailable;
        }
        private IEnumerable<ItemChange> RemoveIgnoredItemsAndDeletedItems(ChangeCollection<ItemChange> changeCollection, HashSet<string> ignoredItemIds)
        {
            var result = changeCollection.Where(c => (c.ChangeType != ChangeType.Delete || c.ChangeType != ChangeType.ReadFlagChange) && !ignoredItemIds.Contains(c.ItemId.ToString()) && (!(c.Item is MeetingRequest) || c.Item is Contact || c.Item is ContactGroup)).Distinct(new ItemChangeComparer());
            var count = result.Count();
            if (changeCollection.Count != count)
            {
                logger.Info("Sync items count, before remove: {0}, after remove: {1}", changeCollection.Count, count);
            }
            return result;
        }

        public bool SyncDeleteItems(int pageSize, ref string syncState, HashSet<string> ignoredItemIds, out List<string> deleteItemIds)
        {
            deleteItemIds = new List<string>();
            var tempSyncState = syncState;//ref 参数不能再匿名委托中
            var sensitivityLabelDef = new ExtendedPropertyDefinition(DefaultExtendedPropertySet.InternetHeaders, "msip_labels", MapiPropertyType.String);
            PropertySet properties = new PropertySet(BasePropertySet.IdOnly, sensitivityLabelDef);
            var changeCollection = this.service.SyncFolderItems(currentFolder.Id, properties,//只获取需要的属性会提升一些性能 
                                                        null, pageSize, SyncFolderItemsScope.NormalItems, tempSyncState).GetAwaiter().GetResult();
            if (changeCollection == null) return false;

            deleteItemIds = changeCollection.Where(c => c.ChangeType == ChangeType.Delete).Select(c => c.ItemId.ToString()).ToList();

            //var changeItems = RemoveIgnoredItemsAndDeletedItems(changeCollection, ignoredItemIds);
            ////LoadAdditionalProperties(changeItems);
            //var itemIds = changeItems.Where(change => change.Item is EmailMessage).Select(change => change.ItemId).ToList();
            //if (itemIds.Count > 0)
            //{
            //    Dictionary<string, Item> mapping = new Dictionary<string, Item>();
            //    try
            //    {
            //        var tempService = this.CloneExchangeService(this.service, 5);
            //        var itemResponse = tempService.BindToItems(itemIds, new PropertySet(EmailMessageSchema.ToRecipients, EmailMessageSchema.Sender, ItemSchema.Attachments));
            //        //var mapping = itemResponse.Where(r => r.Result == ServiceResult.Success).ToDictionary(r => r.Item.Id.UniqueId, r => r.Item, StringComparer.InvariantCultureIgnoreCase);
            //        mapping = GetRecipientsMapping(itemResponse);
            //    }
            //    catch (Exception ex)
            //    {
            //        //log and ingore this error.
            //        logger.Warn("Failed to load additional properties while sync items, error: {0}", ex);
            //    }
            //    foreach (var change in changeItems)
            //    {
            //        try
            //        {
            //            var exchangeItem = ConvertToExchangeItem(change);
            //            Item item2;
            //            if (mapping.TryGetValue(change.Id.UniqueId, out item2))
            //            {
            //                //LoadEmailMessageProperties(change.Item, item2);
            //                var emailMessage1 = change.Item as EmailMessage;
            //                var emailMessage2 = item2 as EmailMessage;
            //                if (emailMessage1 != null && emailMessage2 != null)
            //                {
            //                    Contract.Assert(emailMessage1.ToRecipients != null);
            //                    Contract.Assert(emailMessage2.ToRecipients != null);
            //                    string displayTo = string.Empty;
            //                    if (emailMessage2 != null && emailMessage2.ToRecipients != null && emailMessage2.ToRecipients.Count > 0)
            //                    {
            //                        displayTo = string.Join(";", emailMessage2.ToRecipients.Select(address => address.ToFormatString()));
            //                    }
            //                    exchangeItem.DisplayTo = displayTo;
            //                    exchangeItem.Attachments = emailMessage2.Attachments;
            //                }
            //            }
            //            items.Add(exchangeItem);
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.Info("Item is deleted, and no need to backup. Exception: {0}. ", ex.ToString());
            //        }
            //    }
            //}

            //foreach (ItemChange change in changeItems)
            //{
            //    try
            //    {
            //        items.Add(ConvertToExchangeItem(change));
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Info("Item is deleted, and no need to backup. Exception: {0}. ", ex.ToString());
            //    }
            //}
            syncState = changeCollection.SyncState;
            this.ItemSyncState = syncState;
            return changeCollection.MoreChangesAvailable;
        }

        #region TagLabel
        public Dictionary<string, Guid> GetRetentionLabelDic()
        {
            var result = new Dictionary<string, Guid>();
            try
            {
                var tags = service.GetUserRetentionPolicyTags().GetAwaiter().GetResult();
                result = tags.RetentionPolicyTags.Where(r => !r.IsArchive && r.IsVisible).ToDictionary(r => r.DisplayName, v => v.RetentionId);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Get Retention Label Dictionary failed. Exception: " + ex.ToString());
                throw;
            }
        }
        #endregion

        #region IExchangeFolder Method

        public string ConvertHexEntryId()
        {
            var ewsId = new AlternatePublicFolderId(IdFormat.EwsId, this.FolderId);
            var hexId = service.ConvertId(ewsId, IdFormat.HexEntryId).ExecuteAsyncTask() as AlternatePublicFolderId;
            string idForPowerShell = hexId.FolderId;
            return idForPowerShell;
        }

        List<IExchangeFolder> IExchangeFolder.GetAllSubFolders()
        {
            return this.GetAllSubFolders().ConvertAll(item => item as IExchangeFolder);
        }

        List<IExchangeFolder> IExchangeFolder.GetAllSubFoldersDeep()
        {
            return this.GetAllSubFoldersDeep().ConvertAll(item => item as IExchangeFolder);
        }

        IExchangeItem IExchangeFolder.GetItemById(string itemId)
        {
            return this.GetItemById(itemId);
        }

        public FolderPermissionCollectionM GetFolderPermissions()
        {
            throw new NotImplementedException();
        }

        public List<IExchangeFolder> GetInboxAndCalendarFolder()
        {
            throw new NotImplementedException();
        }

        public (List<IExchangeItem>, List<FailedItemEntity>) GetItemsByIds(List<FailedItemEntity> failedItems)
        {
            throw new NotImplementedException();
        }

        public bool HasItemChange(string syncState)
        {
            throw new NotImplementedException();
        }

        public bool SyncItems(int pageSize, ref string syncState, HashSet<string> ignoredItemIds, out List<IExchangeItem> items, out List<string> deleteItemIds, SyncItemsOptions options = null)
        {
            var result = this.SyncItems(pageSize, ref syncState, ignoredItemIds, out var itemList, out deleteItemIds);
            items = itemList.ConvertAll(item => item as IExchangeItem);
            return result;
        }

        public GraphUtility.IAuthObject GetCredential() => this.AuthObject;

        List<IExchangeItem> IExchangeFolder.FindItems(int pageSize, int offset, out bool moreAvailable, SearchFilter searchFilter)
        {
            return this.FindItems(pageSize, offset, out moreAvailable, searchFilter).ConvertAll(item => item as IExchangeItem);
        }

        #endregion

        #endregion

        #region Private Method

        //private void LoadAdditionalProperties(IEnumerable<ItemChange> changeCollection)
        //{
        //    try
        //    {
        //        var itemIds = changeCollection.Where(change => change.Item is EmailMessage).Select(change => change.ItemId).ToList();
        //        if (itemIds.Count == 0) return;
        //        var itemResponse = this.service.BindToItems(itemIds, new PropertySet(EmailMessageSchema.ToRecipients, EmailMessageSchema.Sender, ItemSchema.Attachments)).GetAwaiter().GetResult();
        //        //var mapping = itemResponse.Where(r => r.Result == ServiceResult.Success).ToDictionary(r => r.Item.Id.UniqueId, r => r.Item, StringComparer.InvariantCultureIgnoreCase);
        //        Dictionary<string, Item> mapping = GetRecipientsMapping(itemResponse);
        //        foreach (var change in changeCollection)
        //        {
        //            Item item2;
        //            if (mapping.TryGetValue(change.ItemId.UniqueId, out item2))
        //            {
        //                LoadEmailMessageProperties(change.Item, item2);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //log and ingore this error.
        //        logger.Warn("Failed to load additional properties while sync items, error: {0}", ex);
        //    }
        //}

        private static Dictionary<string, Item> GetRecipientsMapping(ServiceResponseCollection<GetItemResponse> itemResponse)
        {
            Dictionary<string, Item> mapping = new Dictionary<string, Item>();
            foreach (var tempResponse in itemResponse)
            {
                if (tempResponse.Result == ServiceResult.Success && !mapping.ContainsKey(tempResponse.Item.Id.UniqueId))
                {
                    mapping.Add(tempResponse.Item.Id.UniqueId, tempResponse.Item);
                }
            }
            return mapping;
        }

        //private static void LoadEmailMessageProperties(Item item1, Item item2)
        //{
        //    var emailMessage1 = item1 as EmailMessage;
        //    var emailMessage2 = item2 as EmailMessage;
        //    if (emailMessage1 != null && emailMessage2 != null)
        //    {
        //        Contract.Assert(emailMessage1.ToRecipients != null);
        //        Contract.Assert(emailMessage2.ToRecipients != null);
        //        emailMessage1.ToRecipients.AddRange(emailMessage2.ToRecipients);
        //        item2.Attachments.ForEach(a => emailMessage1.Attachments.InternalAdd(a));
        //    }
        //}
        protected virtual Folder InternalOpen()
        {
            return BindFolder();
        }
        protected Folder BindFolder()
        {
            return Folder.Bind(service, this.inputFolderId).GetAwaiter().GetResult();
        }

        /*private PropertySet AssemblyPropertySet()
        {
            var propertySet = DEFAULT_FOLDER_PROPERTY_SET;
            //if (this.IncludePermission)
            //{
            //    propertySet.Add(FolderSchema.Permissions);
            //}
            return propertySet;
        }*/
        private IEnumerable<ExchangeFolder> AssemblyExchangeFolder(IEnumerable<Folder> folders)
        {
            return folders.
                Where(fArg => !fArg.IsExcludeByFolderClass()).
                Select(fArg => new ExchangeFolder(this.mailbox, fArg, ChangeType.Create, this.AuthObject)
                {
                    DisplayFolderPath = this.DisplayFolderPath + ExchangeConstants.PathCombine + fArg.DisplayName,
                    InternalFolderPath = this.InternalFolderPath + ExchangeConstants.PathParser + EncodeFolerName(fArg.DisplayName),
                });
        }
        private ExchangeItem ConvertToExchangeItem(Item item, ChangeType changeType)
        {
            var exchangeItem = new ExchangeItem(item, changeType, this);
            exchangeItem.ItemPath = this.DisplayFolderPath + ExchangeConstants.PathCombine + exchangeItem.ItemName;
            exchangeItem.ItemInternalPath = this.InternalFolderPath + ExchangeConstants.PathParser + exchangeItem.ExchangeId;
            var retentionId = item.PolicyTag != null ? item.PolicyTag.RetentionId : Guid.Empty;
            exchangeItem.RetentionLabel = LabelIdNameMapping.ContainsKey(retentionId) ? LabelIdNameMapping[retentionId] : string.Empty;
            return exchangeItem;
        }
        protected virtual void SetFolderId(ExchangeMailbox mailbox, string folderId)
        {
            this.inputFolderId = folderId;
        }


        private void SetImpersonateId(string impersonateId)
        {
            if (this.mailbox.IsPublicFolder)
            {
                SetImpersonateId(service, this.AuthObject.UserName);
                return;
            }
            //logger.Info("Impersonate Id:{0}", impersonateId);
            var mbHelper = new ExchangeMailbox(impersonateId, ExchangeMailboxType.None);
            SetImpersonateId(service, mbHelper.MailboxAddress);
        }
        private void AddImpersonationHeader(ExchangeMailbox mailbox)
        {
            if (mailbox.IsPublicFolder) return;
            AddImpersonationHeader(service, mailbox.MailboxAddress);
        }
        /*private Item BindItem(ItemChange change)
        {
            try
            {
                return Item.Bind(service, change.ItemId.UniqueId, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments)).GetAwaiter().GetResult();
            }
            catch (ServiceResponseException ex)
            {
                if (ex.ErrorCode == ServiceError.ErrorItemNotFound)
                {
                    logger.Info("Item not need to backup. Reasion: {0}", ex);
                    throw;
                }
                else
                    return change.Item;
            }
            catch (Exception ex)
            {
                logger.Error("Bind item with exception: {0}", ex);
                return change.Item;
            }
        }*/
        private ExchangeItem ConvertToExchangeItem(ItemChange change)
        {
            Item temp = change.Item;
            //if (change.ChangeType == ChangeType.ReadFlagChange || change.ChangeType == ChangeType.Update)
            //{
            //    temp = BindItem(change);
            //}
            return ConvertToExchangeItem(temp, change.ChangeType);
        }
        private void GenerateFolderInfo(Folder folder, ChangeStatus changeStatus)
        {
            this.FolderName = folder.DisplayName;
            this.FolderId = folder.Id.ToString();
            this.ChildFolderCount = folder.ChildFolderCount;
            this.ItemsCount = folder.TotalCount;
            this.FolderType = folder.FolderClass ?? "IPF.Note";
            this.ChangeStatus = changeStatus;
            SetParentFolderId();
        }
        private static string EncodeFolerName(string name)
        {
            //string result = string.Empty;
            //result = name.Replace("\\", "%5C");
            return name;
        }

        //public string TestExportItemsAPIAvailability(ExchangeItem firstItem, string tempFolder)
        //{
        //    var servicePlus = CloneExchangeService(firstItem.service, -1);
        //    var exportResult = servicePlus.ExportItems(new List<ItemId>() { new ItemId(firstItem.ItemId) }, tempFolder).GetAwaiter().GetResult();
        //    var fileResponse = exportResult.First() as FileExportItemsResponse;
        //    return fileResponse.DataFilePath;
        //}
        #endregion

        public class ExchangeFolderFindResults : IEnumerable<ExchangeFolder>
        {
            internal ExchangeFolderFindResults(FindFoldersResults result, ExchangeFolder parentFolder)
            {
                if (result == null) throw new ArgumentNullException("result");
                if (parentFolder == null) throw new ArgumentNullException("parentFolder");

                this.Folders = result.Folders.
                    Where(fArg => !fArg.IsExcludeByFolderClass()).
                    Select(fArg => new ExchangeFolder(parentFolder.mailbox, fArg, ChangeType.Create, parentFolder.AuthObject)
                    {
                        DisplayFolderPath = parentFolder.DisplayFolderPath + ExchangeConstants.PathCombine + fArg.DisplayName,
                        InternalFolderPath = parentFolder.InternalFolderPath + ExchangeConstants.PathParser + EncodeFolerName(fArg.DisplayName),
                    }).ToList();
                this.MoreAvailable = result.MoreAvailable;
                this.NextPageOffset = result.NextPageOffset;
                this.TotalCount = result.TotalCount;
            }

            public ICollection<ExchangeFolder> Folders { get; private set; }
            public bool MoreAvailable { get; private set; }
            public int? NextPageOffset { get; private set; }
            public int TotalCount { get; private set; }


            public IEnumerator<ExchangeFolder> GetEnumerator()
            {
                return this.Folders.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        [System.Diagnostics.Conditional("DEBUG")]
        [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        sealed class NeedRefactoringAttribute : Attribute
        {
            // See the attribute guidelines at 
            //  http://go.microsoft.com/fwlink/?LinkId=85236
            readonly string positionalString;

            // This is a positional argument
            public NeedRefactoringAttribute(string positionalString)
            {
                this.positionalString = positionalString;

                // TODO: Implement code here

                throw new NotImplementedException();
            }

            public string PositionalString
            {
                get { return positionalString; }
            }

            // This is a named argument
            public int NamedInt { get; set; }
        }
        //todo:qlluo:remove later
        //public void Open(string targetFolderId, int times)
        //{
        //    string errorMessage = string.Empty;

        //    try
        //    {
        //        FolderId targetId = new FolderId(targetFolderId);
        //        currentFolder = Folder.Bind(service, targetFolderId);
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMessage = ex.Message;
        //        currentFolder = OpenWithFieldValue(targetFolderId);
        //    }

        //    if (currentFolder == null)
        //        throw new Exception(errorMessage);
        //    this.ParentFolderId = currentFolder.ParentFolderId.ToString();
        //    this.ChangeStatus = ChangeStatus.Create;
        //    GenerateFolderInfo(currentFolder);
        //}
        //[Obsolete("use Open instead.")]
        //public void BindFolder(string tempFolderId, bool isMailBox, out bool useImpersonate)
        //{
        //    MailBoxHelper mbHelper = new MailBoxHelper(tempFolderId);
        //    try
        //    {
        //        if (isMailBox)
        //        {
        //            currentFolder = Folder.Bind(service, mbHelper.RootFolderId);
        //        }
        //        else
        //        {
        //            FolderId targetId = new FolderId(this.FolderId);
        //            currentFolder = Folder.Bind(service, this.FolderId);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Impersonated access to mailbox with exception: {0}. Mailbox:{1}", ex, tempFolderId);
        //        if (isMailBox)
        //            BindFolderByDelegate(mbHelper.MailboxAddress);
        //    }
        //    useImpersonate = service.ImpersonatedUserId != null;
        //}

        //[Obsolete("use Open instead.")]
        //private void BindFolderByDelegate(string tempFolderId)
        //{
        //    logger.Info("Delegate access to mailbox: {0}", tempFolderId);
        //    MailBoxHelper mbHelper = new MailBoxHelper(tempFolderId);
        //    RemoveImpersonatedUserId(service);
        //    currentFolder = Folder.Bind(service, mbHelper.RootFolderId);
        //}

        //[Obsolete("use Open instead.")]
        //public void OpenMailBox(string tempMailboxAddress, out bool useImpersonate)
        //{
        //    MailBoxHelper mbHelper = new MailBoxHelper(tempMailboxAddress);
        //    try
        //    {
        //        currentFolder = Folder.Bind(service, mbHelper.RootFolderId);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("Impersonated access to mailbox with exception: {0}. MailAddress:{1}. Original MailAddress:{2}. ", ex.ToString(), mbHelper.MailboxAddress, tempMailboxAddress);
        //        string errorMsg = OpenMailBoxByDelegate(tempMailboxAddress);
        //        if (!string.IsNullOrEmpty(errorMsg))
        //            throw;
        //    }
        //    GenerateFolderInfo(currentFolder);
        //    useImpersonate = service.ImpersonatedUserId != null;
        //}
        //[Obsolete("use Open instead.")]
        //private string OpenMailBoxByDelegate(string tempMailboxAddress)
        //{
        //    string errorMsg = string.Empty;
        //    MailBoxHelper mbHelper = new MailBoxHelper(tempMailboxAddress);
        //    try
        //    {
        //        logger.Info("Delegate access to mailbox: {0}. Original mailbox:{1}.", mbHelper.MailboxAddress, tempMailboxAddress);
        //        RemoveImpersonatedUserId(service);
        //        currentFolder = Folder.Bind(service, mbHelper.RootFolderId);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("Connect mailbox with exception : {0}", ex.ToString());
        //        errorMsg = ex.Message;
        //    }
        //    return errorMsg;
        //}
        //public List<ExchangeFolder> GetSubFoldersForPaging(int pageSize, int offset, ref bool moreAvailabe)
        //{
        //    List<ExchangeFolder> findResults = new List<ExchangeFolder>();
        //    FindFoldersResults result;
        //    try
        //    {
        //        logger.Info(string.Format("SubFolders PageSize: {0}", pageSize));
        //        FolderView folderView = new FolderView(pageSize, offset);
        //        folderView.Traversal = FolderTraversal.Shallow;

        //        result = currentFolder.FindFolders(folderView);
        //        foreach (Folder folder in result)
        //        {
        //            ExchangeFolder findFolder = new ExchangeFolder(folder, ChangeType.Create, this);
        //            findFolder.DisplayFolderPath = this.DisplayFolderPath + ExchangeConstants.PathCombine + findFolder.FolderName;
        //            findFolder.InternalFolderPath = this.InternalFolderPath + ExchangeConstants.PathParser + EncodeFolerName(findFolder.FolderName);
        //            findResults.Add(findFolder);
        //        }
        //        moreAvailabe = result.MoreAvailable;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn(string.Format("Get subfolder with exception, reason: {0}", e.ToString()));
        //    }
        //    findResults.Sort(new CompareFolderName());
        //    return findResults;
        //}

        //public List<ExchangeFolder> SyncFolders(string syncState)
        //{
        //    List<ExchangeFolder> findResults = new List<ExchangeFolder>();
        //    try
        //    {
        //        ChangeCollection<FolderChange> changeCollection;
        //        do
        //        {
        //            changeCollection = service.SyncFolderHierarchy(currentFolder.Id, PropertySet.FirstClassProperties, syncState);
        //            foreach (FolderChange change in changeCollection)
        //            {
        //                ExchangeFolder findFolder = new ExchangeFolder(change.Folder, change.ChangeType, this);
        //                findFolder.DisplayFolderPath = this.DisplayFolderPath + ExchangeConstants.PathCombine + findFolder.FolderName;
        //                findFolder.InternalFolderPath = this.InternalFolderPath + ExchangeConstants.PathParser + EncodeFolerName(findFolder.FolderName);
        //                findResults.Add(findFolder);
        //            }
        //            syncState = changeCollection.SyncState;
        //        } while (changeCollection.MoreChangesAvailable);
        //        this.FolderSyncState = syncState;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn(string.Format("Get subfolder, reason: {0}", e.ToString()));
        //    }
        //    findResults.Sort(new CompareFolderName());
        //    return findResults;
        //}
        //public List<ExchangeFolder> GetFoldersByFilter(string filterName, string condition, object targetValue)
        //{
        //    List<ExchangeFolder> findResults = new List<ExchangeFolder>();
        //    FindFoldersResults result;
        //    FolderView folderView = new FolderView(100);
        //    folderView.Traversal = FolderTraversal.Shallow;
        //    do
        //    {
        //        result = currentFolder.FindFolders(GenerateFilter(filterName, condition, targetValue), folderView);
        //        foreach (Folder folder in result)
        //        {
        //            ExchangeFolder findFolder = new ExchangeFolder(folder, ChangeType.Create, this);
        //            if (findFolder.FolderType.StartsWith("IPF.Configuration", StringComparison.OrdinalIgnoreCase) || findFolder.FolderType.StartsWith("IPF.Note.SocialConnector.FeedItems", StringComparison.OrdinalIgnoreCase))
        //                continue;
        //            findFolder.DisplayFolderPath = this.DisplayFolderPath + ExchangeConstants.PathCombine + findFolder.FolderName;
        //            findFolder.InternalFolderPath = this.InternalFolderPath + ExchangeConstants.PathParser + EncodeFolerName(findFolder.FolderName);
        //            findResults.Add(findFolder);
        //        }
        //    } while (result.MoreAvailable);
        //    findResults.Sort(new CompareFolderName());
        //    return findResults;
        //}

        //public List<ExchangeItem> GetItemsByFilter(string filterName, string condition, object targetValue)
        //{
        //    List<ExchangeItem> findResults = new List<ExchangeItem>();
        //    ItemView itemView = new ItemView(100);
        //    itemView.Traversal = ItemTraversal.Shallow;
        //    FindItemsResults<Item> result = null;
        //    do
        //    {
        //        result = currentFolder.FindItems(GenerateFilter(filterName, condition, targetValue), itemView);
        //        foreach (Item item in result)
        //        {
        //            ExchangeItem findItem = new ExchangeItem(item, ChangeType.Create, this);
        //            findItem.ItemPath = this.DisplayFolderPath + ExchangeConstants.PathCombine + findItem.ItemName;
        //            findItem.ItemInternalPath = this.InternalFolderPath + ExchangeConstants.PathParser + findItem.ExchangeId;
        //            findResults.Add(findItem);
        //        }
        //    } while (result.MoreAvailable);
        //    findResults.Sort(new CompareItemModifyTime());
        //    return findResults;
        //}

        //http://msdn.microsoft.com/en-us/library/aa965711(VS.85).aspx
        //public List<ExchangeItem> GetItemsByQueryString(string filterName, string condition, object targetValue)
        //{
        //    //...TO DO
        //    string query = " <m:QueryString>subject:Approval has completed on item001.</m:QueryString>";
        //    List<ExchangeItem> findResults = new List<ExchangeItem>();
        //    ItemView itemView = new ItemView(100);
        //    itemView.Traversal = ItemTraversal.Shallow;
        //    FindItemsResults<Item> result = null;
        //    do
        //    {
        //        result = currentFolder.FindItems(query, itemView);
        //        foreach (Item item in result)
        //        {
        //            ExchangeItem findItem = new ExchangeItem(item, ChangeType.Create, this);
        //            findItem.ItemPath = this.DisplayFolderPath + ExchangeConstants.PathCombine + findItem.ItemName;
        //            findItem.ItemInternalPath = this.InternalFolderPath + ExchangeConstants.PathParser + findItem.ExchangeId;
        //            findResults.Add(findItem);
        //        }
        //    } while (result.MoreAvailable);
        //    findResults.Sort(new CompareItemModifyTime());
        //    return findResults;
        //}

        //private SearchFilter GenerateFilter(string filterName, string condition, object targetValue)
        //{
        //    SearchFilter filter = null;
        //    var def = TransferToFilterDefinition(filterName);
        //    switch (condition.ToUpper(CultureInfo.InvariantCulture))
        //    {
        //        case "ISEQUALTO": filter = new SearchFilter.IsEqualTo(def, targetValue); break;
        //        case "ISGREATERTHAN": filter = new SearchFilter.IsGreaterThan(def, targetValue); break;
        //        case "ISLESSTHANOREQUALTO": filter = new SearchFilter.IsLessThanOrEqualTo(def, targetValue); break;
        //        case "CONTAINSSUBSTRING": filter = new SearchFilter.ContainsSubstring(def, (string)targetValue, ContainmentMode.Substring, ComparisonMode.IgnoreCase); break;
        //        case "NOTCONTAIN": filter = new SearchFilter.Not(new SearchFilter.ContainsSubstring(def, (string)targetValue)); break;
        //    }
        //    return filter;
        //}

        //private PropertyDefinition TransferToFilterDefinition(string filterName)
        //{
        //    switch (filterName.ToUpperInvariant())
        //    {
        //        case "SIZE": return ItemSchema.Size;
        //        case "RECEIVED": return ItemSchema.DateTimeReceived;
        //        case "CREATED": return ItemSchema.DateTimeCreated;
        //        case "ITEMCLASS": return ItemSchema.ItemClass;
        //        case "MODIFIEDTIME": return ItemSchema.LastModifiedTime;
        //        case "SUBJECT":
        //        default:
        //            return ItemSchema.Subject;
        //    }
        //}
    }

    public class ExchangeRootFolder : ExchangeFolder, IExchangeRootFolder
    {
        public ExchangeRootFolder(ExchangeMailbox mailbox, AuthObject authObj)
            : base(mailbox, null, authObj)
        {
            this.isRootFolder = true;
        }

        protected override void SetParentFolderId()
        {
        }
        protected override void SetFolderId(ExchangeMailbox mailbox, string folderId)
        {
            this.inputFolderId = mailbox.RootFolderId;
        }

        public Dictionary<string, FolderChangeStateL> SyncFolderHierarchy()
        {
            throw new NotImplementedException();
        }
    }



    public struct ACLEntry
    {
        public string ObjectSid;
        public string UserId;		//user mail address
        public string DisplayName;			// display name
        public List<string> Permissions;	// string list of permissions
    }

    public class CompareItemModifyTime : IComparer<ExchangeItem>
    {
        public int Compare(ExchangeItem x, ExchangeItem y)
        {
            if (x.Modified > y.Modified)
                return 1;
            else if (x.Modified == y.Modified)
                return 0;
            else return -1;
        }
    }

    public class CompareFolderModifyTime : IComparer<ExchangeFolder>
    {
        public int Compare(ExchangeFolder x, ExchangeFolder y)
        {
            if (x.Modified > y.Modified)
                return 1;
            else if (x.Modified == y.Modified)
                return 0;
            else return -1;
        }
    }

    public class CompareFolderName : IComparer<ExchangeFolder>
    {
        public int Compare(ExchangeFolder x, ExchangeFolder y)
        {
            if (x.FolderName == null)
                return -1;
            else if (y.FolderName == null)
                return 1;
            return string.Compare(x.FolderName, y.FolderName);
        }
    }
}