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
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Backup
{
    public class AveDataCache
    {
        protected Dictionary<int, object> PrincipalIdCache = new Dictionary<int, object>();
        protected readonly object obj = new object();

        public AveUserList UserList = new AveUserList();
        public AveGroupList GroupList = new AveGroupList();

        public AveUserList GetUsersForExport()
        {//Export后清空Users，防止重复备份，不需要清空principalIdCache，避免再次Cache的时候重复加User或者Group
            var users = this.UserList;
            this.UserList = new AveUserList();
            return users;
        }

        public AveGroupList GetGroupsForExport()
        {//Export后清空Groups，防止重复备份，不需要清空principalIdCache，避免再次Cache的时候重复加User或者Group
            var groups = this.GroupList;
            this.GroupList = new AveGroupList();
            return groups;
        }

        public void AddToCache(int principalId, AveUserInfo userInfo)
        {
            if (!PrincipalIdCache.ContainsKey(principalId))
            {
                PrincipalIdCache.Add(principalId, obj);
                UserList.Users.Add(userInfo);
            }
        }

        public void AddToCache(int principalId, AveGroupInfo groupInfo)
        {
            if (!PrincipalIdCache.ContainsKey(principalId))
            {
                PrincipalIdCache.Add(principalId, obj);
                GroupList.Groups.Add(groupInfo);
            }
        }

        public void AddToCache(int principalId)//当某个Group被删掉后，就找不到这个Group信息了，为了不重复查找，把它的Id加到这里
        {
            if (!PrincipalIdCache.ContainsKey(principalId))
            {
                PrincipalIdCache.Add(principalId, obj);
            }
        }

        public bool principalIdAlreadyExists(int principalId)
        {
            return PrincipalIdCache.ContainsKey(principalId);
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
            AveUserInfo userInfo = mAveSPSite.SPSite.UserSerializer.GetObjectData(principalId) as AveUserInfo;
            if (userInfo != null)
            {
                UserCache.Add(principalId, userInfo);
                return userInfo;
            }
            AveGroupInfo groupInfo = mAveSPSite.SPSite.GroupSerializer.GetObjectData(principalId) as AveGroupInfo;
            GetGroupInfoWithMembers(groupInfo);
            if (groupInfo != null)
            {
                GroupCache.Add(principalId, groupInfo);
            }
            return groupInfo;
        }

        private void GetGroupInfoWithMembers(AveGroupInfo groupInfo)
        {
            if (groupInfo != null && groupInfo.Members.Count == 0)
            {
                foreach (int i in groupInfo.Memberships)
                {
                    AveUserInfo userInfo = (AveUserInfo)GetPrincipalInfo(i);
                    groupInfo.Members.Add(userInfo);
                }
            }
        }

        //add for RevIM export
        internal AveUserInfo GetUserInfo(int userId)
        {
            if (UserCache.Contains(userId))
            {
                return UserCache.GetUserInfo(userId);
            }
            AveUserInfo userInfo = mAveSPSite.SPSite.UserSerializer.GetObjectData(userId);
            if (userInfo != null)
            {
                UserCache.Add(userId, userInfo);
            }
            return userInfo;
        }
    }

    public class AveUserCache
    {
        private Dictionary<int, AveUserInfo> mCache = new Dictionary<int, AveUserInfo>();
        private int mCapacity;
        private readonly object mLock = new object();

        public AveUserCache(int capacity)
        {
            mCapacity = capacity;
        }

        public AveUserInfo GetUserInfo(int principalId)
        {
            AveUserInfo userInfo = null;

            lock (mLock)
            {
                if (mCache.ContainsKey(principalId))
                {
                    userInfo = mCache[principalId];
                }
                else
                {
                    //TO DO
                }
            }
            return userInfo;
        }

        public bool Contains(int id)
        {
            lock (mLock)
            {
                return mCache.ContainsKey(id);
            }
        }

        public void Add(int id, AveUserInfo userInfo)
        {
            lock (mLock)
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
        private readonly object mLock = new object();

        public AveGroupCache(int capacity)
        {
            mCapacity = capacity;
        }

        public AveGroupInfo GetGroupInfo(int principalId)
        {
            AveGroupInfo groupInfo = null;
            lock (mLock)
            {
                if (mCache.ContainsKey(principalId))
                {
                    groupInfo = mCache[principalId];
                }
                else
                {
                    //TO DO
                }
            }
            return groupInfo;
        }

        public bool Contains(int id)
        {
            lock (mLock)
            {
                return mCache.ContainsKey(id);
            }
        }

        public void Add(int id, AveGroupInfo groupInfo)
        {
            lock (mLock)
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
        private Dictionary<string, AveSPField> indexColumns = null;

        private FullTextIndexLevel level = FullTextIndexLevel.BaseInfo;

        private List<string> attachments = new List<string>();
        private AveUserInfo author = new AveUserInfo();
        private DateTime created;

        internal Dictionary<string, Dictionary<int, object>> lookupValues = null;
        internal Dictionary<string, IAveList> lookupLists = null;
        Dictionary<int, Guid> attachmentFolderIds = null;
        private readonly object mLock = new object();

        public AveIndexCache(AveSPList list)
        {
            this.list = list;
            this.parentSite = list.ParentSite;
        }

        public FullTextIndex GetIndex(AveSPItem item, FullTextIndexLevel level)
        {
            lock (mLock)
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
                index.Size = GetVaule<int>("#tp_Size");
                index.Attachments = this.attachments;
                index.ContentTypeName = GetContentTypeName();
                index.ColumnValues = GetColumnValues(false);
                index.TimeZoneInfoID = list.ParentWeb.TimeZoneInfoId;
                return index;
            }
        }

        public FullTextIndex GetIndexForAttachment(AveSPItem item)
        {
            var index = new FullTextIndex();
            var attachmentInfo = item.GetAttachmentInfo();
            if (attachmentInfo != null && attachmentInfo.ContainsKey("Created"))
            {
                index.Created = (DateTime)attachmentInfo["Created"];
            }
            if (attachmentInfo != null && attachmentInfo.ContainsKey("Modified"))
            {
                index.Modified = (DateTime)attachmentInfo["Modified"];
            }
            index.TimeZoneInfoID = this.list.ParentWeb.TimeZoneInfoId;
            return index; 
        }

        public Dictionary<string, object> GetAllColumnValues(AveSPItem item, ColumnsLevel getLevel = ColumnsLevel.None)
        {
            lock (mLock)
            {
                var isGetAll = true;
                switch (getLevel)
                {
                    case ColumnsLevel.DisplayColumns:
                        level = FullTextIndexLevel.IncludeDefaultViewColumns;
                        isGetAll = false;
                        break;
                    case ColumnsLevel.AllVisiableColumns:
                        level = FullTextIndexLevel.IncludeAllVisiableColumns;
                        //InitCache(item, FullTextIndexLevel.IncludeAllVisiableColumns);
                        isGetAll = false;
                        break;
                    case ColumnsLevel.AllColumns:
                        level = FullTextIndexLevel.IncludeAllColumnsAndSystemColumns;
                        //InitCache(item, FullTextIndexLevel.IncludeAllColumnsAndSystemColumns);
                        break;
                    default:
                        level = FullTextIndexLevel.IncludeAllColumnsAndSystemColumns;
                        //InitCache(item, FullTextIndexLevel.IncludeAllColumnsAndSystemColumns);
                        break;
                }
                InitCache(item, level);
                var resultValues = GetColumnValues(isGetAll);
                if (!isGetAll && resultValues != null)
                {
                    AddSpecialColumnValues(resultValues);
                }
                return resultValues;
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

        private void InitCache(AveSPItem item, FullTextIndexLevel level)
        {
            if (item == null)
            {
                return;
            }
            this.item = item;//version 不同 item也不同
            if (item.RowId != this.rowId)
            {
                this.rowId = item.RowId;
                //if (this.level != level)
                //{
                    //this.level = level;
                if (indexColumns == null)
                {
                    this.indexColumns = this.list.GetColumns(this.level == FullTextIndexLevel.IncludeDefaultViewColumns, this.level > FullTextIndexLevel.IncludeAllVisiableColumns, this.level == FullTextIndexLevel.IncludeAllColumnsAndSystemColumns);
                }
                //}
                this.attachments = GetAttachments();
                this.author = GetUser("Author");
                this.created = GetVaule<DateTime>("Created");
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
                        this.attachmentFolderIds = this.parentSite.QueryService.GetListAttchmentFolderIds(siteId, attachmentRootFolder.UniqueId);
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
                if (this.item.UserDataCache != null)
                {
                    if (this.item.UserDataCache.ContainsKey(userFiled))
                    {
                        string userId = this.item.UserDataCache[userFiled].ToString();
                        return TryGetUser(userId);
                    }
                }
                return new AveUserInfo();
            }
        }

        private T GetVaule<T>(string fieldName) where T : struct
        {
            if (this.item.UserDataCache != null)
            {
                if (this.item.UserDataCache.ContainsKey(fieldName))
                {
                    return (T)this.item.UserDataCache[fieldName];
                }
            }
            return default(T);
        }

        private string GetContentTypeName()
        {
            string contentTypeName = string.Empty;
            if (this.item.UserDataCache != null && this.item.UserDataCache.ContainsKey("#tp_ContentTypeId"))
            {
                var contentTypeId = this.item.UserDataCache["#tp_ContentTypeId"] as byte[];
                this.list.TryGetContentType(contentTypeId, out contentTypeName);
            }
            return contentTypeName;
        }

        private Dictionary<string, object> GetColumnValues(bool includeAllColumns)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveIndexCache.GetColumnValues"))
            {
                if (this.item.UserDataCache != null)
                {
                    if (this.indexColumns != null)
                    {
                        return includeAllColumns ?
                            GetAllColumnValues(this.item.UserDataCache, this.indexColumns) :
                            GetColumnValues(this.item.UserDataCache, this.indexColumns);
                    }
                }
                return null;
            }
        }

        private Dictionary<string, object> GetColumnValues(Dictionary<string, object> data, Dictionary<string, AveSPField> filedMapping)
        {
            return filedMapping.Distinct(new AveFieldEqualityByDisplayNameComparer()).Where(kv => GetColumnValue(kv.Key, kv.Value.ColumnName, data) != null).ToDictionary(kv => kv.Value.DisplayName,
                kv =>
                {
                    return GetFieldValue(kv.Value.FieldType, kv.Key, GetColumnValue(kv.Key, kv.Value.ColumnName, data), false);
                });
        }

        private Dictionary<string, object> GetAllColumnValues(Dictionary<string, object> data, Dictionary<string, AveSPField> fieldMapping)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveIndexCache.GetAllColumnValues_1"))
            {
                return fieldMapping.Values.ToDictionary(field => field.BackupName,
                    field =>
                    {
                        object value = GetColumnValue(field.BackupName, field.ColumnName, data);
                        return GetFieldValue(field.FieldType, field.BackupName, value, true);
                    });
            }
        }

        private object GetColumnValue(string backupBame, string columnName, Dictionary<string, object> data)
        {
            if (data.ContainsKey(backupBame))
            {
                return data[backupBame];
            }
            return GetColumnValueByColName(columnName, data);
        }

        private object GetColumnValueByColName(string columnName, Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return null;
            }
            if (data.ContainsKey(columnName))
            {
                return data[columnName];
            }
            if (columnName.StartsWith("tp_", StringComparison.OrdinalIgnoreCase) && data.ContainsKey("#" + columnName))
            {
                return data["#" + columnName];
            }
            return null;
        }

        private object GetFieldValue(AveFieldType fieldType, string fieldName, object value, bool fouceGet)
        {
            object returnValue = null;
            switch (fieldType)
            {
                case AveFieldType.User:
                    if (value != null)
                    {
                        returnValue = TryGetUser(value.ToString()).Login;
                    }
                    break;
                case AveFieldType.Lookup:
                    if (this.item.SPListItem.Fields.ContainsField(fieldName))
                    {
                        var field = this.item.SPListItem.Fields.GetField(fieldName);
                        //通过item取到的数据本来就是local 时间，调用GetFieldValueAsText（）是把UTC时间转化为local时间
                        if (field.InternalName == "Last_x0020_Modified" || field.InternalName == "Created_x0020_Date")
                        {
                            returnValue = DateTime.Parse(GetFieldValue(field).ToString()).ToUniversalTime().ToString();
                        }
                        else
                        {
                            returnValue = field.GetFieldValueAsText(GetFieldValue(field));
                        }
                    }
                    else if (value != null)
                    {
                        returnValue = GetLookupFieldValueInCache(fieldName, value.ToString());
                    }
                    break;
                case AveFieldType.ContentTypeId:
                case AveFieldType.ThreadIndex:
                    if (value != null && value is byte[])
                    {
                        returnValue = AveConvert.HexStringFromBytes((byte[])value);
                    }
                    break;
                case AveFieldType.WorkflowStatus:
                    if (value != null)
                    {
                        returnValue = string.Empty;
                        log.Info("Skipping getting the workflow status with BPOS API.Return empty value as default.");
                        //returnValue = item.ParentSite.QueryService.GetWorkflowStatus(value.ToString()).ToString();
                    }
                    break;
                case AveFieldType.ModStat:
                    if (value != null)
                    {
                        returnValue = ((AveModerationStatusType)value).ToString();
                    }
                    break;
                default:
                    returnValue = value;
                    break;
            }
            if (returnValue == null && fouceGet)
            {
                try
                {
                    if (this.item.SPListItem.Fields.ContainsField(fieldName))
                    {
                        var field = this.item.SPListItem.Fields.GetField(fieldName);
                        returnValue = field.GetFieldValueAsText(GetFieldValue(field));
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperBackupResource.GetFieldValueFailed, fieldName, ex);
                }
            }
            return returnValue;
        }

        private object GetFieldValue(IAveField field)
        {

            if (this.item.BaseItemInfo.IsVersion)
            {
                //get 不到document 的check out 或者hold 的version
                var versionItem = item.SPListItem.Versions.GetVersionFromID(this.item.BaseItemInfo.Version);
                return versionItem[field.InternalName];
            }
            else
            {
                return item.SPListItem[field.InternalName];
            }
        }

        private object GetLookupFieldValueInCache(string fieldName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (this.lookupValues == null)
                {
                    lookupValues = new Dictionary<string, Dictionary<int, object>>(StringComparer.OrdinalIgnoreCase);
                }
                if (!this.lookupValues.ContainsKey(fieldName))
                {
                    lookupValues.Add(fieldName, new Dictionary<int, object>());
                }
                var itemId = int.Parse(value);
                if (!this.lookupValues[fieldName].ContainsKey(itemId))
                {
                    this.lookupValues[fieldName].Add(itemId, GetLookupFieldValue(fieldName, itemId));
                }
                return this.lookupValues[fieldName][itemId];
            }
            return string.Empty;
        }

        private object GetLookupFieldValue(string fieldName, int itemId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveIndexCache.GetLookupFieldValue"))
            {
                var lookupList = GetLookupListInCache(fieldName);
                if (lookupList != null)
                {
                    var item = lookupList.GetItemById(itemId);
                    var field = this.list.SPList.Fields[fieldName] as IAveFieldLookup;
                    if (item != null && lookupList.Fields.ContainsField(field.LookupField))
                    {
                        var lookupField = lookupList.Fields[field.LookupField];
                        var value = item[field.LookupField];
                        if (value != null)
                        {
                            return GetFieldValue(lookupField.Type, field.InternalName, value, false);
                        }
                    }
                }
                return itemId.ToString();
            }
        }

        private IAveList GetLookupListInCache(string fieldName)
        {
            if (this.lookupLists == null)
            {
                this.lookupLists = new Dictionary<string, IAveList>(StringComparer.OrdinalIgnoreCase);
            }
            if (!this.lookupLists.ContainsKey(fieldName))
            {
                this.lookupLists.Add(fieldName, GetLookupList(fieldName));
            }
            return this.lookupLists[fieldName];
        }

        private IAveList GetLookupList(string fieldName)
        {
            var field = this.list.SPList.Fields[fieldName] as IAveFieldLookup;
            if (field == null)
            {
                log.Log(AveLogLevel.WARN, "Can not find lookup field by name:{0}.", fieldName);
                return null;
            }

            var web = this.list.ParentWeb.SPWeb;
            bool isCurrentWeb = true;
            if (web.ID != field.LookupWebId)
            {
                try
                {
                    web = this.parentSite.SPSite.OpenWeb(field.LookupWebId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "Can not find lookup field linked web. field name:{0}, web id:{1}. Reason:{2}.", fieldName, field.LookupWebId, ex);
                    return null;
                }
                isCurrentWeb = false;
            }
            try
            {
                var listId = new Guid(field.LookupList);
                return web.Lists[listId];
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "Can not find lookup field linked list. field name:{0}, fist name:{1}. Reason:{2}.", fieldName, field.LookupList, ex);
                return null;
            }
            finally
            {
                if (!isCurrentWeb && web != null)
                {
                    web.Dispose();
                }
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
                if (this.parentSite != null && this.parentSite.DataCache != null)
                {
                    return this.parentSite.DataCache.GetPrincipalInfo(iId) as AveUserInfo ?? new AveUserInfo();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "Get User:{0}.", e.ToString());
                // return new AveUserInfo();
            }
            return new AveUserInfo();
        }

        private class AveFieldEqualityByDisplayNameComparer : IEqualityComparer<KeyValuePair<string, AveSPField>>
        {
            public bool Equals(KeyValuePair<string, AveSPField> x, KeyValuePair<string, AveSPField> y)
            {
                return string.Equals(x.Value.DisplayName, y.Value.DisplayName);
            }

            public int GetHashCode(KeyValuePair<string, AveSPField> obj)
            {
                return obj.Value.DisplayName.GetHashCode();
            }
        }
    }
}