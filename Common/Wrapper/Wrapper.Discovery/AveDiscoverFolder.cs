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
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverFolder : AveDiscoverFilterBase, IAveDiscoverObjectInfo, IDisposable
    {
        private bool IsNewCreated { get; set; }//从Folder级别进入Discover的Query

        internal AveFolderCache FolderCache { get; set; }

        internal bool PropertyAdded { get; set; }//防止多次添加Property

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
        public int Size { get { return Obj.Size; } set { Obj.Size = value; } }

        //Indicates if the current is a built-in system object or not
        public bool IsSystemObject { get { return Obj.IsSystemObject; } }

        //add for SAAS-27045
        public long Length { get { return Obj.Length; } set { Obj.Length = value; } }
        public string CreatedBy { get { return Obj.CreatedBy; } set { Obj.CreatedBy = value; } }

        public string ModifyBy { get { return Obj.ModifyBy; } set { Obj.ModifyBy = value; } }
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
        public List<AveSecurityObject> DeleteRoleAssignments { get { return Obj.DeleteRoleAssignments; } set { Obj.DeleteRoleAssignments = value; } }//存放permission的删除事件
        public Dictionary<string, object> ItemProperties { get { return Obj.ItemProperties; } }
        #endregion

        public string ListUrl { get { return FolderCache.ListUrl; } set { FolderCache.ListUrl = value; } }

        public IAveFolder AveFolder
        {
            get
            {
                return this.FolderCache.AveWeb.GetFolder(this.DocID, -1, this.FullUrl);
            }
        }

        private void Init(AveListCache listCache, string folderRelativeUrl)
        {
            Obj = new AveItemObject
            {
                FullUrl = folderRelativeUrl.TrimEnd('/')
            };
            FolderCache = new AveFolderCache(listCache, Obj)
            {
                AttachNeedInited = true
            };
            //return new AveDiscoverConnection(site.ContentDatabase.DatabaseConnectionString);
        }

        //private void InitDiscoverFolder()
        //{
        //    FolderCache.InitDiscoverFolder(Obj);
        //}

        public AveDiscoverFolder() { }

        public AveDiscoverFolder(AveDiscoverFilterBase parent) : base(parent) { }

        [Obsolete("please use AveDiscoverFolder(IAveSite site, IAveWeb web, string folderRelativeUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory, Guid listId = default(Guid)) instead")]
        public AveDiscoverFolder(IAveSite site, string folderRelativeUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, module);
            IAveWeb web = site.OpenWeb(folderRelativeUrl);
            AveWebCache webCache = new AveWebCache(siteCache, web.ID, web);
            IAveList list = null;
            string webRelativeUrl = web.ServerRelativeUrl;
            if (!webRelativeUrl.Equals(folderRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                list = web.GetList(folderRelativeUrl);
            }
            AveListCache listCache = new AveListCache(webCache, list != null ? list.ID : Guid.Empty);
            Init(listCache, folderRelativeUrl);
            IsNewCreated = true;
        }

        public AveDiscoverFolder(IAveSite site, Guid webId, string folderRelativeUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory, Guid listId = default(Guid), IAveWeb web = null)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, module);
            AveWebCache webCache = new AveWebCache(siteCache, webId, web);
            AveListCache listCache = new AveListCache(webCache, listId);
            Init(listCache, folderRelativeUrl);
            //FolderCache.Query = objectModelFactory.CreateDiscoveryQuery(site, module);
            IsNewCreated = true;
            //InitDiscoverFolder();
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
            return GetSubFolders(false);
        }


        public List<AveDiscoverFolder> GetSubFoldersWithoutCache()
        {
            List<AveDiscoverFolder> subFolders = new List<AveDiscoverFolder>();
            FolderCache.GetSubFoldersWithoutCache(Obj);
            foreach (AveItemObject subFolderObj in Obj.SubFolderObjs)
            {
                AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                {
                    FolderCache = new AveFolderCache(FolderCache)
                    {
                        AttachNeedInited = true
                    },
                    Obj = subFolderObj,
                };
                subFolders.Add(subFolder);
            }
            //Obj.SubFolderObjs.Clear();
            return subFolders;
        }
        /// <summary>
        /// 不会对结果进行Trim，因为如果Sub Folder符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        /// <returns></returns>
        public List<AveDiscoverFolder> GetSubFolders(bool includeRecycleBin, bool includeSystemFolder = false)
        {
            List<AveDiscoverFolder> subFolders = new List<AveDiscoverFolder>();
            this.FolderCache.GetSubFolders(Obj, includeRecycleBin, includeSystemFolder);
            foreach (AveItemObject subFolderObj in Obj.SubFolderObjs)
            {
                AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                {
                    FolderCache = new AveFolderCache(this.FolderCache),
                    Obj = subFolderObj,
                };
                subFolders.Add(subFolder);
            }
            return GetFilterSubFolders(subFolders);
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
            List<AveDiscoverItem> subItems = new List<AveDiscoverItem>();
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsFolderQualified())
            {
                return subItems;
            }
            FolderCache.GetSubs(Obj, includeRecycleBin, false);
            foreach (AveItemObject subItemObj in Obj.SubItemObjs)
            {
                AveDiscoverItem subItem = new AveDiscoverItem(this)
                {
                    ItemCache = new AveItemCache(this.FolderCache),
                    Obj = subItemObj,
                };
                subItems.Add(subItem);
            }
            return GetFilterItems(subItems);
        }

        public List<AveItemObject> GetAttachments()
        {
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsQualified())
            {
                return new List<AveItemObject>();
            }
            FolderCache.GetAttachments(Obj);
            return GetFilterAttachments(Obj.AttachmentObjs);
        }

        /// <summary>
        /// only for replicator to use
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listRootUrl"></param>
        /// <returns></returns>
        public List<AveItemObject> GetAttachmentsForRP(Guid siteId, string listRootUrl)
        {
            //Obj.AttachmentObjs.Clear();
            this.FolderCache.Query.QueryAttachmentByItemObj(siteId, listRootUrl, this.Obj);
            return GetFilterAttachments(Obj.AttachmentObjs);
        }
        #endregion

        #region IB

        public List<AveDiscoverItem> GetNoTypeDeletedItems()
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

        public void InitSubFolder(Dictionary<string, List<int>> dicUrlAndItemId, Dictionary<string, int> parentFolderIdCache)
        {
            foreach (string url in dicUrlAndItemId.Keys.OrderByDescending(key => key.Length))
            {
                string folderName = url.Substring(url.LastIndexOf("/") + 1);
                AveItemObject folder = null;
                string serverRelativeUrl = url;
                string dirName = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'));
                AveItemObject parentFolder = GetParentFolder(dirName, this.Obj, dicUrlAndItemId);
                foreach (var folderObject in parentFolder.SubFolderObjs)
                {
                    if (folderObject.LeafName.Equals(folderName))
                    {
                        //还需要把删除的Item挂到真正变化的Folder下
                        folder = folderObject;
                        List<AveItemObject> list = GetDeleteItemByFolderUrl(folder.ServerRelativeUrl, dicUrlAndItemId);
                        folder.SubItemObjs.AddRange(list);
                        RemoveItem(list);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (folder == null)
                {
                    folder = new AveItemObject();
                    parentFolder.SubFolderObjs.Add(folder);
                    folder.LeafName = folderName;
                    folder.ObjType = ItemType.Folder;
                    folder.ServerRelativeUrl = serverRelativeUrl;
                    int rowId;
                    if (parentFolderIdCache.TryGetValue(serverRelativeUrl, out rowId))
                    {
                        //
                        folder.ID = rowId;
                    }
                    else
                    {
                        log.Warn("can not get folder:{0} id from id cache", serverRelativeUrl);
                    }
                    List<AveItemObject> list = GetDeleteItemByFolderUrl(folder.ServerRelativeUrl, dicUrlAndItemId);
                    folder.SubItemObjs.AddRange(list);
                    RemoveItem(list);
                }
            }
        }

        private void RemoveItem(List<AveItemObject> list)
        {
            if (list == null)
            {
                return;
            }
            foreach (AveItemObject item in list)
            {
                this.Obj.SubItemObjs.Remove(item);
            }
        }

        private AveItemObject GetParentFolder(string dirName, AveItemObject rootFolder, System.Collections.Generic.Dictionary<string, List<int>> dicUrlAndItemId)
        {
            string listRootFolderUrl = rootFolder.FullUrl;
            if (dirName.Trim('/').Equals(listRootFolderUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
            {
                return rootFolder;
            }
            if (!dirName.Trim('/').Contains(listRootFolderUrl.Trim('/')))
            {
                return null;
            }
            string foldersDirName = dirName.Trim('/').Substring(listRootFolderUrl.Trim('/').Length).Trim('/');
            AveItemObject tempFolder = null;
            AveItemObject tempParentFolder = rootFolder;
            foreach (string str in foldersDirName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (FolderExist(ref tempParentFolder, str))
                {
                    continue;
                }
                else
                {
                    tempFolder = new AveItemObject
                    {
                        LeafName = str,
                        DirName = tempParentFolder.FullUrl.Trim('/'),
                        ObjType = ItemType.Folder
                    };
                    tempFolder.ServerRelativeUrl = "/" + (tempFolder.DirName + "/" + tempFolder.LeafName).Trim('/');
                    tempFolder.FullUrl = tempFolder.ServerRelativeUrl;
                    if (tempParentFolder.SubFolderObjs == null)
                    {
                        tempParentFolder.SubFolderObjs = new List<AveItemObject>();
                    }
                    tempParentFolder.SubFolderObjs.Add(tempFolder);
                    List<AveItemObject> list = GetDeleteItemByFolderUrl(tempFolder.ServerRelativeUrl, dicUrlAndItemId);
                    tempFolder.SubItemObjs.AddRange(list);
                    RemoveItem(list);
                    tempParentFolder = tempFolder;
                }
            }
            return tempParentFolder;
        }

        private bool FolderExist(ref AveItemObject tempParentFolder, string str)
        {
            foreach (AveItemObject folder in tempParentFolder.SubFolderObjs)
            {
                if (folder.LeafName.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    tempParentFolder = folder;
                    return true;
                }
            }
            return false;
        }

        private List<AveItemObject> GetDeleteItemByFolderUrl(string url, System.Collections.Generic.Dictionary<string, List<int>> dicUrlAndItemId)
        {
            List<AveItemObject> subItemObjs = new List<AveItemObject>();
            List<int> rowIdList = new List<int>();
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            if (dicUrlAndItemId.ContainsKey(url))
            {
                rowIdList = dicUrlAndItemId[url];
            }

            foreach (AveItemObject obj in this.Obj.SubItemObjs)
            {
                if (rowIdList.Contains((int)obj.ID))
                {
                    obj.ServerRelativeUrl = url + obj.ServerRelativeUrl.Substring(obj.ServerRelativeUrl.LastIndexOf("/"));
                    obj.FullUrl = obj.ServerRelativeUrl;
                    subItemObjs.Add(obj);
                }
            }
            rowIdList.Clear();
            return subItemObjs;
        }

        public List<AveDiscoverFolder> GetChangeSubFolders()
        {
            List<AveDiscoverFolder> fodlers = new List<AveDiscoverFolder>();
            foreach (AveItemObject obj in Obj.SubFolderObjs)
            {
                fodlers.Add(new AveDiscoverFolder(this)
                {
                    Obj = obj,
                    FolderCache = new AveFolderCache(this.FolderCache),
                });
            }
            return GetFilterSubFolders(fodlers);
        }

        public List<AveDiscoverFolder> GetChangeSubFoldersWithoutCache()
        {
            var folders = GetChangeSubFolders();

            this.ClearSubFoldersCache();

            return folders;
        }


        public List<AveDiscoverItem> GetChangeItems()
        {
            List<AveDiscoverItem> items = new List<AveDiscoverItem>();
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsQualified())
            {
                return items;
            }

            foreach (AveItemObject obj in Obj.SubItemObjs)
            {
                items.Add(new AveDiscoverItem(this)
                {
                    Obj = obj,
                    ItemCache = new AveItemCache(this.FolderCache),
                });
            }
            return GetFilterItems(items);
        }

        public List<AveDiscoverItem> GetChangeItemsWithoutCache()
        {
            var items = GetChangeItems();
            this.ClearSubItemsCache();
            return items;
        }

        public List<AveSecurityObject> GetChangeSecuritys()
        {
            var result = new List<AveSecurityObject>();
            foreach (var list in FolderCache.GetChangeSecuritys().Values)
            {
                result.AddRange(list);
            }
            return result;
        }

        public List<AveAlertObject> GetChangeAlerts()
        {
            return Obj.AlertObjs.Values.ToList();
        }

        public List<AveVersionObject> GetVersions()
        {
            return Obj.VersionObjs;
        }

        public List<AveDiscoverItem> GetSystemItems()
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

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (IsNewCreated && this.FolderCache != null && this.FolderCache.Query != null)
            {
                this.FolderCache.Query.Dispose();
            }
            if (this.FolderCache != null && this.FolderCache.Query != null && this.Obj != null)
            {
                this.FolderCache.ClearCache(this.Obj.FullUrl);//list rootfolder 的serverRelativeUrl是""
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

        public void RemoveFolderCache() //分页情况下清除folder下items缓存
        {
            if (this.FolderCache != null && this.FolderCache.Query != null && this.Obj != null)
            {
                this.FolderCache.ClearCache(this.Obj.FullUrl);//list rootfolder 的serverRelativeUrl是""
            }
        }

        /// <summary>
        /// For Archiver Folder Cache
        /// </summary>
        /// <param name="folderIds"></param>
        public void RemoveFolderCache(List<int> folderIds)
        {
            if (this.FolderCache != null && this.FolderCache.Query != null)
            {
                FolderCache.Query.RemoveFolderCache(folderIds);
            }
        }

        public void RemoveItemCache(int itemId)
        {
            this.FolderCache.Query.RemoveItemCache(itemId);
        }

        #endregion


        private List<AveItemObject> GetFilterAttachments(List<AveItemObject> attachments)
        {
            if (HasFilterWithLevel(PolicyLevel.Attachment) && ResultMode.HasMode(FilterResultMode.Trim))
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

        private List<AveDiscoverItem> GetFilterItems(List<AveDiscoverItem> subItems)
        {
            if (HasFilterWithLevel(PolicyLevel.Item | PolicyLevel.Document) && ResultMode.HasMode(FilterResultMode.Trim))
            {
                return subItems.Where(item =>
                    {
                        try
                        {
                            if (!item.ID.HasValue && !ResultMode.HasMode(FilterResultMode.FilterHidden))
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

        private List<AveDiscoverFolder> GetFilterSubFolders(List<AveDiscoverFolder> subFolders)
        {
            if (HasFilterWithLevel(PolicyLevel.Folder) && ResultMode.HasMode(FilterResultMode.Trim))
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

        public bool IsAttachmentQualified(string attachmentName)
        {
            return this.FilterEngine.IsQualified(this.GetFilterAttachmentInfo(this.FilterPolicies, attachmentName));
        }

        public bool IsFolderQualified()
        {
            try
            {
                return IsQualified();
            }
            catch (NotSupportedException ex)
            {
                log.Info(string.Format("Current folder level filter policy is not supported.Message:{0}.", ex.ToString()));
                //set true for RootFolder by default
                return true;
            }
        }

        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            //GetFolder这个方法，本意是取Item的ParentFolder，如果传一个正常的Item的DocId，即可取到其ParentFolder。
            //在这里我们要获取folder本身，所以暂时只能传进去-1，让其走取rootFolder的逻辑，可正常获取folder。
            return FilterAnalyser.GetFolderFilterInfo(policies, this.FolderCache.AveWeb.GetFolder(DocID, -1, this.FullUrl));
        }

        public ObjectInfoBase GetFilterObjectInfoForArchiver(List<FilterPolicy> policies)
        {
            if (this.ID.HasValue && this.ID.Value > 0)
            {
                return FilterAnalyser.GetFolderFilterInfo(policies, this.FolderCache.AveWeb.GetFolderFromCache(this.ID.Value, this.FullUrl));
            }
            else
            {
                return FilterAnalyser.GetFolderFilterInfo(policies, this.FolderCache.AveWeb.GetFolder(DocID, -1, this.FullUrl));
            }
        }

        #endregion

        #region For Archiver/Extender

        public ObjectInfoBase GetFilterAttachmentInfo(List<FilterPolicy> policies, string attachementName)
        {
            IAveFolder folder = this.FolderCache.AveWeb.GetFolder(DocID);
            foreach (IAveAttachment attachemnt in folder.Item.Attachments)
            {
                if (attachemnt.FileName == attachementName)
                {
                    return FilterAnalyser.GetAttachmentFilterInfo(policies, this.FolderCache.AveWeb.GetFile(folder.Item.Attachments.UrlPrefix + attachementName), folder.Item);
                }
            }
            return null;
        }

        #endregion
        public void ResetDataQuery(object dataProvider)
        {
            if (dataProvider != null)
            {
                this.FolderCache.Query = this.FolderCache.Query.CloneObjWithNewRequest(dataProvider);
            }
        }



        #region improve memory

        public List<AveDiscoverItem> GetItems(ref string pageInfo)
        {
            List<AveDiscoverItem> subItems = new List<AveDiscoverItem>();
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsFolderQualified())
            {
                return subItems;
            }
            FolderCache.GetSubItems(Obj, false, ref pageInfo);
            foreach (AveItemObject subItemObj in Obj.SubItemObjs)
            {
                AveDiscoverItem subItem = new AveDiscoverItem(this)
                {
                    ItemCache = new AveItemCache(this.FolderCache),
                    Obj = subItemObj,
                };
                subItems.Add(subItem);
            }
            return GetFilterItems(subItems);
        }

        //for Granular Backup
        public List<AveDiscoverFolder> GetFolders()//(ref string pageinfo)
        {
            List<AveDiscoverFolder> subFolders = new List<AveDiscoverFolder>();
            this.FolderCache.GetSubFolders(Obj, false);
            foreach (AveItemObject subFolderObj in Obj.SubFolderObjs)
            {
                AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                {
                    FolderCache = new AveFolderCache(this.FolderCache),
                    Obj = subFolderObj,
                };
                subFolders.Add(subFolder);
            }
            return GetFilterSubFolders(subFolders);
        }

        //for CM
        public List<AveDiscoverFolder> GetFolders(bool includeSystemFolder)//(ref string pageinfo)
        {
            List<AveDiscoverFolder> subFolders = new List<AveDiscoverFolder>();
            this.FolderCache.GetSubFolders(Obj, includeSystemFolder);
            foreach (AveItemObject subFolderObj in Obj.SubFolderObjs)
            {
                if (includeSystemFolder)
                {
                    if (subFolderObj.Hidden.HasValue && subFolderObj.Hidden.Value)
                    {
                        AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                        {
                            FolderCache = new AveFolderCache(this.FolderCache),
                            Obj = subFolderObj,
                        };
                        subFolders.Add(subFolder);
                    }
                }
                else
                {
                    if (!(subFolderObj.Hidden.HasValue && subFolderObj.Hidden.Value))
                    {
                        AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                        {
                            FolderCache = new AveFolderCache(this.FolderCache),
                            Obj = subFolderObj,
                        };
                        subFolders.Add(subFolder);
                    }
                }
            }
            return subFolders;
        }

        #endregion

        public IEnumerable<List<AveDiscoverFolder>> GetFoldersWithStructure(bool includeSystemFolder)
        {
            List<AveDiscoverFolder> subFolders = new List<AveDiscoverFolder>();
            foreach (var folderCount in FolderCache.GetSubFoldersWithStructure(Obj, includeSystemFolder))
            {
                foreach (AveItemObject subFolderObj in Obj.SubFolderObjs)
                {
                    AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                    {
                        FolderCache = new AveFolderCache(this.FolderCache),
                        Obj = subFolderObj,
                    };
                    subFolders.Add(subFolder);
                }

                yield return GetFilterSubFolders(subFolders);
                subFolders.Clear();
            }
        }

        /// <summary>
        /// 当前方法缓存了IAveFolder，以便CheckRule时减少实例化IAveFolder的时间.
        /// 清除缓存外围需要调用RemoveFolderCache(List<int> folderIds)方法.
        /// </summary>
        /// <param name="includeSystemFolder"></param>
        /// <returns></returns>
        /*public IEnumerable<List<AveDiscoverFolder>> GetFoldersWithStructurForArchiver(bool includeSystemFolder)
        {
            List<AveDiscoverFolder> subFolders = new List<AveDiscoverFolder>();
            foreach (var folderCount in FolderCache.GetSubFoldersWithStructureForArchiver(Obj, includeSystemFolder))
            {
                foreach (AveItemObject subFolderObj in Obj.SubFolderObjs)
                {
                    AveDiscoverFolder subFolder = new AveDiscoverFolder(this)
                    {
                        FolderCache = new AveFolderCache(this.FolderCache),
                        Obj = subFolderObj,
                    };
                    subFolders.Add(subFolder);
                }
                yield return GetFilterSubFolders(subFolders);
                subFolders.Clear();
            }
        }*/

        public IEnumerable<List<AveDiscoverItem>> GetItemsWithStructure()
        {
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsFolderQualified())
            {
                yield break;
            }
            else
            {
                List<AveDiscoverItem> items = new List<AveDiscoverItem>();
                foreach (var itemCount in FolderCache.GetSubItemsWithStructure(Obj))
                {
                    foreach (AveItemObject subItemObj in Obj.SubItemObjs)
                    {
                        AveDiscoverItem subItem = new AveDiscoverItem(this)
                        {
                            ItemCache = new AveItemCache(this.FolderCache),
                            Obj = subItemObj,
                        };
                        items.Add(subItem);
                    }
                    yield return GetFilterItems(items);
                    items.Clear();
                }
            }
        }


        /// <summary>
        /// 1.只Query当前Folder下的数据，不包含SubFolder下数据
        /// 2.Archiver逻辑中不缓存整个List下Items(只缓存当前Query的ListItem)，DPM需要缓存整个List下Items，有内存问题
        /// </summary>
        /// <returns></returns>
        public IEnumerable<List<AveDiscoverItem>> GetItemsWithStructureForArchiver()
        {
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsFolderQualified())
            {
                yield break;
            }
            else
            {
                List<AveDiscoverItem> items = new List<AveDiscoverItem>();
                foreach (var itemCount in FolderCache.GetSubItemsWithStructureForArchiver(Obj))
                {
                    foreach (AveItemObject subItemObj in Obj.SubItemObjs)
                    {
                        AveDiscoverItem subItem = new AveDiscoverItem(this)
                        {
                            ItemCache = new AveItemCache(this.FolderCache),
                            Obj = subItemObj,
                        };
                        items.Add(subItem);
                    }
                    yield return GetFilterItems(items);
                    items.Clear();
                }
            }
        }

        public int GetItemCount()
        {
            return Obj.FolderStructure == null ? 0 : Obj.FolderStructure.Items.Count;
        }

        public IEnumerable<int> GetItemIDsWithStructureForRecords()
        {
            //if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsFolderQualified())
            //{
            //    yield break;
            //}
            //else
            //{
            return FolderCache.GetSubItemIdsWithStructureForRecords(Obj);
            //foreach (var itemCount in FolderCache.GetSubItemsWithStructureForArchiver(Obj))
            //{
            //    //foreach (AveItemObject subItemObj in Obj.SubItemObjs)
            //    //{
            //    //    AveDiscoverItem subItem = new AveDiscoverItem(this)
            //    //    {
            //    //        ItemCache = new AveItemCache(this.FolderCache),
            //    //        Obj = subItemObj,
            //    //    };
            //    //    items.Add(subItem);
            //    //}
            //    yield return itemCount;
            //    //items.Clear();
            //}
            //}
        }
    }
}
