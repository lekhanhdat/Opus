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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using AvePoint.GCommon;
using System.Reflection;
using System.Text;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.GCommon.Utility.TimeZoneConvert;
using AvePoint.RA.Common.Global;
using Microsoft.SharePoint.News.DataModel;

namespace AvePoint.Wrapper.Discovery
{
    public static class FilterAnalyser
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region Common Methods
        public static Hashtable FillSiteColumns(IAveWeb web)
        {
            Hashtable siteCollectionColumn = new Hashtable();
            foreach (string key in web.AllProperties.Keys)
            {
                object value = web.AllProperties[key];
                if (GAPolicyHelper.keysNeedToDecryption.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    //解密成功返回解密后结果，否则返回Value, 不能抛异常 
                    value = GAPolicyHelper.GetPolicyValue(value.ToString(), web.Site.ID, web.ID);
                }
                siteCollectionColumn[key] = value;
            }

            string ga_policy = GAPolicyHelper.GA_PolicyForGranularBackup;
            if (web.IsRootWeb && !siteCollectionColumn.ContainsKey(ga_policy))
            {
                try
                {
                    string documentsLibUrl = web.ServerRelativeUrl.TrimEnd('/') + "/Shared Documents";
                    var folder = web.GetFolder(documentsLibUrl);
                    if (folder != null && folder.Properties.ContainsKey(ga_policy))
                    {
                        string value = folder.Properties[ga_policy].ToString();
                        if (GAPolicyHelper.keysNeedToDecryption.Contains(ga_policy, StringComparer.OrdinalIgnoreCase))
                        {
                            value = GAPolicyHelper.GetPolicyValue(value.ToString(), web.Site.ID, web.ID);
                        }
                        siteCollectionColumn[ga_policy] = value;
                    }
                }
                catch (Exception ex)
                {
                    siteCollectionColumn[ga_policy] = string.Empty;
                    log.Info("Can't get ga_policy from site column.Message:{0}.", ex.ToString());
                }
            }

            return siteCollectionColumn;
        }

        private static void GetUserInfo(IAveListItem item, string columnName, ref string loginName, ref string title, ref string email)
        {
            try
            {
                string itemUserInfo = item[columnName].ToString();
                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                title = sArray[1].ToString();
                IAveUser user = item.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                if (user != null)
                {
                    loginName = user.LoginName;
                    title = user.Name;
                    email = user.Email;   //SAAS-10859 添加对email格式的支持。
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperDiscoverResource.AWDGetUserInfoError, columnName, title, ex.ToString());
            }
        }

        private static void GetUserInfo(AveDiscoverItem item, string columnName, ref string loginName, ref string title, ref string email)
        {
            try
            {
                string itemUserInfo = item.CurrentItem[columnName].ToString();
                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                title = sArray[1].ToString();
                IAveUser user = item.ItemCache.AveWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                if (user != null)
                {
                    loginName = user.LoginName;
                    title = user.Name;
                    email = user.Email;   //SAAS-10859 添加对email格式的支持。
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperDiscoverResource.AWDGetUserInfoError, columnName, title, ex.ToString());
            }
        }

        private static void GetAuthorOrEditorInfo(IAveListItem item, CommonInfoBase result, bool authorOrEditor)
        {
            string logonName = string.Empty;
            string title = string.Empty;
            string email = string.Empty;    //SAAS-10859 添加对email格式的支持。
            string columnName = authorOrEditor ? "Author" : "Editor";
            GetUserInfo(item, columnName, ref logonName, ref title, ref email);
            if (authorOrEditor)
            {
                result.CreatedByTitle = title;
                result.CreatedByLogonName = logonName;
                result.CreateByEmail = email;
            }
            else
            {
                result.ModifiedByTitle = title;
                result.ModifiedByLogonName = logonName;
                result.ModifiedByEmail = email;
            }
        }

        private static void GetVersionEditorInfo(IAveListItemVersion version, CommonInfoBase result)
        {
            try
            {
                string itemUserInfo = version["Editor"].ToString();
                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                result.ModifiedByTitle = sArray[1].ToString();
                IAveUser user = version.ListItem.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                if (user != null)
                {
                    result.ModifiedByLogonName = user.LoginName;
                    result.ModifiedByTitle = user.Name;
                    result.ModifiedByEmail = user.Email;

                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperDiscoverResource.AWDGetVisionEditorError, ex.ToString());
            }
        }

        private static T GetValue<T>(object value, T defalut)
        {
            if (value is T)
            {
                return (T)value;
            }
            return defalut;
        }

