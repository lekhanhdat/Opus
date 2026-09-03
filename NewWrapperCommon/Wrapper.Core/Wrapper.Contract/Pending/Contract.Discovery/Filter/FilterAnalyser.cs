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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.Wrapper.Resource.Discovery;

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
                    try
                    {
                        //解密成功返回解密后结果，否则返回Value, 不能抛异常 
                        value = GAPolicyHelper.GetPolicyValue(value.ToString(), web.Site.ID, web.ID);
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred when get GA plus policy value, key: [{0}], value: {1}. Reason: {2}.", key, value, e);
                    }
                }
                siteCollectionColumn[key] = value;
            }
            return siteCollectionColumn;
        }

        private static void GetUserInfo(IAveListItem item, string columnName, ref string loginName, ref string loginNameWithPrefix, ref string title)
        {
            try
            {
                string itemUserInfo = item[columnName].ToString();
                string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                title = sArray[1].ToString();
                IAveUser user = item.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                if (user != null)
                {
                    loginName = user.NoPrefixLoginName;
                    loginNameWithPrefix = user.LoginName;
                    title = user.Name;
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
            string logonNameWithPrefix = string.Empty;
            string columnName = authorOrEditor ? "Author" : "Editor";
            GetUserInfo(item, columnName, ref logonName, ref logonNameWithPrefix, ref title);
            if (authorOrEditor)
            {
                result.CreatedByTitle = title;
                result.CreatedByLogonName = logonName;
                result.CreatedByLogonNameWithPrefix = logonNameWithPrefix;
            }
            else
            {
                result.ModifiedByTitle = title;
                result.ModifiedByLogonName = logonName;
                result.ModifiedByLogonNameWithPrefix = logonNameWithPrefix;
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
                    result.ModifiedByLogonName = user.NoPrefixLoginName;
                    result.ModifiedByTitle = user.Name;
                    result.ModifiedByLogonNameWithPrefix = user.LoginName;
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperDiscoverResource.AWDGetVisionEditorError, ex.ToString());
            }
        }

        private static void GetVersionEditorInfo(IAveFileVersion version, IAveFile file, CommonInfoBase result)
        {
            try
            {
                string login = version.Properties["vti_modifiedby"].ToString();
                result.ModifiedByLogonName = login;
                IAveUser user = file.ParentFolder.ParentList.ParentWeb.SiteUsers.GetByLoginName(login);
                if (user != null)
                {
                    result.ModifiedByLogonName = user.NoPrefixLoginName;
                    result.ModifiedByTitle = user.Name;
                    result.ModifiedByLogonNameWithPrefix = user.LoginName;
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

        //将从sharepoint取到的时间转换成UTC时间。
        private static DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = web.RegionalSettings.TimeZone.LocalTimeToUTC(datetime);
            }
            return datetime;
        }

        private static List<Hashtable> GetItemColumns(IAveListItem item)
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
                                else if(calculatedValue is DateTime)
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

        //Add for Get Term Path, Only for RA
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
                                    columnCollectionOfDisplayName[fieldTitle] = GetFieldTermIdValue(item[field.ID]);
                                }
                                break;
                            default:
                                break;
                        }
                       // columnCollectionOfDisplayName[fieldTitle] = AveStringHelper.Trim(columnCollectionOfDisplayName[fieldTitle]);
                    }
                    catch (Exception ex)
                    {
                        log.Debug(string.Format("Get the taxnomy metadata of item error.Field Name:{0}.Exception:{1}", field.Title, ex));
                    }
                }
            }
            return columnCollectionOfDisplayName;
        }

        /// <summary>
        /// 1.IAveListItem object this[Guid fieldId] Client & Server has different implementations, 
        ///   client return string object, server return IAveTaxonomyFieldValue/IAveTaxonomyFieldValueCollection object
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Test")]
        private static string GetFieldTermPathValue(IAveTaxonomyField taxonomyField, object value, IAveListItem item)
        {
            string path = string.Empty;
            if (item != null && item.ParentList.ParentWeb.Site.SPMode == Core.Common.WrapperSPMode.Server)
            {
                string stringValue = GetTermGuid(value);
                if (!string.IsNullOrEmpty(stringValue))
                {
                    string[] values = stringValue.Split(';');
                    StringBuilder builder = new StringBuilder();
                    foreach (string key in values)
                    {
                        try
                        {
                            IAveTaxonomySession session = item.ParentList.ParentWeb.Site.AveSPTaxonomySession;
                            int LCID = 0;
                            IAveTermStore termStore = AveTaxonomyFieldUtility.GetTermStore(taxonomyField, session, ref LCID);
                            if (termStore == null)
                            {
                                return string.Empty;
                            }
                            IAveTermSet termSet = null;
                            if (taxonomyField.TermSetId != Guid.Empty && termStore != null)
                            {
                                termSet = termStore.GetTermSet(taxonomyField.TermSetId);
                            }
                            IAveTerm endTerm = termStore.GetTerm(termSet.ID, new Guid(key));
                            path = endTerm.PathOfTerm;
                            builder.Append(path);
                        }
                        catch (Exception ex)
                        {
                            log.Warn("Error in get taxonomy field path.{0}", ex);
                        }
                        builder.Append(';');
                    }
                    return builder.ToString().TrimEnd(';');
                }
            }
            else
            {
                string stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    string[] values = stringValue.Split(';');
                    StringBuilder builder = new StringBuilder();
                    foreach (string key in values)
                    {
                        var index = key.IndexOf('|');
                        if (index == 0)
                        {
                            continue;
                        }
                        if (index < 0)
                        {
                            builder.Append(value);
                        }
                        else
                        {
                            try
                            {
                                IAveTaxonomySession session = item.ParentList.ParentWeb.Site.AveSPTaxonomySession;
                                int LCID = 0;
                                IAveTermStore termStore = AveTaxonomyFieldUtility.GetTermStore(taxonomyField, session, ref LCID);
                                if (termStore == null)
                                {
                                    return string.Empty;
                                }
                                IAveTermSet termSet = null;
                                if (taxonomyField.TermSetId != Guid.Empty && termStore != null)
                                {
                                    termSet = termStore.GetTermSet(taxonomyField.TermSetId);
                                }
                                IAveTerm endTerm = termStore.GetTerm(termSet.ID, new Guid(key.Substring(index + 1)));
                                path = endTerm.PathOfTerm;
                                builder.Append(path);
                            }
                            catch (Exception ex)
                            {
                                log.Warn("Error in get taxonomy field path.{0}", ex);
                            }
                        }
                        builder.Append(';');
                    }
                    return builder.ToString().TrimEnd(';');
                }
            }
            return path;
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
                if (value.GetType() != typeof(string))
                {
                    var dic = ((Dictionary<string, object>)value);
                    var termId = new Guid(dic["TermGuid"].ToString());
                    return termId.ToString();
                }
            }
            catch (Exception e)
            {
                log.Warn("Get Taxnomy Filed Value Error{0}", e.ToString());
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
                return string.Empty;
            }
            return string.Empty;
        }
        private static string GetTermGuid(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            IAveTaxonomyFieldValue value2 = value as IAveTaxonomyFieldValue;
            if (value2 != null)
            {
                return value2.TermGuid;
            }
            IAveTaxonomyFieldValueCollection values = value as IAveTaxonomyFieldValueCollection;
            if (values != null)
            {
                StringBuilder builder = new StringBuilder();
                bool flag = true;
                foreach (IAveTaxonomyFieldValue value3 in values)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(';');
                    }
                    builder.Append(GetTermGuid(value3));
                }
                return builder.ToString();
            }
            throw new ArgumentException();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special file in SharePoint,wrkstat.aspx")]
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
                        workflows.Add(field.Title.ToLower(CultureInfo.CurrentCulture), statusValue);
                    }
                }
                else if (field.Type == AveFieldType.URL)
                {
                    string[] s = ChangeWorkflowsStatusInLanguage(field.GetFieldValueAsText(item[field.ID])).Split(',');
                    if (s.Length >= 2 && s[0].IndexOf("wrkstat.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        const string workflowPattern = @"(?<=wrkstat.aspx\?List=)([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})&WorkflowInstanceName=([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})";
                        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(workflowPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (regex.IsMatch(s[0]))
                        {
                            string statusValue = s[1].ToString().Trim();
                            workflows.Add(field.Title.ToLower(CultureInfo.CurrentCulture), statusValue);
                        }
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

        private static List<FilterPolicy> CreateVersionRuleFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => (filter.Rule is VersionsRule) && filter.Level == level).ToList();
            }
            return new List<FilterPolicy>();
        }

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

        private static ObjectInfoBase CommonDocumentFilter(ref List<FilterPolicy> policies, IAveFile file, IAveListItem item, DocumentInfo result, int versionSequenceNo, int majorVersionSequenceNo, bool isMajorVersion, AveVersionObject version, bool checkVersion)
        {
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
                    case "FileExtensionsRule":
                        try
                        {
                            result.Name = file.Name;
                        }
                        catch (ArgumentException)
                        {
                            result.Name = string.Empty;
                        }
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
                        //systemFile.Item为空
                        var fileModfied = item == null ? file.TimeLastModified : item["Modified"];
                        result.Modified = ToUniversalTimeWithTimeZone((DateTime)fileModfied, file.Web);
                        break;
                    case "CreatedRule":
                        var fileCreated = item == null ? file.TimeCreated : item["Created"];
                        result.Created = ToUniversalTimeWithTimeZone((DateTime)fileCreated, file.Web);
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
                        site = file.Web.Site;
                        result.IsStub = CheckIsStub(site, file.UniqueId, file.Level, file.UIVersion);
                        if (result.IsStub)
                        {
                            result.StubLastAccessTime = GetStubLastAccessTime(site, file.UniqueId, file.Level, file.UIVersion);
                        }
                        else
                        {
                            result.StubLastAccessTime = new DateTime();
                        }

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
                    case "ContentTypeNameRule":
                    case "CustomContentTypeRule":
                        if (item != null && item.ContentType != null)
                        {
                            result.ContentType = item.ContentType.Name;
                        }
                        else
                        {
                            result.ContentType = "Document";
                        }
                        break;
                    case "ContentTypeIdRule":
                        if (item != null && item.ContentType != null)
                        {
                            result.ContentTypeId = item.ContentType.ID.ToString();
                        }
                        else
                        {
                            result.ContentTypeId = "0x01010072635879AE55BF4AA70560362FF4ABF8";//Document
                        }
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "CustomColumnRule":
                        if (result.ColumnInfosOfDisplayName == null || result.ColumnInfosOfInternalName == null)
                        {
                            List<Hashtable> tmpList = GetItemColumns(item);
                            result.ColumnInfosOfDisplayName = tmpList[0];
                            result.ColumnInfosOfInternalName = tmpList[1];
                            result.IntrNameToDispName = tmpList[2];
                            result.DispNameToType = tmpList[3];
                            result.SpecailColumnInfosOfDisplayName = tmpList[4];
                        }
                        break;
                    //This rule is for RA Only
                    case "TermRule":
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item);
                        }
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        result.ColumnInfosOfDisplayName = GetCustomPropertyInfo(result.ColumnInfosOfDisplayName, file.Properties);
                        result.ColumnInfosOfInternalName = GetCustomPropertyInfo(result.ColumnInfosOfInternalName, file.Properties);
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
                            if (file.ParentFolder.ParentList != null)
                            {
                                result.ListType = ((int)file.ParentFolder.ParentList.BaseTemplate).ToString();
                            }
                            else
                            {
                                result.ListType = string.Empty;
                            }
                        }
                        break;
                    case "ParentSiteCustomPropertyColumnTextRule":
                        if (item != null && item.ParentList != null && item.ParentList.ParentWeb != null)
                        {
                            if (item.ParentList.ParentWeb.IsRootWeb)
                            {
                                result.ParentSiteProperties = new Hashtable();
                            }
                            else
                            {
                                result.ParentSiteProperties = FillSiteColumns(item.ParentList.ParentWeb);
                            }
                        }
                        break;
                    case "ParentSiteCollectionCustomPropertyColumnTextRule":
                        if (item != null && item.ParentList != null)
                        {
                            result.ParentSiteCollectionProperties = FillSiteColumns(item.ParentList.ParentWeb.Site.RootWeb);
                        }
                        break;
                    case "AccessTimeRule":
                        if (WrapperConfiguration.UseStubAccessTimeRule)
                        {
                            site = file.Web.Site;
                            result.IsStub = CheckIsStub(site, file.UniqueId, file.Level, file.UIVersion);
                            result.AccessTime = DateTime.MinValue;
                            if (result.IsStub)
                            {
                                result.AccessTime = GetStubLastAccessTime(site, file.UniqueId, file.Level, file.UIVersion);
                            }
                        }
                        else
                        {
                            CheckAccessTimeRuleStatus(file.Web.Site);
                            string listId = file.ParentFolder.ParentList != null ? file.ParentFolder.ParentList.ID.ToString() : null;
                            DateTime modfied = item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                            modfied = ToUniversalTimeWithTimeZone(modfied, file.Web);
                            result.AccessTime = GetAccessTime(file.Web.Site, listId, file.UniqueId.ToString(), modfied);
                        }
                        break;
                    case "ParentFolderNameRule":
                        result.ParentFolderName = file.ParentFolder.Name;
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }
            return result;
        }

        private static ObjectInfoBase CommonItemFilter(ref List<FilterPolicy> policies, IAveListItem item, ItemInfo result)
        {
            return CommonItemFilter(ref policies, item, result, 0, 0, false, null, false);
        }

        //add for Micro Feed Archiver
        private static ObjectInfoBase CommonMicroFeedItemFilter(ref List<FilterPolicy> policies, IAveListItem item, MicroFeedItemInfo result)
        {
            return CommonMicroFeedItemFilter(ref policies, item, result, 0, 0, false, null, false);
        }

        private static ObjectInfoBase CommonItemFilter(ref List<FilterPolicy> policies, IAveListItem item, ItemInfo result, int versionSequenceNo, int majorVersionSequenceNo, bool isMajorVersion, AveVersionObject version, bool checkVersion)
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
                        result.Modified = ToUniversalTimeWithTimeZone((DateTime)item["Modified"], item.Web);
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTimeWithTimeZone((DateTime)item["Created"], item.Web);
                        break;
                    case "ModifiedByRule":
                        GetAuthorOrEditorInfo(item, result, false);
                        break;
                    case "CreatedByRule":
                        GetAuthorOrEditorInfo(item, result, true);
                        break;
                    case "ContentTypeRule":
                    case "CustomContentTypeRule":
                    case "ContentTypeNameRule":
                        result.ContentType = item.ContentType == null ? string.Empty : item.ContentType.Name;
                        break;
                    case "ContentTypeIdRule":
                        if (item != null && item.ContentType != null)
                        {
                            result.ContentTypeId = item.ContentType.ID.ToString();
                        }
                        else
                        {
                            result.ContentTypeId = string.Empty;
                        }
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "CustomColumnRule":
                        if (result.ColumnInfosOfDisplayName == null || result.ColumnInfosOfInternalName == null)
                        {
                            List<Hashtable> tmpList = GetItemColumns(item);
                            result.ColumnInfosOfDisplayName = tmpList[0];
                            result.ColumnInfosOfInternalName = tmpList[1];
                            result.IntrNameToDispName = tmpList[2];
                            result.DispNameToType = tmpList[3];
                            result.SpecailColumnInfosOfDisplayName = tmpList[4];
                        }
                        break;
                    //This rule is for RA Only
                    case "TermRule":
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item);
                        }
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        result.ColumnInfosOfDisplayName = GetCustomPropertyInfo(result.ColumnInfosOfDisplayName, item.Properties);
                        result.ColumnInfosOfInternalName = GetCustomPropertyInfo(result.ColumnInfosOfInternalName, item.Properties);
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
                    case "AccessTimeRule":
                        CheckAccessTimeRuleStatus(item.Web.Site);
                        string listId = item.ParentList != null ? item.ParentList.ID.ToString() : null;
                        DateTime modifiedTime = ToUniversalTimeWithTimeZone((DateTime)item["Modified"], item.Web);
                        result.AccessTime = GetAccessTime(item.Web.Site, listId, item.UniqueId.ToString(), modifiedTime);
                        break;
                    case "ParentSiteCustomPropertyColumnTextRule":
                        if (item != null && item.ParentList != null && item.ParentList.ParentWeb != null)
                        {
                            if (item.ParentList.ParentWeb.IsRootWeb)
                            {
                                result.ParentSiteProperties = new Hashtable();
                            }
                            else
                            {
                                result.ParentSiteProperties = FillSiteColumns(item.ParentList.ParentWeb);
                            }
                        }
                        break;
                    case "ParentSiteCollectionCustomPropertyColumnTextRule":
                        if (item != null && item.Web != null)
                        {
                            result.ParentSiteCollectionProperties = FillSiteColumns(item.ParentList.ParentWeb.Site.RootWeb);
                        }
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }
            return result;
        }

        //add for Micro Feed Archiver
        private static ObjectInfoBase CommonMicroFeedItemFilter(ref List<FilterPolicy> policies, IAveListItem item, MicroFeedItemInfo result, int versionSequenceNo, int majorVersionSequenceNo, bool isMajorVersion, IAveListItemVersion version, bool checkVersion)
        {
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Newsfeed);
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "ParticipationRule":
                        GetParticipationList(item, result);
                        break;
                    case "PostedByRule":
                        GetPostedBy(item, result);
                        break;
                    case "RepliedByRule":
                        GetRepliedByList(item, result);
                        break;
                    case "LikedByRule":
                        GetLikedByList(item, result);
                        break;
                    case "PostContentRule":
                        GetContentList(item, result);
                        break;
                    case "MentionRule":
                        GetMentionList(item, result);
                        break;
                    case "TagRule":
                        GetTagList(item, result);
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }
            return result;
        }


        public static ObjectInfoBase GetSiteFilterInfo(List<FilterPolicy> policies, IAveSite site)
        {
            if (WrapperConfiguration.RecordFilterPolicyLog != RecordFilterPolicyLog.None)
            {
                log.Debug("The url of the filtered site collection is {0}", site.Url);
            }
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
                        //特殊case，API返回时间kind为unspecified 实际上是UTC时间
                        result.Modified = site.LastContentModifiedDate.Kind == DateTimeKind.Unspecified ? new DateTime(site.LastContentModifiedDate.Ticks, DateTimeKind.Utc) : site.LastContentModifiedDate;
                        break;
                    case "CreatedRule":
                        //特殊case，API返回时间kind为unspecified 实际上是UTC时间
                        result.Created = site.RootWeb.Created.Kind == DateTimeKind.Unspecified ? new DateTime(site.RootWeb.Created.Ticks, DateTimeKind.Utc) : site.RootWeb.Created;
                        break;
                    case "CreatedByRule":
                    case "OwnerRule":
                        if (site.APIType == AveAPIType.BPOS_S && site.SPVersion.StartsWith("14.", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new NotSupportedException(string.Format("Site collection level filter policy is not supported.Rule name:{0}.", ruleName));
                        }
                        result.OwnerLogonName = site.Owner.NoPrefixLoginName;
                        result.OwnerLogonNameWithPrefix = site.Owner.LoginName;
                        result.OwnerTitle = site.Owner.Name;
                        break;
                    case "TemplateRule":
                        /*
                         * 需要说明的是:
                         * 对于使用"Save site as template"方式生成的模板创建的site,
                         * 其站点模板Id等同于其基础模板Id.
                         * 因而, 使用名字过滤时要使用其基础模板的名字.
                         * 如, 基于Team site创建的模板, 再使用该模板创建site, 则该site的tmplate id 为"STS#0",
                         * 过滤时应填写"Team Site".
                         * 
                         * 此处逻辑与TemplateIdRule保持一致.(需和QA交代清楚.)
                         * Web级别filter与此相同.
                         */
                        IAveWebTemplateCollection templates = site.GetWebTemplates(site.RootWeb.Language);
                        string templateId = site.RootWeb.WebTemplate + "#" + site.RootWeb.Configuration;
                        IAveWebTemplate current = templates.First(match => match.Name.Equals(templateId, StringComparison.OrdinalIgnoreCase));
                        result.TemplateName = current.Title;
                        break;
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
                        if (result.Properties == null)
                        {
                            result.Properties = FillSiteColumns(site.RootWeb);
                        }
                        break;
                    case "AccessTimeRule":
                        CheckAccessTimeRuleStatus(site);
                        DateTime modifiedTime = site.LastContentModifiedDate.Kind == DateTimeKind.Unspecified ? new DateTime(site.LastContentModifiedDate.Ticks, DateTimeKind.Utc) : site.LastContentModifiedDate;
                        result.AccessTime = GetSiteAccessTime(true, site, null, modifiedTime);
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }
            return result;
        }

        public static ObjectInfoBase GetWebFilterInfo(List<FilterPolicy> policies, IAveWeb web)
        {
            if (WrapperConfiguration.RecordFilterPolicyLog != RecordFilterPolicyLog.None)
            {
                log.Debug("The url of the filtered sub site is {0}", web.Url);
            }
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
                        result.Modified = DateTime.SpecifyKind(web.LastItemModifiedDate, DateTimeKind.Utc);
                        break;
                    case "CreatedRule":
                        result.Created = DateTime.SpecifyKind(web.Created, DateTimeKind.Utc);
                        break;
                    case "CreatedByRule":
                        if (web.Site.APIType == AveAPIType.BPOS_S)
                        {
                            throw new NotSupportedException(WrapperDiscoverResource.AWDOffice365NotSupportWebCreatedByRule);
                        }
                        result.CreatedByLogonName = web.Author.NoPrefixLoginName;
                        result.CreatedByLogonNameWithPrefix = web.Author.LoginName;
                        result.CreatedByTitle = web.Author.Name;
                        break;
                    case "TemplateRule":
                        IAveWebTemplateCollection templates = web.Site.GetWebTemplates(web.Language);
                        string templateId = web.WebTemplate + "#" + web.Configuration;
                        IAveWebTemplate current = templates.First(match => match.Name.Equals(templateId, StringComparison.OrdinalIgnoreCase));
                        result.TemplateName = current.Title;
                        break;
                    case "TemplateIdRule":
                        result.Template = web.WebTemplate + "#" + web.Configuration;
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        if (result.Properties == null)
                        {
                            result.Properties = FillSiteColumns(web);
                        }
                        break;
                    case "AccessTimeRule":
                        CheckAccessTimeRuleStatus(web.Site);
                        DateTime modifiedTime = DateTime.SpecifyKind(web.LastItemModifiedDate, DateTimeKind.Utc);
                        result.AccessTime = GetSiteAccessTime(false, web.Site, web, modifiedTime);
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }
            return result;
        }

        public static ObjectInfoBase GetListFilterInfo(List<FilterPolicy> policies, IAveList list)
        {
            if (WrapperConfiguration.RecordFilterPolicyLog != RecordFilterPolicyLog.None)
            {
                log.Debug("The url of the filtered list is {0}/{1}", list.ParentWeb.Url, list.RootFolder.Url);
            }
            ListInfo result = new ListInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.List);

            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "NameRule":
                        result.Title = list.Title;
                        break;
                    case "UrlRule":
                        result.Url = list.ParentWeb.Url + "/" + list.RootFolder.Url;
                        break;
                    case "ModifiedRule":
                        result.Modified = ToUniversalTimeWithTimeZone(list.LastItemModifiedDate, list.ParentWeb);
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTimeWithTimeZone(list.Created, list.ParentWeb);
                        break;
                    case "CreatedByRule":
                        if (list.ParentWeb.Site.APIType == AveAPIType.BPOS_S)
                        {
                            throw new NotSupportedException(WrapperDiscoverResource.AWDOffice365NotSupportListCreatedByRule);
                        }
                        result.CreatedByTitle = list.Author.Name;
                        result.CreatedByLogonName = list.Author.NoPrefixLoginName;
                        result.CreatedByLogonNameWithPrefix = list.Author.LoginName;
                        break;
                    case "ColumnsRule":
                        List<string> displayNames = new List<string>();
                        List<string> internalNames = new List<string>();
                        var fieldCollection = list.Fields;
                        if (fieldCollection.Count != 0)
                        {
                            foreach (var field in fieldCollection)
                            {
                                displayNames.Add(field.Title);
                                internalNames.Add(field.InternalName);
                            }
                        }
                        result.DisplayColumns = displayNames;
                        result.InternalColumns = internalNames;
                        break;
                    case "ContentTypeCollectionRule":
                    case "ContentTypeCollectionNameRule":
                        List<string> ctStrings = new List<string>();
                        var ctCollection = list.ContentTypes;
                        if (ctCollection.Count != 0)
                        {
                            ctStrings = ctCollection.Select(ct => ct.Name).ToList<string>();
                        }
                        result.ContentTypes = ctStrings;
                        break;
                    case "ContentTypeCollectionIdRule":
                        List<string> ctIdStrings = new List<string>();
                        var cts = list.ContentTypes;
                        if (cts.Count != 0)
                        {
                            ctIdStrings = cts.Select(ct => ct.ID.ToString()).ToList<string>();
                        }
                        result.ContentTypeIds = ctIdStrings;
                        break;
                    case "TemplateRule":
                        IAveListTemplateCollection templates = list.ParentWeb.ListTemplates;
                        string templateId = ((int)list.BaseTemplate).ToString();
                        IAveListTemplate current = templates.FirstOrDefault(match => match.Type_Client.ToString().Equals(templateId, StringComparison.OrdinalIgnoreCase));
                        result.TemplateName = current == null ? string.Empty : current.Name;
                        break;
                    case "TemplateIdRule":
                        result.Template = ((int)list.BaseTemplate).ToString();
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        result.Properties = list.RootFolder.Properties;
                        break;
                    case "AccessTimeRule":
                        CheckAccessTimeRuleStatus(list.ParentWeb.Site);
                        DateTime modifiedTime = ToUniversalTimeWithTimeZone(list.LastItemModifiedDate, list.ParentWeb);
                        result.AccessTime = GetAccessTime(list.ParentWeb.Site, list.ID.ToString(), null, modifiedTime);
                        break;
                    case "ItemCountRule":
                        result.ItemCount = list.ItemCount;
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
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
                        result.Modified = item != null ? ToUniversalTimeWithTimeZone((DateTime)item["Modified"], item.Web) : (isRootFolder ? ToUniversalTimeWithTimeZone((DateTime)folder.ParentList.LastItemModifiedDate, folder.ParentWeb) : DateTime.UtcNow);
                        break;
                    case "CreatedRule":
                        result.Created = item != null ? ToUniversalTimeWithTimeZone((DateTime)item["Created"], item.Web) : (isRootFolder ? ToUniversalTimeWithTimeZone((DateTime)folder.ParentList.Created, folder.ParentWeb) : DateTime.UtcNow);
                        break;
                    case "CreatedByRule":
                        if (item != null)
                        {
                            GetAuthorOrEditorInfo(item, result, true);
                        }
                        else
                        {
                            if (isRootFolder && folder.ParentWeb.Site.APIType == AveAPIType.BPOS_S)
                            {
                                throw new NotSupportedException(string.Format("List root folder filter policy is not supported.Rule name:{0}.", ruleName));
                            }
                            result.CreatedByTitle = isRootFolder ? folder.ParentList.Author.Name : string.Empty;
                            result.CreatedByLogonName = isRootFolder ? folder.ParentList.Author.NoPrefixLoginName : string.Empty;
                            result.CreatedByLogonNameWithPrefix = isRootFolder ? folder.ParentList.Author.LoginName : string.Empty;
                        }
                        break;
                    case "ContentTypeRule":
                    case "CustomContentTypeRule":
                    case "ContentTypeNameRule":
                        result.ContentType = item == null || item.ContentType == null ? AveConstants.SYSTEM_FOLDER : item.ContentType.Name;
                        break;
                    case "ContentTypeIdRule":
                        if (item != null && item.ContentType != null)
                        {
                            result.ContentTypeId = item.ContentType.ID.ToString();
                        }
                        else
                        {
                            result.ContentTypeId = string.Empty;
                        }
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                        if (result.ColumnInfosOfDisplayName == null)
                        {
                            List<Hashtable> tmpList = GetItemColumns(item);
                            result.ColumnInfosOfDisplayName = tmpList[0];
                            result.ColumnInfosOfInternalName = tmpList[1];
                            result.IntrNameToDispName = tmpList[2];
                            result.SpecailColumnInfosOfDisplayName = tmpList[4];
                        }
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        result.ColumnInfosOfDisplayName = GetCustomPropertyInfo(result.ColumnInfosOfDisplayName, folder.Properties);
                        break;
                    case "TermRule":
                        if (result.TermInfosOfDisplayName == null)
                        {
                            result.TermInfosOfDisplayName = GetItemTaxonomyColumns(item);
                        }
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }

            return result;
        }

        public static ObjectInfoBase GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem item)
        {
            ItemInfo result = new ItemInfo();
            return CommonItemFilter(ref policies, item, result);
        }

        //add for Micro Feed Archiver
        public static ObjectInfoBase GetMicroFeedItemFilterInfo(List<FilterPolicy> policies, IAveListItem item)
        {
            MicroFeedItemInfo result = new MicroFeedItemInfo();
            return CommonMicroFeedItemFilter(ref policies, item, result);
        }

        public static ObjectInfoBase GetDocumentFilterInfo(List<FilterPolicy> policies, IAveFile file, IAveListItem item)
        {
            DocumentInfo result = new DocumentInfo();
            return CommonDocumentFilter(ref policies, file, item, result);
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
                    case "FileExtensionsRule":
                        try
                        {
                            result.Name = item.Name;
                        }
                        catch (ArgumentException)
                        {
                            result.Name = string.Empty;
                        }
                        break;
                    case "SizeRule":
                        result.Size = GetFileVersionSize(item, version);
                        break;
                    case "ModifiedRule":
                        //this time is Utc Time ,we only need to give a DateTimeKind
                        result.Modified = DateTime.SpecifyKind(((DateTime)version["Modified"]), DateTimeKind.Utc);
                        break;
                    case "ModifiedByRule":
                        GetVersionEditorInfo(version, result);
                        break;
                    case "KeepHistoryVersionRule":
                        result.VersionSequenceNo = versionSequenceNo;
                        result.MajorVersionSequenceNo = isMajorVersion ? majorVersionSequenceNo : int.MaxValue;
                        result.CurrentMinorVersionSequenceNo = minorOfMajorSequenceNo;
                        result.UIVersion = version.VersionId;
                        result.Approved = true;
                        result.IsLastMajorVersion = isLastMajorVersion;
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
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }
            return result;
        }
        private static long GetFileVersionSize(IAveListItem item, IAveListItemVersion version)
        {
            //Get current version file size.
            if (item.File.UIVersion == version.VersionId)
            {
                return item.File.Length;
            }

            //Get history file version size
            //File.Versions.GetVersionFromID 无法取到file current version
            IAveFileVersion fileVersion = item.File.Versions.GetVersionFromID(version.VersionId);
            if (fileVersion != null)
            {
                return fileVersion.Size;
            }

            //Get checkout file version size
            if (item.File.CheckedOutByUser != null)
            {
                try
                {
                    //ADO-160510 item.file下获取不到其他user checkout的version
                    IAveWeb checkoutWeb = item.Web.Site.GetCheckoutWeb(item.Web.Site.ID, item.Web, item.ParentList, item.File.CheckedOutByUser, item.File.UniqueId, true, true);
                    IAveFile checkoutFile = checkoutWeb.GetFile(item.File.ServerRelativeUrl);
                    if (checkoutFile != null && checkoutFile.UIVersion == version.VersionId)
                    {
                        return checkoutFile.Length;
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while getting checkout file {0} version by id:{1}, error:{2}", item.File.Name, version.VersionId, e);
                    throw;
                }
            }
            log.Warn("Can not get file {0} version by id:{1} using size rule.", item.File.Name, version.VersionId);
            throw new AveFileNotFoundException(AveInternalResourceKey.Wrapper_Exception_Server_FileNotFoundException);
        }
        public static ObjectInfoBase GetDocumentVersionFilterInfo(List<FilterPolicy> policies, IAveFile item, int uiVersion)
        {
            DocumentVersionInfo result = new DocumentVersionInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.DocumentVersion);
            int versionSequenceNo = 0;
            int majorVersionSequenceNo = 0;
            bool isMajorVersion = false;
            IAveFileVersion version = null;
            foreach (var tmpVersion in item.Versions)
            {
                isMajorVersion = tmpVersion.ID % 512 == 0;
                if (tmpVersion.ID == uiVersion)
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
                            result.Title = GetValue<string>(version.Properties["vti_title"], string.Empty);
                        }
                        catch (ArgumentException)
                        {
                            result.Title = string.Empty;
                        }
                        break;
                    case "FileExtensionsRule":
                        try
                        {
                            result.Name = item.Name;
                        }
                        catch (ArgumentException)
                        {
                            result.Name = string.Empty;
                        }
                        break;
                    case "SizeRule":
                        result.Size = version.Size;
                        break;
                    case "ModifiedRule":
                        //this time is Utc Time ,we only need to give a DateTimeKind
                        result.Modified = DateTime.SpecifyKind(((DateTime)version.Properties["vti_timelastmodified"]), DateTimeKind.Utc);
                        break;
                    case "ModifiedByRule":
                        GetVersionEditorInfo(version, item, result);
                        break;
                    case "KeepHistoryVersionRule":
                        result.VersionSequenceNo = versionSequenceNo;
                        result.MajorVersionSequenceNo = isMajorVersion ? majorVersionSequenceNo : int.MaxValue;
                        result.UIVersion = version.ID;
                        result.Approved = true;
                        break;
                    case "ListTypeRule":
                        result.ListType = ((int)item.ParentFolder.ParentList.BaseTemplate).ToString();
                        break;
                    case "IsStubRule":
                        var site = item.Web.Site;
                        result.IsStub = CheckIsStub(site, item.UniqueId, version.Level, version.ID);
                        break;
                    case "StubLastAccessTimeRule":
                        site = item.Web.Site;
                        result.IsStub = CheckIsStub(site, item.UniqueId, version.Level, version.ID);
                        if (result.IsStub)
                        {
                            result.StubLastAccessTime = GetStubLastAccessTime(site, item.UniqueId, version.Level, version.ID);
                        }
                        else
                        {
                            result.StubLastAccessTime = new DateTime();
                        }
                        break;
                    case "StubCreateTimeRule":
                        site = item.Web.Site;
                        result.IsStub = CheckIsStub(site, item.UniqueId, version.Level, version.ID);
                        if (result.IsStub)
                        {
                            result.StubCreated = GetStubCreateTime(site, item.UniqueId, version.Level, version.ID);
                        }
                        else
                        {
                            result.StubCreated = new DateTime();
                        }
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
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
                        throw new AveException("The rule:{0} is invalid", ruleName);
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
                            throw new AveException("The condition:{0} is invalid", policy.Condition.ToString());
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
                    case "FileExtensionsRule":
                        try
                        {
                            result.Name = attachemnt.Name;
                        }
                        catch (ArgumentException)
                        {
                            result.Name = string.Empty;
                        }
                        break;
                    case "CreatedRule":
                        result.Created = ToUniversalTimeWithTimeZone((DateTime)attachemnt.TimeCreated, attachemnt.Web);
                        break;
                    case "CreatedByRule":
                        try
                        {
                            if (attachemnt.Author != null)
                            {
                                result.CreatedByTitle = attachemnt.Author.Name;
                                result.CreatedByLogonName = attachemnt.Author.NoPrefixLoginName;
                                result.CreatedByLogonNameWithPrefix = attachemnt.Author.LoginName;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperDiscoverResource.AWDGetUserError, e.ToString());
                        }
                        if (string.IsNullOrEmpty(result.CreatedByTitle) && string.IsNullOrEmpty(result.CreatedByLogonName) && string.IsNullOrEmpty(result.CreatedByLogonNameWithPrefix))
                        {
                            GetAuthorOrEditorInfo(item, result, true);
                        }
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                        if (result.ColumnInfosOfDisplayName == null)
                        {
                            var tmpList = GetItemColumns(item);
                            result.ColumnInfosOfDisplayName = tmpList[0];
                            result.ColumnInfosOfInternalName = tmpList[1];
                            result.IntrNameToDispName = tmpList[2];
                            result.SpecailColumnInfosOfDisplayName = tmpList[4];
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
                    case "AccessTimeRule":
                        if (WrapperConfiguration.UseStubAccessTimeRule)
                        {
                            site = attachemnt.Web.Site;
                            result.IsStub = CheckIsStub(site, attachemnt.UniqueId, attachemnt.Level, attachemnt.UIVersion);
                            result.AccessTime = DateTime.MinValue;
                            if (result.IsStub)
                            {
                                result.AccessTime = GetStubLastAccessTime(site, attachemnt.UniqueId, attachemnt.Level, attachemnt.UIVersion);
                            }
                        }
                        else
                        {
                            CheckAccessTimeRuleStatus(attachemnt.Web.Site);
                            string listId = item != null && item.ParentList != null ? item.ParentList.ID.ToString() : null;
                            //Attachment没有ModifiedTime，通过CreatedTime 进行比较
                            DateTime createTime = ToUniversalTimeWithTimeZone((DateTime)attachemnt.TimeCreated, attachemnt.Web);
                            result.AccessTime = GetAccessTime(attachemnt.Web.Site, listId, attachemnt.UniqueId.ToString(), createTime);
                        }
                        break;
                    default:
                        throw new AveException("The rule:{0} is invalid", ruleName);
                }
            }

            return result;
        }

        //以下方法为07 migration添加
        #region

        public static ObjectInfoBase GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion)
        {
            return GetItemFilterInfo(policies, item, uiVersion, null);
        }

        public static ObjectInfoBase GetItemFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion, IAveDiscoveryQuery query)
        {
            ItemInfo result = new ItemInfo();
            if (!HasVersionRule(policies))
            {
                return CommonItemFilter(ref policies, item, result, 0, 0, false, null, false);
            }
            int versionSequenceNo = 0;
            int majorVersionSequenceNo = 0;
            bool isMajorVersion = false;
            AveVersionObject version = null;
            List<AveVersionObject> versions = null;
            if (query != null)
            {
                versions = GetVersionByNative(item, query);
            }
            else
            {
                versions = ConvertListItemVersionToVersionObject(item.Versions);
            }
            foreach (var tmpVersion in versions)
            {
                isMajorVersion = tmpVersion.Uiversion % 512 == 0;

                if (tmpVersion.IsCurrentVersion)
                {
                    continue;
                }
                ++versionSequenceNo;
                majorVersionSequenceNo += isMajorVersion ? 1 : 0;
                if (tmpVersion.Uiversion == uiVersion)
                {
                    version = tmpVersion;
                    result.IsCurrentVersion = tmpVersion.IsCurrentVersion;
                    break;
                }
            }
            if (version == null)
            {
                return null;
            }
            return CommonItemFilter(ref policies, item, result, versionSequenceNo, majorVersionSequenceNo, isMajorVersion, version, true);
        }

        private static bool HasVersionRule(List<FilterPolicy> policies)
        {
            bool hasVersionRule = false;
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                if (string.Compare(ruleName, "VersionsRule", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    hasVersionRule = true;
                    break;
                }
            }
            return hasVersionRule;
        }

        private static List<AveVersionObject> GetVersionByNative(IAveListItem item, IAveDiscoveryQuery query)
        {
            return query.GetItemVersions(item.ParentList.ParentWeb.Site.ID, item.ParentList.ParentWeb.ID, item.ParentList.ID, item.ID).VersionObjs;
        }

        public static ObjectInfoBase GetDocumentFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion, IAveFile file)
        {
            return GetDocumentFilterInfo(policies, item, uiVersion, file, null);
        }

        private static List<AveVersionObject> ConvertListItemVersionToVersionObject(IAveListItemVersionCollection versions)
        {
            List<AveVersionObject> versionsObj = new List<AveVersionObject>();
            for (int index = 0; index < versions.Count; index++)
            {
                var version = versions[index];
                AveVersionObject versionObj = new AveVersionObject();
                versionObj.Uiversion = version.VersionId;
                versionObj.Level = (byte)version.Level;
                versionObj.Tp_IsCurrentVersion = version.IsCurrentVersion;//通过API获取到的IsCurrentVersion对应AllUserdata表中的tp_IsCurrentVersion
                versionObj.IsCurrentVersion = index == 0;
                versionObj.UiVersionString = version.VersionLabel;
                versionsObj.Add(versionObj);
            }
            return versionsObj;
        }

        public static ObjectInfoBase GetDocumentFilterInfo(List<FilterPolicy> policies, IAveListItem item, int uiVersion, IAveFile file, IAveDiscoveryQuery query)
        {
            DocumentInfo result = new DocumentInfo();
            if (!HasVersionRule(policies))
            {
                return CommonDocumentFilter(ref policies, file, item, result, 0, 0, false, null, true);
            }
            int versionSequenceNo = 0;
            int majorVersionSequenceNo = 0;
            bool isMajorVersion = false;
            AveVersionObject version = null;
            List<AveVersionObject> versions = null;
            if (query != null)
            {
                versions = GetVersionByNative(item, query);
            }
            else
            {
                versions = ConvertListItemVersionToVersionObject(item.Versions);
            }
            foreach (var tmpVersion in versions)
            {
                isMajorVersion = tmpVersion.Uiversion % 512 == 0;

                if (tmpVersion.IsCurrentVersion)
                {
                    continue;
                }
                ++versionSequenceNo;
                majorVersionSequenceNo += isMajorVersion ? 1 : 0;
                if (tmpVersion.Uiversion == uiVersion)
                {
                    version = tmpVersion;
                    result.IsCurrentVersion = tmpVersion.IsCurrentVersion;
                    break;
                }
            }
            if (version == null)
            {
                return null;
            }
            return CommonDocumentFilter(ref policies, file, item, result, versionSequenceNo, majorVersionSequenceNo, isMajorVersion, version, true);
        }

        private static void CheckVersionRule(VersionedObjectInfoBase result, int versionSequenceNo, int majorVersionSequenceNo, bool isMajorVersion, AveVersionObject version)
        {
            result.VersionSequenceNo = versionSequenceNo;
            result.MajorVersionSequenceNo = isMajorVersion ? majorVersionSequenceNo : int.MaxValue;
            result.UIVersion = version.Uiversion;
            result.Approved = (AveFileLevel)version.Level == AveFileLevel.Published;
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
            //if (StubLastAccessTime == new DateTime())
            //{
            //    StubLastAccessTime = (DateTime)stubInfoClass.GetProperty("CreationTime").GetValue(stubInfo, null);
            //}
            return StubLastAccessTime;
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

        private static DateTime GetSiteAccessTime(bool isSite, IAveSite site, IAveWeb web, DateTime modifiedTime)
        {
            object time;
            Type type = Type.GetType("DocAve.SP2010.ReportCenter.Auditor.LastAccessTime,SP2010RCAuditor");
            object obj = Activator.CreateInstance(type);
            MethodInfo info = AvePoint.Common.Invoker.GetMethod(type, "RetrieveLastAccessTime", new Type[] { typeof(string), typeof(string), typeof(string) });
            if (isSite)
            {
                time = info.Invoke(obj, new object[] { site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString(), site.Url, null });
            }
            else
            {
                time = info.Invoke(obj, new object[] { site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString(), site.Url, web.Url });
            }
            DateTime datetime = (DateTime)time;
            //如果Object的Modified Time大于RC返回的LastAccessTime则把ModifiedTime作为Object的LastAccessTime.
            if (modifiedTime > datetime)
            {
                string url = isSite ? site.Url : web.Url;
                log.Info(string.Format("Modified Time is older than last access time, modified time is : {0}, access time is {1}, site url is : {2}", modifiedTime, datetime, url));
                datetime = modifiedTime;
            }
            return datetime;
        }

        private static DateTime GetAccessTime(IAveSite site, string listId, string itemId, DateTime modifiedTime)
        {
            object time = null;
            if (site.IsOnlineSite)
            {
                //目前Online LastAccessTime只支持Document Level，且不支持Local模拟.
                Type type = Type.GetType("DocAve.SP2010.ReportCenter.Auditor.OnlineLastAccessedTime.OnlineLastAccessedTimeQuerier,SP2010RCAuditor");
                object obj = Activator.CreateInstance(type);
                MethodInfo info = AvePoint.Common.Invoker.GetMethod(type, "QueryItemLastAccessedTime", new Type[] { typeof(string), typeof(string) });
                time = info.Invoke(obj, new object[] { site.Url, itemId });
            }
            else
            {
                Type type = Type.GetType("DocAve.SP2010.ReportCenter.Auditor.LastAccessTime,SP2010RCAuditor");
                object obj = Activator.CreateInstance(type);
                MethodInfo info = AvePoint.Common.Invoker.GetMethod(type, "RetrieveLastAccessTime", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
                time = info.Invoke(obj, new object[] { site.WebApplication == null ? string.Empty : site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString(), site.Url, listId, itemId });
            }
            DateTime datetime = (DateTime)time;
            //如果Object的Modified Time大于RC返回的LastAccessTime则把ModifiedTime作为Object的LastAccessTime.
            if (modifiedTime > datetime)
            {
                log.Info(string.Format("Modified Time is older than last access time, modified time is : {0}, access time is {1}, node Id is : {2}", modifiedTime, datetime, itemId));
                datetime = modifiedTime;
            }
            return datetime;
        }

        private static void CheckAccessTimeRuleStatus(IAveSite site)
        {
            int checkresult = 0;
            if (site.IsOnlineSite)
            {
                //目前Online LastAccessTime只支持Document Level，且不支持Local模拟.
                Type type = Type.GetType("DocAve.SP2010.ReportCenter.Auditor.OnlineLastAccessedTime.OnlineLastAccessedTimeQuerier,SP2010RCAuditor");
                object obj = Activator.CreateInstance(type);
                MethodInfo info = AvePoint.Common.Invoker.GetMethod(type, "CheckRuleStatus", new Type[] { typeof(string), typeof(int) });
                checkresult = (Int32)info.Invoke(obj, new object[] { site.Url, (int)site.Audit.AuditFlags });
            }
            else
            {
                Type type = Type.GetType("DocAve.SP2010.ReportCenter.Auditor.LastAccessTime,SP2010RCAuditor");
                object obj = Activator.CreateInstance(type);
                MethodInfo info = AvePoint.Common.Invoker.GetMethod(type, "CheckAccessTimeRuleStatus", new Type[] { typeof(string), typeof(string), typeof(int) });
                checkresult = (Int32)info.Invoke(obj, new object[] { site.WebApplication == null ? string.Empty : site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString(), site.Url, (int)site.Audit.AuditFlags });
            }
            if (checkresult == 1)
            {
                throw new Exception("StorageOptimization_SOARAuditorEnableException");
            }
            if (checkresult == 2)
            {
                throw new Exception("StorageOptimization_SOARReportServiceException");
            }
            if (checkresult == 3)
            {
                throw new Exception("StorageOptimization_SOARNotFindAuditorJobException");
            }
            if (checkresult == 4)
            {
                throw new Exception("StorageOptimization_SOARAuditorJobException");
            }
            if (checkresult == 5)
            {
                throw new Exception("StorageOptimization_SOARCheckAuditorJobTimeException");
            }
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

        /// <summary>
        /// add for Micro Feed
        /// </summary>
        /// 
        #region add fro Micro Feed
        /// <summary>
        /// safe 下面添加lock
        /// </summary>
        private static Dictionary<int, List<int>> microFeedReplyIDCache = new Dictionary<int, List<int>>();
        /// <summary>
        /// safe 下面添加lock
        /// </summary>
        private static Dictionary<int, List<string>> microFeedLikerCache = new Dictionary<int, List<string>>();
        /// <summary>
        /// safe 下面添加lock
        /// </summary>
        private static Dictionary<int, List<string>> microFeedMentionCache = new Dictionary<int, List<string>>();
        /// <summary>
        /// safe 下面添加lock
        /// </summary>
        private static Dictionary<int, List<string>> microFeedTagCache = new Dictionary<int, List<string>>();

        private static void UpdateMicroFeedReplyIDCache(IAveListItem item)
        {
            Dictionary<int, List<int>> tempDictionary = item.GetMicroFeedReplyID();
            foreach (KeyValuePair<int, List<int>> postIDReplyID in tempDictionary)
            {
                if (!microFeedReplyIDCache.ContainsKey(postIDReplyID.Key))
                {
                    microFeedReplyIDCache.Add(postIDReplyID.Key, postIDReplyID.Value);
                }
            }
        }

        private static void UpdateMicroFeedLikerCache(IAveListItem item)
        {
            Dictionary<int, List<string>> tempDictionary = item.GetMicroFeedLiker();
            foreach (KeyValuePair<int, List<string>> postIDLiker in tempDictionary)
            {
                if (!microFeedLikerCache.ContainsKey(postIDLiker.Key))
                {
                    microFeedLikerCache.Add(postIDLiker.Key, postIDLiker.Value);
                }
            }
        }

        private static void UpdateMicroFeedMentionCacheAndTagCache(IAveListItem item)
        {
            Dictionary<int, List<string>> tempMentionDictionary = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> tempMentionDisPlayDictionary = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> tempTagDictionary = new Dictionary<int, List<string>>();
            item.GetMicroFeedMentionAndTag(ref tempMentionDictionary, ref tempMentionDisPlayDictionary, ref tempTagDictionary);
            foreach (KeyValuePair<int, List<string>> postIDLiker in tempMentionDictionary)
            {
                if (!microFeedMentionCache.ContainsKey(postIDLiker.Key))
                {
                    microFeedMentionCache.Add(postIDLiker.Key, postIDLiker.Value);
                }
            }
            foreach (KeyValuePair<int, List<string>> postIDLiker in tempTagDictionary)
            {
                if (!microFeedTagCache.ContainsKey(postIDLiker.Key))
                {
                    microFeedTagCache.Add(postIDLiker.Key, postIDLiker.Value);
                }
            }
        }

        public static void InitMicroFeedCache()
        {
            microFeedReplyIDCache.Clear();
            microFeedLikerCache.Clear();
            microFeedMentionCache.Clear();
            microFeedTagCache.Clear();
        }

        private static List<int> GetReplyIDs(IAveListItem item)
        {
            List<int> temp = new List<int>();
            lock (microFeedReplyIDCache)
            {
                if (!microFeedReplyIDCache.ContainsKey(Convert.ToInt32(item["ID"])))
                {
                    UpdateMicroFeedReplyIDCache(item);
                }
                try
                {
                    temp = microFeedReplyIDCache[Convert.ToInt32(item["ID"])];
                }
                catch (Exception e)
                {
                    log.Error("an error occurred while getting micro feed reply IDs:{0}", e.ToString());
                }
            }
            return temp;
        }

        private static List<string> GetLikers(IAveListItem item)
        {
            List<string> temp = new List<string>();
            lock (microFeedLikerCache)
            {
                if (!microFeedLikerCache.ContainsKey(Convert.ToInt32(item["ID"])))
                {
                    UpdateMicroFeedLikerCache(item);
                }
                try
                {
                    temp = microFeedLikerCache[Convert.ToInt32(item["ID"])];
                }
                catch (Exception e)
                {
                    log.Error("an error occurred while getting micro feed liker:{0}", e.ToString());
                }
            }
            return temp;
        }

        private static List<string> GetMentions(IAveListItem item)
        {
            List<string> temp = new List<string>();
            lock (microFeedMentionCache)
            {
                if (!microFeedMentionCache.ContainsKey(Convert.ToInt32(item["ID"])))
                {
                    UpdateMicroFeedMentionCacheAndTagCache(item);
                }
                try
                {
                    temp = microFeedMentionCache[Convert.ToInt32(item["ID"])];
                }
                catch (Exception e)
                {
                    log.Error("an error occurred while getting micro feed mentions:{0}", e.ToString());
                }
            }
            return temp;
        }

        private static List<string> GetTags(IAveListItem item)
        {
            List<string> temp = new List<string>();
            lock (microFeedMentionCache)
            {
                if (!microFeedTagCache.ContainsKey(Convert.ToInt32(item["ID"])))
                {
                    UpdateMicroFeedMentionCacheAndTagCache(item);
                }
                try
                {
                    temp = microFeedTagCache[Convert.ToInt32(item["ID"])];
                }
                catch (Exception e)
                {
                    log.Error("an error occurred while getting micro feed tags:{0}", e.ToString());
                }
            }
            return temp;
        }

        private static void GetParticipationList(IAveListItem item, CommonInfoBase result)
        {
            result.ParticipationLogonName = new List<string>();
            result.ParticipationLogonNameWithPrefix = new List<string>();
            result.ParticipationTitle = new List<string>();
            GetPostedBy(item, result, true);
            GetRepliedByList(item, result, true);
            GetLikedByList(item, result, true);
            GetMentionList(item, result, true);
        }

        private static void GetPostedBy(IAveListItem item, CommonInfoBase result, bool isParticipationRule = false)
        {
            string loginName = item["PostAuthor"].ToString();
            string loginNameWithPrefix = string.Empty;
            string title = string.Empty;
            GetUserInfoByLoginName(item, loginName, ref loginNameWithPrefix, ref title);
            if (isParticipationRule)
            {
                result.ParticipationLogonName.Add(loginName);
                result.ParticipationLogonNameWithPrefix.Add(loginNameWithPrefix);
                result.ParticipationTitle.Add(title);
            }
            else
            {
                result.PostedByLogonName = loginName;
                result.PostedByLogonNameWithPrefix = loginNameWithPrefix;
                result.PostedByTitle = title;
            }
        }

        private static void GetRepliedByList(IAveListItem item, CommonInfoBase result, bool isParticipationRule = false)
        {
            List<int> repliesId = GetReplyIDs(item);
            result.RepliedByLogonName = new List<string>();
            result.RepliedByLogonNameWithPrefix = new List<string>();
            result.RepliedByTitle = new List<string>();
            IAveListItem reply = null;
            string loginName = string.Empty;
            string loginNameWithPrefix = string.Empty;
            string title = string.Empty;

            foreach (int Id in repliesId)
            {
                reply = item.ParentList.GetItemById(Id);
                if (reply["PostAuthor"] != null)
                {
                    loginName = reply["PostAuthor"].ToString();
                    GetUserInfoByLoginName(item, loginName, ref loginNameWithPrefix, ref title);
                }
                if (isParticipationRule)
                {
                    if (!result.ParticipationLogonName.Contains(loginName))
                    {
                        result.ParticipationLogonName.Add(loginName);
                        result.ParticipationLogonNameWithPrefix.Add(loginNameWithPrefix);
                        result.ParticipationTitle.Add(title);
                    }
                }
                else if (!result.RepliedByLogonName.Contains(loginName))
                {
                    result.RepliedByLogonName.Add(loginName);
                    result.RepliedByLogonNameWithPrefix.Add(loginNameWithPrefix);
                    result.RepliedByTitle.Add(title);
                }
            }
        }

        private static void GetLikedByList(IAveListItem item, CommonInfoBase result, bool isParticipationRule = false)
        {
            List<string> likers = GetLikers(item);
            result.LikedByLogonName = new List<string>();
            result.LikedByLogonNameWithPrefix = new List<string>();
            result.LikedByTitle = new List<string>();
            string loginName = string.Empty;
            string loginNameWithPrefix = string.Empty;
            string title = string.Empty;
            foreach (string liker in likers)
            {
                loginName = liker;
                GetUserInfoByLoginName(item, loginName, ref loginNameWithPrefix, ref title);
                if (isParticipationRule)
                {
                    if (!result.ParticipationLogonName.Contains(loginName))
                    {
                        result.ParticipationLogonName.Add(loginName);
                        result.ParticipationLogonNameWithPrefix.Add(loginNameWithPrefix);
                        result.ParticipationTitle.Add(title);
                    }
                }
                else
                {
                    result.LikedByLogonName.Add(loginName);
                    result.LikedByLogonNameWithPrefix.Add(loginNameWithPrefix);
                    result.LikedByTitle.Add(title);
                }
            }
        }

        private static void GetMentionList(IAveListItem item, CommonInfoBase result, bool isParticipationRule = false)
        {
            List<string> mentions = GetMentions(item);
            result.MentionLogonName = new List<string>();
            result.MentionLogonNameWithPrefix = new List<string>();
            result.MentionTitle = new List<string>();
            string loginName = string.Empty;
            string loginNameWithPrefix = string.Empty;
            string title = string.Empty;
            foreach (string mention in mentions)
            {
                loginName = mention;
                GetUserInfoByLoginName(item, loginName, ref loginNameWithPrefix, ref title);
                if (isParticipationRule)
                {
                    if (!result.ParticipationLogonName.Contains(loginName))
                    {
                        result.ParticipationLogonName.Add(loginName);
                        result.ParticipationLogonNameWithPrefix.Add(loginNameWithPrefix);
                        result.ParticipationTitle.Add(title);
                    }
                }
                else
                {
                    result.MentionLogonName.Add(loginName);
                    result.MentionLogonNameWithPrefix.Add(loginNameWithPrefix);
                    result.MentionTitle.Add(title);
                }
            }
        }

        private static void GetContentList(IAveListItem item, CommonInfoBase result)
        {
            result.PostContents = new List<string>();
            result.PostContents.Add(item["Content"] == null ? string.Empty : item["Content"].ToString());
            //Get Replies Content
            IAveList list = item.ParentList;

            IAveListItem reply = null;
            List<int> repliesId = GetReplyIDs(item);
            foreach (int Id in repliesId)
            {
                reply = list.GetItemById(Id);
                result.PostContents.Add(reply["Content"] == null ? string.Empty : reply["Content"].ToString());
            }
        }

        private static void GetTagList(IAveListItem item, CommonInfoBase result)
        {
            List<string> tags = GetTags(item);
            result.Tags = tags;
        }

        private static void GetUserInfoByLoginName(IAveListItem item, string loginName, ref string loginNameWithPrefix, ref string title)
        {
            //if (!loginName.StartsWith("i:0#", StringComparison.OrdinalIgnoreCase))
            //{
            //    loginName = "i:0#.w|" + loginName;
            //}
            //IAveUser user = item.ParentList.ParentWeb.SiteUsers.GetByLoginName(loginName);
            IAvePrincipalInfo info = WrapperRuntime.CurrentContext.ModelFactory.Utility.ResolvePrincipal(item.ParentList.ParentWeb, loginName, AvePrincipalType.User, AvePrincipalSource.All, null, false, false);

            if (info != null)
            {
                loginNameWithPrefix = info.LoginName;
                title = info.DisplayName;
            }
        }
        #endregion

        private static Hashtable GetCustomPropertyInfo(Hashtable columnInfo, Hashtable propertyInfo)
        {
            if (columnInfo != null && propertyInfo != null)
            {
                foreach (var info in propertyInfo.Keys)
                {
                    if (!columnInfo.ContainsKey(info))
                    {
                        columnInfo.Add(info, propertyInfo[info]);
                    }
                }
            }
            else
            {
                if (propertyInfo != null)
                    columnInfo = new Hashtable(propertyInfo);
            }
            return columnInfo;
        }
    }
}
