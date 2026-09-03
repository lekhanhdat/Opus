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
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Threading;
using System.Globalization;
using AvePoint.Common;
using Microsoft.Data.SqlClient;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AvePoint.Wrapper.Common
{
    public class AveSPUtility
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public const string mWebRelativeUrlPrefix = "~site/";
        public const string mSiteRelativeUrlPrefix = "~sitecollection/";
        public static Regex MatchShareLink = new Regex(@"^SharingLinks\.[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}\.(AnonymousEdit|AnonymousView|OrganizationEdit|OrganizationView|Flexible)\.[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$");

        private static Hashtable mDictType = null;
        private static AveVolatileCache<string, bool> mSP1DBSchemaTable = new AveVolatileCache<string, bool>();

        static AveSPUtility()
        {            
        }

        public static string WebRelativeUrlPrefix
        {
            get
            {
                return mWebRelativeUrlPrefix;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")] 
        public static string SiteRelativeUrlPrefix
        {
            get
            {
                return mSiteRelativeUrlPrefix;
            }
        }

        public static bool IsEbsArchivedData(int docFlags)
        {
            if ((docFlags & 65536) != 0)
            {
                return true;
            }
            return false;
        }

        public static bool IsRbsArchivedData(byte[] rbsId)
        {
            return rbsId != null;
        }

        public static bool IsOrInSystemFormsFolder(IAveFolder folder)
        {
            IAveList list = folder.ParentWeb.Lists[folder.ParentListId];
            return folder.ServerRelativeUrl.StartsWith(list.RootFolder.ServerRelativeUrl + "/Forms", StringComparison.OrdinalIgnoreCase);
        }

        public static bool StsCompareStrings(string str1, string str2)
        {
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            return (0 == compareInfo.Compare(str1, str2, CompareOptions.IgnoreCase));
        }

        /// <summary>
        /// 用旧名字和修改时间组成冲突名
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="modifyTime"></param>
        /// <returns></returns>
        public static string GetConflictNewName(String oldName, DateTime modifyTime)
        {
            string NewName = string.Empty;
            string[] ExtendName = oldName.Split('.');
            if (ExtendName.Length >= 2)
            {
                ExtendName[ExtendName.Length - 2] += "(" + AveDateTimeUtility.ConvertToType008(modifyTime) + ")";
            }
            else
            {
                ExtendName[0] += "(" + AveDateTimeUtility.ConvertToType008(modifyTime) + ")";
            }
            for (int i = 0; i < ExtendName.Length; i++)
            {
                if (i != ExtendName.Length - 1)
                    NewName += ExtendName[i] + ".";
                else
                    NewName += ExtendName[i];
            }
            return NewName;
        }

        public static IAveFile LoadCheckOutFile(IAveWeb web, Guid fileId, int checkOutUserId, AveObjectModelFactory modelFactory)
        {
            IAveUser user = web.SiteUsers.GetByID(checkOutUserId);
            IAveUserToken userToken = user.UserToken;
            Guid webId = web.ID;
            Guid siteId = web.Site.ID;
            IAveFile file = null;
            using (IAveSite site = modelFactory.CreateSite(siteId, userToken))
            {
                using (IAveWeb curWeb = site.OpenWeb(webId))
                {
                    file = curWeb.GetFile(fileId);
                }
            }
            return file;
        }

        public static string GetServerUrl(IAveSite site)
        {
            StringBuilder builder = new StringBuilder(site.Protocol, 0x200);
            builder.Append("//");
            builder.Append(site.HostName);
            if ((site.Protocol.Equals("http:", StringComparison.OrdinalIgnoreCase) && (site.Port != 80))
                || (site.Protocol.Equals("https:", StringComparison.OrdinalIgnoreCase) && (site.Port != 0x1bb)))
            {
                builder.Append(":");
                builder.Append(site.Port);
            }
            return builder.ToString();
        }

        public static AveFieldType GetFieldType(string strType)
        {
            if (mDictType == null)
            {
                Hashtable hashtable = new Hashtable();
                foreach (AveFieldType type in Enum.GetValues(typeof(AveFieldType)))
                {
                    hashtable[Enum.GetName(typeof(AveFieldType), type)] = type;
                }
                hashtable["LookupMulti"] = hashtable["Lookup"];
                hashtable["UserMulti"] = hashtable["User"];
                Interlocked.CompareExchange<Hashtable>(ref mDictType, hashtable, null);
            }
            object obj2 = mDictType[strType];
            if (obj2 != null)
            {
                return (AveFieldType)obj2;
            }
            return AveFieldType.Invalid;
        }

        public static bool IsDependentLookupField(IAveField field)
        {
            IAveFieldLookup lookupfield = field as IAveFieldLookup;
            return lookupfield != null && lookupfield.IsDependentLookup;
        }

        //public static Guid CreateExternalList(AveObjectModelFactory omFactory, IAveWeb web, string listTitle, string description, string dataSourceXml)
        //{
        //    Guid id = Guid.Empty;

        //    if (IfServiceAvailable(web.Site.WebApplication, ServiceApplicationType.BDCService))
        //    {
        //        string LobSystemInstance = string.Empty;
        //        string EntityNamespace = string.Empty;
        //        string Entity = string.Empty; ;
        //        string SpecificFinder = string.Empty;
        //        XmlDocument xDoc = new XmlDocument();
        //        xDoc.LoadXml(dataSourceXml);
        //        foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
        //        {
        //            switch (node.Attributes["Name"].Value)
        //            {
        //                case "LobSystemInstance":
        //                    LobSystemInstance = node.Attributes["Value"].Value;
        //                    break;
        //                case "EntityNamespace":
        //                    EntityNamespace = node.Attributes["Value"].Value;
        //                    break;
        //                case "Entity":
        //                    Entity = node.Attributes["Value"].Value;
        //                    break;
        //                case "SpecificFinder":
        //                    SpecificFinder = node.Attributes["Value"].Value;
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //        IAveListDataSource dataSource = omFactory.CreateListDataSource();
        //        AveAssemblyUtility.InvokeMethod(dataSource, dataSource.GetType(), "EnsurePropertyWhiteListDict", null);
        //        dataSource.SetProperty("EntityNamespace", EntityNamespace);
        //        dataSource.SetProperty("Entity", Entity);
        //        dataSource.SetProperty("SpecificFinder", SpecificFinder);
        //        dataSource.SetProperty("LobSystemInstance", LobSystemInstance);
        //        AveAssemblyUtility.InvokeMethod(dataSource, dataSource.GetType(), "InitializeByEntityNameAndNamespace", new object[] { web });

        //        id = web.Lists.Add(listTitle, description, listTitle, dataSource);

        //        //SPServiceContext context = SPServiceContext.GetContext(web.Site.WebApplication.ServiceApplicationProxyGroup, SPSiteSubscriptionIdentifier.Default);
        //        //Type t = Type.GetType(ServiceApplicationType.BDCService);
        //        //BdcServiceApplicationProxy bdcProxy = (BdcServiceApplicationProxy)context.GetDefaultProxy(t);

        //        //DatabaseBackedMetadataCatalog catalog = bdcProxy.GetDatabaseBackedMetadataCatalog();
        //    }

        //    return id;
        //}

        public static bool IfServiceAvailable(IAveWebApplication webApp, string serviceAppType)
        {
            string assemblyQualifiedName = serviceAppType;
            Type type = Type.GetType(assemblyQualifiedName);
            if (type != null)
            {
                if (webApp.ServiceApplicationProxyGroup.ContainsType(type))
                {
                    foreach (IAveServiceApplicationProxy p in webApp.ServiceApplicationProxyGroup.Proxies)
                    {
                        if (p.CheckAssemblyQualifiedName(assemblyQualifiedName)
                            && p.Status == AveObjectStatus.Online)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool TryParseMultiColumnValue(string fieldValue, out List<string> subColumnValues)
        {
            subColumnValues = new List<string>();
            if (!string.IsNullOrEmpty(fieldValue))
            {
                string str = ";#";
                if (str.Length != 2)
                {
                    return false;
                }
                char c = str[0];
                char ch2 = str[1];
                string oldValue = new string(c, 2);
                string newValue = new string(c, 1);
                int startIndex = 0;
                if (fieldValue.StartsWith(str, StringComparison.OrdinalIgnoreCase))
                {
                    startIndex = str.Length;
                }
                int num2 = startIndex;
                bool flag = false;
                while (num2 < fieldValue.Length)
                {
                    if (fieldValue[num2] == c)
                    {
                        num2++;
                        if (num2 < fieldValue.Length)
                        {
                            if (fieldValue[num2] != ch2)
                            {
                                if (fieldValue[num2] != c)
                                {
                                    return false;
                                }
                                num2++;
                                flag = true;
                            }
                            else
                            {
                                if ((num2 - 1) > startIndex)
                                {
                                    string item = fieldValue.Substring(startIndex, (num2 - startIndex) - 1);
                                    if (flag)
                                    {
                                        item = item.Replace(oldValue, newValue);
                                    }
                                    subColumnValues.Add(item);
                                    flag = false;
                                }
                                else
                                {
                                    subColumnValues.Add(string.Empty);
                                }
                                num2++;
                                startIndex = num2;
                            }
                            continue;
                        }
                        break;
                    }
                    num2++;
                }
                if (num2 > startIndex)
                {
                    string str5 = fieldValue.Substring(startIndex, num2 - startIndex);
                    if (flag)
                    {
                        str5 = str5.Replace(oldValue, newValue);
                    }
                    subColumnValues.Add(str5);
                }
            }
            return true;
        }

        public static bool IsSP1DBSchema(AveSqlConnection sqlConn)
        {
            bool isSP1Schema = false;
            try
            {
                if (mSP1DBSchemaTable.ContainsKey(sqlConn.ConnectionString))
                {
                    return mSP1DBSchemaTable[sqlConn.ConnectionString];
                }
                string cmdText = "SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME=@TABLE_NAME";
                sqlConn.AddParameter("@TABLE_NAME", "AllSites");
                using (SqlDataReader reader = sqlConn.ExecuteReader(cmdText))
                {
                    if (reader.HasRows)
                    {
                        isSP1Schema = true;
                        mSP1DBSchemaTable[sqlConn.ConnectionString] = isSP1Schema;
                    }
                    else
                    {
                        isSP1Schema = false;
                        mSP1DBSchemaTable[sqlConn.ConnectionString] = isSP1Schema;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCCheckDBSchemaError, ex.ToString());
            }
            return isSP1Schema;
        }

        public static byte[] ParseContentTypeId(string id)
        {
            if (id == null)
            {
                throw new ArgumentException();
            }
            if ((id.Length % 2) != 0)
            {
                throw new ArgumentException();
            }
            char[] chArray = id.ToCharArray();
            if (((chArray.Length < 2) || (chArray[0] != '0')) || (char.ToLowerInvariant(chArray[1]) != 'x'))
            {
                throw new ArgumentException();
            }
            int index = 2;
            int num2 = (chArray.Length - index) / 2;
            byte[] buffer = null;
            if (num2 > 0)
            {
                int num3 = 0;
                buffer = new byte[num2];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)((AveConvert.Hex(chArray[index]) << 4) | AveConvert.Hex(chArray[index + 1]));
                    index += 2;
                    if (num3 > 0)
                    {
                        num3--;
                    }
                    else if (buffer[i] == 0)
                    {
                        num3 = 0x10;
                    }
                }
                if (num3 > 0)
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                buffer = new byte[0];
            }
            return buffer;
        }

        public static bool IsChildOfContentType(byte[] childIdBytes, string parentId)
        {            
            byte[] parentIdBytes = ParseContentTypeId(parentId);
            if (childIdBytes.Length < parentIdBytes.Length)
            {
                return false;
            }
            for (int i = 0; i < parentIdBytes.Length; i++)
            {
                if (parentIdBytes[i] != childIdBytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsWorkflowTaskItem(string contentTypeId)
        {
            bool isWorkflowInstance = false;
            if (!string.IsNullOrEmpty(contentTypeId) && contentTypeId.StartsWith("0x010801", StringComparison.OrdinalIgnoreCase))
            {
                isWorkflowInstance = true;
            }
            return isWorkflowInstance;
        }

        public static bool IsGuid(string strId)
        {
            if (string.IsNullOrEmpty(strId))
            {
                return false;
            }
            strId = strId.Trim();
            if (strId.Length < 0x20)
            {
                return false;
            }
            if (strId.Contains("x") || strId.Contains("X"))
            {
                strId = strId.Replace(" ", "");
                return Regex.IsMatch(strId, @"^\{0[x|X][a-fA-F\d]{8},(0[x|X][a-fA-F\d]{4},){2}\{(0[x|X][a-fA-F\d]{2},){7}0[x|X][a-fA-F\d]{2}\}\}$", RegexOptions.Compiled);
            }
            return Regex.IsMatch(strId, @"^([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\}|[a-fA-F\d]{32})$", RegexOptions.Compiled);
        }

        // Microsoft.SharePoint.Utilities.SPUtilityInternal
        public static string NormalizeSharePointGroupName(string name, string webTitle)
        {
            name = name.Trim();
            int num = name.Length - 255;
            if (num > 0)
            {
                name = name.Replace(webTitle, webTitle.Substring(0, webTitle.Length - num));
                if (name.Length > 255)
                {
                    name = name.Substring(0, 255);
                }
            }
            char[] array = name.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if ("/\\[]:|<>+=;,?*'\"@".IndexOf(array[i]) >= 0)
                {
                    array[i] = '_';
                }
            }
            return new string(array);
        }

        public static bool ShouldUseEditRole(IAveWeb web)
        {
            return web.RoleDefinitions.GetByType(AveRoleType.Editor) != null && web.Site.CompatibilityLevel >= 15 &&
                web.WebTemplateId == 1 && web.Configuration == 0;
        }
    }

    public class AveSPListUtility
    {
        public static void EnsureRssView(IAveList list)
        {
            StringCollection strCollViewFields = new StringCollection();

            list.Views.Add("RssView", strCollViewFields, string.Empty, 0x19, false, false);

            IAveView view = list.Views["RssView"];
            view.Hidden = true;
            IAveViewFieldCollection viewFields = view.ViewFields;
            viewFields.RemoveAll();
            foreach (IAveField field in list.Fields)
            {
                if ((CanIncludeInDescription(field) && !IsAutomaticallyMapped(field)) && field.Reorderable)
                {
                    viewFields.Add(field);
                }
            }
            view.Update();
        }

        private static bool IsAutomaticallyMapped(IAveField spField)
        {
            string str;
            if (((str = spField.InternalName) == null) || ((!(str == "Title") && !(str == "Editor")) && !(str == "Modified")))
            {
                return false;
            }
            return true;
        }

        private static bool IsSpecialIncludedField(IAveField spField)
        {
            if (spField.ParentList.BaseTemplate != AveListTemplateType.Events)
            {
                return (spField.InternalName == "Title");
            }
            if ((!(spField.InternalName == "Title") && !(spField.InternalName == "EventDate")) && (!(spField.InternalName == "EndDate") && !(spField.InternalName == "Description")))
            {
                return (spField.InternalName == "Location");
            }
            return true;
        }

        public static bool CanIncludeInDescription(IAveField spField)
        {
            if ((!spField.Hidden && (spField.ShowInDisplayForm != false)) || IsSpecialIncludedField(spField))
            {
                AveFieldType outputType;
                if (spField is IAveFieldCalculated)
                {
                    outputType = (spField as IAveFieldCalculated).OutputType;
                }
                else
                {
                    outputType = spField.Type;
                }
                switch (outputType)
                {
                    case AveFieldType.Invalid:
                    case AveFieldType.Integer:
                    case AveFieldType.Text:
                    case AveFieldType.Note:
                    case AveFieldType.DateTime:
                    case AveFieldType.Choice:
                    case AveFieldType.Lookup:
                    case AveFieldType.Boolean:
                    case AveFieldType.Number:
                    case AveFieldType.Currency:
                    case AveFieldType.URL:
                    case AveFieldType.Guid:
                    case AveFieldType.MultiChoice:
                    case AveFieldType.GridChoice:
                    case AveFieldType.User:
                        return true;
                }
            }
            return false;
        }

        public static bool IsViewExist(IAveList list, string viewTitle)
        {
            foreach (IAveView view in list.Views)
            {
                if (view.Title.Equals(viewTitle))
                    return true;
            }
            return false;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        public static Hashtable EnsureRssSettings(IAveWeb web, IAveList list)
        {
            StringBuilder builder = new StringBuilder(web.ServerRelativeUrl);
            if (!web.ServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append("/");
            }
            builder.Append("_layouts/images/siteIcon.png");
            string str = AveSPResource.GetString("ListRssChannelTitle", new object[] { web.Title, list.Title });
            string str2 = AveSPResource.GetString("ListRssChannelDescription", new object[] { list.Title });
            string str3 = builder.ToString();
            Hashtable properties = new Hashtable();
            properties["vti_rss_DisplayOnQuicklaunch"] = 0;
            properties["vti_rss_DisplayRssIcon"] = 1;
            properties["vti_rss_LimitDescriptionLength"] = 0;
            properties["vti_rss_ChannelTitle"] = str;
            properties["vti_rss_ChannelDescription"] = str2;
            properties["vti_rss_ChannelImageUrl"] = str3;
            properties["vti_rss_ItemLimit"] = 25;
            properties["vti_rss_DayLimit"] = 7;
            return properties;
        }        
    }

    public class ServiceApplicationType
    {
        public const string UserProfileService = "Microsoft.Office.Server.Administration.UserProfileApplicationProxy, Microsoft.Office.Server.UserProfiles, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        public const string BDCService = "Microsoft.SharePoint.BusinessData.SharedService.BdcServiceApplicationProxy, Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        public const string ManagedMetadataService = "Microsoft.SharePoint.Taxonomy.MetadataWebServiceApplicationProxy, Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        public const string ManagedMetadataServiceApplication = "Microsoft.SharePoint.Taxonomy.MetadataWebServiceApplication, Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        public const string ManagedMetadataServiceApplicationUtilities = "Microsoft.Office.Server.Utilities.SPServiceApplicationUtilities,Microsoft.Office.Server, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        public const string PartionSettings = "Microsoft.SharePoint.Taxonomy.PartitionSettings,Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
    }
}
