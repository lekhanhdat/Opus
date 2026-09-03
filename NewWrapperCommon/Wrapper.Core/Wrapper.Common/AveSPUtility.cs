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
using System.Data.SqlClient;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    public class AveSPUtility
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public const string mWebRelativeUrlPrefix = "~site/";
        public const string mSiteRelativeUrlPrefix = "~sitecollection/";

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
            if (list.BaseType != AveBaseType.DocumentLibrary)
            {
                return false;
            }
            return folder.ServerRelativeUrl.StartsWith(list.RootFolder.ServerRelativeUrl + "/Forms/", StringComparison.OrdinalIgnoreCase)
                || folder.ServerRelativeUrl.Equals(list.RootFolder.ServerRelativeUrl + "/Forms", StringComparison.OrdinalIgnoreCase);
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

        public static bool IfServiceAvailable(IAveWebApplication webApp, AveServiceApplicationType serviceAppType)
        {
            return WrapperRuntime.CurrentContext.ModelFactory.Utility.IfServiceAvailable(webApp, serviceAppType);
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

        public static bool IsSP1DBSchema(AveSqlConnection sqlConn, string columnName, string tableName)
        {
            bool isSP1Schema = false;
            try
            {
                if (mSP1DBSchemaTable.ContainsKey(sqlConn.ConnectionString))
                {
                    return (bool)mSP1DBSchemaTable[sqlConn.ConnectionString];
                }
                string cmdText = "Select * from syscolumns Where Name=@COLUMN_NAME And ID=OBJECT_ID(@TABLE_NAME)";
                sqlConn.AddParameter("@TABLE_NAME", tableName);
                sqlConn.AddParameter("@COLUMN_NAME", columnName);
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

        internal static bool IsSP1DBSchema(AveQueryWorker worker, string columnName, string tableName)
        {
            bool isSP1Schema = false;
            try
            {
                if (mSP1DBSchemaTable.ContainsKey(worker.ConnectionString))
                {
                    return (bool)mSP1DBSchemaTable[worker.ConnectionString];
                }
                string cmdText = "Select * from syscolumns Where Name=@COLUMN_NAME And ID=OBJECT_ID(@TABLE_NAME)";
                worker.AddParameter("@TABLE_NAME", tableName);
                worker.AddParameter("@COLUMN_NAME", columnName);
                using (var reader = worker.ExecuteReader(cmdText))
                {
                    if (reader.HasRows)
                    {
                        isSP1Schema = true;
                        mSP1DBSchemaTable[worker.ConnectionString] = isSP1Schema;
                    }
                    else
                    {
                        isSP1Schema = false;
                        mSP1DBSchemaTable[worker.ConnectionString] = isSP1Schema;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCCheckDBSchemaError, ex);
            }
            return isSP1Schema;
        }

        public static bool IsSPInstalled(string siteUrl)
        {
            return AveEnvironment.IsSPInstalled(siteUrl);
        }

        public static void ExceptionNeedNotLog(Exception ex)
        {
 
        }

        internal static bool GetBooleanProperty(Hashtable properties, string propertyName, bool defaultValue)
        {
            if (!properties.ContainsKey(propertyName))
            {
                return defaultValue;
            }
            bool result = defaultValue;
            try
            {
                result = Convert.ToBoolean(properties[propertyName], CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                result = defaultValue;
            }
            return result;
        }
    }

    public class AveSPListUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSPListUtility));

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

        #region get list setting

        public static bool IsViewExist(IAveList list, string viewTitle)
        {
            return list.Views.Any(view =>string.Equals(viewTitle,view.Title,StringComparison.OrdinalIgnoreCase));
        }

        public static void AssemblyListAllSettingInfo(IAveList list,AveListSettingInfo listSettingInfo)
        {
            GetListSettingByAPI(list, listSettingInfo);
            GetListSettingByFlag(list, listSettingInfo);
            GetListSettingFor10Upper(list,listSettingInfo);
            AssemblyRootFolderMetaInfo(listSettingInfo);
        }

        private static void GetListSettingFor10Upper(IAveList list, AveListSettingInfo listSettingInfo)
        {
            if (AveEnv.IsSharePoint2007)
            {
                return;
            }
            GetListSettingByFlagFor10Upper(list,listSettingInfo);
            GetListSettingByAPIFor10Upper(list, listSettingInfo);
        }

        private static void GetListSettingByFlag(IAveList list,AveListSettingInfo listSettingInfo)
        {
            var flags = (ulong)listSettingInfo.Flags.Value;
            AssemblyListSettingFromFlagsCommon(listSettingInfo, (ulong)listSettingInfo.Flags.Value);
            if (listSettingInfo.Flags2 !=null)
            {
                AssemblyListSettingFromFlags2Common(listSettingInfo, (ulong)listSettingInfo.Flags2.Value);
            }
            listSettingInfo.ServerTemplateCanCreateFolders = IsServerTemplateCanCreateFolders(flags, list.BaseTemplate, list.ParentWeb, list.BaseType);
        }

        private static void GetListSettingByFlagFor10Upper(IAveList list, AveListSettingInfo listSettingInfo)
        {
            listSettingInfo.EnableThrottling = !listSettingInfo.NoThrottleListOperations.Value;//07 not supported
        }

        /// <summary>
        /// 通过API获取一些List Setting信息
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listSettingInfo"></param>
        private static void GetListSettingByAPI(IAveList list, AveListSettingInfo listSettingInfo)
        {
            listSettingInfo.AnonymousPermMask64 = (ulong)list.AnonymousPermMask64;
            listSettingInfo.HasUniqueRoleAssigntments = list.HasUniqueRoleAssignments;
            listSettingInfo.OnQuickLaunch = list.OnQuickLaunch;
            listSettingInfo.RssViewField = IsViewExist(list, "RssView") ? list.Views["RssView"].ViewFields.SchemaXml : string.Empty;
            listSettingInfo.LastModifiedTime = list.LastItemModifiedDate;
            listSettingInfo.EnableManagedIndexes = list.EnableManagedIndexes;
            var docLib = list as IAveDocumentLibrary;
            if (docLib != null)
            {
                listSettingInfo.DocumentTemplateUrl = docLib.DocumentTemplateUrl;
            }
            try
            {
                listSettingInfo.DefaultView = list.ParentWeb.Url.Substring(0, list.ParentWeb.Url.Length - (list.ParentWebUrl.Length > 1 ? list.ParentWebUrl.Length : 0)) + list.DefaultViewUrl;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred when getting list default view:{0}. ID:{1}. Reason:{2}.", list.Title, list.ID, e);
            }
        }

        private static void GetListSettingByAPIFor10Upper(IAveList list, AveListSettingInfo listSettingInfo)
        {           
            if (AveEnv.IsMoss)
            {
                listSettingInfo.AllowRatingSetting = GetListRatingSettingByMossAPI(list);
            }
            listSettingInfo.ValidationFormula = list.ValidationFormula;
            listSettingInfo.ValidationMessage = list.ValidationMessage;
            listSettingInfo.IsTaxonomyHiddenList = string.Equals(list.ID.ToString(), list.ParentWeb.TaxonomyList, StringComparison.OrdinalIgnoreCase);
            listSettingInfo.AllowRssFeads = list.AllowRssFeeds;
            listSettingInfo.IsThrottled = list.IsThrottled;
            var flags = (ulong)list.Flags;
            if (!Ave2010ListFlags.DefaultItemOpenUseListSetting(flags))
            {
                listSettingInfo.DefaultItemOpen = 0;
            }
            else
            {
                if (list.ParentWeb.Site != null)
                {
                    listSettingInfo.DefaultItemOpen = Ave2010ListFlags.DefaultItemOpen(flags, list.ParentWeb.Site.BrowserDocumentsEnabled) == AveDefaultItemOpen.Browser ? 1 : 2;
                }
            }
        }

        private static bool GetListRatingSettingByMossAPI(IAveList list)
        {
            return list.Fields.Contains(AveFieldId.AverageRatings) && list.Fields.Contains(AveFieldId.RatingsCount);
        }

        private static void AssemblyListSettingFromFlags2Common(AveListSettingInfo listSettingInfo, ulong flags)
        {
            listSettingInfo.CrawlNonDefaultViews = Ave2010ListFlags.EnableCrawlNonDefaultViews(flags);
        }

        private static void AssemblyListSettingFromFlagsCommon(AveListSettingInfo listSettingInfo, ulong flags)
        {
            var template = (AveListTemplateType)listSettingInfo.ServerTemplate.Value;
            var baseType = (AveBaseType)listSettingInfo.BaseType.Value;

            listSettingInfo.AllowContentTypes = Ave2010ListFlags.AllowContentTypes(flags, template);
            listSettingInfo.AllowDeletion = Ave2010ListFlags.AllowDeletion(flags);
            listSettingInfo.AllowMultiResponses = Ave2010ListFlags.AllowMultiResponses(flags);
            listSettingInfo.EnableFolderCreation = Ave2010ListFlags.EnableFolderCreation(flags);
            listSettingInfo.EnableModeration = Ave2010ListFlags.EnableModeration(flags);
            listSettingInfo.IrmEnabled = Ave2010ListFlags.IrmEnabled(flags);
            listSettingInfo.IrmExpire = Ave2010ListFlags.IrmExpire(flags);
            listSettingInfo.IrmReject = Ave2010ListFlags.IrmReject(flags);
            listSettingInfo.EnableVersioning = Ave2010ListFlags.EnableVersioning(flags);
            listSettingInfo.Ordered = Ave2010ListFlags.IrmReject(flags);
            listSettingInfo.ContentTypesEnabled = Ave2010ListFlags.ContentTypesEnabled(flags);
            listSettingInfo.EnableAssignToEmail = Ave2010ListFlags.EnableAssignToEmail(flags);
            listSettingInfo.RequestAccessEnabled = Ave2010ListFlags.RequestAccessEnabled(flags);
            listSettingInfo.EnableDeployWithDependentList = Ave2010ListFlags.EnableDeployWithDependentList(flags);
            listSettingInfo.EnableDeployingList = Ave2010ListFlags.EnableDeployingList();
            listSettingInfo.EnablePeopleSelector = Ave2010ListFlags.EnablePeopleSelector(flags);
            listSettingInfo.EnableResourceSelector = Ave2010ListFlags.EnableResourceSelector(flags);
            listSettingInfo.EnableSchemaCaching = Ave2010ListFlags.EnableSchemaCaching(flags);
            listSettingInfo.EnforceDataValidation = Ave2010ListFlags.EnforceDataValidation(flags);
            listSettingInfo.EnableSyndication = Ave2010ListFlags.EnableSyndication(flags);
            listSettingInfo.ExcludeFromOfflineClient = Ave2010ListFlags.ExcludeFromOfflineClient(flags);
            listSettingInfo.ExcludeFromTemplate = Ave2010ListFlags.ExcludeFromTemplate(flags);
            listSettingInfo.Hidden = Ave2010ListFlags.Hidden(flags);
            listSettingInfo.MultipleDataList = Ave2010ListFlags.MultipleDataList(flags);
            listSettingInfo.NoCrawl = Ave2010ListFlags.NoCrawl(flags);
            listSettingInfo.EnableAttachments = Ave2010ListFlags.EnableAttachments(flags, baseType, template);
            listSettingInfo.EnableMinorVersions = Ave2010ListFlags.EnableMinorVersions(flags, baseType);
            listSettingInfo.ForceCheckout = Ave2010ListFlags.ForceCheckout(flags, baseType);
            listSettingInfo.WorkflowsAssociated = Ave2010ListFlags.WorkflowsAssociated(flags);
            listSettingInfo.DraftVersionVisibility = (int)Ave2010ListFlags.DraftVersionVisibility(flags, baseType);//reader =0,author=1,approval = 3
            listSettingInfo.ShowUser = Ave2010ListFlags.ShowUser(flags);
            listSettingInfo.DisableGridEditing = Ave2010ListFlags.DisableGridEditing(flags);
            listSettingInfo.NavigateForFormsPages = Ave2010ListFlags.NavigateForFormsPages(flags);
            //AllList.tp_SendToLocation, split with '|' SendToLoacationName = string[0] and SendToLoacationUrl = string[1];
            if (listSettingInfo.SendToLocation != null && listSettingInfo.SendToLocation.IsAvailable && listSettingInfo.SendToLocation.Value != null)
            {
                string[] splitLoacationProperty = listSettingInfo.SendToLocation.Value.Split(new char[] { '|' });
                listSettingInfo.SendToLocationName = splitLoacationProperty[0];
                listSettingInfo.SendToLocationUrl = splitLoacationProperty[1];
            }
            else
            {
                listSettingInfo.SendToLocationName = null;
                listSettingInfo.SendToLocationUrl = null;
            }
        }

        internal static void AssemblyRootFolderMetaInfo(AveListSettingInfo listSettingInfo)
        {
            if (listSettingInfo.RootFolderInfo.Value.MetaInfo != null)
            {
                var metaDic = new MetaInfoHandler(listSettingInfo.RootFolderInfo.Value.MetaInfo).ToHashtable();
                if (metaDic.ContainsKey("Ratings_VotingExperience") && !string.IsNullOrEmpty(metaDic["Ratings_VotingExperience"] as string))
                {
                    //add for Rating Setting
                    listSettingInfo.AllowRatingSetting = metaDic.ContainsKey("Ratings_VotingExperience") && !string.IsNullOrEmpty(metaDic["Ratings_VotingExperience"] as string);
                    listSettingInfo.RatingSettingType = listSettingInfo.AllowRatingSetting.Value ?
                        (int)Enum.Parse(typeof(AveRatingSettingType), metaDic["Ratings_VotingExperience"].ToString(), true) :
                        (int)AveRatingSettingType.None;
                }
                listSettingInfo.IsSiteAssetsLibrary = metaDic.ContainsKey("IsAttachmentLibrary") ? Int32.Parse(metaDic["IsAttachmentLibrary"].ToString()) != 0 : false;
                ChangeUserFormatForConnectorListSetting(listSettingInfo, metaDic);

                listSettingInfo.RootFolderInfo.Value.MetaInfoDic = metaDic;
            }
            else
            {
                listSettingInfo.RootFolderInfo.Value.MetaInfoDic = null;
                listSettingInfo.IsSiteAssetsLibrary = false;
            }
        }

        private static void ChangeUserFormatForConnectorListSetting(AveListSettingInfo listSettingInfo, Hashtable metaDic)
        {
            //ADO-120446 对ftp类型Storage的username去掉多余'\'，目的使Backup username结果正确
            if (metaDic.ContainsKey("ConnectorStorageSetting") && metaDic["ConnectorStorageSetting"].ToString().Contains("ftp_vim"))
            {
                var doc = new XmlDocument();
                doc.LoadXml(metaDic["ConnectorStorageSetting"].ToString());
                var node = doc.DocumentElement.SelectSingleNode("StorageUnits/StorageUnit[@Key=\"name\"]");
                if (node != null && node.Attributes["Value"].Value.Contains("\\\\"))
                {
                    string[] arrayStr = node.Attributes["Value"].Value.Split('\\');
                    node.Attributes["Value"].Value = arrayStr[0] + "\\" + arrayStr[arrayStr.Length - 1];
                    listSettingInfo.RootFolderInfo.Value.MetaInfoDic["ConnectorStorageSetting"] = doc.InnerXml;
                }
            }
        }

        private static bool IsServerTemplateCanCreateFolders(ulong flags, AveListTemplateType baseTemplate, IAveWeb parentWeb, AveBaseType baseType)
        {
            if ((((((baseTemplate == AveListTemplateType.WebTemplateCatalog) ||
                (baseTemplate == AveListTemplateType.WebPartCatalog)) ||
                ((baseTemplate == AveListTemplateType.ListTemplateCatalog))) ||
                (((baseTemplate == AveListTemplateType.UserInformation)) ||
                (baseTemplate == AveListTemplateType.Survey))) ||
                ((baseType != AveBaseType.DocumentLibrary))) || Ave2010ListFlags.HasExternalDataSource(flags))
            {
                return false;
            }
            var listTemplates = parentWeb.ListTemplates;
            foreach (var template in listTemplates)
            {
                if (template.Type == baseTemplate)
                {
                    return template.AllowsFolderCreation;
                }
            }
            return true;

        }

        #endregion

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
