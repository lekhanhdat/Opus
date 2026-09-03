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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;

namespace AvePoint.Wrapper.Backup
{

    public abstract class AveColumnCache
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveSPItem item;
        protected AveSPList list;
        protected bool forceGetByAPI = true;
        internal Dictionary<string, CachedField> needGetFields = null;
        private bool init = false;
        protected Dictionary<Guid, StringCollection> WorkflowStatusCache = null;

        protected AveColumnCache() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">所在Item</param>
        /// <param name="list">所在List</param>
        /// <param name="fieldsMapping"></param>
        /// <param name="forceGet"></param>
        protected AveColumnCache(AveSPItem item, AveSPList list, Dictionary<string, CachedField> fieldsMapping, bool forceGet)
        {
            Init(item, list, fieldsMapping, forceGet);
        }

        internal void Init(AveSPItem item, AveSPList list, Dictionary<string, CachedField> fieldsMapping, bool forceGet)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (list == null) throw new ArgumentNullException("list");
            this.item = item;
            this.list = list;
            this.needGetFields = fieldsMapping;
            this.forceGetByAPI = forceGet;
            this.init = true;
        }

        public static AveColumnCache CreatInstance(AveSPItem item, AveSPList list, Dictionary<string, CachedField> fieldsMapping, FullTextIndexLevel level, bool forceGet)
        {
            switch (level)
            {
                case FullTextIndexLevel.IncludeDefaultViewColumns:
                case FullTextIndexLevel.IncludeAllVisiableColumns:
                    return new AveDisPlayNameColumnCache(item, list, fieldsMapping, forceGet);
                case FullTextIndexLevel.IncludeAllColumnsAndSystemColumns:
                    return new AveInternalNameColumnCache(item, list, fieldsMapping, forceGet);
                default:
                    return new AveInternalNameColumnCache(item, list, fieldsMapping, forceGet);
            }
        }
        public Dictionary<string, object> GetColumnValues()
        {
            if (!this.init) throw new InvalidOperationException("AveColumnCache is not initialized.");
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveIndexCache.GetColumnValues"))
            {
                if (this.item.UserDataCache != null && this.item.UserDataCache.Count > 0)
                {
                    if (this.needGetFields != null)
                    {
                        return RealGetColumnValues();
                    }
                }
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// 不同子类，实现不同方法。父类暂不实现。
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<string, object> RealGetColumnValues();

        protected virtual object GetFieldValue(CachedField field)
        {
            object value = GetFieldValueFromUserData(field.InternalName, field.ColName, item.UserDataCache);
            value = HandleFieldValue(field, value);

            if (value == null && forceGetByAPI && string.IsNullOrEmpty(field.ColName))
            {
                value = GetFieldValueByAPI(field.InternalName);
            }
            return value;
        }



        /// <summary>
        /// Get column value form UserDataCache.
        /// </summary>
        /// <param name="backupBame"></param>
        /// <param name="columnName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private object GetFieldValueFromUserData(string backupBame, string columnName, Dictionary<string, object> data)
        {
            if (data.ContainsKey(backupBame))
            {
                return data[backupBame];
            }
            return GetFieldValueByColName(columnName, data);
        }
        private object GetFieldValueByColName(string columnName, Dictionary<string, object> data)
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

        private object GetFieldValueByAPI(string fieldName)
        {
            if (!this.forceGetByAPI) throw new InvalidOperationException("Cannot invoke AveColumnCache.GetFieldValueByAPI if AveColumnCache.forceGetByAPI is false.");
            object returnValue = null;
            try
            {
                if (this.item.SPListItem.Fields.ContainsField(fieldName))
                {
                    var field = this.item.SPListItem.Fields.GetField(fieldName);
                    returnValue = field.GetFieldValueAsText(GetFieldValueByAPI(field));
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, WrapperBackupResource.GetFieldValueFailed, fieldName, ex);
            }
            return returnValue;
        }
        //one column failed should not affect the whole item
        /// <summary>
        /// 调用此方法需要调用API
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private object GetFieldValueByAPI(IAveField field)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveColumnCache.GetFieldValueByAPI"))
            {
                object result = null;
                try
                {
                    if (this.item.BaseItemInfo.IsVersion)
                    {
                        //get 不到document 的check out 或者hold 的version                
                        var versionItem = item.SPListItem.Versions.GetVersionFromID(this.item.BaseItemInfo.Version);
                        result = versionItem[field.InternalName];
                    }
                    if (result == null)
                    {
                        result = item.SPListItem[field.InternalName];
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.GetFieldValueFailed, field.InternalName, e);
                    return null;
                }
                //通过API获取的SPFeildUserValue转换成string格式“1073741823:#System Account”,而UserMulti类型的可以正常转换
                if (field.TypeAsString.Equals("User", StringComparison.OrdinalIgnoreCase) && result != null)
                {
                    var index = result.ToString().IndexOf(";#", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        result = result.ToString().Substring(index + 2);
                    }
                }
                return result;
            }
        }

        private object HandleFieldValue(CachedField field, object value)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveColumnCache.HandleFieldValue"))
            {
                object returnValue = null;
                switch (field.TypeAsString.ToUpperInvariant())
                {
                    case "USER":
                        if (value != null)
                        {
                            returnValue = GetUserValue(TryGetUser(value.ToString()));
                        }
                        break;
                    case "LOOKUP":
                    case "LOOKUPMULTI":
                    case "USERMULTI":
                        returnValue = forceGetByAPI ? HandleLookupField(field.InternalName, value) : value;
                        break;
                    case "CONTENTTYPEID":
                    case "THREADINDEX":
                        if (value != null && value is byte[])
                        {
                            returnValue = AveConvert.HexStringFromBytes((byte[])value);
                        }
                        break;
                    case "WORKFLOWSTATUS":
                        if (value != null)
                        {
                            if (item.ParentSite.QueryService == null)
                            {
                                log.Debug("Can not get the workflow status with BPOS API.");
                            }
                            else
                            {
                                //自定义的workflow status无法通过sql查询后转换,需要用API通过status field中的choice获取
                                int workflowStatus = (int)item.QueryService.GetWorkflowStatus(value.ToString());
                                returnValue = HandleWorkflowStatusFieldValue(field, workflowStatus);
                            }
                        }
                        break;
                    case "MODSTAT":
                        if (value != null)
                        {
                            returnValue = GetModerationStatusValue((AveModerationStatusType)value);
                        }
                        break;
                    case "DATETIME":
                        if (value != null)
                        {
                            returnValue = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
                        }
                        break;
                    case "TAXONOMYFIELDTYPE":
                    case "TAXONOMYFIELDTYPEMULTI":
                        returnValue = GetMetaDataValueInCache(field, value);
                        break;
                    case "URL":
                        returnValue = GetUrlValue(field.InternalName);
                        break;
                    case "CALCULATED"://To handle the "Calculated Column" cannot give the right data which is from UserData. Use API to realize.
                    case "CURRENCY":
                        //Merge CI[CI-30114]: 此处应该使用fouceGetByAPI控制是否使用API获取，无控制情况下会出现效率问题。
                        returnValue = forceGetByAPI ? GetFieldValueByAPI(field.InternalName) : value;
                        break;
                    case "TEXT":
                        returnValue = value;
                        if (value != null && (field.InternalName.Equals("Created_x0020_By", StringComparison.OrdinalIgnoreCase) || field.InternalName.Equals("Modified_x0020_By", StringComparison.OrdinalIgnoreCase)))
                        {
                            var tmpValue = value as String;
                            return tmpValue.IndexOf('|') > 0 ? tmpValue.Substring(tmpValue.IndexOf('|') + 1) : tmpValue;
                        }
                        break;
                    default:
                        returnValue = value;
                        break;
                }
                return returnValue;
            }
        }

        private object HandleWorkflowStatusFieldValue(CachedField field, int workflowStatus)
        {
            object returnValue;
            try
            {
                if (WorkflowStatusCache == null)
                {
                    WorkflowStatusCache = new Dictionary<Guid, StringCollection>();
                }
                if (!WorkflowStatusCache.ContainsKey(field.Id) && item.SPListItem != null && item.SPListItem.Fields.ContainsField(field.InternalName))
                {
                    var aveField = item.SPListItem.Fields.GetField(field.InternalName) as IAveFieldWorkflowStatus;
                    if (aveField != null)
                    {
                        WorkflowStatusCache.Add(field.Id, aveField.Choices);
                    }
                }
                StringCollection choices;
                if (WorkflowStatusCache.TryGetValue(field.Id, out choices))
                {
                    returnValue = choices[workflowStatus];
                }
                else
                {
                    returnValue = (AveWorkflowStatus)workflowStatus;
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while translate workflow status field value to string.Value:{0},Error:{1}", workflowStatus, e);
                returnValue = (AveWorkflowStatus)workflowStatus;
            }
            return returnValue;
        }

        #region User
        internal virtual object GetUserValue(AveUserInfo user)
        {
            //FBA 和 SP13是混合认证模式
            if (!string.IsNullOrEmpty(user.Login) && user.Login.IndexOf('|') > 0)
            {
                return user.Login.Substring(user.Login.IndexOf('|') + 1);
            }
            else
            {
                return user.Login;
            }
        }

        internal virtual object GetModerationStatusValue(AveModerationStatusType status)
        {
            return status.ToString();
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
        #endregion
        #region URL
        private object GetUrlValue(String internalName)
        {
            String urlRealDataName = internalName + "#2";
            if (item.UserDataCache.ContainsKey(urlRealDataName))
            {
                return item.UserDataCache[urlRealDataName].ToString();
            }
            return null;
        }
        #endregion
        #region MateData
        private object GetMetaDataValueInCache(CachedField fieldInfo, object value)
        {
            object resultMetaDataValue = null;
            string metaDisplayName = fieldInfo.InternalName + "_0";
            foreach (AveSPField field in list.Fields.FieldMap.Values)
            {
                if (field.FieldType.ToString().ToUpper(CultureInfo.InvariantCulture).Equals("NOTE", StringComparison.OrdinalIgnoreCase) && field.DisplayName.Equals(metaDisplayName))
                {
                    if (!item.UserDataCache.ContainsKey(field.BackupName))
                    {
                        break;
                    }
                    var tempValue = item.UserDataCache[field.BackupName].ToString();
                    resultMetaDataValue = DeleteTermId(tempValue);
                    break;
                }
            }
            // 处理Enterprise Keywords
            if (resultMetaDataValue == null && fieldInfo.Id.Equals(new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38")))
            {
                if (item.UserDataCache.ContainsKey("TaxKeywordTaxHTField"))
                {
                    var tempValue = item.UserDataCache["TaxKeywordTaxHTField"].ToString();
                    resultMetaDataValue = DeleteTermId(tempValue);
                }
            }
            if (resultMetaDataValue == null && forceGetByAPI)
            {
                resultMetaDataValue = GetFieldValueByAPI(fieldInfo.InternalName);
            }
            return resultMetaDataValue;
        }

        private string DeleteTermId(string tempValue)
        {
            try
            {
                var index = tempValue.IndexOf('|');
                if (index >= 0)
                {
                    tempValue = tempValue.Remove(index, 37);
                    return DeleteTermId(tempValue);
                }
                else
                {
                    return tempValue;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperBackupResource.GetFieldValueFailed, tempValue, e);
                return tempValue;
            }
        }
        #endregion

        #region Look up field

        private object HandleLookupField(string internalName, object value)
        {
            if (this.item.SPListItem.Fields.ContainsField(internalName))
            {
                var field = this.item.SPListItem.Fields.GetField(internalName);
                //通过item取到的数据本来就是local 时间，调用GetFieldValueAsText（）是把UTC时间转化为local时间
                if (field.InternalName == "Last_x0020_Modified" || field.InternalName == "Created_x0020_Date")
                {
                    return DateTime.Parse(GetFieldValueByAPI(field).ToString()).ToUniversalTime();
                }
                else
                {
                    return field.GetFieldValueAsText(GetFieldValueByAPI(field));
                }
            }
            else if (value != null)
            {
                return GetLookupFieldValueInCache(internalName, value.ToString());
            }
            else
            {
                return null;
            }
        }
        private object GetLookupFieldValueInCache(string fieldName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (this.list.AveIndexCache.lookupValues == null)
                {
                    this.list.AveIndexCache.lookupValues = new Dictionary<string, Dictionary<int, object>>(StringComparer.OrdinalIgnoreCase);
                }
                if (!this.list.AveIndexCache.lookupValues.ContainsKey(fieldName))
                {
                    this.list.AveIndexCache.lookupValues.Add(fieldName, new Dictionary<int, object>());
                }
                var itemId = int.Parse(value);
                if (!this.list.AveIndexCache.lookupValues[fieldName].ContainsKey(itemId))
                {
                    this.list.AveIndexCache.lookupValues[fieldName].Add(itemId, GetLookupFieldValue(fieldName, itemId));
                }
                return this.list.AveIndexCache.lookupValues[fieldName][itemId];
            }
            return string.Empty;
        }

        /// <summary>
        /// 调用此方法需要调用API
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
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
                            return forceGetByAPI ? HandleLookupField(field.InternalName, value) : value;
                        }
                    }
                }
                return itemId.ToString();
            }
        }

        private IAveList GetLookupListInCache(string fieldName)
        {
            if (this.list.AveIndexCache.lookupLists == null)
            {
                this.list.AveIndexCache.lookupLists = new Dictionary<string, IAveList>(StringComparer.OrdinalIgnoreCase);
            }
            if (!this.list.AveIndexCache.lookupLists.ContainsKey(fieldName))
            {
                this.list.AveIndexCache.lookupLists.Add(fieldName, GetLookupList(fieldName));
            }
            return this.list.AveIndexCache.lookupLists[fieldName];
        }
        /// <summary>
        /// 调用此方法需要调用API
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private IAveList GetLookupList(string fieldName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveColumnCache.GetLookupList"))
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
                        web = this.item.ParentSite.SPSite.OpenWeb(field.LookupWebId);
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
        }
        #endregion

    }

    public class CachedField
    {
        public CachedField(AveSPField field)
        {
            this.Id = field.FieldId;
            this.ColName = field.ColumnName;
            this.InternalName = field.BackupName;
            this.TypeAsString = field.FieldTypeAsString;
            this.Title = field.DisplayName;
        }

        /// <summary>
        /// IAveField.ID
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// IAveField.ColName
        /// </summary>
        public string ColName { get; private set; }
        /// <summary>
        /// IAveField.InternalName
        /// </summary>
        public string InternalName { get; private set; }

        /// <summary>
        /// IAveField.TypeAsString
        /// </summary>
        public string TypeAsString { get; private set; }
        //public AveFieldType FieldType { get; private set; }

        /// <summary>
        /// IAveField.Title
        /// </summary>
        public string Title { get; private set; }
    }

    public class AveDisPlayNameColumnCache : AveColumnCache
    {
        protected AveDisPlayNameColumnCache() { }

        public AveDisPlayNameColumnCache(AveSPItem item, AveSPList list, Dictionary<string, CachedField> fieldsMapping, bool fouceGet)
            : base(item, list, fieldsMapping, fouceGet)
        { }

        protected override Dictionary<string, object> RealGetColumnValues()
        {
            return needGetFields.Distinct(new AveFieldEqualityByDisplayNameComparer()).ToDictionary(kv => kv.Value.InternalName,
                kv =>
                {
                    return GetFieldValue(kv.Value);
                });
        }

        internal override object GetUserValue(AveUserInfo user)
        {
            return user.Title;
        }

        internal override object GetModerationStatusValue(AveModerationStatusType status)
        {
            return status == AveModerationStatusType.Denied ? "Rejected" : status.ToString();
        }

        private class AveFieldEqualityByDisplayNameComparer : IEqualityComparer<KeyValuePair<string, CachedField>>
        {
            public bool Equals(KeyValuePair<string, CachedField> x, KeyValuePair<string, CachedField> y)
            {
                return string.Equals(x.Value.InternalName, y.Value.InternalName);
            }

            public int GetHashCode(KeyValuePair<string, CachedField> obj)
            {
                return obj.Value.InternalName.GetHashCode();
            }
        }
    }

    public class AveInternalNameColumnCache : AveColumnCache
    {
        protected AveInternalNameColumnCache() { }

        public AveInternalNameColumnCache(AveSPItem item, AveSPList list, Dictionary<string, CachedField> fieldsMapping, bool fouceGet)
            : base(item, list, fieldsMapping, fouceGet)
        { }

        protected override Dictionary<string, object> RealGetColumnValues()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveIndexCache.GetAllColumnValues_1"))
            {
                return needGetFields.Values.ToDictionary(field => field.InternalName,
                    field =>
                    {
                        return GetFieldValue(field);
                    });
            }
        }
    }
}
