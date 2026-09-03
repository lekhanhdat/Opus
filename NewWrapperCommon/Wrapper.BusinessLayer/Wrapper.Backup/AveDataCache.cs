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
using System.Linq;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Backup;

namespace AvePoint.Wrapper.Backup
{
    public class AveDataCache
    {
        private Dictionary<int, object> principalIdCache = new Dictionary<int, object>();
        private object obj = new object();

        private AveUserList userList = new AveUserList();
        private AveGroupList groupList = new AveGroupList();

        public AveUserList GetUsersForExport()
        {//Export后清空Users，防止重复备份，不需要清空principalIdCache，避免再次Cache的时候重复加User或者Group
            var users = this.userList;
            this.userList = new AveUserList();
            return users;
        }

        public AveGroupList GetGroupsForExport()
        {//Export后清空Groups，防止重复备份，不需要清空principalIdCache，避免再次Cache的时候重复加User或者Group
            var groups = this.groupList;
            this.groupList = new AveGroupList();
            return groups;
        }

        public void AddToCache(int principalId, AveUserInfo userInfo)
        {
            if (!principalIdCache.ContainsKey(principalId))
            {
                principalIdCache.Add(principalId, obj);
                userList.Users.Add(userInfo);
            }
        }

        public void AddToCache(int principalId, AveGroupInfo groupInfo)
        {
            if (!principalIdCache.ContainsKey(principalId))
            {
                principalIdCache.Add(principalId, obj);
                groupList.Groups.Add(groupInfo);
            }
        }

        public void AddToCache(int principalId)//当某个Group被删掉后，就找不到这个Group信息了，为了不重复查找，把它的Id加到这里
        {
            if (!principalIdCache.ContainsKey(principalId))
            {
                principalIdCache.Add(principalId, obj);
            }
        }

        public bool PrincipalIdAlreadyExists(int principalId)
        {
            return principalIdCache.ContainsKey(principalId);
        }
    }

    public class AveItemDataCache : AveDataCache
    {
    }

    public class AveListDataCache : AveDataCache
    {
    }

    public class CacheCapacity
    {
        public int UserCacheMax;
        public int GroupCacheMax;
        private const int Default_Capacity = 1000;

        public CacheCapacity()
        {
            UserCacheMax = Default_Capacity;
            GroupCacheMax = Default_Capacity;
        }
    }

    public class AveSiteDataCache
    {
        private AveSPSite mAveSPSite;
        public AveUserCache UserCache;
        public AveGroupCache GroupCache;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSiteDataCache(AveSPSite aveSite)
            : this(aveSite, new CacheCapacity())
        {
        }

        public AveSiteDataCache(AveSPSite aveSite, CacheCapacity capacity)
        {
            mAveSPSite = aveSite;
            UserCache = new AveUserCache(capacity.UserCacheMax);
            GroupCache = new AveGroupCache(capacity.GroupCacheMax);
        }

        public object GetPrincipalInfo(int principalId)
        {
            if (UserCache.Contains(principalId))
            {
                return UserCache.GetUserInfo(principalId);
            }
            if (GroupCache.Contains(principalId))
            {
                return GroupCache.GetGroupInfo(principalId);
            }
            AveUserInfo userInfo = mAveSPSite.SPSite.UserSerializer.GetObjectData(principalId);
            if (userInfo != null)
            {
                userInfo = AveUserUtility.ConvertDomainGroupSidToAccount(userInfo, this.mAveSPSite.ObjectModelFactory);
                UserCache.Add(principalId, userInfo);
                return userInfo;
            }
            AveGroupInfo groupInfo = mAveSPSite.SPSite.GroupSerializer.GetObjectData(principalId);
            AveGroup.SetAboutMeToGroupInfo(groupInfo, mAveSPSite.SPSite.RootWeb);
            GetGroupInfoWithMembers(groupInfo);
            if (groupInfo != null)
            {
                GroupCache.Add(principalId, groupInfo);
            }
            return groupInfo;
        }

