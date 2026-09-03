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
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;

namespace RAExportCommon
{
    class VEOV3CustomizeProperty
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType);

        private const string ERROR = null;

        private static readonly List<string> str =
        [
            "@Title@",
            "@Name@",
            "@CreatedTime@",
            "@TimeNow@",
            "@ModifiedTime@",
            "@ID@",
            "@ExtensionName@",
            "@FileContent@",
            "@CreatedBy@",
            "@ContentType@",
            "@ModifiedBy@",
            "@UniqueId@",
            "@Url@",
            "@Type@",
            "@Version@",
            "@Size@",
            "@NewGuid@"
        ];

        internal static string? GetPropertyValueByLevel(bool metadataAsSource, string defaultValue, string metadata, RecordVEOParametersV3? vParams)
        {
            string? value = null;
            if (vParams == null) return value;
            try
            {
                switch (vParams.Level)
                {
                    case AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection:
                        if (vParams.AveSPSite != null) value = GetPropertyValue(metadataAsSource, defaultValue, metadata, vParams.AveSPSite);
                        break;
                    case AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Site:
                        if (vParams.AveSPWeb != null) value = GetPropertyValue(metadataAsSource, defaultValue, metadata, vParams.AveSPWeb);
                        break;
                    case AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.List:
                        if (vParams.AveSPList != null) value = GetPropertyValue(metadataAsSource, defaultValue, metadata, vParams.AveSPList);
                        break;
                    case AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Folder:
                        if (vParams.AveSPFolder != null) value = GetPropertyValue(metadataAsSource, defaultValue, metadata, vParams.AveSPFolder);
                        break;
                    case AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document:
                        if (vParams.AveSPDoc != null) value = GetPropertyValue(metadataAsSource, defaultValue, metadata, vParams.AveSPDoc);
                        break;
                    default:
                        mLog.Warn($"vParam init as {vParams.Level.ToString()} but does not has value. MetadataAsSource: {metadataAsSource}, defaultValue: {defaultValue}, metadata: {metadata} ");
                        break;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred while getting property value for {vParams.Level.ToString()}. MetadataAsSource: {metadataAsSource}, defaultValue: {defaultValue}, metadata: {metadata}, ex: {e} ");
                return null;
            }
            if (value.IsNullOrWhiteSpace())
            {
                return null;
            }
            return value;
        }

        #region Site
        internal static string GetPropertyValue(bool SharePointMetadataAsSource, string defaultValue, string columnName, AveSPSite aveSite)
        {
            string value = string.Empty;
            if (SharePointMetadataAsSource)
            {
                value = GetValueFromSharepoint(columnName, aveSite);
            }
            else
            {
                value = GetDefaultValue(defaultValue, aveSite);
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetValueFromSharepoint(string columnName, AveSPSite aveSite)
        {
            string value = string.Empty;
            if (str.Contains(columnName))
            {
                try
                {
                    switch (columnName)
                    {
                        case "@Title@":
                            value = aveSite.SPSite.RootWeb.Title;
                            break;
                        case "@Name@":
                            value = aveSite.SPSite.RootWeb.Title;
                            break;
                        case "@CreatedTime@":
                            if (aveSite.SPSite.RootWeb.Created.Kind != DateTimeKind.Utc)
                            {
                                value = (aveSite.SPSite.RootWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveSite.SPSite.RootWeb.Created)).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveSite.SPSite.RootWeb.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                            if (aveSite.SPSite.LastContentModifiedDate.Kind != DateTimeKind.Utc)
                            {
                                value = (aveSite.SPSite.RootWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveSite.SPSite.LastContentModifiedDate)).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveSite.SPSite.LastContentModifiedDate.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }

                            break;
                        case "@ID@":
                            value = aveSite.SPSite.ID.ToString();
                            break;
                        case "@UniqueId@":
                            value = aveSite.SPSite.ID.ToString();
                            break;
                        case "@ExtensionName@":
                            value = ERROR;
                            break;
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = aveSite.SPSite.Url;
                            break;
                        case "@CreatedBy@":
                            {
                                if (aveSite.SPSite.RootWeb.Author == null)
                                {
                                    value = ERROR;
                                }
                                else
                                {
                                    int index = aveSite.SPSite.RootWeb.Author.LoginName.LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                                    if (index > 0)
                                    {
                                        value = aveSite.SPSite.RootWeb.Author.LoginName.Substring(index + 1);
                                    }
                                    else
                                    {
                                        value = aveSite.SPSite.RootWeb.Author.LoginName.ToString();
                                    }

                                }

                                break;
                            }

                        case "@ContentType@":
                        case "@ModifiedBy@":
                        default:
                            //进入这里必须满足两种条件：1.语法糖集合中有。2.语法糖中没有对此特殊语法进行处理，可能性低。
                            value = ERROR;
                            break;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get column value from SharePoint with grammar in site level.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            else if (aveSite.SPSite.RootWeb.Properties.ContainsKey(columnName))
            {
                try
                {
                    object tempValue = aveSite.SPSite.RootWeb.Properties[columnName];
                    if (tempValue is DateTime)
                    {
                        DateTime temp = (DateTime)tempValue;
                        value = temp.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                    }
                    else
                    {
                        if (tempValue == null)
                        {
                            mLog.Info("Get site property value is null,column name: {0}.", columnName);
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
                    mLog.Info("Can not get column value in site level by site property,Info: {0}.", ex.ToString());
                    value = ERROR;
                }
            }
            else
            {
                mLog.Info("Site level not support get Metadata from site column.");
                value = ERROR;
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetDefaultValue(string defaultValue, AveSPSite aveSite)
        {
            string value = string.Empty;
            if (str.Contains(defaultValue))
            {
                try
                {
                    switch (defaultValue)
                    {
                        case "@Title@":
                            value = aveSite.SPSite.RootWeb.Title;
                            break;
                        case "@Name@":
                            value = aveSite.SPSite.RootWeb.Name;
                            break;
                        case "@CreatedTime@":
                            if (aveSite.SPSite.RootWeb.Created.Kind != DateTimeKind.Utc)
                            {
                                value = (aveSite.SPSite.RootWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveSite.SPSite.RootWeb.Created)).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveSite.SPSite.RootWeb.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                            if (aveSite.SPSite.LastContentModifiedDate.Kind != DateTimeKind.Utc)
                            {
                                value = aveSite.SPSite.RootWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveSite.SPSite.LastContentModifiedDate).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveSite.SPSite.LastContentModifiedDate.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }

                            break;
                        case "@ID@":
                            value = aveSite.SPSite.ID.ToString();
                            break;
                        case "@ExtensionName@":
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = aveSite.SPSite.Url;
                            break;
                        case "@CreatedBy@":
                            {
                                //office 365 不支持此API，author为null
                                if (aveSite.SPSite.RootWeb.Author == null)
                                {
                                    value = ERROR;
                                }
                                else
                                {
                                    int index = aveSite.SPSite.RootWeb.Author.LoginName.LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                                    if (index > 0)
                                    {
                                        value = aveSite.SPSite.RootWeb.Author.LoginName.Substring(index + 1);
                                    }
                                    else
                                    {
                                        value = aveSite.SPSite.RootWeb.Author.LoginName.ToString();
                                    }
                                }

                                break;
                            }

                        case "@ContentType@":
                        case "@ModifiedBy@":
                            value = ERROR;
                            break;
                        default:
                            value = defaultValue;
                            break;
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
        #endregion

        #region Web
        internal static string GetPropertyValue(bool SharePointMetadataAsSource, string defaultValue, string columnName, AveSPWeb aveWeb)
        {
            string value = string.Empty;
            if (SharePointMetadataAsSource)
            {
                value = GetValueFromSharepoint(columnName, aveWeb);
            }
            else
            {
                value = GetDefaultValue(defaultValue, aveWeb);
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetValueFromSharepoint(string columnName, AveSPWeb aveWeb)
        {
            string value = string.Empty;
            if (str.Contains(columnName))
            {
                try
                {
                    switch (columnName)
                    {
                        case "@Title@":
                            value = aveWeb.SPWeb.Title;
                            break;
                        case "@Name@":
                            value = aveWeb.SPWeb.Name;
                            break;
                        case "@CreatedTime@":
                            if (aveWeb.SPWeb.Created.Kind != DateTimeKind.Utc)
                            {
                                value = aveWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveWeb.SPWeb.Created).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveWeb.SPWeb.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                            if (aveWeb.SPWeb.LastItemModifiedDate.Kind != DateTimeKind.Utc)
                            {
                                value = aveWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveWeb.SPWeb.LastItemModifiedDate).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveWeb.SPWeb.LastItemModifiedDate.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }

                            break;
                        case "@ID@":
                            value = aveWeb.SPWeb.ID.ToString();
                            break;
                        case "@UniqueId@":
                            value = aveWeb.SPWeb.ID.ToString();
                            break;
                        case "@ExtensionName@":
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = aveWeb.SPWeb.Url;
                            break;
                        case "@CreatedBy@":
                            {
                                if (aveWeb.SPWeb.Author == null)
                                {
                                    value = ERROR;
                                }
                                else
                                {
                                    int index = aveWeb.SPWeb.Author.LoginName.LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                                    if (index > 0)
                                    {
                                        value = aveWeb.SPWeb.Author.LoginName.Substring(index + 1);
                                    }
                                    else
                                    {
                                        value = aveWeb.SPWeb.Author.LoginName.ToString();
                                    }

                                }

                                break;
                            }

                        case "@ContentType@":
                        case "@ModifiedBy@":
                        default:
                            //进入这里必须满足两种条件：1.语法糖集合中有。2.语法糖中没有对此特殊语法进行处理，可能性低。
                            value = ERROR;
                            break;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get column value from SharePoint with grammar in list level.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            else if (aveWeb.SPWeb.Properties.ContainsKey(columnName))
            {
                try
                {
                    object tempValue = aveWeb.SPWeb.Properties[columnName];
                    if (tempValue is DateTime)
                    {
                        DateTime temp = (DateTime)tempValue;
                        value = temp.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                    }
                    else
                    {
                        if (tempValue == null)
                        {
                            mLog.Info("Get web property value is null,column name: {0}.", columnName);
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
                    mLog.Info("Can not get column value in web level by web property,Info: {0}.", ex.ToString());
                    value = ERROR;
                }
            }
            else
            {
                mLog.Info("Web level not support get Metadata from web column.");
                value = ERROR;
            }
            return value;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetDefaultValue(string defaultValue, AveSPWeb aveWeb)
        {
            string value = string.Empty;
            if (str.Contains(defaultValue))
            {
                try
                {
                    switch (defaultValue)
                    {
                        case "@Title@":
                            value = aveWeb.SPWeb.Title;
                            break;
                        case "@Name@":
                            value = aveWeb.SPWeb.Name;
                            break;
                        case "@CreatedTime@":
                            if (aveWeb.SPWeb.Created.Kind != DateTimeKind.Utc)
                            {
                                value = aveWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveWeb.SPWeb.Created).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveWeb.SPWeb.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                            if (aveWeb.SPWeb.LastItemModifiedDate.Kind != DateTimeKind.Utc)
                            {
                                value = aveWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveWeb.SPWeb.LastItemModifiedDate).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveWeb.SPWeb.LastItemModifiedDate.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }

                            break;
                        case "@ID@":
                            value = aveWeb.SPWeb.ID.ToString();
                            break;
                        case "@ExtensionName@":
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = aveWeb.SPWeb.Url;
                            break;
                        case "@CreatedBy@":
                            {
                                //office 365 不支持此API，author为null
                                if (aveWeb.SPWeb.Author == null)
                                {
                                    value = ERROR;
                                }
                                else
                                {
                                    int index = aveWeb.SPWeb.Author.LoginName.LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                                    if (index > 0)
                                    {
                                        value = aveWeb.SPWeb.Author.LoginName.Substring(index + 1);
                                    }
                                    else
                                    {
                                        value = aveWeb.SPWeb.Author.LoginName.ToString();
                                    }
                                }

                                break;
                            }

                        case "@ContentType@":
                        case "@ModifiedBy@":
                            value = ERROR;
                            break;
                        default:
                            value = defaultValue;
                            break;
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
        #endregion

        #region List
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
                    switch (columnName)
                    {
                        case "@Title@":
                            value = aveList.SPList.Title;
                            break;
                        case "@Name@":
                            value = aveList.SPList.RootFolder.Name;
                            break;
                        case "@CreatedTime@":
                            if (aveList.SPList.Created.Kind != DateTimeKind.Utc)
                            {
                                value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.Created)).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveList.SPList.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                            if (aveList.SPList.LastItemModifiedDate.Kind != DateTimeKind.Utc)
                            {
                                value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.LastItemModifiedDate)).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }
                            else
                            {
                                value = aveList.SPList.LastItemModifiedDate.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                            }

                            break;
                        case "@ID@":
                            value = aveList.SPList.ID.ToString();
                            break;
                        case "@ExtensionName@":
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = GetWebappUrl(aveList.ParentSite) + aveList.ServerRelativeUrl;
                            break;
                        case "@UniqueId@":
                            value = aveList.SPList.ID.ToString();
                            break;
                        case "@CreatedBy@":
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

                                break;
                            }

                        case "@ContentType@":
                        case "@ModifiedBy@":
                        default:
                            //进入这里必须满足两种条件：1.语法糖集合中有。2.语法糖中没有对此特殊语法进行处理，可能性低。
                            value = ERROR;
                            break;
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
                    switch (defaultValue)
                    {
                        case "@Title@":
                            value = aveList.SPList.Title;
                            break;
                        case "@Name@":
                            value = aveList.SPList.RootFolder.Name;
                            break;
                        case "@CreatedTime@":
                            if (aveList.SPList.Created.Kind != DateTimeKind.Utc)
                            {
                                value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.Created)).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = aveList.SPList.Created.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            //目前WrapperAPI Check rule ModifiedTime 用的是LastItemModifiedDate，此处暂时与其保持一致。ADO-168950
                            if (aveList.SPList.LastItemModifiedDate.Kind != DateTimeKind.Utc)
                            {
                                value = (aveList.SPList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC(aveList.SPList.LastItemModifiedDate)).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = aveList.SPList.LastItemModifiedDate.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }

                            break;
                        case "@ID@":
                            value = aveList.SPList.ID.ToString();
                            break;
                        case "@ExtensionName@":
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = GetWebappUrl(aveList.ParentSite) + aveList.ServerRelativeUrl;
                            break;
                        case "@UniqueId@":
                            value = aveList.SPList.ID.ToString();
                            break;
                        case "@CreatedBy@":
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

                                break;
                            }

                        case "@ContentType@":
                        case "@ModifiedBy@":
                            value = ERROR;
                            break;
                        default:
                            value = defaultValue;
                            break;
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
        #endregion

        #region Folder
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
                    switch (columnName)
                    {
                        case "@Title@":
                            value = aveFolder.SPFolder.Name;
                            break;
                        case "@Name@":
                            value = aveFolder.SPFolder.Name;
                            break;
                        case "@CreatedTime@":
                            if (((DateTime)aveFolder.SPFolder.Item["Created"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = ((DateTime)aveFolder.SPFolder.Item["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            if (((DateTime)aveFolder.SPFolder.Item["Modified"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = ((DateTime)aveFolder.SPFolder.Item["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@ID@":
                            value = aveFolder.AveItem.RowId.ToString();
                            break;
                        case "@ExtensionName@":
                            value = ERROR;
                            break;
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = GetWebappUrl(aveFolder.ParentSite) + aveFolder.ServerRelativeUrl;
                            break;
                        case "@UniqueId@":
                            value = aveFolder.SPFolder.UniqueId.ToString();
                            break;
                        case "@CreatedBy@":
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

                                break;
                            }

                        case "@ContentType@":
                            value = aveFolder.SPFolder.Item.ContentType.Name;
                            break;
                        case "@ModifiedBy@":
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

                                break;
                            }

                        default:
                            //进入这里必须满足两种条件：1.语法糖集合中有。2.语法糖中没有对此特殊语法进行处理，可能性低。
                            value = ERROR;
                            break;
                    }
                }
                catch (Exception e)
                {
                    mLog.Info("Can not get column value from SharePoint with SharePoint API in folder level.Info: {0}.", e.ToString());
                    value = ERROR;
                }
            }
            else
            {
                try
                {
                    var field = aveFolder.AveItem.SPListItem.Fields.GetField(columnName);
                    var internalName = field.InternalName;
                    Dictionary<string, object> columns = aveFolder.AveItem.GetAllColumnValues(AvePoint.Wrapper.Backup.ColumnsLevel.AllColumns); //aveDoc.AveSPItem.SPListItem.FieldValues;
                    if (columns.TryGetValue(internalName, out object? tempValue))
                    {
                        switch (field.Type)
                        {
                            case AveFieldType.DateTime:
                                if (tempValue != null)
                                {
                                    DateTime temp = (DateTime)tempValue;
                                    if (temp.Kind == DateTimeKind.Utc)
                                    {
                                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.ID));
                                        temp = temp + cstZone.GetUtcOffset(temp);
                                        value = (temp).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                                    }
                                    else
                                    {
                                        value = temp.ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
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
                                    tempValue = field.GetFieldValueAsText(aveFolder.AveItem.SPListItem[field.ID]);
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
                            case AveFieldType.Currency:
                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                {
                                    var currencyField = (IAveFieldCurrency)field;
                                    var locate = currencyField.CurrencyLocaleId;
                                    value = string.Format(CultureInfo.GetCultureInfo((int)locate), "{0:C}", tempValue);
                                }
                                break;
                            case AveFieldType.Boolean:
                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                {
                                    if (Boolean.TryParse(tempValue.ToString(), out var boolVal) && boolVal)
                                    {
                                        value = "Yes";
                                    }
                                    else if (!boolVal)
                                    {
                                        value = "No";
                                    }
                                    else
                                    {
                                        value = tempValue.ToString();
                                    }
                                }
                                break;
                            default:
                                value = tempValue.ToString();
                                break;
                        }
                    }
                    else
                    {
                        value = ERROR;
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetDefaultValue(string defaultValue, AveSPFolder aveFolder)
        {
            string value = string.Empty;
            if (str.Contains(defaultValue))
            {
                try
                {
                    switch (defaultValue)
                    {
                        case "@Title@":
                            //SP API获取不到Folder级别Title，只能通过Folder Name获取.
                            //通过aveFolder.AveItem.Title & aveFolder.AveItem.Name 获取，10，13环境Title能获取到，Name为空，16&365环境，这俩属性都为空.
                            value = aveFolder.SPFolder.Name;
                            break;
                        case "@Name@":
                            value = aveFolder.SPFolder.Name;
                            break;
                        case "@CreatedTime@":
                            if (((DateTime)aveFolder.SPFolder.Item["Created"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = ((DateTime)aveFolder.SPFolder.Item["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            if (((DateTime)aveFolder.SPFolder.Item["Modified"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = ((DateTime)aveFolder.SPFolder.Item["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@ID@":
                            value = aveFolder.AveItem.RowId.ToString();
                            break;
                        case "@ExtensionName@":
                            value = ERROR;
                            break;
                        case "@FileContent@":
                            value = ERROR;
                            break;
                        case "@Url@":
                            value = GetWebappUrl(aveFolder.ParentSite) + aveFolder.ServerRelativeUrl;
                            break;
                        case "@UniqueId@":
                            value = aveFolder.SPFolder.UniqueId.ToString();
                            break;
                        case "@CreatedBy@":
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

                                break;
                            }

                        case "@ContentType@":
                            value = aveFolder.SPFolder.Item.ContentType.Name;
                            break;
                        case "@ModifiedBy@":
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

                                break;
                            }

                        default:
                            value = defaultValue;
                            break;
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
        #endregion

        #region Document
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
                    switch (columnName)
                    {
                        case "@Title@":
                            value = aveDoc.AveSPItem.SPListItem.Name;
                            break;
                        case "@Name@":
                            value = aveDoc.AveSPItem.SPListItem.Name;
                            break;
                        case "@CreatedTime@":
                            if (((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");

                            }
                            else
                            {
                                value = ((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            if (((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = ((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@ID@":
                            value = aveDoc.AveSPItem.SPListItem.ID.ToString();
                            break;
                        case "@UniqueId@":
                            value = aveDoc.AveSPItem.SPListItem.UniqueId.ToString();
                            break;
                        case "@ExtensionName@":
                            value = aveDoc.AveSPItem.SPListItem.Name.Substring(aveDoc.AveSPItem.SPListItem.Name.LastIndexOf('.') + 1);
                            break;
                        case "@Url@":
                            value = aveDoc.Url + aveDoc.AveSPItem.SPListItem.Name;
                            break;
                        case "@Type@":
                            value = Path.GetExtension(aveDoc.AveSPItem.SPListItem.Name);
                            break;
                        case "@Version@":
                            value = NameFactory.ParseVersionString(aveDoc.AveSPItem.Version);
                            break;
                        case "@Size@":
                            try
                            {
                                value = aveDoc.AveSPItem.SPListItem["File Size"]?.ToString() ?? "0";
                            }
                            catch (Exception)
                            {
                                value = "0";
                            }
                            break;
                        case "@FileContent@":
                            {
                                using (Stream tempstream = aveDoc.AveSPItem.GetContent())
                                {
                                    value = string.Format("{0}{1}{0}", "\n", VaultCover.ConverStreamToString(tempstream));
                                }

                                break;
                            }

                        case "@CreatedBy@":
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

                                break;
                            }

                        case "@ContentType@":
                            value = aveDoc.AveSPItem.SPListItem.ContentType.Name;
                            break;
                        case "@ModifiedBy@":
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

                                break;
                            }

                        default:
                            value = ERROR;
                            break;
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
                    if (columns.TryGetValue(internalName, out object? tempValue))
                    {
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
                            case AveFieldType.Currency:
                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                {
                                    var currencyField = (IAveFieldCurrency)field;
                                    var locate = currencyField.CurrencyLocaleId;
                                    value = string.Format(CultureInfo.GetCultureInfo((int)locate), "{0:C}", tempValue);
                                }
                                break;
                            case AveFieldType.Boolean:
                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                {
                                    if (Boolean.TryParse(tempValue.ToString(), out var boolVal) && boolVal)
                                    {
                                        value = "Yes";
                                    }
                                    else if (!boolVal)
                                    {
                                        value = "No";
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetDefaultValue(string defaultValue, AveSPDoc aveDoc)
        {
            string value = string.Empty;
            if (str.Contains(defaultValue))
            {
                try
                {
                    switch (defaultValue)
                    {
                        case "@Title@":
                            value = aveDoc.AveSPItem.SPListItem.Name;
                            break;
                        case "@Name@":
                            value = aveDoc.AveSPItem.SPListItem.Name;
                            break;
                        case "@CreatedTime@":
                            if (((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Created"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");

                            }
                            else
                            {
                                value = ((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@TimeNow@":
                            value = DateTime.Now.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            break;
                        case "@NewGuid@":
                            value = Guid.NewGuid().ToString();
                            break;
                        case "@ModifiedTime@":
                            if (((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).Kind != DateTimeKind.Utc)
                            {
                                value = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Modified"])).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            else
                            {
                                value = ((DateTime)aveDoc.AveSPItem.SPListItem["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                            }
                            break;
                        case "@ID@":
                            value = aveDoc.AveSPItem.SPListItem.ID.ToString();
                            break;
                        case "@UniqueId@":
                            value = aveDoc.AveSPItem.SPListItem.UniqueId.ToString();
                            break;
                        case "@ExtensionName@":
                            value = aveDoc.AveSPItem.SPListItem.Name.Substring(aveDoc.AveSPItem.SPListItem.Name.LastIndexOf('.') + 1);
                            break;
                        case "@Url@":
                            value = aveDoc.Url + aveDoc.AveSPItem.SPListItem.Name;
                            break;
                        case "@Type@":
                            value = Path.GetExtension(aveDoc.AveSPItem.SPListItem.Name);
                            break;
                        case "@Version@":
                            value = NameFactory.ParseVersionString(aveDoc.AveSPItem.Version);
                            break;
                        case "@Size@":
                            try
                            {
                                value = aveDoc.AveSPItem.SPListItem["File Size"]?.ToString() ?? "0";
                            }
                            catch (Exception)
                            {
                                value = "0";
                            }
                            break;
                        case "@FileContent@":
                            {
                                using (Stream tempstream = aveDoc.AveSPItem.GetContent())
                                {
                                    value = string.Format("{0}{1}{0}", "\n", VaultCover.ConverStreamToString(tempstream));
                                }

                                break;
                            }

                        case "@CreatedBy@":
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

                                break;
                            }

                        case "@ContentType@":
                            value = aveDoc.AveSPItem.SPListItem.ContentType.Name;
                            break;
                        case "@ModifiedBy@":
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

                                break;
                            }

                        default:
                            value = defaultValue;
                            break;
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
        #endregion

        private static string GetWebappUrl(AveSPSite aveSite)
        {
            Uri webAppUri = new Uri(aveSite.SPSite.Url);
            string webAppUrl;
            string siteUrl = aveSite.SPSite.Url;
            int lengh = 0;
            if (siteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                lengh = "https://".Length;
            }
            else
            {
                lengh = "http://".Length;
            }
            int indexOfSlash = siteUrl.IndexOf("/", lengh, StringComparison.OrdinalIgnoreCase);
            webAppUrl = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppUrl = siteUrl.Substring(0, indexOfSlash);
            }
            webAppUri = new Uri(webAppUrl);
            return webAppUri.AbsoluteUri.Trim('/');
        }
    }
}
