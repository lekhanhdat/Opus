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
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.RACommonUtility.Extension
{
    public static class AveListItemExtension
    {
        private static RALogger mLog = RALogger.GetInstance(typeof(AveListItemExtension));
        private static string key = "3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E";
        public static bool CheckIsRecord(this IAveListItem item)
        {
            bool isRecord = false;
            int result = 0;
            try
            {
                object obj = item[new Guid(key)];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                mLog.Debug(ex.Message);
                result = 0;
            }
            if ((result & 0x1000) != 0 || (result & 0x10) != 0 || (result & 1) != 0)
            {
                isRecord = true;
            }
            return isRecord;
        }

        /// <summary>
        /// 此方法返回True 时，表示是hold，不一定是不是Declare；但是返回False 时，一定不是hold。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool CheckHasHold(this IAveListItem item)
        {
            bool hasHold = false;
            int result = 0;
            try
            {
                object obj = item[new Guid(key)];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                mLog.Debug("This Item is not On Hold " + ex.Message);
                result = 0;
            }
            if ((result & 0x1000) != 0 && ((result & 1) != 0 || (result & 0x10) != 0))
            {
                //进入这里说明 Item 是Hold 的 不一定是不是Declare 的
                hasHold = true;
            }
            return hasHold;
        }

        public static bool GetSingleTaxonomyFieldValue(this IAveListItem item, string fieldName, out Guid termId, out string termName)
        {
            bool result = true;
            termName = string.Empty;
            termId = new Guid();
            try
            {
                var fileObj = item[fieldName];
                if (fileObj == null)
                {
                    //有时 RevIMBCS column 没有值，需要利用其关联的hidde field来获取value。
                    try
                    {
                        fieldName = item.Fields.GetById((item.Fields.GetField("RevIMBCS") as IAveTaxonomyField).TextField).InternalName;
                        fileObj = item[fieldName];
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("get RevIMBCS column associated hidden column error: {0}", e.ToString());
                    }
                    if (fileObj == null)
                    {
                        return false;
                    }
                }
                if (fileObj.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                {
                    var dic = ((Dictionary<string, object>)item[fieldName]);
                    termName = dic["Label"].ToString();
                    termId = new Guid(dic["TermGuid"].ToString());
                }
                else
                {
                    var valueString = item[fieldName].ToString();
                    var values = valueString.Split('|');
                    termId = new Guid(values[1]);
                    termName = values[0];
                }
                //如果term以full path形式显示会包含“:”，Content Due和Term Usage Report要求只显示Name，不显示路径
                if (termName.Contains(":"))
                {
                    termName = termName.Substring(termName.LastIndexOf(":") + 1);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Get single taxonomy field value failed! Item url: {0}, fieldName: {1}, error message: {2}.", item.Url, fieldName, ex.ToString());
                result = false;
            }
            return result;
        }

        public static bool IsBlockEditAndDeleteRecord(this IAveListItem item)
        {
            return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }

        public static bool IsBlockDeleteOnlyRecord(this IAveListItem item)
        {
            return IsBlockDeleteOnlyRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockDeleteOnlyRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.RecordMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.DeleteBlockedMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.EditBlockedMask)) == 0L);
        }

        public static string GetSingleUserFieldValue(this IAveListItem item, string fieldName)
        {
            string userName = string.Empty;
            try
            {
                string fieldValue = item[fieldName].ToString();
                if (fieldValue.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                {
                    fieldValue = fieldValue.Substring("i:0#.w|".Length);
                }
                if (fieldValue.IndexOf(";#") == -1)
                {
                    userName = fieldValue;
                }
                else
                {
                    var userValues = fieldValue.Split(new string[] { ";#" }, StringSplitOptions.None);
                    userName = userValues[1];
                }

            }
            catch (Exception ex)
            {
                mLog.Warn("Get single user field value failed! Item url: {0}, fieldName: {1}, error message: {2}.", item.Url, fieldName, ex.ToString());
            }
            return userName;
        }

        public static DateTime GetUTCDateWithTimeZone(this IAveListItem item, string fieldName)
        {
            DateTime dateTime = DateTime.MinValue;
            if (item.FieldValues.ContainsKey(fieldName))
            {
                try
                {
                    var date = (DateTime)item[fieldName];
                    if (date.Kind == DateTimeKind.Utc)
                    {
                        dateTime = date;
                    }
                    else if (date.Kind == DateTimeKind.Local)
                    {
                        dateTime = date.ToUniversalTime();
                    }
                    else
                    {
                        dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, AveTimeZoneUtility.ToTimeZoneInfoId(item.ParentList.ParentWeb.RegionalSettings.TimeZone.ID), "UTC");
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while getting utc time for field:{0} error:{1}", fieldName, e.ToString());
                    dateTime = Convert.ToDateTime(item.FieldValues[fieldName]);
                }
            }
            return dateTime;
        }

        public static DateTime GetDateTimeFieldValue(this IAveListItem item, IAveTimeZone SPWebTimeZone, string fieldName)
        {
            var dt = DateTime.Parse(item[fieldName].ToString());

            try
            {
                TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(SPWebTimeZone.ID));
                var utcDateTime = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                var dt0 = dt + cstZone.GetUtcOffset(utcDateTime);
                return dt0;
                //if (RegionalSetting != null)
                //{
                //    var utcTime = RegionalSetting.TimeZone.UTCToLocalTime(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified));
                //    context.ExecuteQuery();
                //    return utcTime.Value;
                //}
                //else
                //{
                //    //dt = DateTime.Parse(item[fieldName].ToString());
                //    return SPWebTimeZone.UTCToLocalTime(dt);
                //}
            }

            catch (Exception ex)
            {
                mLog.Warn("Get datetime field value failed! Item url: {0}, fieldName: {1}, error message: {2}.", item.Url, fieldName, ex.ToString());
                try
                {
                    return SPWebTimeZone.UTCToLocalTime(dt);
                }
                catch (Exception e1)
                {
                    mLog.Warn("Get datetime field value failed! Item url: {0}, fieldName: {1}, error message: {2}.", item.Url, fieldName, e1.ToString());
                }
            }
            return new DateTime();
        }

        public static string GetItemFieldValue(this IAveListItem item, string fieldName)
        {
            string fieldValue = string.Empty;
            try
            {
                if (!item.Fields.ContainsField(fieldName))
                {
                    mLog.Debug("The item doesn't have this field. Item url: {0}, field name: {1}", item.Url, fieldName);
                }
                else if (item[fieldName] != null && !string.IsNullOrEmpty(item[fieldName].ToString()))
                {
                    fieldValue = item[fieldName].ToString();
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Get field value failed! Item url: {0}, fieldName: {1}, error message: {2}.", item.Url, fieldName, ex.ToString());
            }
            return fieldValue;
        }

        public static string GetObjectName(this IAveListItem item)
        {
            string objName = string.Empty;
            var isLibrary = item.ParentList.BaseType == AveBaseType.DocumentLibrary;
            if (isLibrary)
            {
                objName = item.Name;
            }
            else
            {
                if (item.FieldValues.ContainsKey(SPColumnConstants.SP_Title) && item.FieldValues[SPColumnConstants.SP_Title] != null)
                {
                    objName = item.FieldValues[SPColumnConstants.SP_Title].ToString();
                }
            }
            if (string.IsNullOrEmpty(objName))
            {
                if (item.FieldValues.ContainsKey(SPColumnConstants.SP_NAME) && item.FieldValues[SPColumnConstants.SP_NAME] != null)
                {
                    objName = item.FieldValues[SPColumnConstants.SP_NAME].ToString();
                }
                else if (item.FieldValues.ContainsKey(SPColumnConstants.SP_URL) && item.FieldValues[SPColumnConstants.SP_URL] != null)
                {
                    var url = item.FieldValues[SPColumnConstants.SP_URL].ToString();
                    var urlArr = url.Split(',');
                    if (urlArr.Length == 2)
                    {
                        objName = urlArr[1];
                    }
                }
                else if (item.FieldValues.ContainsKey(SPColumnConstants.FileLeafRef) && item.FieldValues[SPColumnConstants.FileLeafRef] != null)
                {
                    objName = item.FieldValues[SPColumnConstants.FileLeafRef].ToString();
                }
            }
            return objName;
        }

        public static bool ExistComplianceTag(this IAveListItem aveListItem)
        {
            bool existLabel = false;
            try
            {
                if (aveListItem.FieldValues.ContainsKey(SPColumnConstants.SP_ComplianceTag))
                {
                    existLabel = !string.IsNullOrEmpty(aveListItem.FieldValues[SPColumnConstants.SP_ComplianceTag]?.ToString());
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"check label exist error:{ex.ToString()}");
            }
            return existLabel;
        }

        public static string GetComplianceTagName(this IAveListItem aveListItem)
        {
            string labelName = string.Empty;
            try
            {
                if (aveListItem?.FieldValues != null && aveListItem.FieldValues.ContainsKey(SPColumnConstants.SP_ComplianceTag))
                {
                    labelName = aveListItem.FieldValues[SPColumnConstants.SP_ComplianceTag]?.ToString();
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"get label name error:{ex.ToString()}");
            }
            return labelName;
        }

        /// <summary>
        /// for sp make full url
        /// <returns></returns>
        public static string FullPath(this IAveListItem aveListItem)
        {
            var webUrl = aveListItem.ParentList.ParentWeb.Url;
            var strUrl = aveListItem.Url;
            if (webUrl == null || strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            if (webUrl == strUrl)
            {
                return webUrl;
            }
            if (strUrl.StartsWith("http:") || strUrl.StartsWith("https:"))
            {
                return strUrl;
            }
            strUrl = strUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (strUrl.StartsWith("/"))
            {
                var siteUri = new Uri(webUrl);
                var protocol = siteUri.Scheme + ":";
                stringBuilder.Append(protocol);
                stringBuilder.Append("//");
                stringBuilder.Append(siteUri.Host);
                if ((StsCompareStrings(protocol, "http:") && siteUri.Port != 80) || (StsCompareStrings(protocol, "https:") && siteUri.Port != 443))
                {
                    stringBuilder.Append(":");
                    stringBuilder.Append(siteUri.Port);
                }
                stringBuilder.Append(strUrl);
            }
            else
            {
                stringBuilder.Append(webUrl);
                if (strUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(strUrl);
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        public static string DirPath(this IAveListItem aveListItem)
        {

            var strUrl = aveListItem.Url;
            var webUrl = aveListItem.ParentList.ParentWeb.ServerRelativeUrl;
            if (!webUrl.EndsWith("/"))
            {
                webUrl += "/";
            }

            if (strUrl.Contains(webUrl))
            {
                return strUrl;
            }
            else
            {
                strUrl = webUrl + strUrl;

            }
            return strUrl;
        }
        public static bool StsCompareStrings(string str1, string str2)
        {
            System.Globalization.CompareInfo compareInfo = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
            return 0 == compareInfo.Compare(str1, str2, System.Globalization.CompareOptions.IgnoreCase);
        }

        private static int GetHoldAndRecordStatus(IAveListItem item)
        {
            int result = 0;
            try
            {
                if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
                {
                    try
                    {
                        if (item.Fields.Contains(new Guid(key)))
                        {
                            object obj2 = item[new Guid(key)];
                            if ((obj2 != null) && !int.TryParse(obj2.ToString(), out result))
                            {
                                result = 0;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
            }
            return result;
        }
        private static bool IsHoldOrRecordsEnabled(IAveList list)
        {
            if (list == null || list.Fields == null)
            {
                throw new ArgumentNullException("list");
            }
            if (list.Fields.Contains(new Guid(key)))
            {
                return (list.Fields[new Guid(key)] != null);
            }
            else
            {
                return false;
            }
        }

        private static bool GetBoolIprPropertyCore(IAveList list, string propName)
        {
            bool? nullable = null;
            if (list != null && list.RootFolder != null && list.RootFolder.Properties != null)
            {
                object obj = list.RootFolder.Properties[propName];
                if (obj != null) nullable = new bool?(obj.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
            }
            return (nullable == true);
        }

        public static bool IsRecord(this IAveListItem item)
        {
            return IsRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsRecord(int holdAndRecordStatus)
        {
            return (holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L;
        }

        public static bool IsRecordOnly(this IAveListItem item)
        {
            var status = GetHoldAndRecordStatus(item);
            return IsRecordOnly(status);
        }

        public static bool IsRecordOnly(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.HoldMask) == 0L);
        }

        public static bool IsStubItem(this IAveListItem item)
        {
            var linkFileFieldName = "ArchiverLinkFileType";
            bool isStub = false;
            try
            {
                if (item.FieldValues.TryGetValue(linkFileFieldName, out object value) && value != null
                    && value.ToString().Length > 0)
                {
                    isStub = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"check stub item error:{ex.ToString()}");
            }
            return isStub;
        }
    }

    internal enum HoldAndRecordStatusMask
    {
        EditBlockedMask = 1, //只要不允许编辑, 这位值就为1, 包括Hold 和 Block edit and delete
        RecordMask = 0x10, //Record 文件，这位值 就是1 ， 包含Block edit and delete， block delete
        DeleteBlockedMask = 0x100,//只要不允许删除，这位值就为1, 包括 Hold， block edit and delete， block delete
        HoldMask = 0x1000, //Hold 文件，这位值就是 1， 
    }
}
