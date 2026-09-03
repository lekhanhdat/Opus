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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.Server16
{
    internal class AveWebDatabaseSite
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region For Backup
        private static Dictionary<uint, List<TablesInTemplate>> TemplateCache = new Dictionary<uint, List<TablesInTemplate>>();
        /// <summary>
        /// Inite tables as marks of template from local files.
        /// </summary>
        /// <param name="lcid"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "accsrv,sitetemplates: Folder name in a path.")]
        private static void InitTemplateTables(uint lcid)
        {
            lock (TemplateCache)
            {
                if (null != TemplateCache && TemplateCache.ContainsKey(lcid))
                {
                    return;
                }
            }
            DirectoryInfo fPath = new DirectoryInfo(SPUtility.GetVersionedGenericSetupPath(Path.Combine(@"template\sitetemplates\accsrv\", lcid.ToString()), 14));
            if (!fPath.Exists)
            {
                return;
            }
            Assembly ass = Assembly.Load("Microsoft.Office.Access.Server.Application, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
            Type tempType = ass.GetType("Microsoft.Office.Access.Server.Template.Template");
            foreach (FileInfo file in fPath.GetFiles("*.accdt", SearchOption.TopDirectoryOnly))
            {
                if (!file.Exists || !file.Extension.Equals(".accdt", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                using (Stream stream = file.OpenRead())
                {
                    object templateObj = AveAssemblyUtility.InvokeStaticMethod(tempType, "ReadTemplate", new Type[] { typeof(Stream), typeof(string), typeof(int) }, new object[] { stream, file.FullName, -1 });
                    object tableObj = AveAssemblyUtility.GetPropertyValue(templateObj, "Tables");
                    if (tableObj is IDictionary)
                    {
                        lock (TemplateCache)
                        {
                            AppendToTemplateCache(TemplateCache, file.Name.ToLower(CultureInfo.InvariantCulture), GetKeysAsString((IDictionary)tableObj), lcid);
                        }
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "charitablecontributions.accdt: File of sharepoint local path.")]
        private static bool AppendToTemplateCache(Dictionary<uint, List<TablesInTemplate>> iDictionary, string fileName, string tables, uint lcid)
        {
            TablesInTemplate cache = null;
            if (iDictionary == null)
            {
                iDictionary = new Dictionary<uint, List<TablesInTemplate>>();
            }
            if (!iDictionary.ContainsKey(lcid))
            {
                iDictionary[lcid] = new List<TablesInTemplate>();
            }

            switch (fileName)
            {
                case "assets.accdt":
                    cache = new TablesInTemplate("ACCSRV#1", tables);
                    break;
                case "charitablecontributions.accdt":
                    cache = new TablesInTemplate("ACCSRV#3", tables);
                    break;
                case "contacts.accdt":
                    cache = new TablesInTemplate("ACCSRV#4", tables);
                    break;
                case "issues.accdt":
                    cache = new TablesInTemplate("ACCSRV#6", tables);
                    break;
                case "projects.accdt":
                    cache = new TablesInTemplate("ACCSRV#5", tables);
                    break;
            }
            if (!iDictionary[lcid].Contains(cache))
            {
                iDictionary[lcid].Add(cache);
            }
            return iDictionary[lcid].Contains(cache);
        }

        private static string GetKeysAsString(IDictionary iDictionary)
        {
            StringBuilder str = new StringBuilder();
            foreach (object k in iDictionary.Keys)
            {
                str.Append(k.ToString() + "|#;");
            }
            str.Remove(str.Length - 3, 3);
            return str.ToString();
        }

        private static bool TryGetItemValue(string objectName, int objectTypeNum, SPList list)
        {
            SPQuery query = new SPQuery();
            StringBuilder builder = new StringBuilder();
            builder.Append("<Where>");
            if (!string.IsNullOrEmpty(objectName))
            {
                builder.Append("<And>");
                WriteEq(builder, "Title", SPFieldType.Text, objectName);

            }
            switch (((AccessServerObjectType)objectTypeNum))
            {
                case AccessServerObjectType.Entity:
                case AccessServerObjectType.Query:
                    builder.Append("<Or>");
                    WriteEq(builder, "Type", SPFieldType.Integer, 0);
                    WriteEq(builder, "Type", SPFieldType.Integer, 1);
                    builder.Append("</Or>");
                    break;

                default:
                    WriteEq(builder, "Type", SPFieldType.Integer, objectTypeNum);
                    break;
            }
            if (!string.IsNullOrEmpty(objectName))
            {
                builder.Append("</And>");
            }
            builder.Append("</Where>");
            query.IncludeMandatoryColumns = false;
            query.RowLimit = 1;
            query.Query = builder.ToString();
            query.IncludeAllUserPermissions = false;
            query.IncludeAttachmentUrls = false;
            query.IncludePermissions = false;
            SPListItemCollection items = list.GetItems(query);
            if (items.Count > 0)
            {
                return true;
            }
            return false;
        }

        private static void WriteEq(StringBuilder builder, string fieldName, SPFieldType valueType, object value)
        {
            builder.Append("<Eq>");
            builder.Append(GetFeildRef(fieldName, valueType));
            builder.Append(GetValue(valueType, value));
            builder.Append("</Eq>");
        }

        private static string GetFeildRef(string fieldName, SPFieldType fieldType)
        {
            if (fieldType == SPFieldType.Lookup)
            {
                return string.Format(CultureInfo.InvariantCulture, "<FieldRef LookupId='true' Name='{0}'/>", new object[] { fieldName });
            }
            return string.Format(CultureInfo.InvariantCulture, "<FieldRef Name='{0}'/>", new object[] { fieldName });
        }

        private static string GetValue(SPFieldType valueType, object value)
        {
            string str = SecurityElement.Escape(string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { value }));
            string str2 = string.Empty;
            if (valueType == SPFieldType.DateTime)
            {
                str2 = "IncludeTimeType='True'";
            }
            return string.Format(CultureInfo.InvariantCulture, "<Value Type = '{0}'{1}>{2}</Value>", new object[] { GetType(valueType).ToString(), str2, str });
        }

        private static SPFieldType GetType(SPFieldType valueType)
        {
            SPFieldType type = valueType;
            if (type != SPFieldType.Lookup)
            {
                if (type == SPFieldType.User)
                {
                    return SPFieldType.Text;
                }
                return valueType;
            }
            return SPFieldType.Integer;
        }

        public static string TryGetACCSRVWebTemplate(SPWeb web)
        {
            uint lcid = web.Language;
            string template = string.Empty;
            try
            {
                InitTemplateTables(lcid);
                List<TablesInTemplate> tablesTemplates;
                lock (TemplateCache)
                {
                    if (TemplateCache == null || !TemplateCache.ContainsKey(lcid))
                    {
                        return template;
                    }
                    tablesTemplates = TemplateCache[lcid];
                }
                SPList list = null;
                if (web.AllProperties.ContainsKey("___MSysASOId"))
                {
                    list = web.Lists[new Guid((string)web.AllProperties["___MSysASOId"])];
                }
                else
                {
                    list = web.Lists["MSysASO"];
                }

                foreach (TablesInTemplate tableTemplate in tablesTemplates)
                {
                    bool hasFoundTemplate = true;
                    string[] tables = tableTemplate.tablesString.Split(new string[] { "|#;" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string subTable in tables)
                    {
                        if (!(hasFoundTemplate &= TryGetItemValue(subTable, 0, list)))
                        {
                            break;
                        }
                    }
                    if (hasFoundTemplate)
                    {
                        template = tableTemplate.webTempate;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetWebTemplateError, e.ToString());
            }
            return template;
        }

        internal class TablesInTemplate
        {
            internal string webTempate;
            internal string tablesString;
            internal TablesInTemplate(string template, string tables)
            {
                this.webTempate = template;
                this.tablesString = tables;
            }
        }
        #endregion

        #region For Restore
        public static bool IsWebDatabaseWeb(SPWeb desWeb)
        {
            return desWeb.WebTemplate.Equals("ACCSRV", StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Append Required Fields Before Update for new item
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static SPListItem AppendRequiredFieldsForNewItem(SPListItem newItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            SPFieldCollection desFields = newItem.ParentList.Fields;
            foreach (SPField sf in desFields)
            {
                if (sf.Required)
                {
                    if (userData.ContainsKey(sf.InternalName) || userData.ContainsKey("#" + sf.InternalName))
                    {
                        string tempkey = userData.ContainsKey(sf.InternalName) ? sf.InternalName : "#" + sf.InternalName;
                        newItem[sf.Id] = userData[tempkey];
                    }
                    else
                    {
                        //Don't need to set value, maybe do something in feature.
                    }
                }
            }
            return newItem;
        }
        #endregion
    }

    public enum AccessServerObjectType
    {
        Unknown = -1,
        Entity = 0,
        Query = 1,
        Form = 2,
        Report = 3,
        Macro = 4,
        Module = 5,
        Link = 6,
        SQLLink = 7,
        ImExSpec = 8,
        NavigationPane = 9,
        VbaReferences = 10,
        DBProps = 11,
        Image = 12,
        Theme = 13,
        Cluster = 14,
        CompilationStatus = 15,
        PivotTable = 16,
        PivotChart = 17,
        Cmdbar = 18,
    }
}