        private static DateTime ToUniversalTime(DateTime datetime)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = datetime.ToUniversalTime();
            }
            return datetime;
        }

        /// <summary>
        /// 1.Folder Leve获取的Created Time和Modified Time为Local时间，而Document/Item Level获取的Created Time和Modified Time为UTC时间.
        /// 2.Document/Item Level自定义DateTime Column获取的时间为Local时间.
        /// 3.DateTime.ToUniversalTime()转换根据的是当前Agent机器时区时间，而不是SP时区时间转换.
        /// 4.Folder.Item取出来的是web时区的，List.GetItemById拿出来的是UTC.
        /// </summary>
        private static DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                //TimeZoneInfo webZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(web.RegionalSettings.TimeZone.ID));  //TODO Cyrus
                TimeZoneInfo webZone = TimeZoneConvertHelper.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(web.RegionalSettings.TimeZone.ID));
                datetime = TimeZoneInfo.ConvertTimeToUtc(datetime, webZone);
            }
            return datetime;
        }

        private static Hashtable GetItemColumns(IAveListItem item)
        {
            Hashtable columnCollection = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (item != null)
            {
                foreach (var field in item.Fields)
                {
                    try
                    {
                        if (field.Hidden && !string.Equals(field.InternalName, "FileDirRef"))
                        {
                            continue;
                        }
                        if (string.Equals(field.InternalName, "FileDirRef"))
                        {
                            string dirName = item[field.ID].ToString();
                            if (dirName != string.Empty)
                            {
                                string folderName = dirName.TrimEnd('/').Substring(dirName.TrimEnd('/').LastIndexOf('/')).TrimStart('/');
                                columnCollection[field.InternalName.ToLower()] = folderName;//为符合Parent folder name需要做的特殊处理。 
                            }
                            continue;
                        }
                        if (item[field.ID] == null)
                        {
                            if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                            {//text match * need this.
                                columnCollection[field.Title.ToLower()] = string.Empty;
                            }
                            continue;
                        }
                        switch (field.Type)
                        {
                            //在rule判断时，会判断数据类型。
                            case AveFieldType.Boolean:
                            case AveFieldType.Number:
                                columnCollection[field.Title.ToLower()] = item[field.ID];
                                break;
                            case AveFieldType.DateTime:
                                columnCollection[field.Title.ToLower()] = ToUniversalTimeWithTimeZone((DateTime)item[field.ID], item.ParentList.ParentWeb);
                                break;
                            case AveFieldType.User:
                                var value = item[field.ID];
                                var stringVlue = value as string;
                                if (stringVlue != null)
                                {
                                    columnCollection[field.Title.ToLower()] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                }
                                else if (value is IEnumerable)
                                {
                                    StringBuilder users = new StringBuilder();
                                    foreach (var userinfo in (value as IEnumerable))
                                    {
                                        var user = userinfo.ToString();
                                        users.Append(user.Substring(user.IndexOf('#') + 1));
                                        users.Append(';');
                                    }
                                    users.Length = Math.Max(0, users.Length - 1);
                                    columnCollection[field.Title.ToLower()] = users.ToString();
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = value;
                                }
                                break;
                            case AveFieldType.Lookup:
                                var lookupValue = item[field.ID];
                                var realValue = lookupValue as IAveFieldLookupValue;
                                if (realValue != null)
                                {
                                    columnCollection[field.Title.ToLower()] = realValue.LookupValue;
                                }
                                else if (lookupValue is string)
                                {
                                    var vaules = (lookupValue as string).Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (vaules.Length == 2)
                                    {
                                        columnCollection[field.Title.ToLower()] = vaules[1];
                                    }
                                    else
                                    {
                                        columnCollection[field.Title.ToLower()] = vaules[0];
                                    }
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = lookupValue;
                                }
                                break;
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnCollection[field.Title.ToLower()] = field.GetFieldValueAsText(item[field.ID]);
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = item[field.ID];
                                }
                                break;
                            default:
                                //field.GetFieldValueAsText should not throw exception, if any modify the override method.(Luo Qinglong)
                                columnCollection[field.Title.ToLower()] = field.GetFieldValueAsText(item[field.ID]).Trim();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug(string.Format("Get the metadata of item error.Field Name:{0}.Exception:{1}", field.Title, ex.ToString()));
                    }
                }
            }
            return columnCollection;
        }
        private static Hashtable GetItemVersionColumns(IAveListItemVersion itemVersion)
        {
            Hashtable columnCollection = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (itemVersion != null)
            {
                foreach (var field in itemVersion.Fields)
                {
                    try
                    {
                        if (field.Hidden && !string.Equals(field.InternalName, "FileDirRef"))
                        {
                            continue;
                        }
                        if (string.Equals(field.InternalName, "FileDirRef"))
                        {
                            string dirName = itemVersion[field.ColName].ToString();
                            if (dirName != string.Empty)
                            {
                                string folderName = dirName.TrimEnd('/').Substring(dirName.TrimEnd('/').LastIndexOf('/')).TrimStart('/');
                                columnCollection[field.InternalName.ToLower()] = folderName;//为符合Parent folder name需要做的特殊处理。 
                            }
                            continue;
                        }
                        if (itemVersion[field.InternalName] == null && itemVersion[field.ColName] == null && !string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                        {
                            if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                            {//text match * need this.
                                columnCollection[field.Title.ToLower()] = string.Empty;
                            }
                            continue;
                        }
                        switch (field.Type)
                        {
                            case AveFieldType.Boolean:
                            case AveFieldType.Number:
                            case AveFieldType.DateTime:
                                break;
                            case AveFieldType.Lookup:
                                var lookupValue = itemVersion[field.Title] ?? itemVersion[field.InternalName];
                                var realValue = lookupValue as IAveFieldLookupValue;
                                if (realValue != null)
                                {
                                    columnCollection[field.Title.ToLower()] = realValue.LookupValue;
                                }
                                else if (lookupValue is string)
                                {
                                    var vaules = (lookupValue as string).Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (vaules.Length == 2)
                                    {
                                        columnCollection[field.Title.ToLower()] = vaules[1];
                                    }
                                    else
                                    {
                                        columnCollection[field.Title.ToLower()] = vaules[0];
                                    }
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = lookupValue;
                                }
                                break;
                            case AveFieldType.User:
                                var value = itemVersion[field.Title] ?? itemVersion[field.InternalName];
                                var stringVlue = value as string;
                                if (!string.IsNullOrEmpty(stringVlue))
                                {
                                    columnCollection[field.Title.ToLower()] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                }
                                else if (value is IEnumerable)
                                {
                                    StringBuilder users = new StringBuilder();
                                    foreach (var userinfo in (value as IEnumerable))
                                    {
                                        var user = userinfo.ToString();
                                        users.Append(user.Substring(user.IndexOf('#') + 1));
                                        users.Append(';');
                                    }
                                    users.Length = Math.Max(0, users.Length - 1);
                                    columnCollection[field.Title.ToLower()] = users.ToString();
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = value;
                                }
                                break;
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnCollection[field.Title.ToLower()] = field.GetFieldValueAsText(itemVersion[field.Title.ToString()] ?? itemVersion[field.InternalName]);
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = itemVersion[field.Title.ToString()] ?? itemVersion[field.InternalName];
                                }
                                break;
                            default:
                                columnCollection[field.Title.ToLower()] = field.GetFieldValueAsText(itemVersion[field.InternalName] ?? itemVersion[field.ColName]).Trim();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug(string.Format("Get the metadata of item version error.Field Name:{0}.Exception:{1}", field.Title, ex.ToString()));
                    }
                }
            }
            return columnCollection;
        }
        private static List<Hashtable> GetItemInternalColumns(IAveListItem item)
        {
            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable columnCollectionOfInterName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable intrToDisp = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable dispToType = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable specialCollection = new Hashtable(StringComparer.OrdinalIgnoreCase);
            List<Hashtable> ret = new List<Hashtable>();
            if (item != null)
            {
                foreach (var field in item.Fields)
                {
                    try
                    {
                        string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);
                        string fieldInternalName = field.InternalName.ToLower(CultureInfo.InvariantCulture);
                        if (field.Hidden)
                        {
                            continue;
                        }
                        if (item[field.ID] == null)
                        {
                            if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                            {//text match * need this.                                
                                columnCollectionOfDisplayName[fieldTitle] = string.Empty;
                                columnCollectionOfInterName[fieldInternalName] = string.Empty;
                                intrToDisp[fieldInternalName] = fieldTitle;
                                dispToType[fieldTitle] = field.Type.ToString().ToLower(CultureInfo.InvariantCulture);
                            }
                            continue;
                        }
                        switch (field.Type)
                        {
                            //在rule判断时，会判断数据类型。
                            case AveFieldType.Boolean:
                            case AveFieldType.Number:
                                //columnCollection[fieldTitle] = item[field.ID];
                                columnCollectionOfDisplayName[fieldTitle] = item[field.ID];
                                columnCollectionOfInterName[fieldInternalName] = item[field.ID];
                                break;
                            case AveFieldType.Counter:
                                columnCollectionOfDisplayName[fieldTitle] = Convert.ToDouble(item[field.ID]);
                                columnCollectionOfInterName[fieldInternalName] = Convert.ToDouble(item[field.ID]);
                                break;
                            case AveFieldType.DateTime:
                                //columnCollection[fieldTitle] = ToUniversalTime((DateTime)item[field.ID]);
                                columnCollectionOfDisplayName[fieldTitle] = ToUniversalTimeWithTimeZone((DateTime)item[field.ID], item.Web);
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                            case AveFieldType.User:
                                var value = item[field.ID];
                                var stringVlue = value as string;
                                if (stringVlue != null)
                                {
                                    //columnCollection[fieldTitle] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                    columnCollectionOfDisplayName[fieldTitle] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                }
                                else if (value is IEnumerable)
                                {
                                    StringBuilder users = new StringBuilder();
                                    foreach (var userinfo in (value as IEnumerable))
                                    {
                                        var user = userinfo.ToString();
                                        users.Append(user.Substring(user.IndexOf('#') + 1));
                                        users.Append(';');
                                    }
                                    users.Length = Math.Max(0, users.Length - 1);
                                    //columnCollection[fieldTitle] = users.ToString();
                                    columnCollectionOfDisplayName[fieldTitle] = users.ToString();
                                    columnCollectionOfInterName[fieldInternalName] = users.ToString();
                                }
                                else
                                {
                                    //columnCollection[fieldTitle] = value;
                                    columnCollectionOfDisplayName[fieldTitle] = value;
                                    columnCollectionOfInterName[fieldInternalName] = value;
                                }
                                break;
                            case AveFieldType.Lookup:
                                var lookupValue = item[field.ID];
                                DateTime lookupDateTime = DateTime.MinValue;
                                if (DateTime.TryParse(lookupValue.ToString(), out lookupDateTime))
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = ToUniversalTimeWithTimeZone(lookupDateTime, item.Web);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                    break;
                                }
                                var realValue = lookupValue as IAveFieldLookupValue;
                                if (realValue != null)
                                {
                                    //columnCollection[fieldTitle] = realValue.LookupValue;
                                    columnCollectionOfDisplayName[fieldTitle] = realValue.LookupValue;
                                    columnCollectionOfInterName[fieldInternalName] = realValue.LookupValue;
                                }
                                else if (string.Equals(field.TypeAsString, "Lookup", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(field.TypeAsString, "LookupMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(lookupValue);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                }
                                else
                                {
                                    //columnCollection[fieldTitle] = lookupValue;
                                    columnCollectionOfDisplayName[fieldTitle] = lookupValue;
                                    columnCollectionOfInterName[fieldInternalName] = lookupValue;
                                }
                                break;
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    //columnCollection[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                }
                                else
                                {
                                    //columnCollection[fieldTitle] = item[field.ID];
                                    columnCollectionOfDisplayName[fieldTitle] = item[field.ID];
                                    columnCollectionOfInterName[fieldInternalName] = item[field.ID];
                                }
                                break;
                            case AveFieldType.ModStat:
                                specialCollection[fieldInternalName] = item[field.ID];
                                //columnCollection[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                            case AveFieldType.Calculated:
                                var calculatedValue = item[field.ID];
                                var calValue = calculatedValue as IAveFieldCalculated;
                                if (calValue != null)
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = calValue.Formula;
                                }
                                else if (calculatedValue is string)
                                {
                                    var vaules = (calculatedValue as string).Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (vaules.Length == 2)
                                    {
                                        string colValue = vaules[1];
                                        if ((field as IAveFieldCalculated).OutputType == AveFieldType.DateTime)
                                        {
                                            DateTime columnValue;
                                            if (DateTime.TryParse(colValue, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out columnValue))
                                            {
                                                columnCollectionOfDisplayName[fieldTitle] = ToUniversalTimeWithTimeZone(columnValue, item.Web);
                                            }
                                            else
                                            {
                                                columnCollectionOfDisplayName[fieldTitle] = colValue;
                                            }
                                        }
                                        else if (vaules.Length == 1)//13环境Check In Comment在没有值得情况下为empty
                                        {
                                            columnCollectionOfDisplayName[fieldTitle] = vaules[0];
                                        }
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[fieldTitle] = calculatedValue;
                                    }
                                }
                                else if (calculatedValue is DateTime)
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = (DateTime)calculatedValue;
                                }
                                else
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                }
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                            default:
                                //field.GetFieldValueAsText should not throw exception, if any modify the override method.(Luo Qinglong)
                                //columnCollection[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                        }
                        //columnCollection[fieldTitle] = AveStringHelper.Trim(columnCollection[fieldTitle]);
                        columnCollectionOfDisplayName[fieldTitle] = AveStringHelper.Trim(columnCollectionOfDisplayName[fieldTitle]);
                        columnCollectionOfInterName[fieldInternalName] = AveStringHelper.Trim(columnCollectionOfInterName[fieldInternalName]);
                        intrToDisp[fieldInternalName] = fieldTitle;
                        dispToType[fieldTitle] = field.Type.ToString().ToLower(CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        log.Debug(string.Format("Get the metadata of item error.Field Name:{0}.Exception:{1}", field.Title, ex.ToString()));
                    }
                }
            }
            ret.Add(columnCollectionOfDisplayName);
            ret.Add(columnCollectionOfInterName);
            ret.Add(intrToDisp);
            ret.Add(dispToType);
            ret.Add(specialCollection);
            return ret;
        }
        private static Hashtable GetItemVersionInternalColumns(IAveListItemVersion itemVersion)
        {
            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable columnCollectionOfInterName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (itemVersion != null)
            {
                foreach (var field in itemVersion.Fields)
                {
                    try
                    {
                        string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);
                        string fieldInternalName = field.InternalName.ToLower(CultureInfo.InvariantCulture);
                        if (field.Hidden)
                        {
                            continue;
                        }
                        if (itemVersion[field.ColName] == null)
                        {
                            if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                            {//text match * need this.                                
                                columnCollectionOfDisplayName[fieldTitle] = string.Empty;
                                columnCollectionOfInterName[fieldInternalName] = string.Empty;
                            }
                            continue;
                        }
                        switch (field.Type)
                        {
                            //在rule判断时，会判断数据类型。
                            case AveFieldType.Boolean:
                            case AveFieldType.Number:
                            case AveFieldType.Counter:
                            case AveFieldType.DateTime:
                            case AveFieldType.User:
                            case AveFieldType.Lookup:
                            case AveFieldType.Invalid:
                            case AveFieldType.ModStat:
                            case AveFieldType.Calculated:
                                log.Info("version rule just can surpport Column(Text),not surpport other");
                                break;
                            default:
                                columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(itemVersion[field.ColName]);
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                        }
                        columnCollectionOfDisplayName[fieldTitle] = AveStringHelper.Trim(columnCollectionOfDisplayName[fieldTitle]);
                        columnCollectionOfInterName[fieldInternalName] = AveStringHelper.Trim(columnCollectionOfInterName[fieldInternalName]);
                    }
                    catch (Exception ex)
                    {
                        log.Debug(string.Format("Get the metadata of item version error.Field Name:{0}.Exception:{1}", field.Title, ex.ToString()));
                    }
                }
            }
            return columnCollectionOfInterName;
        }

        private static Hashtable GetItemWorkflows(IAveListItem item)
        {
            Hashtable workflows = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (var field in item.Fields)
            {
                if (field.Type == AveFieldType.WorkflowStatus)
                {
                    if (item[field.ID] != null)
                    {
                        string statusValue = ChangeWorkflowsStatusInLanguage(field.GetFieldValueAsText(item[field.ID]));
                        workflows.Add(field.Title.ToLower(), statusValue);
                    }
                }
            }
            return workflows;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Japanese and German of workflow status")]
        private static string ChangeWorkflowsStatusInLanguage(string instanceStatus)
        {
            string commonStatus = string.Empty;
            switch (instanceStatus)
            {
                case "進行中":
                case "In Bearbeitung":
                    commonStatus = "In Progress";
                    break;
                case "完了":
                case "Abgeschlossen":
                    commonStatus = "Completed";
                    break;
                case "取り消し":
                case "Abgebrochen":
                    commonStatus = "Canceled";
                    break;
                case "承認済み":
                case "Genehmigt":
                    commonStatus = "Approved";
                    break;
                case "却下":
                case "Abgelehnt":
                    commonStatus = "Rejected";
                    break;
                default:
                    commonStatus = instanceStatus;
                    break;
            }
            return commonStatus;
        }


        /// <summary>
        /// 获得该Level每种Filter的不重复的Rule
        /// </summary>
        private static List<FilterPolicy> CreateDistinctFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => filter.Level == level).Distinct(FilterRuleTypeEqualityComparer.GetInstance()).ToList();
            }
            return new List<FilterPolicy>();
        }

        private static List<FilterPolicy> CreateDocumentFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => filter.Level == level).ToList();
            }
            return new List<FilterPolicy>();
        }

        private static List<FilterPolicy> CreateVersionRuleFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => (filter.Rule is VersionsRule) && filter.Level == level).ToList();
            }
            return new List<FilterPolicy>();
        }

        #region add for RevIM term path

        public static string Trim(string str, params char[] trimchars)
        {
            return string.IsNullOrEmpty(str) ? str : str.Trim(trimchars);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Test")]
        private static Hashtable GetItemTaxonomyColumns(IAveListItem item)
        {
            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (item != null)
            {
                foreach (var field in item.Fields)
                {
                    try
                    {
                        string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);
                        switch (field.Type)
                        {
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    IAveTaxonomyField taxnomyField = field as IAveTaxonomyField;
                                    //Get Term Path Method
                                    //RECO-11440
                                    object fieldValue = null;
                                    try
                                    {
                                        fieldValue = item[field.ID];
                                    }
                                    catch (Exception ie)
                                    {
                                        log.Warn(ie.ToString());
                                    }
                                    if (fieldValue == null)
                                    {
                                        //Sometimes the TaxonomyField column has no value, and its associated hidden field needs to be used to get the value.
                                        try
                                        {
                                            string fieldName = null;

                                            if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fieldName = item.Fields.GetById((field as IAveTaxonomyField).TextField).InternalName;
                                                log.Info("Will get field value by TextField, InternalName is :{0}", fieldName);
                                                fieldValue = item[fieldName];
                                            }
                                            else if (string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                            {
                                                //Since Record does not fully support multi-value TaxonomyFieldType, special handling is currently skipped.
                                                log.Warn("Skip special handling for TaxonomyFieldTypeMulti data.");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            log.Warn("get TaxonomyField column associated hidden column error: {0}", e.ToString());
                                        }
                                        if (fieldValue == null)
                                        {
                                            continue;
                                        }

                                    }
                                    columnCollectionOfDisplayName[fieldTitle] = Trim(GetFieldTermIdValue(fieldValue));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Get the taxnomy metadata of item error.Field Name:{0} Field.ID:{1}.Exception:{2}", field.Title, field.ID, ex));
                    }
                }
            }
            return columnCollectionOfDisplayName;
        }

        /// <summary>
        /// Records Use Term Unique Id to check rule (Replace Term Path)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetFieldTermIdValue(object value)
        {
            try
            {
                if (value is Dictionary<string, object> || value.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                {
                    try
                    {
                        var dic = ((Dictionary<string, object>)value);
                        if (dic != null && dic.ContainsKey("TermGuid"))
                        {
                            var termId = new Guid(dic["TermGuid"].ToString());
                            return termId.ToString();
                        }
                        else
                        {
                            log.Warn("Current FieldTermIdValue:{0} is null or does not ContainsKey TermGuid.", value.ToString());
                            return string.Empty;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("Get Taxnomy Filed Value by Dictionary Error, {0}", e.ToString());
                    }
                }
                else if (value is IAveTaxonomyFieldValue)
                {
                    var taxValue = value as IAveTaxonomyFieldValue;
                    var termId = new Guid(taxValue.TermGuid);
                    return termId.ToString();
                }
                else if (!(value is string))
                {
                    log.Info("Get Taxnomy Filed Value Error, the value is :{0}", value.ToString());
                }
            }
            catch (Exception e)
            {
                log.Warn("Get Taxnomy Filed Value:{0} Error:{1}.", value == null ? string.Empty : value.ToString(), e.ToString());
            }
            string stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                string[] values = stringValue.Split(';');
                foreach (string key in values)
                {
                    var index = key.IndexOf('|');
                    if (index == 0)
                    {
                        continue;
                    }
                    if (index < 0)
                    {
                        continue;
                    }
                    else
                    {
                        return key.Substring(index + 1);
                    }
                }
            }
            else
            {
                log.Warn("Current FieldTermIdValue IsNullOrEmpty.");
                return string.Empty;
            }
            return string.Empty;
        }
      
        #endregion

        #endregion

        #region Internal Classes

        internal class FilterRuleTypeEqualityComparer : IEqualityComparer<FilterPolicy>
        {
            private static FilterRuleTypeEqualityComparer instance;

            private FilterRuleTypeEqualityComparer()
            {
            }
            public static FilterRuleTypeEqualityComparer GetInstance()
            {
                if (instance == null)
                {
                    instance = new FilterRuleTypeEqualityComparer();
                }
                return instance;
            }
            public bool Equals(FilterPolicy x, FilterPolicy y)
            {
                return x.Rule.GetType().Equals(y.Rule.GetType());
            }

            public int GetHashCode(FilterPolicy obj)
            {
                return 0;
            }
        }

        #endregion
        private static ObjectInfoBase CommonDocumentFilter(ref List<FilterPolicy> policies, IAveFile file, IAveListItem item, DocumentInfo result)
        {
            return CommonDocumentFilter(ref policies, file, item, result, 0, 0, false, null, false);
        }
        private static ObjectInfoBase CommonDocumentFilter(ref List<FilterPolicy> policies, IAveListItem item, DocumentInfo result)
        {
            if (item == null)
            {
                throw new Exception("item is null");
            }
            var documentPolicies = CreateDocumentFiltersCopy(policies, PolicyLevel.Document);
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Document);
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "SizeRule":
                        result.Size = Convert.ToInt64(item["File_x0020_Size"]);
                        break;
                    case "NameRule":
                        result.Name = item.Name;
                        break;
                    case "UrlRule":
                        result.Url = item.ParentList.ParentWeb.Url + "/" + item.Url;
                        break;
                    case "ModifiedRule":
                        result.Modified = ToUniversalTime(Convert.ToDateTime(item["Modified"]));
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime(Convert.ToDateTime(item["Created"]));
                        break;
                    case "ModifiedByRule":
                        GetAuthorOrEditorInfo(item, result, false);
                        break;
                    case "CreatedByRule":
                        GetAuthorOrEditorInfo(item, result, true);
                        break;
                    case "ContentTypeRule":
                        if (item != null && item.ContentType != null)
                        {
                            result.ContentType = item.ContentType.Name;
                        }
                        else
                        {
                            result.ContentType = "Document";
                        }
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "MetadataTextColumnRule":
                    case "MetadataNumberColumnRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemColumns(item);
                        }
                        foreach (var documentPolicy in documentPolicies)
                        {
                            string documentRuleName = documentPolicy.Rule.GetType().Name;
                            documentRuleName = documentRuleName.Substring(documentRuleName.LastIndexOf('.') + 1);
                            if (documentRuleName.EqualIgnoreCase("ColumnBooleanRule") || documentRuleName.EqualIgnoreCase("ColumnTextRule"))
                            {
                                if (result.ListColumnExistInfos == null)
                                {
                                    result.ListColumnExistInfos = new Dictionary<string, bool>();
                                }
                                if (item.ParentList.Fields != null && item.ParentList.Fields.ContainsField(documentPolicy.Rule.Value1))
                                {
                                    if (result.ListColumnExistInfos.ContainsKey(documentPolicy.Rule.Value1))
                                    {
                                        result.ListColumnExistInfos[documentPolicy.Rule.Value1] = true;
                                    }
                                    else
                                    {
                                        result.ListColumnExistInfos.Add(documentPolicy.Rule.Value1, true);
                                    }
                                }
                                else
                                {
                                    if (result.ListColumnExistInfos.ContainsKey(documentPolicy.Rule.Value1))
                                    {
                                        result.ListColumnExistInfos[documentPolicy.Rule.Value1] = false;
                                    }
                                    else
                                    {
                                        result.ListColumnExistInfos.Add(documentPolicy.Rule.Value1, false);
                                    }
                                }
                            }
                        }
                        break;
                    case "WorkflowRule":
                        if (item != null && result.WorkflowStatus == null)
                        {
                            result.WorkflowStatus = GetItemWorkflows(item);
                        }
                        break;
                    case "ListTypeRule":
                        result.ListType = ((int)item.ParentList.BaseTemplate).ToString();
                        break;
                    case "TermRule":
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item);
                        }
                        break;
                    case "ParentListNameRule":                     
                        string listName = item.ParentList.Title;
                        result.ParentListName = listName;
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }
            return result;
        }

        private static List<FilterPolicy> CommonDocumentFilter(List<FilterPolicy> policies, AveDiscoverItem item, DocumentInfo result)
        {
            ArgumentNullException.ThrowIfNull(item);
            List<FilterPolicy> pendingPolicyes = new List<FilterPolicy>();
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "SizeRule":
                        result.Size = item.Length;
                        break;
                    case "NameRule":
                        result.Name = item.LeafName;
                        break;
                    case "UrlRule":
                        //Archiver use FullUrl check rule.
                        result.Url = item.CurrentItem.ParentList.ParentWeb.Url + "/" + item.CurrentItem.Url; //item.FullUrl not full url which like "/sites/test524/Shared Documents/628_1.txt"
                        break;
                    case "ModifiedRule":
                        result.Modified = ToUniversalTime(item.TimeLastModified);
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime(item.TimeCreated);
                        break;
                    case "ModifiedByRule":
                        string modifiedByLogonName = string.Empty;
                        string modifiedByTitle = string.Empty;
                        string modifiedByEmail = string.Empty;
                        GetUserInfo(item, "Editor", ref modifiedByLogonName, ref modifiedByTitle, ref modifiedByEmail);
                        result.ModifiedByTitle = modifiedByTitle;
                        result.ModifiedByLogonName = modifiedByLogonName;
                        result.ModifiedByEmail = modifiedByEmail;
                        break;
                    case "CreatedByRule":
                        string createdByLogonName = string.Empty;
                        string createdByTitle = string.Empty;
                        string createdByEmail = string.Empty;
                        GetUserInfo(item, "Author", ref createdByLogonName, ref createdByTitle, ref createdByEmail);
                        result.CreatedByTitle = createdByTitle;
                        result.CreatedByLogonName = createdByLogonName;
                        result.CreateByEmail = createdByEmail;
                        break;
                    case "ContentTypeRule":
                        if (item != null && item.CurrentItem.ContentType != null)
                        {
                            result.ContentType = item.CurrentItem.ContentType.Name;
                        }
                        else
                        {
                            result.ContentType = "Document";
                        }
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "MetadataTextColumnRule":
                    case "MetadataNumberColumnRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemColumns(item.CurrentItem);
                        }
                        break;
                    case "ListTypeRule":
                        if (item != null && item.ItemCache.ListId != Guid.Empty)
                        {
                            var list = item.ItemCache.AveWeb.Lists.GetById(item.ItemCache.ListId);

                            if (list != null)
                            {
                                result.ListType = ((int)list.BaseTemplate).ToString();
                            }
                        }
                        break;
                    case "ParentFolderNameRule":
                        var folderName = item?.DirName;
                        var index = item.DirName.LastIndexOf('/');
                        if (index >= 0)
                        {
                            folderName = folderName.Substring(index + 1);
                        }

                        result.ParentFolderName = folderName;
                        break;
                    case "ParentFolderNameHeirarchicallyRule":
                        //use web url is because the ParentFolderNameRule will set list url name to folder name, so keep this logic.
                        var rootFolderPath = item.CurrentItem.ParentList.ParentWeb.ServerRelativeUrl;
                        var folderPath = item.DirName;

                        string folderNameHeirarchically = string.Empty;
                        if (rootFolderPath.Length + 1 < folderPath.Length)
                        {
                            folderNameHeirarchically = folderPath.Substring(rootFolderPath.Length + 1);
                        }
                        else
                        {
                            var dirindex = item.DirName.LastIndexOf('/');
                            if (dirindex >= 0)
                            {
                                folderNameHeirarchically = item.DirName.Substring(dirindex + 1);
                            }
                        }
                        result.ParentFolderName = folderNameHeirarchically;
                        break;
                    case "ParentListNameRule":                    
                        string listName = item?.CurrentItem.ParentList.Title;
                        result.ParentListName = listName;
                        break;
                    //add for RevIM term path
                    case "TermRule":
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item?.CurrentItem);
                        }
                        break;
                    case "StubLastAccessTimeRule":
                        result.StubLastAccessTime = item.CurrentItem.GetLastAccessTime(item.DocID, item.DirName, ToUniversalTime(item.TimeLastModified));
                        break;
                    case "StubLastActiveTimeRule":
                        result.LastAccessCompatibleModifiedTime = item.CurrentItem.GetLastAccessTime(item.DocID, item.DirName, ToUniversalTime(item.TimeLastModified), isCompatibleByModifiedTime: true);
                        break;
                    default:
                        pendingPolicyes.Add(policy);
                        break;
                }
            }
            return pendingPolicyes;
        }

        private static ObjectInfoBase CommonDocumentFilter(ref List<FilterPolicy> policies, IAveFile file, IAveListItem item, DocumentInfo result, int versionSequenceNo, int majorVersionSequenceNo, bool isMajorVersion, IAveListItemVersion version, bool checkVersion)
        {
            var documentPolicies = CreateDocumentFiltersCopy(policies, PolicyLevel.Document);
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Document);
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "SizeRule":
                        result.Size = file.Length;
                        break;
                    case "NameRule":
                        result.Name = file.Name;
                        break;
                    case "UrlRule":
                        if (item != null)
                        {
                            result.Url = item.ParentList.ParentWeb.Url + "/" + item.Url;
                        }
                        else
                        {

                            result.Url = file.ParentFolder.ParentWeb.Url + "/" + file.Url;
                        }
                        break;
                    case "ModifiedRule":
                        result.Modified = ToUniversalTime(file.TimeLastModified);
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime(file.TimeCreated);
                        break;
                    case "ModifiedByRule":
                        GetAuthorOrEditorInfo(item, result, false);
                        break;
                    case "CreatedByRule":
                        GetAuthorOrEditorInfo(item, result, true);
                        break;
                    case "IsStubRule":
                        var site = file.Web.Site;
                        result.IsStub = CheckIsStub(site, file.UniqueId, file.Level, file.UIVersion);
                        break;
                    case "StubLastAccessTimeRule":
                        result.StubLastAccessTime = file.GetLastAccessTime(file.UniqueId, file.ParentFolder.ServerRelativeUrl, ToUniversalTime(file.TimeLastModified));
                        break;
                    case "StubLastActiveTimeRule":
                        result.LastAccessCompatibleModifiedTime = file.GetLastAccessTime(file.UniqueId, file.ParentFolder.ServerRelativeUrl, ToUniversalTime(file.TimeLastModified), isCompatibleByModifiedTime: true);
                        break;
                    case "StubCreateTimeRule":
                        site = file.Web.Site;
                        result.IsStub = CheckIsStub(site, file.UniqueId, file.Level, file.UIVersion);
                        if (result.IsStub)
                        {
                            result.StubCreated = GetStubCreateTime(site, file.UniqueId, file.Level, file.UIVersion);
                        }
                        else
                        {
                            result.StubCreated = new DateTime();
                        }
                        break;
                    case "ContentTypeRule":
                        if (item != null && item.ContentType != null)
                        {
                            result.ContentType = item.ContentType.Name;
                        }
                        else
                        {
                            result.ContentType = "Document";
                        }
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "MetadataTextColumnRule":
                    case "MetadataNumberColumnRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemColumns(item);
                        }
                        if (result.ColumnInfosOfInternalName == null)
                        {
                            result.ColumnInfosOfInternalName = GetItemInternalColumns(item)[1];
                        }
                        foreach (var documentPolicy in documentPolicies)
                        {
                            string documentRuleName = documentPolicy.Rule.GetType().Name;
                            documentRuleName = documentRuleName.Substring(documentRuleName.LastIndexOf('.') + 1);
                            if (documentRuleName.EqualIgnoreCase("ColumnBooleanRule") || documentRuleName.EqualIgnoreCase("ColumnTextRule"))
                            {
                                if (result.ListColumnExistInfos == null)
                                {
                                    result.ListColumnExistInfos = new Dictionary<string, bool>();
                                }
                                if (item.ParentList.Fields != null && item.ParentList.Fields.ContainsField(documentPolicy.Rule.Value1))
                                {
                                    if (result.ListColumnExistInfos.ContainsKey(documentPolicy.Rule.Value1))
                                    {
                                        result.ListColumnExistInfos[documentPolicy.Rule.Value1] = true;
                                    }
                                    else
                                    {
                                        result.ListColumnExistInfos.Add(documentPolicy.Rule.Value1, true);
                                    }
                                }
                                else
                                {
                                    if (result.ListColumnExistInfos.ContainsKey(documentPolicy.Rule.Value1))
                                    {
                                        result.ListColumnExistInfos[documentPolicy.Rule.Value1] = false;
                                    }
                                    else
                                    {
                                        result.ListColumnExistInfos.Add(documentPolicy.Rule.Value1, false);
                                    }
                                }
                            }
                        }
                        break;
                    case "WorkflowRule":
                        if (item != null && result.WorkflowStatus == null)
                        {
                            result.WorkflowStatus = GetItemWorkflows(item);
                        }
                        break;
                    case "VersionsRule":
                        if (checkVersion)
                        {
                            CheckVersionRule(result, versionSequenceNo, majorVersionSequenceNo, isMajorVersion, version);
                        }
                        break;
                    case "ListTypeRule":
                        if (item != null)
                        {
                            result.ListType = ((int)item.ParentList.BaseTemplate).ToString();
                        }
                        else
                        {
                            result.ListType = ((int)file.ParentFolder.ParentList.BaseTemplate).ToString();
                        }
                        break;
                    case "ParentFolderNameRule":
                        result.ParentFolderName = file.ParentFolder.Name;
                        break;
                    case "ParentFolderNameHeirarchicallyRule":
                        //use web url is because the ParentFolderNameRule will set list url name to folder name, so keep this logic.
                        var rootFolderPath = file.ParentFolder.ParentList.ParentWeb.ServerRelativeUrl;
                        var folderPath = file.ServerRelativeUrl;

                        string folderNameHeirarchically = string.Empty;
                        if (rootFolderPath.Length + 1 < folderPath.Length)
                        {
                            folderNameHeirarchically = folderPath.Substring(rootFolderPath.Length + 1);
                        }
                        else
                        {
                            var dirindex = file.ServerRelativeUrl.LastIndexOf('/');
                            if (dirindex >= 0)
                            {
                                folderNameHeirarchically = file.ServerRelativeUrl.Substring(dirindex + 1);
                            }
                        }
                        result.ParentFolderName = folderNameHeirarchically;
                        break;
                    case "ParentListNameRule":
                        result.ParentListName = file.ParentFolder.ParentList.Title;
                        break;
                    //add for RevIM term path
                    case "TermRule":
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item);
                        }
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }
            return result;
        }

        private static ObjectInfoBase CommonItemFilter(ref List<FilterPolicy> policies, IAveListItem item, ItemInfo result)
        {
            return CommonItemFilter(ref policies, item, result, 0, 0, false, null, false);
        }
        private static ObjectInfoBase CommonItemFilter(ref List<FilterPolicy> policies, IAveListItem item, ItemInfo result, int versionSequenceNo, int majorVersionSequenceNo, bool isMajorVersion, IAveListItemVersion version, bool checkVersion)
        {
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Item);
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "TitleRule":
                        result.Title = item.Title;
                        break;
                    case "UrlRule":
                        result.Url = item.ParentList.ParentWeb.Url + "/" + item.Url;
                        result.DisplayFormUrl = GetDisplayFormUrlForListItem(item);
                        break;
                    case "ModifiedRule":
                        result.Modified = ToUniversalTime((DateTime)item["Modified"]);
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime((DateTime)item["Created"]);
                        break;
                    case "ModifiedByRule":
                        GetAuthorOrEditorInfo(item, result, false);
                        break;
                    case "CreatedByRule":
                        GetAuthorOrEditorInfo(item, result, true);
                        break;
                    case "ContentTypeRule":
                        result.ContentType = item.ContentType == null ? string.Empty : item.ContentType.Name;
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "MetadataTextColumnRule":
                    case "MetadataNumberColumnRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemColumns(item);
                        }
                        break;
                    case "WorkflowRule":
                        if (result.WorkflowStatus == null)
                        {
                            result.WorkflowStatus = GetItemWorkflows(item);
                        }
                        break;
                    case "VersionsRule":
                        if (checkVersion)
                        {
                            CheckVersionRule(result, versionSequenceNo, majorVersionSequenceNo, isMajorVersion, version);
                        }
                        break;
                    case "ListTypeRule":
                        result.ListType = ((int)item.ParentList.BaseTemplate).ToString();
                        break;
                    //add for RevIM term path
                    case "TermRule":
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item);
                        }
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }
            return result;
        }



        public static ObjectInfoBase GetSiteFilterInfo(List<FilterPolicy> policies, IAveSite site)
        {
            log.Info("Get Filter Site Info:{0}", site.Url);
            SiteCollectionInfo result = new SiteCollectionInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.SiteCollection);

            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "UrlRule":
                        result.Url = site.Url;
                        break;
                    case "TitleRule":
                        result.Title = site.RootWeb.Title;
                        break;
                    case "ModifiedRule":
                        result.Modified = site.LastItemUserModifiedDate;
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime(site.RootWeb.Created);
                        break;
                    case "CreatedByRule":
                    case "OwnerRule":
                        result.OwnerLogonNameWithPrefix = site.Owner.LoginName;
                        result.OwnerLogonName = site.Owner.NoPrefixLoginName;
                        result.OwnerTitle = site.Owner.Name;
                        Int32 index = site.Owner.NoPrefixLoginName.IndexOf("|");
                        if (index != -1)
                        {
                            result.Owner = site.Owner.NoPrefixLoginName.Substring(index + 1);
                        }

                        break;
                    //case "TemplateRule":
                    //    /*
                    //     * 需要说明的是:
                    //     * 对于使用"Save site as template"方式生成的模板创建的site,
                    //     * 其站点模板Id等同于其基础模板Id.
                    //     * 因而, 使用名字过滤时要使用其基础模板的名字.
                    //     * 如, 基于Team site创建的模板, 再使用该模板创建site, 则该site的tmplate id 为"STS#0",
                    //     * 过滤时应填写"Team Site".
                    //     * 
                    //     * 此处逻辑与TemplateIdRule保持一致.(需和QA交代清楚.)
                    //     * Web级别filter与此相同.
                    //     */
                    //    IAveWebTemplateCollection templates = site.GetWebTemplates(site.RootWeb.Language);
                    //    string templateId = site.RootWeb.WebTemplate + "#" + site.RootWeb.Configuration;
                    //    IAveWebTemplate current = templates.First(match => match.Name.Equals(templateId, StringComparison.OrdinalIgnoreCase));
                    //    result.TemplateName = current.Title;
                    //    break;
                    case "TemplateIdRule":
                        result.Template = site.RootWeb.WebTemplate + "#" + site.RootWeb.Configuration;
                        break;
                    case "SizeRule":
                        result.Size = site.Size;
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = FillSiteColumns(site.RootWeb);
                        }
                        break;
                    case "StubLastAccessTimeRule":
                        result.LastAccessTime = site.GetLastAccessTime(site.Url);
                        break;
                    case "StubLastActiveTimeRule":
                        result.LastAccessCompatibleModifiedTime = site.GetLastAccessTime(site.Url, site.LastItemUserModifiedDate, true);
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }
            return result;
        }

        public static ObjectInfoBase GetWebFilterInfo(List<FilterPolicy> policies, IAveWeb web)
        {
            log.Info("Get Filter Web Info:{0}", web.Url);
            SiteInfo result = new SiteInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Site);

            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "UrlRule":
                        result.Url = web.Url;
                        break;
                    case "TitleRule":
                        result.Title = web.Title;
                        break;
                    case "ModifiedRule":
                        result.Modified = web.LastItemUserModifiedDate;
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime(web.Created);
                        break;
                    case "CreatedByRule":
                        if (web.Author != null)
                        {
                            result.CreatedByLogonName = web.Author.LoginName;
                            result.CreatedByTitle = web.Author.Name;
                            result.CreateByEmail = web.Author.Email;    //SAAS-10859 添加对email格式的支持。
                        }
                        else
                        {
                            result.CreatedByLogonName = string.Empty;
                            result.CreatedByTitle = string.Empty;
                            result.CreateByEmail = string.Empty;    //SAAS-10859 添加对email格式的支持。
                        }
                        break;
                    //case "TemplateRule":
                    //    IAveWebTemplateCollection templates = web.Site.GetWebTemplates(web.Language);
                    //    string templateId = web.WebTemplate + "#" + web.Configuration;
                    //    IAveWebTemplate current = templates.FirstOrDefault(match => match.Name.Equals(templateId, StringComparison.OrdinalIgnoreCase));
                    //    result.TemplateName = current == null ? string.Empty : current.Title;
                    //    break;
                    case "TemplateIdRule":
                        result.Template = web.WebTemplate + "#" + web.Configuration;
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = FillSiteColumns(web);
                        }
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }
            return result;
        }

        public static ObjectInfoBase GetListFilterInfo(List<FilterPolicy> policies, IAveList list)
        {
            log.Info("Get Filter List Info:{0}",list.Title);
            ListInfo result = new ListInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.List);

            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "NameRule":
                        result.Name = list.Title;
                        break;
                    case "UrlRule":
                        result.Url = list.ParentWeb.Url + "/" + list.RootFolder.Url;
                        break;
                    case "ModifiedRule":
                        result.Modified = list.LastItemUserModifiedDate;
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime(list.Created);
                        break;
                    case "CreatedByRule":
                        if (list.Author != null)    //Client API do not support this property
                        {
                            result.CreatedByTitle = list.Author.Name;
                            result.CreatedByLogonName = list.Author.LoginName;
                            result.CreateByEmail = list.Author.Email;    //SAAS-10859 添加对email格式的支持。
                        }
                        else
                        {
                            result.CreatedByTitle = string.Empty;
                            result.CreatedByLogonName = string.Empty;
                            result.CreateByEmail = string.Empty;    //SAAS-10859 添加对email格式的支持。
                        }
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        result.ColumnInfos = list.RootFolder.Properties;
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }

            return result;
        }

        public static ObjectInfoBase GetFolderFilterInfo(List<FilterPolicy> policies, IAveFolder folder)
        {
            FolderInfo result = new FolderInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Folder);

            foreach (FilterPolicy policy in policies)
            {
                if (policy.Rule is NameRule)
                {
                    result.Name = folder.Name;
                    continue;
                }
                var item = folder.Item;
                Boolean isRootFolder = false;
                if (item == null && folder.ParentList != null)
                {
                    isRootFolder = string.Equals(folder.ServerRelativeUrl, folder.ParentList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
                }
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "UrlRule":
                        result.Url = folder.ParentWeb.Url + "/" + folder.Url;
                        break;
                    case "ModifiedRule":
                        result.Modified = ToUniversalTimeWithTimeZone(item != null ? (DateTime)item["Modified"] : (isRootFolder ? folder.ParentList.LastItemModifiedDate : DateTime.Now), folder.ParentWeb);
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTimeWithTimeZone(item != null ? (DateTime)item["Created"] : (isRootFolder ? folder.ParentList.Created : DateTime.Now), folder.ParentWeb);
                        break;
                    case "CreatedByRule":
                        if (item != null)
                        {
                            GetAuthorOrEditorInfo(item, result, true);
                        }
                        else
                        {
                            if (isRootFolder)
                            {
                                throw new NotSupportedException(string.Format("List root folder filter policy is not supported.Rule name:{0}.", ruleName));
                            }
                            else
                            {
                                result.CreatedByTitle = string.Empty;
                                result.CreatedByLogonName = string.Empty;
                                result.CreateByEmail = string.Empty;    //SAAS-10859 添加对email格式的支持。
                            }
                        }
                        break;
                    case "ContentTypeRule":
                    case "CustomContentTypeRule":
                    case "ContentTypeNameRule":
                        result.ContentType = item == null || item.ContentType == null ? AveConstants.SYSTEM_FOLDER : item.ContentType.Name;
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemColumns(item);
                        }
                        break;
                    case "TermRule"://add for RevIM folder rule
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item);
                        }
                        break;
                    case "OrphanedFolderRule":
                        var itemCount = item.FieldValues.ContainsKey("ItemChildCount") ? int.Parse(item.FieldValues["ItemChildCount"].ToString()) : -1;
                        var subFolderCount = item.FieldValues.ContainsKey("FolderChildCount") ? int.Parse(item.FieldValues["FolderChildCount"].ToString()) : -1;
                        log.Info(string.Format("ItemCount: {0}, SubFolderCount: {1}", itemCount, subFolderCount));
                        result.IsOrphanedFolder = itemCount == 0 && subFolderCount == 0;
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }

            return result;
        }

        public static ObjectInfoBase GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem item)
        {
            ItemInfo result = new ItemInfo();
            return CommonItemFilter(ref policies, item, result);
        }

        public static ObjectInfoBase GetDocumentFilterInfo(List<FilterPolicy> policies, IAveFile file, IAveListItem item)
        {
            DocumentInfo result = new DocumentInfo();
            return CommonDocumentFilter(ref policies, file, item, result);
        }
        public static ObjectInfoBase GetDocumentFilterInfo(DocumentInfo info, List<FilterPolicy> policies, IAveFile file, IAveListItem item)
        {
            return CommonDocumentFilter(ref policies, file, item, info);
        }

        public static ObjectInfoBase GetDocumentFilterInfo(List<FilterPolicy> policies, IAveListItem item)
        {
            DocumentInfo result = new DocumentInfo();
            return CommonDocumentFilter(ref policies, item, result);
        }

        public static Tuple<DocumentInfo, List<FilterPolicy>> GetDocumentFilterInfo(List<FilterPolicy> policies, AveDiscoverItem item)
        {
            var info = new DocumentInfo();

            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Document);

            var pendingPolicies = CommonDocumentFilter(policies, item, info);

            return new Tuple<DocumentInfo, List<FilterPolicy>>(info, pendingPolicies);
        }

        public static ObjectInfoBase GetDocumentVersionFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion)
        {
            DocumentVersionInfo result = new DocumentVersionInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.DocumentVersion);
            int versionSequenceNo = 0;
            int majorVersionSequenceNo = 0;
            int minorOfMajorSequenceNo = 0;
            bool isMajorVersion = false;
            bool isLastMajorVersion = false;
            if ((uiVersion / 512) == (item.Versions[0].VersionId / 512))
            {
                isLastMajorVersion = true;
            }
            IAveListItemVersion version = null;
            foreach (IAveListItemVersion tmpVersion in item.Versions)
            {
                isMajorVersion = tmpVersion.VersionId % 512 == 0;
                if (tmpVersion.VersionId == uiVersion)
                {
                    version = tmpVersion;
                    result.IsCurrentVersion = tmpVersion.IsCurrentVersion;
                    break;
                }
                if (tmpVersion.IsCurrentVersion)
                {
                    continue;
                }
                if ((tmpVersion.VersionId / 512 == uiVersion / 512) && !isMajorVersion)
                {
                    ++minorOfMajorSequenceNo;
                }
                ++versionSequenceNo;
                majorVersionSequenceNo += isMajorVersion ? 1 : 0;
            }
            if (version == null)
            {
                return null;
            }
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "NameRule":
                        result.Name = item.Name;
                        break;
                    case "DocumentName":
                        result.Name = item.Name;
                        break;
                    case "TitleRule":
                        try
                        {
                            result.Title = GetValue<string>(version["Title"], string.Empty);
                        }
                        catch (ArgumentException)
                        {
                            result.Title = string.Empty;
                        }
                        break;
                    case "SizeRule":
                        if (policy.Rule.Value1.Equals("Version Size", StringComparison.OrdinalIgnoreCase))
                        {
                            var fileVersion = item.File.Versions.GetVersionFromID(version.VersionId);
                            result.Size = fileVersion.Size;
                        }
                        else
                        {
                            result.Size = item.File.Length;
                        }
                        break;
                    case "ModifiedRule":
                        result.Modified = DateTime.SpecifyKind(((DateTime)version["Modified"]), DateTimeKind.Utc);
                        break;
                    case "DocumentModifiedRule":
                        try
                        {
                            result.Modified = DateTime.SpecifyKind((DateTime)item.FieldValues["Modified"], DateTimeKind.Utc);
                            log.Info($"Get item modified time, result.Modified:{result.Modified.Ticks}");
                        }
                        catch (Exception e)
                        {
                            log.Error($"get item modified time failed,error:{e}");
                        }
                        break;
                    case "CreatedRule":
                        //this time is Utc Time ,we only need to give a DateTimeKind
                        result.Created = DateTime.SpecifyKind(((DateTime)version["Created"]), DateTimeKind.Utc);
                        break;
                    case "ModifiedByRule":
                        GetVersionEditorInfo(version, result);
                        break;
                    case "KeepHistoryVersionRule":
                        result.VersionSequenceNo = versionSequenceNo;
                        result.MajorVersionSequenceNo = isMajorVersion ? majorVersionSequenceNo : int.MaxValue;
                        result.UIVersion = version.VersionId;
                        result.CurrentMinorVersionSequenceNo = minorOfMajorSequenceNo;
                        result.IsLastMajorVersion = isLastMajorVersion;
                        result.Approved = true;
                        break;
                    case "ListTypeRule":
                        result.ListType = ((int)item.ParentList.BaseTemplate).ToString();
                        break;
                    case "IsStubRule":
                        var site = item.Web.Site;
                        result.IsStub = CheckIsStub(site, item.UniqueId, version.Level, version.VersionId);
                        break;
                    case "StubLastAccessTimeRule":
                        site = item.Web.Site;
                        result.IsStub = CheckIsStub(site, item.UniqueId, version.Level, version.VersionId);
                        if (result.IsStub)
                        {
                            result.StubLastAccessTime = GetStubLastAccessTime(site, item.UniqueId, version.Level, version.VersionId);
                        }
                        else
                        {
                            result.StubLastAccessTime = new DateTime();
                        }
                        break;
                    case "StubCreateTimeRule":
                        site = item.Web.Site;
                        result.IsStub = CheckIsStub(site, item.UniqueId, version.Level, version.VersionId);
                        if (result.IsStub)
                        {
                            result.StubCreated = GetStubCreateTime(site, item.UniqueId, version.Level, version.VersionId);
                        }
                        else
                        {
                            result.StubCreated = new DateTime();
                        }
                        break;
                    //case "SensitivityLabelRule":
                    //    result.SensitivityLabel = string.Empty;
                    //    if (item?.FieldValues != null)
                    //    {
                    //        if (item.FieldValues.ContainsKey(SPColumnConstants.Sensitive_Label_Display_Name))
                    //        {
                    //            var labelDispalyName = (item.FieldValues[SPColumnConstants.Sensitive_Label_Display_Name] as string) ?? string.Empty;
                    //            try
                    //            {
                    //                var splitLabelDisplayName = labelDispalyName.Split("\\");
                    //                result.SensitivityLabel = splitLabelDisplayName.Length > 0 ? splitLabelDisplayName[splitLabelDisplayName.Length - 1].TrimStart() : string.Empty;
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                log.Info($"Get item sensitive label display name have some issue {ex}");
                    //                result.SensitivityLabel = labelDispalyName;
                    //            }
                    //        }
                    //    }
                    //    break;
                    case "SensitivityLabelFullNameRule":
                        result.SensitivityLabelFullName = string.Empty;
                        if (item?.FieldValues != null)
                        {
                            if (item.FieldValues.ContainsKey(SPColumnConstants.Sensitive_Label_Display_Name))
                            {
                                var labelDispalyName = (item.FieldValues[SPColumnConstants.Sensitive_Label_Display_Name] as string) ?? string.Empty;
                                try
                                {
                                    result.SensitivityLabelFullName = labelDispalyName;
                                }
                                catch (Exception ex)
                                {
                                    log.Info($"Get item sensitive label display name have some issue {ex}");
                                    result.SensitivityLabelFullName = labelDispalyName;
                                }
                            }
                        }
                        break;
                    case "ColumnTextRule":
                    case "MetadataTextColumnRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemVersionColumns(version);
                        }
                        if (result.ColumnInfosOfInternalName == null)
                        {
                            result.ColumnInfosOfInternalName = GetItemVersionInternalColumns(version);
                        }
                        string documentRuleName = policy.Rule.GetType().Name;
                        documentRuleName = documentRuleName.Substring(documentRuleName.LastIndexOf('.') + 1);
                        if (documentRuleName.EqualIgnoreCase("ColumnTextRule"))
                        {
                            if (result.ListColumnExistInfos == null)
                            {
                                result.ListColumnExistInfos = new Dictionary<string, bool>();
                            }
                            if (item.ParentList.Fields != null && item.ParentList.Fields.ContainsField(policy.Rule.Value1))
                            {
                                if (result.ListColumnExistInfos.ContainsKey(policy.Rule.Value1))
                                {
                                    result.ListColumnExistInfos[policy.Rule.Value1] = true;
                                }
                                else
                                {
                                    result.ListColumnExistInfos.Add(policy.Rule.Value1, true);
                                }
                            }
                            else
                            {
                                if (result.ListColumnExistInfos.ContainsKey(policy.Rule.Value1))
                                {
                                    result.ListColumnExistInfos[policy.Rule.Value1] = false;
                                }
                                else
                                {
                                    result.ListColumnExistInfos.Add(policy.Rule.Value1, false);
                                }
                            }
                        }
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }
            return result;
        }

        public static ObjectInfoBase GetItemVersionFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion)
        {
            ItemVersionInfo result = new ItemVersionInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.ItemVersion);
            int versionSequenceNo = 0;
            int majorVersionSequenceNo = 0;
            bool isMajorVersion = false;
            IAveListItemVersion version = null;
            foreach (IAveListItemVersion tmpVersion in item.Versions)
            {
                isMajorVersion = tmpVersion.VersionId % 512 == 0;
                if (tmpVersion.VersionId == uiVersion)
                {
                    version = tmpVersion;
                    result.IsCurrentVersion = tmpVersion.IsCurrentVersion;
                    break;
                }
                if (tmpVersion.IsCurrentVersion)
                {
                    continue;
                }
                ++versionSequenceNo;
                majorVersionSequenceNo += isMajorVersion ? 1 : 0;
            }
            if (version == null)
            {
                return null;
            }
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "TitleRule":
                        try
                        {
                            result.Title = GetValue<string>(version["Title"], string.Empty);
                        }
                        catch (ArgumentException)//Column named Title does not exist
                        {
                            result.Title = string.Empty;
                        }
                        break;
                    case "ModifiedRule":
                        result.Modified = DateTime.SpecifyKind(((DateTime)version["Modified"]), DateTimeKind.Utc);//ToUniversalTime((DateTime)version["Modified"]);
                        break;
                    case "ModifiedByRule":
                        GetVersionEditorInfo(version, result);
                        break;
                    case "KeepHistoryVersionRule":
                        result.VersionSequenceNo = versionSequenceNo;
                        result.MajorVersionSequenceNo = isMajorVersion ? majorVersionSequenceNo : int.MaxValue;
                        result.UIVersion = version.VersionId;
                        result.Approved = true;
                        break;
                    case "ListTypeRule":
                        result.ListType = ((int)item.ParentList.BaseTemplate).ToString();
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }

            return result;
        }

        /// <summary>
        /// 由于Discover自己会过滤Version，所以用此方法强制将VersionRule强制满足过滤
        /// </summary>
        public static ObjectInfoBase SetVersionAlwaysTrue(List<FilterPolicy> policies, ObjectInfoBase result)
        {
            if (result is ItemInfo)
            {
                policies = CreateVersionRuleFiltersCopy(policies, PolicyLevel.Item);
            }
            else
            {
                policies = CreateVersionRuleFiltersCopy(policies, PolicyLevel.Document);
            }

            foreach (FilterPolicy policy in policies)
            {
                if (policy.Rule is VersionsRule)
                {
                    switch (policy.Condition)
                    {
                        case PolicyCondition.OnlyLastNVersions:
                            int lastVersionCount = int.Parse(policy.Value.Value1);
                            if (result is ItemInfo)
                            {
                                (result as ItemInfo).VersionSequenceNo = lastVersionCount - 1;
                            }
                            else
                            {
                                (result as DocumentInfo).VersionSequenceNo = lastVersionCount - 1;
                            }
                            break;
                        case PolicyCondition.ExceptLastNVersions:
                        case PolicyCondition.MajorAndMintorVersions:
                            int leaveLastVersionCount = int.Parse(policy.Value.Value1);
                            if (result is ItemInfo)
                            {
                                (result as ItemInfo).VersionSequenceNo = leaveLastVersionCount + 1;
                            }
                            else
                            {
                                (result as DocumentInfo).VersionSequenceNo = leaveLastVersionCount + 1;
                            }
                            break;
                        case PolicyCondition.OnlyLastMajorNVersions:
                            int leaveLastMajorVersionCount = int.Parse(policy.Value.Value1);
                            if (result is ItemInfo)
                            {
                                (result as ItemInfo).MajorVersionSequenceNo = leaveLastMajorVersionCount - 1;
                            }
                            else
                            {
                                (result as DocumentInfo).MajorVersionSequenceNo = leaveLastMajorVersionCount - 1;
                            }
                            break;
                        case PolicyCondition.OnlyMajorVersions:
                            if (result is ItemInfo)
                            {
                                (result as ItemInfo).UIVersion = 512;
                            }
                            else
                            {
                                (result as DocumentInfo).UIVersion = 512;
                            }
                            break;
                        case PolicyCondition.OnlyMionrVersions:
                            if (result is ItemInfo)
                            {
                                (result as ItemInfo).UIVersion = 511;
                            }
                            else
                            {
                                (result as DocumentInfo).UIVersion = 511;
                            }
                            break;
                        case PolicyCondition.OnlyApproved:
                            if (result is ItemInfo)
                            {
                                (result as ItemInfo).Approved = true;
                            }
                            else
                            {
                                (result as DocumentInfo).Approved = true;
                            }
                            break;
                        case PolicyCondition.Exactly:
                        case PolicyCondition.Contains:
                        case PolicyCondition.StartWith:
                        case PolicyCondition.EndWith:
                        case PolicyCondition.LessOrEqualThan:
                        case PolicyCondition.GreaterOrEqualThan:
                        case PolicyCondition.FromTo:
                        case PolicyCondition.Before:
                        case PolicyCondition.After:
                        case PolicyCondition.On:
                        case PolicyCondition.WithIn:
                        default:
                            throw new AveException("Invalid Condition.{0}", policy.Condition.ToString());
                    }
                }
            }
            return result;
        }

        public static ObjectInfoBase GetAttachmentFilterInfo(List<FilterPolicy> policies, IAveFile attachemnt, IAveListItem item)
        {
            AttachmentInfo result = new AttachmentInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Attachment);

            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "SizeRule":
                        result.Size = attachemnt.Length;
                        break;
                    case "NameRule":
                        result.Name = attachemnt.Name;
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTime(attachemnt.TimeCreated);
                        break;
                    case "CreatedByRule":
                        result.CreatedByTitle = attachemnt.Author.Name;
                        result.CreatedByLogonName = attachemnt.Author.LoginName;
                        result.CreateByEmail = attachemnt.Author.Email;    //SAAS-10859 添加对email格式的支持。
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemColumns(item);
                        }
                        break;
                    case "ListTypeRule":
                        result.ListType = ((int)item.ParentList.BaseTemplate).ToString();
                        break;
                    case "IsStubRule":
                        var site = attachemnt.Web.Site;
                        result.IsStub = CheckIsStub(site, attachemnt.UniqueId, attachemnt.Level, attachemnt.UIVersion);
                        break;
                    case "StubLastAccessTimeRule":
                        site = attachemnt.Web.Site;
                        result.IsStub = CheckIsStub(site, attachemnt.UniqueId, attachemnt.Level, attachemnt.UIVersion);
                        if (result.IsStub)
                        {
                            result.StubLastAccessTime = GetStubLastAccessTime(site, attachemnt.UniqueId, attachemnt.Level, attachemnt.UIVersion);
                        }
                        else
                        {
                            result.StubLastAccessTime = new DateTime();
                        }
                        break;
                    case "StubCreateTimeRule":
                        site = attachemnt.Web.Site;
                        result.IsStub = CheckIsStub(site, attachemnt.UniqueId, attachemnt.Level, attachemnt.UIVersion);
                        if (result.IsStub)
                        {
                            result.StubCreated = GetStubCreateTime(site, attachemnt.UniqueId, attachemnt.Level, attachemnt.UIVersion);
                        }
                        else
                        {
                            result.StubCreated = new DateTime();
                        }
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }

            return result;
        }

        //以下方法为07 migration添加
        #region
        public static ObjectInfoBase GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion)
        {
            ItemInfo result = new ItemInfo();

            int versionSequenceNo = 0;
            int majorVersionSequenceNo = 0;
            bool isMajorVersion = false;
            IAveListItemVersion version = null;
            foreach (IAveListItemVersion tmpVersion in item.Versions)
            {
                isMajorVersion = tmpVersion.VersionId % 512 == 0;
                if (tmpVersion.VersionId == uiVersion)
                {
                    version = tmpVersion;
                    result.IsCurrentVersion = tmpVersion.IsCurrentVersion;
                    break;
                }
                if (tmpVersion.IsCurrentVersion)
                {
                    continue;
                }
                ++versionSequenceNo;
                majorVersionSequenceNo += isMajorVersion ? 1 : 0;
            }
            if (version == null)
            {
                return null;
            }
            return CommonItemFilter(ref policies, item, result, versionSequenceNo, majorVersionSequenceNo, isMajorVersion, version, true);

        }

        public static ObjectInfoBase GetDocumentFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion, IAveFile file)
        {
            DocumentInfo result = new DocumentInfo();

            int versionSequenceNo = 0;
            int majorVersionSequenceNo = 0;
            bool isMajorVersion = false;
            IAveListItemVersion version = null;
            foreach (IAveListItemVersion tmpVersion in item.Versions)
            {
                isMajorVersion = tmpVersion.VersionId % 512 == 0;
                if (tmpVersion.VersionId == uiVersion)
                {
                    version = tmpVersion;
                    result.IsCurrentVersion = tmpVersion.IsCurrentVersion;
                    break;
                }
                if (tmpVersion.IsCurrentVersion)
                {
                    continue;
                }
                ++versionSequenceNo;
                majorVersionSequenceNo += isMajorVersion ? 1 : 0;
            }
            if (version == null)
            {
                return null;
            }
            return CommonDocumentFilter(ref policies, file, item, result, versionSequenceNo, majorVersionSequenceNo, isMajorVersion, version, true);
        }

        private static void CheckVersionRule(VersionedObjectInfoBase result, int versionSequenceNo, int majorVersionSequenceNo, bool isMajorVersion, IAveListItemVersion version)
        {
            result.VersionSequenceNo = versionSequenceNo;
            result.MajorVersionSequenceNo = isMajorVersion ? majorVersionSequenceNo : int.MaxValue;
            result.UIVersion = version.VersionId;
            result.Approved = version.Level == AveFileLevel.Published;
            result.IsCurrentVersion = version.IsCurrentVersion;
        }
        #endregion

        private static bool CheckIsStub(IAveSite site, Guid guid, AveFileLevel level, int uiVersion)
        {
            var contentDatabase = site.ContentDatabase;
            string contentDBConnectionString = contentDatabase.DatabaseConnectionString;
            Assembly blobAssembly = Assembly.Load("AgentCommonBlobUtility");
            Type DBUtilityType = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.DBUtility");
            var DBUtiltiy = blobAssembly.CreateInstance(DBUtilityType.FullName, true, BindingFlags.Default, null, new object[] { contentDBConnectionString }, null, null);
            return (bool)DBUtilityType.GetMethod("IsStub").Invoke(DBUtiltiy, new Object[] { site.ID, guid, level, uiVersion });
        }

        private static DateTime GetStubLastAccessTime(IAveSite site, Guid guid, AveFileLevel level, int uiVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.LATPerformance.GetStubLastAccessTime"))
            {
                var contentDatabase = site.ContentDatabase;
                string contentDBConnectionString = contentDatabase.DatabaseConnectionString;
                Assembly blobAssembly = Assembly.Load("AgentCommonBlobUtility");
                var blobParamsClass = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.BlobParams");
                var parmas = blobParamsClass.GetMethod("GenerateConvertBlobParams").Invoke(null, new object[] { site.WebApplication.ID, site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString(), contentDatabase.ID, contentDBConnectionString, site.ID, guid, uiVersion, level, null });
                var blobInfoClass = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.BlobInfo");
                var soModuleType = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.SOModuleType");

                var blobInfo = blobInfoClass.GetMethod("CreateBlobInfo", new Type[] { soModuleType, parmas.GetType() }).Invoke(null, new object[] { 4, parmas });

                blobInfoClass.GetMethod("InnerGetBlob", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(blobInfo, null);
                var stubInfo = blobInfoClass.GetProperty("StubInfo").GetValue(blobInfo, null);
                var stubInfoClass = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.StubInfo");
                var StubLastAccessTime = (DateTime)stubInfoClass.GetProperty("LastAccessTime").GetValue(stubInfo, null);
                if (StubLastAccessTime == new DateTime())
                {
                    StubLastAccessTime = (DateTime)stubInfoClass.GetProperty("CreationTime").GetValue(stubInfo, null);
                }
                return StubLastAccessTime;
            }
        }

        private static DateTime GetStubCreateTime(IAveSite site, Guid guid, AveFileLevel level, int uiVersion)
        {
            var contentDatabase = site.ContentDatabase;
            string contentDBConnectionString = contentDatabase.DatabaseConnectionString;
            Assembly blobAssembly = Assembly.Load("AgentCommonBlobUtility");
            var blobParamsClass = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.BlobParams");
            var parmas = blobParamsClass.GetMethod("GenerateConvertBlobParams").Invoke(null, new object[] { site.WebApplication.ID, site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString(), contentDatabase.ID, contentDBConnectionString, site.ID, guid, uiVersion, level, null });
            var blobInfoClass = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.BlobInfo");
            var soModuleType = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.SOModuleType");

            var blobInfo = blobInfoClass.GetMethod("CreateBlobInfo", new Type[] { soModuleType, parmas.GetType() }).Invoke(null, new object[] { 4, parmas });

            blobInfoClass.GetMethod("InnerGetBlob", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(blobInfo, null);
            var stubInfo = blobInfoClass.GetProperty("StubInfo").GetValue(blobInfo, null);
            var stubInfoClass = blobAssembly.GetType("AvePoint.StorageOptimization.BlobUtility.StubInfo");
            var StubCreateTime = (DateTime)stubInfoClass.GetProperty("CreationTime").GetValue(stubInfo, null);
            return StubCreateTime;
        }

        private static string GetDisplayFormUrlForListItem(IAveListItem item)
        {
            string url = string.Empty;
            string displayFormUrl = item.ParentList.DefaultDisplayFormUrl;
            string webUrl = item.ParentList.ParentWeb.Url;
            string webRelativeUrl = item.ParentList.ParentWeb.ServerRelativeUrl;
            //Meetings类型下面的Item没有displayForm,暂时不处理
            if (!string.IsNullOrEmpty(displayFormUrl))
            {
                //User Information List DisplayFormUrl is Absolute URL.
                if (AveUrlUtility.IsUrlRelative(displayFormUrl))
                {
                    url = webUrl.TrimEnd('/') + "/" + displayFormUrl.TrimStart('/').Substring(webRelativeUrl.TrimStart('/').Length).TrimStart('/') + "?ID=" + item.ID;
                }
                else
                {
                    url = displayFormUrl + "?ID=" + item.ID;
                }
            }
            return url;
        }

    }
}
