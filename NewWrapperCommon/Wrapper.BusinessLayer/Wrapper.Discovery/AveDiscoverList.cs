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



using System;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverList : AveDiscoverFilterBase, IAveDiscoverList
    {
        internal bool IsNewCreated { get; set; }//从List级别进入Discover的Query

        internal AveListCache ListCache { get; set; }      

        internal AveListObject ListObject { get; set; }

        public string ModifiedBy { get { return ListObject.ModifiedBy; } set { ListObject.ModifiedBy = value; } }
        public Guid ListId { get { return ListObject.ListId; } set { ListObject.ListId = value; } }
        public Guid RootFolderId { get { return ListObject.RootFolderId; } set { ListObject.RootFolderId = value; } }
        public string Name { get { return ListObject.Name; } set { ListObject.Name = value; } }
        public string Title { get { return ListObject.Title; } set { ListObject.Title = value; } }
        public int Type { get { return ListObject.Type; } set { ListObject.Type = value; } }
        public string RootFolderUrl { get { return ListObject.RootFolderUrl; } set { ListObject.RootFolderUrl = value; } }
        public object Flag { get { return ListObject.Flag; } set { ListObject.Flag = value; } }
        public ChangeType ChangeType { get { return ListObject.ChangeType; } set { ListObject.ChangeType = value; } }
        public int? ServerTemplate { get { return ListObject.ServerTemplate; } set { ListObject.ServerTemplate = value; } }
        public bool? Hidden { get { return ListObject.Hidden; } set { ListObject.Hidden = value; } }
        public DateTime ModifiedTime { get { return ListObject.ModifiedTime; } set { ListObject.ModifiedTime = value; } }
        public List<AveSecurityObject> DeleteRoleAssignments { get { return ListObject.DeleteRoleAssignments; } set { ListObject.DeleteRoleAssignments = value; } }//存放permission的删除事件
        /// <summary>
        /// 表示Role Assignments是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType RoleAssignmentsChangeType { get { return ListObject.RoleAssignmentsChangeType; } }
        /// <summary>
        /// 表示Alert是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType AlertChangeType { get { return ListObject.AlertChangeType; } }
        public byte[] DeleteTransactionId { get { return ListObject.DeleteTransactionId; } set { ListObject.DeleteTransactionId = value; } }
        private void Init(AveSiteCache siteCache, Guid webId, string listRootFolderUrl, IAveWeb web = null)
        {
            ListObject = new AveListObject { RootFolderUrl = listRootFolderUrl };
            AveWebCache webCache = new AveWebCache(siteCache, webId, web);
            ListCache = new AveListCache(webCache, ListObject);
        }

        public AveDiscoverList() { }

        public AveDiscoverList(AveDiscoverFilterBase parent) : base(parent) { }

        public AveDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, kind, module);
            Init(siteCache, webId, listRootFolderUrl.Trim('/'), null);
            IsNewCreated = true;
        }

        public AveDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, kind, module);
            Init(siteCache, web.ID, listRootFolderUrl.Trim('/'), web);
            IsNewCreated = true;
        }

        public AveDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, kind, module, startTime, endTime);
            Init(siteCache, webId, listRootFolderUrl.Trim('/'), null);
            IsNewCreated = true;
        }

        public AveDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, kind, module, startTime, endTime);
            Init(siteCache, web.ID, listRootFolderUrl.Trim('/'), web);
            IsNewCreated = true;
        }

        #region FB

        /// <summary>
        ///Query List Root Folder, 不会对结果进行Trim，因为如果Sub Folder符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        /// <returns>List RootFolder</returns>
        [Obsolete("Use IAveDiscoverFolder IAveDiscoverList.GetRootFolder() isntead")]
        public AveDiscoverFolder GetRootFolder()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetRootFolder"))
            {
                try
                {
                    AveDiscoverFolder rootFolder = new AveDiscoverFolder(this)
                    {
                        Obj = new AveItemObject(),
                        FolderCache = new AveFolderCache(this.ListCache),
                        ParentListObject = ListObject.ListId.Equals(Guid.Empty) ? null : ListObject,
                    };
                    ListCache.InitRootFolder(ListObject, rootFolder.FolderCache, rootFolder.Obj);
                    return rootFolder;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetRootFolder.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetListRoorFolderError);
                }
            }
        }
        /// <summary>
        /// 调用此方法后，会query List下所有的object，包括Version和Attachment。
        /// 1.只有真实365有实现。2.传进来的Folder必须是root folder。
        /// </summary>
        /// <param name="listRootFolder"></param>
        /// <param name="includeRecycleBin"></param>
        /// <param name="discoverStubOption"></param>
        /// <param name="maxItemCount">当List中的ItemCount超出此限制，则不query所有content，防止内存问题。</param>
        /// <param name="includeSystemFolder"></param>
        [Obsolete("Use void IAveDiscoverList.DiscoverAllListContent() isntead")]
        public void DiscoverAllListContent(IAveDiscoverFolder listRootFolder, bool includeRecycleBin, DiscoverStubOption discoverStubOption, int maxItemCount = 50000, bool includeSystemFolder = false)
        {
            var rootFolder = listRootFolder as AveDiscoverFolder;
            try
            {
                using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.DiscoverListAllContent"))
                {
                    if (!ListCache.AveSite.IsOnlineSite
                        || !ListObject.Title.Equals("{System Folder}") && !string.Equals(rootFolder.FullUrl.Trim('/'), ListObject.RootFolderUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    ListCache.DiscoverAllListContent(rootFolder.Obj, maxItemCount, includeRecycleBin, includeSystemFolder, discoverStubOption);
                }
            }
            catch (Exception e)
            {
                log.Error("Query list all content failed. List: {0}, Error: {1}", Title, e); ;
                rootFolder.Obj.ClearSubFoldersCache();
                rootFolder.Obj.ClearSubItemsCache();
            }
        }

        /// <summary>
        /// Query list Views and fill the View relatived Item
        /// </summary>
        public Dictionary<Guid, AveViewObject> GetViews()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetViews"))
            {
                try
                {
                    return ListCache.GetViews();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetViews.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetViewsError);
                }
            }
        }

        #endregion

        #region IB
        [Obsolete("no use now, will remove later")]
        public IAveDiscoverFolder GetChangeRootFolder(List<AveDiscoverExtraItemBaseInfo> extraItems = null,DiscoverModeForSOIB discoverMode = DiscoverModeForSOIB.UseBoth)
        {
            return GetChangeRootFolder(extraItems);
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, DateTime startTime, DateTime endTime)
        {
            Dictionary<string, object> listChangeItems = ListCache.Query.GetListChangedItems(webId, ListId, startTime, endTime);
            return listChangeItems;
        }

        /// <summary>
        /// Query All the  Changed Items in Current List, 不会对结果进行Trim，因为如果Sub Site符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        /// <returns>List RootFolder</returns>
        [Obsolete("Use IAveDiscoverFolder IAveDiscoverList.GetChangeRootFolder(List<AveDiscoverExtraItemBaseInfo> extraItems = null) instead")]
        public IAveDiscoverFolder GetChangeRootFolder(List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        {
            try
            {
                var rootFolder = new AveDiscoverFolder(this)
                {
                    Obj = new AveItemObject(),
                    FolderCache = new AveFolderCache(ListCache),
                    ParentListObject = ListObject,
                };
                ListCache.InitChangeRootFolder(ListObject, rootFolder.FolderCache, rootFolder.Obj, extraItems);
                return rootFolder;
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeRootFolder.");
                log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetListRoorFolderError);
            }
        }

        public Dictionary<Guid, AveAlertObject> GetChangeAlerts()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetChangeAlerts"))
            {
                try
                {
                    return ListCache.GetChangeAlerts();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeAlerts.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetAlertsError);
                }
            }
        }

        public Dictionary<byte[], AveContentTypeObject> GetChangeListContentTypes()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetChangeListContentTypes"))
            {
                try
                {
                    return ListCache.GetChangeListContentTypes();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeListContentTypes.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetCTsError);
                }
            }
        }

        public Dictionary<Guid, AveViewObject> GetChangeViews()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetChangeViews"))
            {
                try
                {
                    return ListCache.GetChangeViews();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeViews.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetViewsError);
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "GetChangeSecuritys is function name")]
        public List<AveSecurityObject> GetChangeSecuritys()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetChangeSecuritys"))
            {
                try
                {
                    var result = new List<AveSecurityObject>();
                    foreach (var list in ListCache.GetChangeSecuritys().Values)
                    {
                        result.AddRange(list);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while getting change security.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSecuritiesError);
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (IsNewCreated && this.ListCache != null && this.ListCache.Query != null)
            {
                this.ListCache.Query.Dispose();
            }
            ListCache = null;
        }

        #endregion

        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetFilterObjectInfo"))
            {
                if (!HasFilterOnThisLevel(policies, PolicyLevel.List))
                {
                    return new ListInfo();
                }
                return FilterAnalyser.GetListFilterInfo(policies, this.ListCache.AveWeb.GetList(this.RootFolderUrl));
            }
        }

        #endregion

        public IAveList GetListObject()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetListObject"))
            {
                try
                {
                    return this.ListCache.AveWeb.GetList(this.RootFolderUrl);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetListObject.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetListError);
                }
            }
        }

        #region support migration license
        public long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderUrl, DateTime beginTime)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetObjectChangeSize"))
            {
                try
                {
                    return ListCache.Query.GetObjectChangedSize(siteId, webId, listId, folderUrl, beginTime);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetObjectChangedSize.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetChangeSizeError);
                }
            }
        }
        public long GetListSize(Guid siteId, Guid webId, Guid listId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetListSize"))
            {
                try
                {
                    return ListCache.Query.GetListSize(siteId, webId, listId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetListSize.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetListSizeError);
                }
            }
        }
        public long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverList.GetFolderSize"))
            {
                try
                {
                    return ListCache.Query.GetFolderSize(siteId, webId, listId, folderUrl);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetFolderSize.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetFolderSizeError);
                }
            }
        }
        #endregion


        IAveDiscoverFolder IAveDiscoverList.GetRootFolder()
        {
            return this.GetRootFolder() as IAveDiscoverFolder;
        }

        public IAveDiscoverFolder GetVirtualSystemFolder(string leafName, string fullUrl)
        {
            return new AveDiscoverFolderForSystemFolder(this, leafName, fullUrl);
        }

        
    }
    public class AveDiscoverSystemFolderList : AveDiscoverList
    {
        public AveDiscoverSystemFolderList(AveDiscoverFilterBase parent) : base(parent) { }

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            return new ListInfo()
            {
                Title = this.Title
            };
        }
    }
}
