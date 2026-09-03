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
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Backup;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Web;

namespace RAExportCommon
{
    class CustomizeProperty
    {
        #region DateTime Column Summary
        //        语法糖时间——UTC：
        //          1.Local&Office365：list级别，通过Wrapper API获取，Created，LastItemModifiedDate 获取的是UTC时间，并且Kind为UTC.
        //          2.1.Local：Folder，Document 级别，通过index获取Created，Modified，获取的是页面时间，Kind为Unspecified.
        //          2.2.1.Office365，Folder，级别，通过index获取Created，Modified，获取的是页面时间，Kind为Local.
        //          2.2.2.Office365，Document级别,通过index获取Created，Modified，获取的是UTC时间，并且Kind为UTC.
        //        SP时间类型Column——SP页面时间：
        //          1.Local&Office365 Folder，Document级别(List没有Column方法), 通过GetColumnValues获取时间类型的column，获取的是UTC时间，并且Kind为UTC.
        #endregion

        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private const string ERROR = @"[!ERROR!]";

        private static List<string> str = new List<string>()
        {
            "@Title@","@Name@","@CreatedTime@","@TimeNow@","@ModifiedTime@","@ID@","@ExtensionName@","@FileContent@","@CreatedBy@","@ContentType@","@ModifiedBy@","@UniqueId@","@FileEncodingText@","@EncodingContextPath@"
        };

