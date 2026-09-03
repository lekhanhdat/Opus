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



using System.Collections.Generic;
using System.Linq;
using System;
using AvePoint.Wrapper.Common;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.Discovery;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverFolder : AveDiscoverFilterBase, IAveDiscoverFolder, IAveDiscoverObjectInfo
    {
        private bool IsNewCreated { get; set; }//从Folder级别进入Discover的Query

        internal AveFolderCache FolderCache { get; set; }

        internal bool PropertyAdded { get; set; }//防止多次添加Property

        internal AveListObject ParentListObject;

        #region IAveDiscoverObjectInfo Members
        internal AveItemObject Obj;
        public int? ID { get { return Obj.ID; } set { Obj.ID = value; } }
        public Guid DocID { get { return Obj.DocID; } set { Obj.DocID = value; } }
        public Guid tp_GUID { get { return Obj.tp_GUID; } set { Obj.tp_GUID = value; } }
        public ChangeType ChangeType { get { return Obj.ChangeType; } set { Obj.ChangeType = value; } }
        public ItemType ObjType { get { return Obj.ObjType; } set { Obj.ObjType = value; } }
        public string SourceName { get { return Obj.SourceName; } set { Obj.SourceName = value; } }
        public bool isRename { get { return Obj.isRename; } set { Obj.isRename = value; } }
        public string FullUrl { get { return Obj.FullUrl; } set { Obj.FullUrl = value; } }
        public string ItemName { get { return Obj.ItemName; } set { Obj.FullUrl = value; } }
        public long Size { get { return Obj.Size; } set { Obj.Size = value; } }
        public bool IsSystemObject { get { return Obj.IsSystemObject; } }
        public string ModifyBy { get { return Obj.ModifyBy; } set { Obj.ModifyBy = value; } }
        public string CreatedBy { get { return Obj.CreatedBy; } set { Obj.CreatedBy = value; } }
        public DateTime TimeLastModified { get { return Obj.TimeLastModified; } set { Obj.TimeLastModified = value; } }
        public string DirName { get { return Obj.DirName; } set { Obj.DirName = value; } }
        public string LeafName { get { return Obj.LeafName; } set { Obj.LeafName = value; } }
        public byte Level { get { return Obj.Level; } set { Obj.Level = value; } }
        public int Uiversion { get { return Obj.Uiversion; } set { Obj.Uiversion = value; } }
        public string UiVersionString { get { return Obj.UiVersionString; } set { Obj.UiVersionString = value; } }
        public bool IsCurrentVersion { get { return Obj.IsCurrentVersion; } set { Obj.IsCurrentVersion = value; } }
        public Guid ParentID { get { return Obj.ParentID; } set { Obj.ParentID = value; } }
        public byte Type { get { return Obj.Type; } set { Obj.Type = value; } }
        public DateTime TimeCreated { get { return Obj.TimeCreated; } set { Obj.TimeCreated = value; } }
        public int? DocFlags { get { return Obj.DocFlags; } set { Obj.DocFlags = value; } }
        public byte[] RbsId { get { return Obj.RbsId; } set { Obj.RbsId = value; } }
        public DateTime EventTime { get { return Obj.EventTime; } set { Obj.EventTime = value; } }
        public int? CheckoutUserId { get { return Obj.CheckoutUserId; } set { Obj.CheckoutUserId = value; } }
        public bool HasStream { get { return Obj.HasStream; } set { Obj.HasStream = value; } }
        public bool? Hidden { get { return Obj.Hidden; } set { Obj.Hidden = value; } }
        public int QueryType { get { return Obj.QueryType; } set { Obj.QueryType = value; } }
        public byte[] Content { get { return Obj.Content; } set { Obj.Content = value; } }
        public bool ItemPermissionChanged { get { return Obj.ItemPermissionChanged; } set { Obj.ItemPermissionChanged = value; } }
        public byte[] DeleteTransactionId { get { return Obj.DeleteTransactionId; } set { Obj.DeleteTransactionId = value; } }       
        public List<AveSecurityObject> DeleteRoleAssignments { get { return Obj.DeleteRoleAssignments; } set { Obj.DeleteRoleAssignments = value; } }//存放permission的删除事件
        
        /// <summary>
        /// 表示Role Assignments是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType RoleAssignmentsChangeType { get { return Obj.RoleAssignmentsChangeType; } }
        /// <summary>
        /// 表示Alert是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType AlertChangeType { get { return Obj.AlertChangeType; } }
        #endregion

        public string ListUrl { get { return FolderCache.ListUrl; } set { FolderCache.ListUrl = value; } }

        private void Init(AveListCache listCache, string folderRelativeUrl)
        {
            Obj = new AveItemObject
            {
                FullUrl = folderRelativeUrl.Trim('/'),
                ObjType = ItemType.Folder,
            };
            FolderCache = new AveFolderCache(listCache);
            FolderCache.InitFolder(ref ParentListObject, Obj);
        }

        public AveDiscoverFolder() { }

        public AveDiscoverFolder(AveDiscoverFilterBase parent) : base(parent) { }

        public AveDiscoverFolder(IAveSite site, Guid webId, string folderRelativeUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory, Guid listId = default(Guid), IAveWeb web = null)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, kind, module);
            AveWebCache webCache = new AveWebCache(siteCache, webId, web);
            AveListCache listCache = new AveListCache(webCache, listId);
            Init(listCache, folderRelativeUrl);
            IsNewCreated = true;
        }

        /// <summary>
        /// 填充当前folder的  SubFolder 和  Item,深度遍历
        /// 现在Folder已经支持分层查询，该方法去掉
        /// </summary>
        public void FillCurrentFolder(string listUrl)
        {

        }

        #region FB
        /// <summary>
        /// 不会对结果进行Trim，因为如果Sub Folder符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        /// <returns></returns>
        public List<AveDiscoverFolder> GetSubFolders()
        {
            try
            {
                return GetSubFolders(false,true);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An exception occurred while do GetSubFolders.Exception detail:{0}", ex);
                throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSubFoldersError);
            }
        }

        /// <summary>
        /// 不使用Cache, 调用之后释放内存
        /// </summary>
        /// <returns></returns>
        public List<AveDiscoverFolder> GetSubFoldersWithoutCache()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetSubFolderWithoutCache"))
            {
                try
                {
                    List<AveDiscoverFolder> subFolders = GetSubFolders();
                    Obj.SubFolderObjs.Clear();
                    this.FolderCache.HasQuery = false;
                    return subFolders;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetSubFoldersWithoutCache.Exception detail:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSubFoldersError);
                }
            }
        }
        /// <summary>
        /// 不会对结果进行Trim，因为如果Sub Folder符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        /// <returns></returns>
        public List<AveDiscoverFolder> GetSubFolders(bool includeRecycleBin, bool includeVersion, bool includeSystemFolder = false)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetSubFolders"))
            {
                try
                {
                    List<AveDiscoverFolder> subFolders = new List<AveDiscoverFolder>();
                    this.FolderCache.GetSubs(Obj, includeRecycleBin, includeVersion, this.ParentListObject, includeSystemFolder);
                    foreach (AveItemObject subFolderObj in Obj.SubFolderObjs)
                    {
                        AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                        {
                            FolderCache = new AveFolderCache(this.FolderCache),
                            Obj = subFolderObj,
                            ParentListObject = this.ParentListObject,
                        };
                        subFolders.Add(subFolder);
                    }
                    return subFolders;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetSubFolders.Exception detail:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSubFoldersError);
                }
            }
        }

        /// <summary>
        /// 取得item之后，直接反馈给外围
        /// </summary>
        /// <returns></returns>
        public List<AveDiscoverItem> GetItemsWithoutCache()
        {
            var items = GetItems();
            this.ClearSubItemsCache();
            return items;
        }

        public List<AveDiscoverItem> GetItems()
        {
            return GetItems(false);
        }
        public List<AveDiscoverItem> GetItems(bool includeRecycleBin)
        {
            return GetItems(includeRecycleBin, true, DiscoverStubOption.All);
        }
        public List<AveDiscoverItem> GetItems(bool includeRecycleBin, bool includeVersion, DiscoverStubOption discoverStubOption)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetItems"))
            {
                try
                {
                    List<AveDiscoverItem> subItems = new List<AveDiscoverItem>();
                    if (WrapperConfiguration.BPOS_S.QueryAllPropertiesInDiscver && HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsFolderQualified())
                    {
                        return subItems;
                    }
                    FolderCache.GetSubs(Obj, includeRecycleBin, includeVersion, this.ParentListObject, false);
                    foreach (AveItemObject subItemObj in Obj.SubItemObjs)
                    {
                        if (IsNeedAdd(discoverStubOption, subItemObj.RbsId, subItemObj.DocFlags, subItemObj.Content))
                        {
                            AveDiscoverItem subItem = new AveDiscoverItem(this)
                            {
                                ItemCache = new AveItemCache(this.FolderCache),
                                Obj = subItemObj,
                            };
                            subItems.Add(subItem);
                        }
                        else { continue; }
                    }
                    return WrapperConfiguration.BPOS_S.QueryAllPropertiesInDiscver ? GetFilterItems(subItems) : subItems;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItems.Exception detail:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemsError);
                }
            }
        }

        private bool IsNeedAdd(DiscoverStubOption discoverStubOption, byte[] rbsId, int? docFlags, byte[] content)
        {
            switch (discoverStubOption)
            {
                case DiscoverStubOption.All:
                    return true;
                case DiscoverStubOption.OnlyDiscoverStub:
                    if ((rbsId != null) || (docFlags != null && (docFlags & 65536) != 0))
                    {
                        return true;
                    }
                    return false;
                case DiscoverStubOption.OnlyDiscoverNoneStub:
                    if ((rbsId == null) && (docFlags == null || (docFlags & 65536) == 0))
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        [Obsolete("It will delete soon")]
        public List<AveItemObject> GetAttachments()
        {
            return GetAttachments(DiscoverStubOption.All);
        }

        [Obsolete("It will delete soon")]
        public List<AveItemObject> GetAttachments(DiscoverStubOption discoverStubOption)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetAttachments"))
            {
                try
                {
                    FolderCache.GetAttachments(Obj);
                    if (discoverStubOption != DiscoverStubOption.All)
                    {
                        List<AveItemObject> tempAttachments = new List<AveItemObject>();
                        foreach (var attachment in Obj.AttachmentObjs)
                        {
                            if (IsNeedAdd(discoverStubOption, attachment.RbsId, attachment.DocFlags, attachment.Content))
                            {
                                tempAttachments.Add(attachment);
                            }
                        }
                        return GetFilterAttachments(tempAttachments);
                    }
                    return GetFilterAttachments(Obj.AttachmentObjs);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetAttachments.Error:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetAttachmentsError);
                }
            }
        }

        /// <summary>
        /// IB的时候：该方法只得到改变的Attachment, att 集合在查item 时已经加入集合
        /// </summary>
        /// <returns></returns>
        public List<AveItemObject> GetAttachmentsForIB()
        {
            return GetAttachmentsForIB(DiscoverStubOption.All);
        }

        public List<AveItemObject> GetAttachmentsForIB(DiscoverStubOption discoverStubOption)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetAttachmentsForIB"))
            {
                try
                {
                    if (discoverStubOption != DiscoverStubOption.All)
                    {
                        List<AveItemObject> tempAttachments = new List<AveItemObject>();
                        foreach (var attachment in Obj.AttachmentObjs)
                        {
                            if (IsNeedAdd(discoverStubOption, attachment.RbsId, attachment.DocFlags, attachment.Content))
                            {
                                tempAttachments.Add(attachment);
                            }
                        }
                        return GetFilterAttachments(tempAttachments);
                    }
                    return GetFilterAttachments(Obj.AttachmentObjs);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetAttachments.Error:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetAttachmentsError);
                }
            }
        }


        /// <summary>
        /// FB的时候：该方法会得到所有的Attachment. SQL 与IB 一样，在查item 时，att集合已经填充完。API 方式需要重新查询，所以后续调用GetAttachmentsForIB
        /// </summary>
        /// <returns></returns>
        public List<AveItemObject> GetAttachmentsForFB()
        {
            return GetAttachmentsForFB(DiscoverStubOption.All);
        }

        public List<AveItemObject> GetAttachmentsForFB(DiscoverStubOption discoverStubOption)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetAttachmentsForFB"))
            {
                FolderCache.GetAttachments(Obj);
                return GetAttachmentsForIB(discoverStubOption);
            }
        }

        /// <summary>
        /// only for replicator to use
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listRootUrl"></param>
        /// <returns></returns>
        public List<AveItemObject> GetAttachmentsForRP(Guid siteId, string listRootUrl)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetAttachmentsForRP"))
            {
                try
                {
                    //Obj.AttachmentObjs.Clear();
                    this.FolderCache.GetAttachments(listRootUrl, this.Obj);
                    return GetFilterAttachments(Obj.AttachmentObjs);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetAttachmentsForRP.Exception detail:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetAttachmentsForPRError);
                }
            }
        }
        #endregion

        #region IB

        public List<AveDiscoverItem> GetNoTypeDeletedItems()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetNoTypeDeletedItems"))
            {
                List<AveDiscoverItem> items = new List<AveDiscoverItem>();
                foreach (AveItemObject obj in Obj.NoTypeDeleteItems.Values)
                {
                    items.Add(new AveDiscoverItem(this)
                    {
                        Obj = obj,
                        ItemCache = new AveItemCache(this.FolderCache),
                    });
                }
                return items;
            }
        }

        public List<AveDiscoverFolder> GetChangeSubFolders()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetChangeSubFolders"))
            {
                List<AveDiscoverFolder> fodlers = new List<AveDiscoverFolder>();
                foreach (AveItemObject obj in Obj.SubFolderObjs)
                {
                    fodlers.Add(new AveDiscoverFolder(this)
                    {
                        Obj = obj,
                        FolderCache = new AveFolderCache(this.FolderCache),
                        ParentListObject = this.ParentListObject,
                    });
                }
                return GetFilterSubFolders(fodlers);
            }
        }

        public List<AveDiscoverFolder> GetChangeSubFoldersWithoutCache()
        {
            var folders = GetChangeSubFolders();

            this.ClearSubFoldersCache();

            return folders;
        }

        public List<AveDiscoverItem> GetChangeItems()
        {
            return GetChangeItems(DiscoverStubOption.All);
        }

        public List<AveDiscoverItem> GetChangeItems(DiscoverStubOption discoverStubOption)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetChangeItems"))
            {
                List<AveDiscoverItem> items = new List<AveDiscoverItem>();
                if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsQualified())
                {
                    return items;
                }
                foreach (AveItemObject obj in Obj.SubItemObjs)
                {
                    if (IsNeedAdd(discoverStubOption, obj.RbsId, obj.DocFlags, obj.Content))
                    {
                        items.Add(new AveDiscoverItem(this)
                        {
                            Obj = obj,
                            ItemCache = new AveItemCache(this.FolderCache),
                        });
                    }
                }
                return GetFilterItems(items);
            }
        }

        public List<AveDiscoverItem> GetChangeItemsWithoutCache()
        {
            var items = GetChangeItems();
            this.ClearSubItemsCache();
            return items;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "GetChangeSecuritys is function name")]
        public List<AveSecurityObject> GetChangeSecuritys()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetChangeSecuritys"))
            {
                try
                {
                    var result = new List<AveSecurityObject>();
                    foreach (var list in FolderCache.GetChangeSecuritys().Values)
                    {
                        result.AddRange(list);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while getting change security.Exception detail:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSecuritiesError);
                }
            }
        }

        public List<AveAlertObject> GetChangeAlerts()
        {
            return Obj.AlertObjs.Values.ToList();
        }

        public List<AveVersionObject> GetVersions()
        {
            return Obj.VersionObjs;
        }

        public void DiscoverExtraItems(List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            FolderCache.AddExtraItemsIntoFolderCatch(Obj, ParentListObject, extraItems);
        }

        public List<AveDiscoverItem> GetSystemItems()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetSystemItems"))
            {
                List<AveDiscoverItem> systemItems = new List<AveDiscoverItem>();
                foreach (AveItemObject obj in Obj.SubItemObjs)
                {
                    if (!obj.ID.HasValue)
                    {
                        systemItems.Add(new AveDiscoverItem(this)
                        {
                            Obj = obj,
                            ItemCache = new AveItemCache(this.FolderCache)
                            {
                                ItemId = obj.ID,
                            }
                        });
                    }
                }
                return systemItems;
            }
        }

        #endregion

        #region Stub
        /// <summary>
        /// Query Stub Items
        /// </summary>
        public List<AveDiscoverItem> GetStubItems()
        {
            return GetStubItems(false);
        }
        /// <summary>
        /// Query Stub Items
        /// </summary>
        public List<AveDiscoverItem> GetStubItems(bool includeRecycleBin)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetStubItems"))
            {
                try
                {
                    List<AveDiscoverItem> stubItems = new List<AveDiscoverItem>();
                    FolderCache.GetStubItems(Obj, this.ParentListObject, includeRecycleBin);
                    foreach (AveItemObject subItemObj in Obj.SubItemObjs)
                    {
                        AveDiscoverItem subItem = new AveDiscoverItem(this)
                        {
                            ItemCache = new AveItemCache(this.FolderCache),
                            Obj = subItemObj
                        };
                        stubItems.Add(subItem);
                    }
                    return stubItems;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetStubItems.Exception detail:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetStubItemsError);
                }
            }
        }
        /// <summary>
        /// Return Stub Attachments
        /// </summary>
        public List<AveItemObject> GetStubAttachments()
        {
            return Obj.StubAttachmentObjs;
        }
        /// <summary>
        /// Return all stub(version/item/current folder attachment/item attachment) count related folder
        /// </summary>
        /// <returns></returns>
        public int GetAllStubCount(bool includeRecycleBin = false)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetAllStubCount"))
            {
                try
                {
                    return FolderCache.GetAllStubCount(Obj, ParentListObject,includeRecycleBin);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetAllStubCount.Exception detail:{0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetStubContentsError);
                }
            }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (IsNewCreated && this.FolderCache != null && this.FolderCache.Query != null)
            {
                this.FolderCache.Query.Dispose();
            }
            if (this.Obj != null)
            {
                this.Obj.Dispose();
                this.Obj = null;
            }
        }

        /// <summary>
        /// 外围调用清除cache
        /// </summary>
        public void ClearSubItemsCache()
        {
            if (this.Obj != null)
            {
                this.Obj.ClearSubItemsCache();
            }
        }

        /// <summary>
        /// 外围调用清除cache
        /// </summary>
        public void ClearSubFoldersCache()
        {
            if (this.Obj != null)
            {
                this.Obj.ClearSubFoldersCache();
            }
        }

        #endregion


        private List<AveItemObject> GetFilterAttachments(List<AveItemObject> attachments)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetFilterAttachments"))
            {
                if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim))
                {
                    return attachments.Where(attachemnt =>
                        {
                            try
                            {
                                return this.FilterEngine.IsQualified(this.GetFilterAttachmentInfo(this.FilterPolicies, attachemnt.LeafName));
                            }
                            catch (Exception ex)
                            {
                                log.Warn("An error occurred when filter attachment. Name:{0}, Reason:{1}.", attachemnt.LeafName, ex.ToString());
                                return false;
                            }
                        }).ToList();
                }
                return attachments;
            }
        }

        private List<AveDiscoverItem> GetFilterItems(List<AveDiscoverItem> subItems)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetFilterItems"))
            {
                if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim))
                {
                    return subItems.Where(item =>
                        {
                            try
                            {
                                if ((!item.ID.HasValue || item.ID <= 0) && !ResultMode.HasMode(FilterResultMode.FilterHidden))
                                {
                                    return true;
                                }
                                return item.ID.HasValue && this.FilterEngine.IsQualified(FilterAnalyser.SetVersionAlwaysTrue(this.FilterPolicies, item.GetFilterObjectInfo(this.FilterPolicies)));
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperDiscoverResource.AWDGetFilterItemsError, e.ToString());
                                return false;
                            }
                        }).ToList();

                }
                return subItems;
            }
        }

        private List<AveDiscoverFolder> GetFilterSubFolders(List<AveDiscoverFolder> subFolders)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetFilterSubFolders"))
            {
                if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim))
                {
                    return subFolders.Where(folder =>
                        {
                            try
                            {
                                if (!folder.ID.HasValue && !ResultMode.HasMode(FilterResultMode.FilterHidden))
                                {
                                    return true;
                                }
                                return this.FilterEngine.IsQualified(folder.GetFilterObjectInfo(this.FilterPolicies));
                            }
                            catch (Exception ex)
                            {
                                log.Warn("An error occurred when filter sub folders. Name:{0}, Reason:{1}.", string.Concat(folder.DirName, folder.LeafName), ex.ToString());
                                return false;
                            }
                        }).ToList();
                }
                return subFolders;
            }
        }

        public bool IsAttachmentQualified(string attachmentName)
        {
            return this.FilterEngine.IsQualified(this.GetFilterAttachmentInfo(this.FilterPolicies, attachmentName));
        }

        public bool IsFolderQualified()
        {
            bool isFolderQualified = false;
            try
            {
                isFolderQualified = IsQualified();
            }
            catch (NotSupportedException ex)
            {
                log.Debug(string.Format("Current folder level filter policy is not supported.Message:{0}.", ex.ToString()));
                if (string.Equals(this.AveFolder.ServerRelativeUrl, this.AveFolder.ParentList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    //set true for RootFolder by default
                    isFolderQualified = true;
                }
            }
            return isFolderQualified;
        }
        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetFilterObjectInfo"))
            {
                if (!HasFilterOnThisLevel(policies, PolicyLevel.Folder))
                {
                    return new FolderInfo();
                }
                //GetFolder这个方法，本意是取Item的ParentFolder，如果传一个正常的Item的DocId，即可取到其ParentFolder。
                //在这里我们要获取folder本身，所以暂时只能传进去-1，让其走取rootFolder的逻辑，可正常获取folder。
                return FilterAnalyser.GetFolderFilterInfo(policies, this.FolderCache.AveWeb.GetFolder(DocID, -1, "/" + this.FullUrl.Trim('/')));
            }
        }

        IAveFolder aveFolder = null;
        public IAveFolder AveFolder
        {
            get
            {
                if (aveFolder == null)
                {
                    aveFolder = this.FolderCache.AveWeb.GetFolder(DocID, -1, this.FullUrl);
                }
                return aveFolder;
            }
        }

        #endregion

        #region For Archiver/Extender

        public ObjectInfoBase GetFilterAttachmentInfo(List<FilterPolicy> policies, string attachementName)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverFolder.GetFilterAttachmentInfo"))
            {
                if (!HasFilterOnThisLevel(policies, PolicyLevel.Attachment))
                {
                    return new AttachmentInfo();
                }
                IAveFolder folder = this.FolderCache.AveWeb.GetFolder(DocID);
                foreach (IAveAttachment attachemnt in folder.Item.Attachments)
                {
                    if (attachemnt.FileName == attachementName)
                    {
                        return FilterAnalyser.GetAttachmentFilterInfo(policies, this.FolderCache.AveWeb.GetFile(attachemnt.ServerRelativeUrl), folder.Item);
                    }
                }
                return null;
            }
        }

        #endregion
        public void ResetDataQuery(object dataProvider)
        {
            if (dataProvider != null)
            {
                this.FolderCache.Query = this.FolderCache.Query.CloneObjWithNewRequest(dataProvider);
            }
        }

        
        List<IAveDiscoverItem> IAveDiscoverFolder.GetChangeItems()
        {
            return this.GetChangeItems().Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetChangeItems(DiscoverStubOption discoverStubOption)
        {
            return this.GetChangeItems(discoverStubOption).Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetChangeItemsWithoutCache()
        {
            return this.GetChangeItemsWithoutCache().Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverFolder> IAveDiscoverFolder.GetChangeSubFolders()
        {
            return this.GetChangeSubFolders().Select(item => item as IAveDiscoverFolder).ToList();
        }

        List<IAveDiscoverFolder> IAveDiscoverFolder.GetChangeSubFoldersWithoutCache()
        {
            return this.GetChangeSubFoldersWithoutCache().Select(item => item as IAveDiscoverFolder).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetItems()
        {
            return this.GetItems().Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetItems(bool includeRecycleBin)
        {
            return this.GetItems(includeRecycleBin).Select(item => item as IAveDiscoverItem).ToList();
        }
        List<IAveDiscoverItem> IAveDiscoverFolder.GetItems(bool includeRecycleBin, DiscoverStubOption discoverStubOption)
        {
            return this.GetItems(includeRecycleBin, true, discoverStubOption).Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetItems(bool includeRecycleBin,bool includeVersion, DiscoverStubOption discoverStubOption)
        {
            return this.GetItems(includeRecycleBin, includeVersion, discoverStubOption).Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetItemsWithoutCache()
        {
            return this.GetItemsWithoutCache().Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetNoTypeDeletedItems()
        {
            return this.GetNoTypeDeletedItems().Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetStubItems()
        {
            return this.GetStubItems().Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetStubItems(bool includeRecycleBin)
        {
            return this.GetStubItems(includeRecycleBin).Select(item => item as IAveDiscoverItem).ToList();
        }

        List<IAveDiscoverFolder> IAveDiscoverFolder.GetSubFolders()
        {
            return this.GetSubFolders().Select(item => item as IAveDiscoverFolder).ToList();
        }

        List<IAveDiscoverFolder> IAveDiscoverFolder.GetSubFolders(bool includeRecycleBin, bool includeSystemFolder = false)
        {
            return this.GetSubFolders(includeRecycleBin, includeSystemFolder).Select(item => item as IAveDiscoverFolder).ToList();
        }

        List<IAveDiscoverFolder> IAveDiscoverFolder.GetSubFoldersWithoutCache()
        {
            return this.GetSubFoldersWithoutCache().Select(item => item as IAveDiscoverFolder).ToList();
        }

        List<IAveDiscoverItem> IAveDiscoverFolder.GetSystemItems()
        {
            return this.GetSystemItems().Select(item => item as IAveDiscoverItem).ToList();
        }

    }
    public class AveDiscoverFolderForSystemFolder : AveDiscoverFolder
    {
        public AveDiscoverFolderForSystemFolder(AveDiscoverList parent, string leafName, string fullUrl)
            : base(parent)
        {
            Obj = new AveItemObject()
            {
                LeafName = leafName,
                FullUrl = fullUrl,
            };
            ParentListObject = parent.ListObject;
            FolderCache = new AveFolderCache(parent.ListCache, 0);
        }

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            return new FolderInfo()
            {
                Title = this.LeafName,
                Url = this.FullUrl
            };
        }
    }
}