        public AveUserInfo GetUserInfo(int userId)
        {
            if (UserCache.Contains(userId))
            {
                return UserCache.GetUserInfo(userId);
            }
            AveUserInfo userInfo = mAveSPSite.SPSite.UserSerializer.GetObjectData(userId);
            if (userInfo != null)
            {
                userInfo = AveUserUtility.ConvertDomainGroupSidToAccount(userInfo, this.mAveSPSite.ObjectModelFactory);
                UserCache.Add(userId, userInfo);
            }
            return userInfo;
        }

        public AveGroupInfo GetGroupInfo(int groupId)
        {
            if (GroupCache.Contains(groupId))
            {
                return GroupCache.GetGroupInfo(groupId);
            }
            AveGroupInfo groupInfo = mAveSPSite.SPSite.GroupSerializer.GetObjectData(groupId);
            AveGroup.SetAboutMeToGroupInfo(groupInfo, mAveSPSite.SPSite.RootWeb);
            GetGroupInfoWithMembers(groupInfo);
            if (groupInfo != null)
            {
                GroupCache.Add(groupId, groupInfo);
            }
            return groupInfo;
        }
        
        private void GetGroupInfoWithMembers(AveGroupInfo groupInfo)
        {
            if (groupInfo != null && groupInfo.Members.Count == 0)
            {
                foreach (int id in groupInfo.Memberships)
                {
                    AveUserInfo userInfo = GetUserInfo(id);
                    if (userInfo == null)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperBackupResource.GroupMemberTypeInvalidate, id);
                        continue;
                    }
                    userInfo = AveUserUtility.ConvertDomainGroupSidToAccount(userInfo, this.mAveSPSite.ObjectModelFactory);
                    groupInfo.Members.Add(userInfo);
                }
            }
        }
    }

    public class AveUserCache
    {
        private Dictionary<int, AveUserInfo> mCache = new Dictionary<int, AveUserInfo>();
        private int mCapacity;

        public AveUserCache(int capacity)
        {
            mCapacity = capacity;
        }

        public AveUserInfo GetUserInfo(int principalId)
        {
            AveUserInfo userInfo = null;

            if (mCache.ContainsKey(principalId))
            {
                userInfo = mCache[principalId];
            }
            else
            {
                //TO DO
            }
            return userInfo;
        }

        public bool Contains(int id)
        {
            lock(this)
            {
                return mCache.ContainsKey(id);
            }
        }

        public void Add(int id, AveUserInfo userInfo)
        {
            lock (this)
            {
                if (mCache.Count == mCapacity)
                {
                    int key = mCache.First().Key;
                    mCache.Remove(key);
                }
                if (!mCache.ContainsKey(id))
                {
                    mCache.Add(id, userInfo);
                }
            }
        }
    }

    public class AveGroupCache
    {
        private Dictionary<int, AveGroupInfo> mCache = new Dictionary<int, AveGroupInfo>();
        private int mCapacity;

        public AveGroupCache(int capacity)
        {
            mCapacity = capacity;
        }

        public AveGroupInfo GetGroupInfo(int principalId)
        {
            AveGroupInfo groupInfo = null;

            if (mCache.ContainsKey(principalId))
            {
                groupInfo = mCache[principalId];
            }
            else
            {
                //TO DO
            }

            return groupInfo;
        }

        public bool Contains(int id)
        {
            return mCache.ContainsKey(id);
        }

        public void Add(int id, AveGroupInfo groupInfo)
        {
            if (mCache.Count == mCapacity)
            {
                int key = mCache.First().Key;
                mCache.Remove(key);
            }
            if (!mCache.ContainsKey(id))
            {
                mCache.Add(id, groupInfo);
            }
        }
    }

    //Code Review, Qinglong.Luo@avepoint.com. Sid.You@avepoint.com
    public class AveIndexCache
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveSPSite parentSite;
        private AveSPList list;
        private AveSPItem item;
        private int rowId = 0;

        //private Dictionary<string, AveFieldType> indexColumns = null;
        private Dictionary<FullTextIndexLevel, Dictionary<string, CachedField>> indexColumns = new Dictionary<FullTextIndexLevel, Dictionary<string, CachedField>>();
        private Dictionary<string, object> columnValueCache = null;

        private List<string> attachments = new List<string>();
        private AveUserInfo author = new AveUserInfo();
        private DateTime created;

        internal Dictionary<string, Dictionary<int, object>> LookupValues = null;
        internal Dictionary<string, IAveList> LookupLists = null;
        Dictionary<int, Guid> attachmentFolderIds = null;
        private Guid tempDependItemId = Guid.Empty;
        internal AveColumnCache CustomColumnCache { get; set; }

        public AveIndexCache(AveSPList list)
        {
            this.list = list;
            this.parentSite = list.ParentSite;
            this.list.Fields.Load(true);
        }

        public FullTextIndex GetIndex(AveSPItem item, FullTextIndexLevel level)
        {
            InitCache(item, level);
            FullTextIndex index = new FullTextIndex();
            var docInfo = item.GetDocInfo();
            if (docInfo != null && docInfo.ContainsKey("CheckinComment"))
            {
                index.VersionComment = docInfo["CheckinComment"] as string;
            }
            index.CreatedByDisplayName = this.author.Title;
            index.CreatedByLoginName = this.author.Login;
            var editor = GetUser("Editor");
            index.ModifiedByDisplayName = editor.Title;
            index.ModifiedByLoginName = editor.Login;
            index.Created = this.created;
            index.Modified = GetVaule<DateTime>("Modified");
            index.Title = GetTitle();
            index.Size = GetVaule<int>("#tp_Size");
            index.Attachments = this.attachments;
            index.ContentTypeName = GetContentTypeName();
            columnValueCache = GetColumnValues(item, level,true);
            index.ColumnValues = columnValueCache;
            index.TimeZoneInfoID = list.ParentWeb.TimeZoneInfoId;
            return index;
        }

        public FullTextIndex GetIndexForAttachment(AveSPItem item, AveSPItem dependItem, FullTextIndexLevel level)
        {
            var index = new FullTextIndex();
            var attachmentInfo = item.GetAttachmentInfo();
            if (attachmentInfo != null)
            {

                if (attachmentInfo.ContainsKey("Created"))
                {
                    index.Created = (DateTime)attachmentInfo["Created"];
                }
                if (attachmentInfo.ContainsKey("Modified"))
                {
                    index.Modified = (DateTime)attachmentInfo["Modified"];
                }
                if (attachmentInfo.ContainsKey("MetaInfo"))
                {
                    byte[] metaInfoCompressByte = attachmentInfo["MetaInfo"] as byte[];
                    if (AveCompressedUtility.IsTCompressedBytes(metaInfoCompressByte))
                    {
                        string metaInfoCompressString = AveCompressedUtility.GetTCompressedString(metaInfoCompressByte);
                        Dictionary<string, string> metaInfoDictionary = AveCompressedUtility.GetMetaInfoDictionary(metaInfoCompressString);
                        if (metaInfoDictionary.ContainsKey("vti_author"))
                        {
                            string loginName = metaInfoDictionary["vti_author"];
                            //从metainfo中取出的loginname是带"\\"的，这样与下面SiteUserInfoCache中取出的login带"\"不匹配，需要替换下
                            loginName = ReplaceUserName(loginName);
                            index.CreatedByLoginName = loginName;
                            index.CreatedByDisplayName = loginName;//SiteUserInfoCache中的User都是allAvailableUser，现在优先赋值为login Name
                            if (item.ParentSite.SiteUserInfoCache != null)
                            {
                                foreach (var user in item.ParentSite.SiteUserInfoCache)
                                {
                                    if (user.Login.Equals(loginName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        index.CreatedByDisplayName = user.Title;
                                        break;
                                    }
                                }
                            }
                        }
                        if (metaInfoDictionary.ContainsKey("vti_modifiedby"))
                        {
                            string modifiedName = metaInfoDictionary["vti_modifiedby"];
                            modifiedName = ReplaceUserName(modifiedName);
                            index.ModifiedByLoginName = modifiedName;
                            index.ModifiedByDisplayName = modifiedName;//SiteUserInfoCache中的User都是allAvailableUser，现在优先赋值为login Name
                            if (item.ParentSite.SiteUserInfoCache != null)
                            {
                                foreach (var user in item.ParentSite.SiteUserInfoCache)
                                {
                                    if (user.Login.Equals(modifiedName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        index.ModifiedByDisplayName = user.Title;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (dependItem != null && dependItem.Id != tempDependItemId)
            {
                InitCacheForAttachment(dependItem, level);
                columnValueCache = GetColumnValues(dependItem, level, true);
                tempDependItemId = dependItem.Id;
            }
            index.ColumnValues = columnValueCache;
            index.TimeZoneInfoID = this.list.ParentWeb.TimeZoneInfoId;
            return index;
        }

        private string ReplaceUserName(string name)
        {
            if (name.Contains("\\\\"))
            {
                name = name.Replace("\\\\", "\\");
            }
            return name;
        }

        internal Dictionary<string, object> GetColumnValues(AveSPItem item, ColumnsLevel getLevel, bool forceGetByAPI)
        {
            var level = FullTextIndexLevel.IncludeAllColumnsAndSystemColumns;
            switch (getLevel)
            {
                case ColumnsLevel.DisplayColumns:
                    level = FullTextIndexLevel.IncludeDefaultViewColumns;
                    break;
                case ColumnsLevel.AllVisiableColumns:
                    level = FullTextIndexLevel.IncludeAllVisiableColumns;
                    break;
                case ColumnsLevel.AllColumns:
                    level = FullTextIndexLevel.IncludeAllColumnsAndSystemColumns;
                    break;
                default:
                    level = FullTextIndexLevel.IncludeAllColumnsAndSystemColumns;
                    break;
            }
            InitCache(item, level);
            return GetColumnValues(item, level, forceGetByAPI);
        }

        private Dictionary<string, object> GetColumnValues(AveSPItem item, FullTextIndexLevel getLevel, bool forceGetByAPI)
        {
            var columnCache = GetColumnCache(item, getLevel, forceGetByAPI);
            return columnCache.GetColumnValues();
        }

        private AveColumnCache GetColumnCache(AveSPItem item, FullTextIndexLevel getLevel, bool forceGetByAPI)
        {
            if (this.CustomColumnCache != null)
            {
                this.CustomColumnCache.Init(item, list, this.indexColumns[getLevel], forceGetByAPI);
                return this.CustomColumnCache;
            }
            else
            {
                return AveColumnCache.CreatInstance(item, list, this.indexColumns[getLevel], getLevel, forceGetByAPI);
            }
        }


        /// <summary>
        /// 某些column在数据库里没有值，需要特殊处理。Get all可以通过API取，不需要此处理
        /// </summary>
        /// <param name="resultValues"></param>
        private void AddSpecialColumnValues(Dictionary<string, object> resultValues)
        {
            //ContentTpye
            resultValues.Add("Content Type", GetContentTypeName());
        }
        private object locker = new object();
        private void InitCache(AveSPItem item, FullTextIndexLevel level)
        {
            if (item == null)
            {
                return;
            }
            lock (locker)
            {
                if (!indexColumns.ContainsKey(level))
                {
                    this.indexColumns.Add(level, this.list.GetColumns(level).ToDictionary(kv => kv.Key, kv => new CachedField(kv.Value)));
                }
            }
            this.item = item;//version 不同 item也不同
            if (item.RowId != this.rowId)
            {
                this.rowId = item.RowId;
                this.attachments = GetAttachments();
                this.author = GetUser("Author");
                this.created = GetVaule<DateTime>("Created");
            }
        }
        /// <summary>
        /// 缓存attachment parentitem的indexcolumn信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        private void InitCacheForAttachment(AveSPItem item, FullTextIndexLevel level)
        {
            if (item == null)
            {
                return;
            }
            lock (locker)
            {
                if (!indexColumns.ContainsKey(level))
                {
                    this.indexColumns.Add(level, this.list.GetColumns(level).ToDictionary(kv => kv.Key, kv => new CachedField(kv.Value)));
                }
            }
        }
        private Guid GetAttachmentFolderId(int itemId)
        {
            if (this.attachmentFolderIds == null)
            {
                if (this.list.SPList.EnableAttachments && this.parentSite.QueryService != null)
                {
                    var attachmentRootFolder = list.ParentWeb.SPWeb.GetFolder(list.ServerRelativeUrl + "/Attachments");
                    if (attachmentRootFolder.Exists)
                    {
                        Guid siteId = this.parentSite.SPSite.ID;
                        this.attachmentFolderIds = this.parentSite.QueryService.GetListAttachmentFolderIds(siteId, attachmentRootFolder.UniqueId);
                    }
                }
                if (this.attachmentFolderIds == null)
                {
                    this.attachmentFolderIds = new Dictionary<int, Guid>();
                }
            }
            return attachmentFolderIds.ContainsKey(itemId) ? attachmentFolderIds[itemId] : Guid.Empty;
        }

        private List<string> GetAttachments()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveIndexCache.SetAttachment"))
            {
                var attachmentFolderId = GetAttachmentFolderId(this.rowId);
                if (attachmentFolderId != Guid.Empty)
                {
                    try
                    {
                        Guid siteId = this.item.BaseItemInfo.SiteId;
                        return this.parentSite.QueryService.GetAttachments(siteId, attachmentFolderId);
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, "Error occurred when get item attachments. Item id:{0}. Reason:{1}.", this.rowId, ex);
                    }
                }
            }
            return new List<string>();
        }

        private AveUserInfo GetUser(string userFiled)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveIndexCache.GetUser"))
            {
                if (this.item.UserDataCache.ContainsKey(userFiled))
                {
                    string userId = this.item.UserDataCache[userFiled].ToString();
                    return TryGetUser(userId);
                }
                return new AveUserInfo();
            }
        }

        private AveUserInfo TryGetUser(string userId)
        {
            int iId;
            if (!Int32.TryParse(userId, out iId))
            {
                return new AveUserInfo();
            }
            try
            {
                if (this.item.ParentSite != null && this.item.ParentSite.DataCache != null)
                {
                    return this.item.ParentSite.DataCache.GetUserInfo(iId) ?? new AveUserInfo();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "Get User:{0}.", e.ToString());
                // return new AveUserInfo();
            }
            return new AveUserInfo();
        }


        private T GetVaule<T>(string fieldName) where T : struct
        {
            if (this.item.UserDataCache.ContainsKey(fieldName))
            {
                return (T)this.item.UserDataCache[fieldName];
            }
            return default(T);
        }

        private string GetContentTypeName()
        {
            string contentTypeName = string.Empty;
            if (this.item.UserDataCache.ContainsKey("#tp_ContentTypeId"))
            {
                var contentTypeId = this.item.UserDataCache["#tp_ContentTypeId"] as byte[];
                this.list.TryGetContentType(contentTypeId, out contentTypeName);
            }
            return contentTypeName;
        }

        private string GetTitle()
        {
            string title = null;
            if (this.item.UserDataCache.ContainsKey("Title"))
            {
                title = this.item.UserDataCache["Title"].ToString();
            }
            return title;
        }

    }

}