        internal static string GetPropertyValue(bool SharePointMetadataAsSource, string defaultValue, string columnName, AveSPList aveList)
        {
            string value = string.Empty;
            if (SharePointMetadataAsSource)
            {
                value = GetValueFromSharepoint(columnName, aveList);
            }
            else
            {
                value = GetDefaultValue(defaultValue, aveList);
            }
            return value;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetValueFromSharepoint(string columnName, AveSPList aveList)
        {
            string value = string.Empty;
            if (str.Contains(columnName))
            {
                try
                {
                    if (columnName == "@Title@")
                    {
                        value = aveList.SPList.Title;
                    }
                    else if (columnName == "@Name@")
                    {
                        value = aveList.SPList.RootFolder.Name;
                    }
                    else if (columnName == "@CreatedTime@")
                    {
                        if (aveList.SPList.Created.Kind != DateTimeKind.Utc)
                        {
                            value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.Created)).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }
                        else
                        {
                            value = aveList.SPList.Created.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }
                    }
                    else if (columnName == "@TimeNow@")
                    {
                        value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else if (columnName == "@ModifiedTime@")
                    {
                        //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                        if (aveList.SPList.LastItemModifiedDate.Kind != DateTimeKind.Utc)
                        {
                            value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.LastItemModifiedDate)).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }
                        else
                        {
                            value = aveList.SPList.LastItemModifiedDate.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }

                    }
                    else if (columnName == "@ID@")
                    {
                        value = aveList.SPList.ID.ToString();
                    }
                    else if (columnName == "@ExtensionName@")
                    {
                        value = ERROR;
                    }
                    else if (columnName == "@FileContent@")
                    {
                        value = ERROR;
                    }
                    else if (columnName == "@EncodingContextPath@")
                    {
                        string fullPath = GetWebappUrl(aveList.ParentSite) + aveList.ServerRelativeUrl;
                        value = HttpUtility.UrlEncode(fullPath);
                    }
                    else if (columnName == "@CreatedBy@")
                    {
                        if (aveList.SPList.Author == null)
                        {
                            value = ERROR;
                        }
                        else
                        {
                            if (aveList.SPList.Author.LoginName.Contains("|"))
                            {
                                int index = aveList.SPList.Author.LoginName.IndexOf('|');
                                value = aveList.SPList.Author.LoginName.Substring(index + 1, aveList.SPList.Author.LoginName.Length - index - 1);
                            }
                            else
                            {
                                value = aveList.SPList.Author.LoginName;
                            }
                        }
                    }
                    else if (columnName == "@ContentType@")
                    {
                        value = ERROR;
                    }
                    else if (columnName == "@ModifiedBy@")
                    {
                        value = ERROR;
                    }
                    else
                    {
                        //进入这里必须满足两种条件：1.语法糖集合中有。2.语法糖中没有对此特殊语法进行处理，可能性低。
                        value = ERROR;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get column value from SharePoint with grammar in list level.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            else if (aveList.SPList.RootFolder.Properties.Contains(columnName))
            {
                try
                {
                    object tempValue = aveList.SPList.RootFolder.Properties[columnName];
                    if (tempValue is DateTime)
                    {
                        DateTime temp = (DateTime)tempValue;
                        //if (temp.Kind == DateTimeKind.Unspecified)
                        //{
                        //    temp = DateTime.SpecifyKind(temp, DateTimeKind.Utc).ToLocalTime();
                        //}
                        //else if (temp.Kind == DateTimeKind.Utc)
                        //{
                        //    temp = temp.ToLocalTime();
                        //}
                        value = temp.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else
                    {
                        if (tempValue == null)
                        {
                            mLog.Info("Get list property value is null,column name: {0}.", columnName);
                            value = string.Empty;
                        }
                        else
                        {
                            value = tempValue.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info("Can not get column value in list level by list property,Info: {0}.", ex.ToString());
                    value = ERROR;
                }
            }
            else if (columnName == "Name")
            {
                value = aveList.SPList.RootFolder.Name;
            }
            else
            {
                mLog.Info("List level not support get Metadata from list column.");
                value = ERROR;
            }
            return value;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetDefaultValue(string defaultValue, AveSPList aveList)
        {
            string value = string.Empty;
            if (str.Contains(defaultValue))
            {
                try
                {
                    if (defaultValue == "@Title@")
                    {
                        value = aveList.SPList.Title;
                    }
                    else if (defaultValue == "@Name@")
                    {
                        value = aveList.SPList.RootFolder.Name;
                    }
                    else if (defaultValue == "@CreatedTime@")
                    {
                        if (aveList.SPList.Created.Kind != DateTimeKind.Utc)
                        {
                            value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.Created)).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }
                        else
                        {
                            value = aveList.SPList.Created.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }
                    }
                    else if (defaultValue == "@TimeNow@")
                    {
                        value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else if (defaultValue == "@ModifiedTime@")
                    {
                        //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                        if (aveList.SPList.LastItemModifiedDate.Kind != DateTimeKind.Utc)
                        {
                            value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.LastItemModifiedDate)).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }
                        else
                        {
                            value = aveList.SPList.LastItemModifiedDate.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        }

                    }
                    else if (defaultValue == "@ID@")
                    {
                        value = aveList.SPList.ID.ToString();
                    }
                    else if (defaultValue == "@ExtensionName@")
                    {
                        value = ERROR;
                    }
                    else if (defaultValue == "@FileContent@")
                    {
                        value = ERROR;
                    }
                    else if (defaultValue == "@EncodingContextPath@")
                    {
                        string fullPath = GetWebappUrl(aveList.ParentSite) + aveList.ServerRelativeUrl;
                        value = HttpUtility.UrlEncode(fullPath);
                    }
                    else if (defaultValue == "@CreatedBy@")
                    {
                        //office 365 不支持此API，author为null
                        if (aveList.SPList.Author == null)
                        {
                            value = ERROR;
                        }
                        else
                        {
                            if (aveList.SPList.Author.LoginName.Contains("|"))
                            {
                                int index = aveList.SPList.Author.LoginName.IndexOf('|');
                                value = aveList.SPList.Author.LoginName.Substring(index + 1, aveList.SPList.Author.LoginName.Length - index - 1);
                            }
                            else
                            {
                                value = aveList.SPList.Author.LoginName;
                            }
                        }
                    }
                    else if (defaultValue == "@ContentType@")
                    {
                        value = ERROR;
                    }
                    else if (defaultValue == "@ModifiedBy@")
                    {
                        value = ERROR;
                    }
                    else
                    {
                        value = defaultValue;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get column value from SharePoint with SharePoint API in list level.Info: {0}.", e.ToString());
                    value = defaultValue;
                }
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        internal static string GetPropertyValue(bool SharePointMetadataAsSource, string defaultValue, string columnName, AveSPFolder aveFolder)
        {
            string value = string.Empty;
            if (SharePointMetadataAsSource)
            {
                value = GetValueFromSharepoint(columnName, aveFolder);
            }
            else
            {
                value = GetDefaultValue(defaultValue, aveFolder);
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetValueFromSharepoint(string columnName, AveSPFolder aveFolder)
        {
            string value = string.Empty;
            if (str.Contains(columnName))
            {
                try
                {
                    if (columnName == "@Title@")
                    {
                        value = aveFolder.SPFolder.Name;
                    }
                    else if (columnName == "@Name@")
                    {
                        value = aveFolder.SPFolder.Name;
                    }
                    else if (columnName == "@CreatedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveFolder.SPFolder.Item["Created"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveFolder.SPFolder.Item["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.ID, (DateTime)aveFolder.SPFolder.Item["Created"]);
                    }
                    else if (columnName == "@TimeNow@")
                    {
                        value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else if (columnName == "@ModifiedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveFolder.SPFolder.Item["Modified"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveFolder.SPFolder.Item["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.ID, (DateTime)aveFolder.SPFolder.Item["Modified"]);
                    }
                    else if (columnName == "@ID@")
                    {
                        value = aveFolder.AveItem.RowId.ToString();
                    }
                    else if (columnName == "@ExtensionName@")
                    {
                        value = ERROR;
                    }
                    else if (columnName == "@FileContent@")
                    {
                        value = ERROR;
                    }
                    else if (columnName == "@EncodingContextPath@")
                    {
                        string fullPath = GetWebappUrl(aveFolder.ParentSite) + aveFolder.ServerRelativeUrl;
                        value = HttpUtility.UrlEncode(fullPath);
                    }
                    else if (columnName == "@CreatedBy@")
                    {
                        //Login Name:"i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，由于local和365目前创建user不可以添加"|"特殊字符，因此通过此字符进行截取.
                        int indexUser = (aveFolder.SPFolder.Item.Author.LoginName).LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                        if (indexUser > 0)
                        {
                            value = (aveFolder.SPFolder.Item.Author.LoginName).Substring(indexUser + 1);
                        }
                        else
                        {
                            value = (aveFolder.SPFolder.Item.Author.LoginName).ToString();
                        }
                    }
                    else if (columnName == "@ContentType@")
                    {
                        value = aveFolder.SPFolder.Item.ContentType.Name;
                    }
                    else if (columnName == "@ModifiedBy@")
                    {
                        //Editor获取的是Display Name，不需要考虑其它语言，ModifiedBy获取的Logon Name，需要考虑其它语言支持.
                        //UserName获取的为："i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，需要用NoPrefixLoginName获取正确的username
                        try
                        {
                            if (aveFolder.SPFolder.Item != null)
                            {
                                string itemUserInfo = aveFolder.SPFolder.Item["Editor"].ToString();
                                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                IAveUser user = aveFolder.SPFolder.Item.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                                if (user != null)
                                {
                                    value = user.NoPrefixLoginName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            value = ERROR;
                            mLog.Warn("Can not get ModifiedBy,Info: {0}.", ex.ToString());
                        }
                    }
                    else
                    {
                        //进入这里必须满足两种条件：1.语法糖集合中有。2.语法糖中没有对此特殊语法进行处理，可能性低。
                        value = ERROR;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get column value from SharePoint with SharePoint API in folder level.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            else if (aveFolder.SPFolder.Properties.Contains(columnName))
            {
                try
                {
                    object tempValue = aveFolder.SPFolder.Properties[columnName];
                    if (tempValue is DateTime)
                    {
                        DateTime temp = (DateTime)tempValue;
                        //if (temp.Kind == DateTimeKind.Unspecified)
                        //{
                        //    temp = DateTime.SpecifyKind(temp, DateTimeKind.Utc).ToLocalTime();
                        //}
                        //else if (temp.Kind == DateTimeKind.Utc)
                        //{
                        //    temp = temp.ToLocalTime();
                        //}
                        value = temp.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else
                    {
                        if (tempValue == null)
                        {
                            mLog.Info("Get folder property value is null,column name: {0}.", columnName);
                            value = string.Empty;
                        }
                        else
                        {
                            value = tempValue.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("Can not get column value in folder level by folder property,Info: {0}.", ex.ToString());
                    value = ERROR;
                }
            }
            else if (columnName == "Name")
            {
                value = aveFolder.SPFolder.Name;
            }
            else
            {
                try
                {
                    Dictionary<string, object> columns = aveFolder.AveItem.GetColumnValues(AvePoint.Wrapper.Backup.ColumnsLevel.AllVisiableColumns);
                    if (columns == null)
                    {
                        value = aveFolder.SPFolder.Item[columnName].ToString();
                    }
                    else
                    {
                        if (columns.ContainsKey(columnName))
                        {
                            object tempValue = columns[columnName];
                            if (tempValue is DateTime)
                            {
                                DateTime temp = (DateTime)tempValue;
                                if (temp.Kind == DateTimeKind.Utc)
                                {
                                    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveFolder.AveList.ParentWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                    temp = temp + cstZone.GetUtcOffset(temp);
                                    value = (temp).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                                }
                                else
                                {
                                    value = temp.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                                }
                            }
                            else if (tempValue is bool)
                            {
                                try
                                {
                                    Array diplayColumn = aveFolder.SPFolder.Item.Fields[columnName].TypeDisplayName.Split('/').ToArray();
                                    if (tempValue.Equals(true))
                                    {
                                        value = diplayColumn.GetValue(0).ToString();
                                    }
                                    else
                                    {
                                        value = diplayColumn.GetValue(1).ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info("Can not get bool column value from SharePoint in folder level by folder column.Info: {0}.", ex.ToString());
                                    value = tempValue.ToString();
                                }
                            }
                            else
                            {
                                if (tempValue == null)
                                {
                                    mLog.Info("Get folder property value is null,column name: {0}.", columnName);
                                    value = string.Empty;
                                }
                                else
                                {
                                    value = tempValue.ToString();
                                }
                            }
                        }
                        else
                        {
                            //最后都不符合给出提示语。
                            value = ERROR;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Can not get column value from SharePoint in folder level by folder column.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetDefaultValue(string defaultValue, AveSPFolder aveFolder)
        {
            string value = string.Empty;
            if (str.Contains(defaultValue))
            {
                try
                {
                    if (defaultValue == "@Title@")
                    {
                        //SP API获取不到Folder级别Title，只能通过Folder Name获取.
                        //通过aveFolder.AveItem.Title & aveFolder.AveItem.Name 获取，10，13环境Title能获取到，Name为空，16&365环境，这俩属性都为空.
                        value = aveFolder.SPFolder.Name;
                    }
                    else if (defaultValue == "@Name@")
                    {
                        value = aveFolder.SPFolder.Name;
                    }
                    else if (defaultValue == "@CreatedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveFolder.SPFolder.Item["Created"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveFolder.SPFolder.Item["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.ID, (DateTime)aveFolder.SPFolder.Item["Created"]);
                    }
                    else if (defaultValue == "@TimeNow@")
                    {
                        value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else if (defaultValue == "@ModifiedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveFolder.SPFolder.Item["Modified"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveFolder.SPFolder.Item["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.ID, (DateTime)aveFolder.SPFolder.Item["Modified"]);
                    }
                    else if (defaultValue == "@ID@")
                    {
                        value = aveFolder.AveItem.RowId.ToString();
                    }
                    else if (defaultValue == "@ExtensionName@")
                    {
                        value = ERROR;
                    }
                    else if (defaultValue == "@FileContent@")
                    {
                        value = ERROR;
                    }
                    else if (defaultValue == "@EncodingContextPath@")
                    {
                        string fullPath = GetWebappUrl(aveFolder.ParentSite) + aveFolder.ServerRelativeUrl;
                        value = HttpUtility.UrlEncode(fullPath);
                    }
                    else if (defaultValue == "@CreatedBy@")
                    {
                        //Login Name:"i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，由于local和365目前创建user不可以添加"|"特殊字符，因此通过此字符进行截取.
                        int indexUser = (aveFolder.AveItem.Author.Login).LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                        if (indexUser > 0)
                        {
                            value = (aveFolder.AveItem.Author.Login).Substring(indexUser + 1);
                        }
                        else
                        {
                            value = (aveFolder.AveItem.Author.Login).ToString();
                        }
                    }
                    else if (defaultValue == "@ContentType@")
                    {
                        value = aveFolder.SPFolder.Item.ContentType.Name;
                    }
                    else if (defaultValue == "@ModifiedBy@")
                    {
                        //Editor获取的是Display Name，不需要考虑其它语言，ModifiedBy获取的Logon Name，需要考虑其它语言支持.
                        //UserName获取的为："i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，需要用NoPrefixLoginName获取正确的username
                        try
                        {
                            if (aveFolder.SPFolder.Item != null)
                            {
                                string itemUserInfo = aveFolder.SPFolder.Item["Editor"].ToString();
                                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                IAveUser user = aveFolder.SPFolder.Item.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                                if (user != null)
                                {
                                    value = user.NoPrefixLoginName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            value = ERROR;
                            mLog.Warn("Can not get ModifiedBy,Info: {0}.", ex.ToString());
                        }
                    }
                    else
                    {
                        value = defaultValue;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get Default value from SharePoint with SharePoint API.Info: {0}.", e.ToString());
                    value = defaultValue;
                }
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }


        internal static string GetPropertyValue(bool SharePointMetadataAsSource, string defaultValue, string columnName, AveSPDoc aveDoc)
        {
            string value = string.Empty;
            if (SharePointMetadataAsSource)
            {
                value = GetValueFromSharepoint(columnName, aveDoc);
            }
            else
            {
                value = GetDefaultValue(defaultValue, aveDoc);
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetValueFromSharepoint(string columnName, AveSPDoc aveDoc)
        {
            string value = string.Empty;
            if (str.Contains(columnName))
            {
                try
                {
                    if (columnName == "@Title@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.Title;
                    }
                    else if (columnName == "@Name@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.Name;
                    }
                    else if (columnName == "@CreatedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");

                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID, (DateTime)aveDoc.AveSPItem.SPListItem["Created"]);
                    }
                    else if (columnName == "@TimeNow@")
                    {
                        value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else if (columnName == "@ModifiedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID, (DateTime)aveDoc.AveSPItem.SPListItem["Modified"]);
                    }
                    else if (columnName == "@ID@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.ID.ToString();
                    }
                    else if (columnName == "@UniqueId@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.UniqueId.ToString();
                    }
                    else if (columnName == "@ExtensionName@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.Name.Substring(aveDoc.AveSPItem.SPListItem.Name.LastIndexOf('.') + 1);
                    }
                    else if (columnName == "@FileEncodingText@")
                    {
                        string fileExtension = aveDoc.AveSPItem.SPListItem.Name.Substring(aveDoc.AveSPItem.SPListItem.Name.LastIndexOf('.') + 1);
                        value = GetFileEncodingTextByFileExtension(fileExtension);
                    }
                    else if (columnName == "@FileContent@")
                    {
                        using (Stream tempstream = aveDoc.AveSPItem.GetContent())
                        {
                            value = string.Format("{0}{1}{0}", "\n", VaultCover.ConverStreamToString(tempstream));
                        }
                    }
                    else if (columnName == "@CreatedBy@")
                    {
                        //Login Name:"i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，由于local和365目前创建user不可以添加"|"特殊字符，因此通过此字符进行截取.
                        int indexUser = (aveDoc.AveSPItem.Author.Login).LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                        if (indexUser > 0)
                        {
                            value = (aveDoc.AveSPItem.Author.Login).Substring(indexUser + 1);
                        }
                        else
                        {
                            value = (aveDoc.AveSPItem.Author.Login).ToString();
                        }
                    }
                    else if (columnName == "@ContentType@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.ContentType.Name;
                    }
                    else if (columnName == "@ModifiedBy@")
                    {
                        //Editor获取的是Display Name，不需要考虑其它语言，ModifiedBy获取的Logon Name，需要考虑其它语言支持.
                        //UserName获取的为："i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，需要用NoPrefixLoginName获取正确的username
                        try
                        {
                            if (aveDoc.AveSPItem.SPListItem != null)
                            {
                                string itemUserInfo = aveDoc.AveSPItem.SPListItem["Editor"].ToString();
                                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                IAveUser user = aveDoc.AveSPItem.SPListItem.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                                if (user != null)
                                {
                                    value = user.NoPrefixLoginName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            value = ERROR;
                            mLog.Warn("Can not get ModifiedBy,Info: {0}.", ex.ToString());
                        }
                    }
                    else
                    {
                        value = ERROR;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get column value from SharePoint with SharePoint API in item level.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            else
            {
                try
                {
                    var field = aveDoc.AveSPItem.SPListItem.Fields.GetField(columnName);
                    var internalName = field.InternalName;
                    Dictionary<string, object> columns = aveDoc.AveSPItem.GetAllColumnValues(AvePoint.Wrapper.Backup.ColumnsLevel.AllColumns); //aveDoc.AveSPItem.SPListItem.FieldValues;
                    if (columns.ContainsKey(internalName))
                    {
                        object tempValue = columns[internalName];
                        switch (field.Type)
                        {
                            case AveFieldType.DateTime:
                                if (tempValue != null)
                                {
                                    DateTime temp = (DateTime)tempValue;
                                    if (temp.Kind == DateTimeKind.Utc)
                                    {
                                        //value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.UTCToLocalTime(temp)).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                        temp = temp + cstZone.GetUtcOffset(temp);
                                        value = (temp).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                                    }
                                    else
                                    {
                                        value = temp.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                                    }
                                }
                                else
                                {
                                    value = string.Empty;
                                }
                                break;
                            case AveFieldType.Invalid:
                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                {
                                    StringBuilder sb = new StringBuilder();
                                    string[] taxValues = tempValue.ToString().Split(';');
                                    foreach (var taxValue in taxValues)
                                    {
                                        sb.Append(taxValue.Split('|')[0] + ";");
                                    }
                                    value = sb.ToString().TrimEnd(';');
                                }
                                break;
                            case AveFieldType.User:
                            case AveFieldType.Lookup:
                                if (field.Type == AveFieldType.User)
                                {
                                    tempValue = field.GetFieldValueAsText(aveDoc.AveSPItem.SPListItem[field.ID]);
                                }
                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                {
                                    StringBuilder sb = new StringBuilder();
                                    string[] taxValues = tempValue.ToString().Split('#');
                                    bool needAdd = false;
                                    foreach (var taxValue in taxValues)
                                    {
                                        if (needAdd)
                                        {
                                            sb.Append(taxValue);
                                            needAdd = false;
                                        }
                                        else
                                        {
                                            needAdd = true;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(sb.ToString()))
                                    {
                                        value = sb.ToString().TrimEnd(';');
                                    }
                                    else
                                    {
                                        value = tempValue.ToString();
                                    }

                                }

                                break;
                            default:
                                #region special column.
                                if (internalName.Equals("RecordsRelated", StringComparison.OrdinalIgnoreCase))
                                {
                                    mLog.Info("Current column is RecordsRelated and get display name in VEO Export.");
                                    string recordsRelatedValue = tempValue.ToString();
                                    if (!string.IsNullOrEmpty(recordsRelatedValue))
                                    {
                                        try
                                        {
                                            var sourceUrlValue = recordsRelatedValue;
                                            XmlDocument xmlDoc = new XmlDocument();
                                            sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
                                            xmlDoc.LoadXml(sourceUrlValue);
                                            foreach (XmlNode ele in xmlDoc.GetElementsByTagName("a"))
                                            {
                                                value += HttpUtility.UrlDecode(ele.InnerText) + ";";
                                            }
                                            value = value.TrimEnd(';');
                                        }
                                        catch (Exception ex)
                                        {
                                            value = tempValue.ToString();
                                            mLog.Info("Can not get RecordsRelated,Message:{0}.", ex.ToString());
                                        }
                                    }
                                    else
                                    {
                                        mLog.Info("Current column is RecordsRelated and column value is null in VEO Export.");
                                    }
                                }
                                else if (internalName.Equals("ImageSize") && columns.ContainsKey("ImageWidth") && columns.ContainsKey("ImageHeight"))
                                {
                                    value = columns["ImageWidth"].ToString() + " x " + columns["ImageHeight"].ToString();
                                }
                                else if (internalName.Equals("_dlc_DocIdUrl") && columns.ContainsKey("_dlc_DocId"))
                                {
                                    value = columns["_dlc_DocId"].ToString();
                                }
                                #endregion
                                else
                                {
                                    value = tempValue.ToString();
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (internalName.Equals("ImageSize") && columns.ContainsKey("ImageWidth") && columns.ContainsKey("ImageHeight"))
                        {
                            value = columns["ImageWidth"].ToString() + " x " + columns["ImageHeight"].ToString();
                        }
                        else if (internalName.Equals("FileLeafRef"))
                        {
                            value = aveDoc.AveSPItem.SPListItem.Name;
                        }
                        else if (internalName.Equals("ContentType") || internalName.Equals("Content Type"))
                        {
                            value = aveDoc.AveSPItem.SPListItem.ContentType.Name;
                        }
                        else
                        {
                            value = ERROR;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Can not get column value from SharePoint in item level by item column.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            return value;
        }

        private static string GetFileEncodingTextByFileExtension(string fileExtension)
        {
            string fileEncodingText = string.Empty;
            switch (fileExtension.ToLower())
            {
                case "txt":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_PlainTextEncodedAsBase64;
                    break;
                case "html":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_HTML_Hypertext_Markup_Language;
                    break;
                case "css":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_CSS_Cascading_Style_Sheets;
                    break;
                case "xml":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_XML_Extensible_Markup_Language;
                    break;
                case "pdf":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_PDFOrPDFA;
                    break;
                case "doc":
                case "docx":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_DOCOrDOCX;
                    break;
                case "xls":
                case "xlsx":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_XLSOrXLSX;
                    break;
                case "ppt":
                case "pptx":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_PPTOrPPTX;
                    break;
                case "tiff":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_TIFF;
                    break;
                case "jpeg":
                case "jfif":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_JPEGOrJFIF;
                    break;
                case "jp2":
                case "jpx":
                case "j2k":
                case "j2c":
                case "jpf":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_JPEG_2000;
                    break;
                case "mp4":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_MPEG4_Video_MP_File_Format;
                    break;
                case "avc":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_MPEG4_Video_AVC_File_Format;
                    break;
                case "warc":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_WARC_Web_Archive;
                    break;
                case "csv":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_CSVEncodedAsBase64;
                    break;
                case "mp3":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_MP3_MPEG1_And_MPEG2_Audio_Layer_III;
                    break;
                case "m4a":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_MP4Audio_MPEG4_Audio;
                    break;
                case "wav":
                case "lpcm":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_WAVOrLPCM;
                    break;
                case "mime":
                    fileEncodingText = VEOCommonString.M128_File_Encoding_MIME_Email_Encoded_In_Base64;
                    break;
                default:
                    fileEncodingText = fileExtension;
                    break;
            }
            return fileEncodingText;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetDefaultValue(string defaultValue, AveSPDoc aveDoc)
        {
            string value = string.Empty;
            if (str.Contains(defaultValue))
            {
                try
                {
                    if (defaultValue == "@Title@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.Title;
                    }
                    else if (defaultValue == "@Name@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.Name;
                    }
                    else if (defaultValue == "@CreatedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID, (DateTime)aveDoc.AveSPItem.SPListItem["Created"]);
                    }
                    else if (defaultValue == "@TimeNow@")
                    {
                        value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    }
                    else if (defaultValue == "@ModifiedTime@")
                    {
                        #region old logic
                        //if (((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).Kind != DateTimeKind.Utc)
                        //{
                        //    value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        //else
                        //{
                        //    value = ((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        //}
                        #endregion

                        value = GetTimeStr(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID, (DateTime)aveDoc.AveSPItem.SPListItem["Modified"]);
                    }
                    else if (defaultValue == "@ID@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.ID.ToString();
                    }
                    else if (defaultValue == "@UniqueId@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.UniqueId.ToString();
                    }
                    else if (defaultValue == "@ExtensionName@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.Name.Substring(aveDoc.AveSPItem.SPListItem.Name.LastIndexOf('.') + 1);
                    }
                    else if (defaultValue == "@FileEncodingText@")
                    {
                        string fileExtension = aveDoc.AveSPItem.SPListItem.Name.Substring(aveDoc.AveSPItem.SPListItem.Name.LastIndexOf('.') + 1);
                        value = GetFileEncodingTextByFileExtension(fileExtension);
                    }
                    else if (defaultValue == "@FileContent@")
                    {
                        using (Stream tempstream = aveDoc.AveSPItem.GetContent())
                        {
                            value = string.Format("{0}{1}{0}", "\n", VaultCover.ConverStreamToString(tempstream));
                        }
                    }
                    else if (defaultValue == "@CreatedBy@")
                    {
                        //Login Name:"i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，由于local和365目前创建user不可以添加"|"特殊字符，因此通过此字符进行截取.
                        int indexUser = (aveDoc.AveSPItem.Author.Login).LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                        if (indexUser > 0)
                        {
                            value = (aveDoc.AveSPItem.Author.Login).Substring(indexUser + 1);
                        }
                        else
                        {
                            value = (aveDoc.AveSPItem.Author.Login).ToString();
                        }
                    }
                    else if (defaultValue == "@ContentType@")
                    {
                        value = aveDoc.AveSPItem.SPListItem.ContentType.Name;
                    }
                    else if (defaultValue == "@ModifiedBy@")
                    {
                        //Editor获取的是Display Name，不需要考虑其它语言，ModifiedBy获取的Logon Name，需要考虑其它语言支持.
                        //UserName获取的为："i:0#.f|membership|xdx@xdx.partner.onmschina.cn"，需要用NoPrefixLoginName获取正确的username
                        try
                        {
                            if (aveDoc.AveSPItem.SPListItem != null)
                            {
                                string itemUserInfo = aveDoc.AveSPItem.SPListItem["Editor"].ToString();
                                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                IAveUser user = aveDoc.AveSPItem.SPListItem.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                                if (user != null)
                                {
                                    value = user.NoPrefixLoginName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            value = ERROR;
                            mLog.Warn("Can not get ModifiedBy,Info: {0}.", ex.ToString());
                        }
                    }
                    else
                    {
                        value = defaultValue;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get Default value from SharePoint with SharePoint API.Info: {0}.", e.ToString());
                    value = defaultValue;
                }
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        internal static string GetPropertyValue(string defaultValue, string columnName, XmlDocument xml)
        {
            string value = string.Empty;
            if (!string.IsNullOrEmpty(columnName))
            {
                value = GetValueFromVEOXML(columnName, xml);
            }
            else
            {
                if (defaultValue == "@TimeNow@")
                {
                    value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                }
                else
                {
                    value = defaultValue;
                }
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetValueFromVEOXML(string columnName, XmlDocument xml)
        {
            string value = string.Empty;
            try
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("vers", "http://www.prov.vic.gov.au/gservice/standard/pros99007.htm");
                nsmgr.AddNamespace("naa", "http://www.naa.gov.au/recordkeeping/control/rkms/contents.html");
                //XmlNode node = xml.SelectSingleNode(columnName, nsmgr);
                XmlNodeList node1 = xml.GetElementsByTagName(columnName);
                if (node1.Count != 0)
                {
                    value = node1[0].InnerText;
                }
                else
                {
                    value = ERROR;
                }
            }
            catch (Exception e)
            {
                mLog.Info("Can not get value from VEO XML.Info: {0}.", e.ToString());
                value = ERROR;
            }
            return value;
        }

        /// <summary>
        /// 为以后扩展语法糖，暂时返回Default Value
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static string GetPropertyValue(string defaultValue)
        {
            string value = string.Empty;
            value = defaultValue;
            return value;
        }

        private static string GetWebappUrl(AveSPSite aveSite)
        {
            Uri webAppUri = new Uri(aveSite.SPSite.Url);
            //if (aveSite.SPSite.SPMode == AvePoint.Wrapper.Core.Common.WrapperSPMode.O365)
            //{
            string webAppUrl;
            string siteUrl = aveSite.SPSite.Url;
            int lengh = 0;
            if (siteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                lengh = "https://".Length;
            }
            else
            {
                //Server Farm Regist as O365
                lengh = "http://".Length;
            }
            int indexOfSlash = siteUrl.IndexOf("/", lengh, StringComparison.OrdinalIgnoreCase);
            webAppUrl = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppUrl = siteUrl.Substring(0, indexOfSlash);
            }
            webAppUri = new Uri(webAppUrl);
            //}
            //else
            //{
            //    webAppUri = aveSite.SPSite.WebApplication.GetResponseUri(AveUrlZone.Default);
            //}
            return webAppUri.AbsoluteUri.Trim('/');
        }

        private static string GetTimeStr(UInt16 timeZoneId, DateTime dateTime)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(timeZoneId));
            var temp = dateTime;
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                temp += timeZone.GetUtcOffset(temp);
            }
            var result = new DateTimeOffset(temp.Ticks, timeZone.BaseUtcOffset).ToString("yyyy-MM-ddTHH:mm:sszzz");
            mLog.Info($"datetime: {temp}, timeZone: {timeZone}, timeStr: {result}");
            return result;
        }
    }
}
