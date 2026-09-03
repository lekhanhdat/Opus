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
using Microsoft.SharePoint.Client.UserProfiles;
using AvePoint.GCommon;
using Microsoft.SharePoint.Client;
using System.Collections;
using Microsoft.Online.SharePoint.TenantAdministration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using AveClientRequest.Common;
using Microsoft.SharePoint.Client.Application;
using Microsoft.SharePoint.Client.Taxonomy;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Resource.Client;
using ClientFile = Microsoft.SharePoint.Client.File;
using System.IO;
using Microsoft.SharePoint.Client.Utilities;
using System.Web.Script.Serialization;
using Microsoft.SharePoint.Client.WorkflowServices;
using Microsoft.SharePoint.Client.Workflow;
using System.Xml;
using System.Globalization;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveClientOMOffice365Request : AveClientOM2016Request
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientOMOffice365Request));
        private bool? haveAddAndCustomizePagesPermission = null;
        private AveNintexAPIProcessor nintexAPIProcessor;
        public AveClientOMOffice365Request(string url, AveBPOSAccountInfo userAccountInfo, object obj, string serverVersion)
            : base(url, userAccountInfo, obj, serverVersion)
        {
            Type = AveClientRequestType.AveClientOMOffice365Request;
            nintexAPIProcessor = new AveNintexAPIProcessor(url, obj, Nintex.O365API.APIMethod.HTTP);
        }

        public override Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.CreateMigrationJob(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri);
                context.ExecuteQuery();
                return result.Value;
            }
        }
        protected override bool IsSpecialLibrary(AveClientContext context, string webUrl, Guid webId, Guid listId, out List list)
        {
            list = null;
            try
            {
                var web = webId == Guid.Empty ? context.Site.OpenWeb(webUrl) : context.Site.OpenWebById(webId);
                list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType, l => l.MajorWithMinorVersionsLimit);
                context.ExecuteQuery();
                if (list.BaseType == BaseType.DocumentLibrary && list.MajorWithMinorVersionsLimit > 0)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while confirm whether this list is special library. WebURl: {0}, WebId: {1} ListId: {2},  Error: {3}", webUrl, webId, listId, e);
            }
            return false;
        }
        public override Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.CreateMigrationJobEncrypted(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri, new EncryptionOption() { AES256CBCKey = options.AES256CBCKey });
                context.ExecuteQuery();
                return result.Value;
            }
        }
        public override Dictionary<string, object> GetItemVersionsForBrowser(string webServerRelativeUrl, string listId, int itemId, Dictionary<string, string> fields)
        {
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            using (ClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(new Guid(listId));
                var item = list.GetItemById(itemId);
                context.Load(item.Versions, version => version.Include(v => v.VersionLabel));
                context.ExecuteQuery();

                foreach (var version in item.Versions)
                {
                    var listItemVersionProperties = new Dictionary<string, object>();
                    listItemVersionProperties["VersionLabel"] = version.VersionLabel;
                    itemVersionPropertiesList.Add(listItemVersionProperties);
                }
                listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
                return listItemVersionsProperties;
            }
        }
        public override Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listId, int itemId, string itemUrl, Dictionary<string, string> needLoadFields)
        {
            var fileVersions = new Dictionary<int, FileVersion>();
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            using (ClientContext context = CreateContext())
            {
                var web = context.Site.OpenWeb(webRelativeUrl);
                var list = web.Lists.GetById(new Guid(listId));
                var item = list.GetItemById(itemId);
                context.Load(item, i => i.Versions, i => i.File.Exists);
                context.ExecuteQuery();
                var isFile = item.File.IsPropertyAvailable("Exists") && item.File.Exists;
                if (isFile)
                {
                    context.Load(item.File.Versions, fv => fv.Include(f => f.CheckInComment,
                        f => f.ID));
                    context.ExecuteQuery();
                    fileVersions = item.File.Versions.ToDictionary(fv => fv.ID, fv => fv);
                }
                if (item.Versions.Count <= 0)
                {
                    listItemVersionsProperties["HasVersion"] = false;
                }
                foreach (var version in item.Versions)
                {
                    Dictionary<string, object> fieldValues = new Dictionary<string, object>();
                    var listItemVersionProperties = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> fieldValue in version.FieldValues)
                    {
                        var value = fieldValue.Value;
                        // 
                        if (string.Equals(fieldValue.Key, "Created_x0020_Date", StringComparison.Ordinal))
                        {
                            value = DateTime.Parse(value.ToString(), null, DateTimeStyles.AdjustToUniversal);
                        }
                        AssembleItemProperties(fieldValues, value, fieldValue.Key);
                    }
                    #region set check in comment for documents
                    if (isFile)
                    {
                        if (version.VersionLabel.Equals(item.File.UIVersionLabel))
                        {
                            fieldValues["_CheckinComment"] = item.File.CheckInComment;
                        }
                        else if (fileVersions.ContainsKey(version.VersionId))
                        {
                            var fileVersion = fileVersions[version.VersionId];
                            fieldValues["_CheckinComment"] = fileVersion.CheckInComment;
                        }
                    }
                    #endregion
                    itemVersionPropertiesList.Add(GetNeedLoadFields(fieldValues, needLoadFields));
                }
                listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
                return listItemVersionsProperties;
            }
        }
        private static object lockObj = new object();
        private Dictionary<string, object> GetNeedLoadFields(Dictionary<string, object> fieldValues, Dictionary<string, string> needLoadFields)
        {
            // needLoadFields是list上的属性，在多线程的情况下，多个item多线程的时候会出现问题。参考CI-41498
            lock (lockObj)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                if (!needLoadFields.ContainsKey("Modified"))
                {
                    needLoadFields.Add("Modified", "DateTime");
                }
                if (!needLoadFields.ContainsKey("Editor"))
                {
                    needLoadFields.Add("Editor", "User");
                }
                var values = new Dictionary<string, object>();
                result.Add("FieldValues", values);
                Dictionary<string, string> KeyMapping = new Dictionary<string, string>();
                KeyMapping["_UIVersion"] = "VersionId";
                KeyMapping["_UIVersionString"] = "VersionLabel";
                //KeyMapping["ID"] = "VersionId";
                KeyMapping["_IsCurrentVersion"] = "IsCurrentVersion";
                KeyMapping["FileRef"] = "Url";
                KeyMapping["File_x0020_Size"] = "Length";
                KeyMapping["_ModerationStatus"] = "ModerationStatus";
                KeyMapping["Created_x0020_By"] = "CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix;
                KeyMapping["_Level"] = "Level";
                foreach (var needLoadField in needLoadFields)
                {
                    var columnNameKey = needLoadField.Key;
                    if (string.Equals(columnNameKey, "Created", StringComparison.Ordinal))
                    {
                        columnNameKey = "Created_x0020_Date";
                    }
                    if (fieldValues.ContainsKey(columnNameKey))
                    {
                        var value = fieldValues[columnNameKey];
                        if (value == null)
                        {
                            continue;
                        }
                        if (string.Equals(needLoadField.Key, "_Level", StringComparison.Ordinal))
                        {
                            try
                            {
                                value = Byte.Parse(value.ToString());
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Failed to convert Level value to byte, value: {0}, exception: {1}.", value.ToString(), ex);
                            }
                        }
                        //ADO-198874 On-premise 对于MultiChoice 获取的field  value是;#value;# 这种格式的，OnlineAPI 获取的是string[] 格式，为了保持和on-premise一致，此处需要修改
                        if (string.Equals(needLoadField.Value, "MultiChoice", StringComparison.OrdinalIgnoreCase) && value is string[])
                        {
                            string temp = ";#";
                            foreach (var tempVlaue in (string[])value)
                            {
                                temp += string.Format("{0};#", tempVlaue);
                            }
                            value = temp;
                        }
                        var mappedFieldName = KeyMapping.ContainsKey(needLoadField.Key) ? KeyMapping[needLoadField.Key] : needLoadField.Key;
                        result.Add(mappedFieldName, value);
                        values.Add(needLoadField.Key, value);
                    }
                }
                return result;
            }
        }
        public override Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            // 由于已经马上6.10 hard了，为了防止客户问题发现问题不好修改，提供配置文件控制是否使用以前web service的方式来获取column value。
            // 如果之后客户已经测试都没有问题，请删除这个控制，直接使用API方式。
            if (WrapperConfiguration.BPOS_S.BackupItemVersionByAPI)
            {
                return GetItemVersions(webRelativeUrl, listId, itemId, "", needLoadFields);
            }
            else
            {
                return mWebServiceRequest.GetItemVersions(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, cultureInfo, needLoadFields);
            }
        }
        protected override Dictionary<string, object> QueryItemVersionsForDiscover(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            if (WrapperConfiguration.BPOS_S.BackupItemVersionByAPI)
            {
                var fileVersions = new Dictionary<int, FileVersion>();
                Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
                List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
                using (ClientContext context = CreateContext())
                {
                    var web = context.Site.OpenWeb(webRelativeUrl);
                    var list = web.Lists.GetById(new Guid(listId));
                    var item = list.GetItemById(itemId);
                    context.Load(item, i => i.Versions.Include(
                    v => v["Modified"],
                    v => v["Editor"],
                    v => v["_Level"],
                    v => v["_UIVersion"]));
                    context.ExecuteQuery();
                    if (item.Versions.Count <= 0)
                    {
                        listItemVersionsProperties["HasVersion"] = false;
                    }
                    foreach (var version in item.Versions)
                    {
                        Dictionary<string, object> fieldValues = new Dictionary<string, object>();
                        var listItemVersionProperties = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, object> fieldValue in version.FieldValues)
                        {
                            var value = fieldValue.Value;
                            // 
                            if (string.Equals(fieldValue.Key, "Created_x0020_Date", StringComparison.Ordinal))
                            {
                                value = DateTime.Parse(value.ToString(), null, DateTimeStyles.AdjustToUniversal);
                            }
                            AssembleItemProperties(fieldValues, value, fieldValue.Key);
                        }
                        itemVersionPropertiesList.Add(GetNeedLoadFields(fieldValues, needLoadFields));
                    }
                    listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
                    return listItemVersionsProperties;
                }
            }
            else
            {
                return mWebServiceRequest.GetItemVersions(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, cultureInfo, needLoadFields);
            }
        }
        public override AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.GetMigrationJobStatus(id);
                context.ExecuteQuery();
                return (AveMigrationJobState)result.Value;
            }
        }

        public override AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers()
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.ProvisionMigrationContainers();
                context.ExecuteQuery();
                var info = (ProvisionedMigrationContainersInfo)result.Value;
                return new AveProvisionedMigrationContainersInfo()
                {
                    DataContainerUri = info.DataContainerUri,
                    EncryptionKey = info.EncryptionKey,
                    MetadataContainerUri = info.MetadataContainerUri,
                    TypeId = info.TypeId
                };
            }
        }

        public override AveProvisionedMigrationQueueInfo ProvisionMigrationQueue()
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.ProvisionMigrationQueue();
                context.ExecuteQuery();
                var info = (ProvisionedMigrationQueueInfo)result.Value;
                return new AveProvisionedMigrationQueueInfo()
                {
                    JobQueueUri = info.JobQueueUri,
                    TypeId = info.TypeId
                };
            }
        }

        public override bool DeleteMigrationJob(Guid id)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = context.Site.DeleteMigrationJob(id);
                context.ExecuteQuery();
                return result.Value;
            }
        }

        public override void MoveTo(string parentWebUrl, string parentWebServerRelativeUrl, string folderServerRelativeUrl, string newUrl)
        {
            using (var context = CreateContext(parentWebUrl))
            {
                var folderPath = ResourcePath.FromDecodedUrl(folderServerRelativeUrl);
                var folder = context.Web.GetFolderByServerRelativePath(folderPath);
                folder.MoveToUsingPath(ResourcePath.FromDecodedUrl(newUrl));
                context.ExecuteQuery();
            }
        }

        public override AveRequestAudit GetAuditValues()
        {
            try
            {
                using (var context = CreateContext(mWebUrl))
                {
                    context.Load(context.Site, site => site.AuditLogTrimmingRetention, site => site.TrimAuditLog);
                    context.Load(context.Site.Audit, audit => audit.AuditFlags);
                    context.ExecuteQuery();

                    return new AveRequestAudit()
                    {
                        AuditFlags = (AveAuditMaskType)context.Site.Audit.AuditFlags,
                        AuditLogTrimmingRetention = context.Site.AuditLogTrimmingRetention,
                        TrimAuditLog = context.Site.TrimAuditLog,
                    };
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get site audit flags failed. Message:{0}", ex);
            }
            return new AveRequestAudit();
        }

        public override Dictionary<string, object> GetAdminCenterSite()
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                try
                {
                    context.Load(context.Site);
                    context.Load(context.Site.RootWeb);
                    context.ExecuteQuery();
                    CopyProperty(siteProperties, context.Site);

                    mCompatibilityLevel = context.Site.CompatibilityLevel;
                    if (mCompatibilityLevel == 15)
                    {
                        mRequestCommon = new AveHttpWebRequestCommon2013(mWebUrl, mObj, mServerVersion);
                    }
                    else
                    {
                        mRequestCommon = new AveHttpWebRequestCommon2010(mWebUrl, mObj, mServerVersion);
                    }
                    Dictionary<string, object> rootWebProperties = new Dictionary<string, object>();
                    CopyProperty(rootWebProperties, context.Site.RootWeb);
                    rootWebProperties["IsRootWeb"] = true;
                    siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                    mSiteRelativeUrl = context.Site.ServerRelativeUrl;
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetSiteError, context.Url, e);
                    throw;
                }
                return siteProperties;
            }
        }

        /// <summary>
        /// Only get wanted property for browser
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> GetBrowserSiteInfo()
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                try
                {
                    context.Load(context.Site, site => site.Id, site => site.ReadOnly, site => site.CompatibilityLevel);
                    context.Load(context.Site.RootWeb, web => web.WebTemplate, web => web.Configuration, web => web.Language);
                    context.ExecuteQuery();
                    CopyProperty(siteProperties, context.Site);
                    Dictionary<string, object> rootWebProperties = new Dictionary<string, object>();
                    CopyProperty(rootWebProperties, context.Site.RootWeb);
                    rootWebProperties["IsRootWeb"] = true;
                    siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                }
                catch (Exception e)
                {
                    mLogger.Debug("An error occurred while get browser site info, url: {0}, error: {1}", context.Url, e);
                    throw;
                }
                return siteProperties;
            }
        }

        public override AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(option.ParentWebId);
                List list = web.Lists.GetById(option.ParentListId);
                Folder folder = list.RootFolder;
                context.Load(context.Site, s => s.Url);
                context.Load(folder, f => f.UniqueId, f => f.ServerRelativeUrl, f => f.Name);
                context.Load(folder.ListItemAllFields, item => item.HasUniqueRoleAssignments);
                context.ExecuteQuery();
                return new AveFolderBrowserInfo
                {
                    HasUniqueRoleAssignments = folder.ListItemAllFields.IsPropertyAvailable("HasUniqueRoleAssignments") ? folder.ListItemAllFields.HasUniqueRoleAssignments : false,
                    UniqueId = folder.UniqueId,
                    Name = folder.Name,
                    ParentId = option.ParentListId,
                    Url = new Uri(new Uri(context.Site.Url), folder.ServerRelativeUrl).ToString(),
                    ServerRelativeUrl = folder.ServerRelativeUrl,
                };
            }
        }

        private string GetPageInfo(AveBrowserOption option)
        {
            if (option.StartIndex == 0 || string.IsNullOrEmpty(option.PageInfo))
            {
                return string.Empty;
            }
            var data = option.PageInfo.Trim(',').Split(',');
            var index = option.StartIndex / 10;
            return string.Format("Paged=TRUE&p_ID={0}", data[index]);
        }

        protected List<Folder> QueryFoldersForLargeListV5(ClientContext context, List list, string folderUrl)
        {
            List<Folder> folders = new List<Folder>();
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, null);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item.Folder.ListItemAllFields.HasUniqueRoleAssignments,
                        item => item.Folder.ListItemAllFields.Id,
                        item => item.Folder.UniqueId,
                        item => item.Folder.Name,
                        item => item.Folder.ServerRelativeUrl));
            };
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                folders.Add(itemArg.Folder);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                folders.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin discover folders in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, folderUrl);
            worker.Run();
            context.ExecuteQuery();
            mLogger.Debug("Finish discover folders in large list, {0} folders in folder {1}", folders.Count, folderUrl);
            return folders;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is property name")]
        public override List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            int childrenCount = 0;
            string Ids = string.Empty;
            List<AveFolderBrowserInfo> folders = new List<AveFolderBrowserInfo>();
            var queryItem = new CamlQuery
            {
                ViewXml = string.Format("<View Scope=\"\"><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">1</Value></Eq></Where></Query><RowLimit>{0}</RowLimit></View>", option.PerPage),
                FolderServerRelativePath = ResourcePath.FromDecodedUrl(option.ParentFolderServerRelativeUrl),
                ListItemCollectionPosition = new ListItemCollectionPosition { PagingInfo = GetPageInfo(option) },
            };

            var queryCount = new CamlQuery()
            {
                ViewXml = "<View><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">1</Value></Eq></Where></Query></View>",
                FolderServerRelativePath = ResourcePath.FromDecodedUrl(option.ParentFolderServerRelativeUrl),
            };

            using (AveClientContext context = CreateContext())
            {
                List<Folder> tempFolders = null;
                Web web = context.Site.OpenWebById(option.ParentWebId);
                List list = web.Lists.GetById(option.ParentListId);
                context.Load(list,l=>l.BaseType,l=>l.ItemCount);
                context.Load(list.RootFolder, folder => folder.ServerRelativeUrl);
                context.ExecuteQuery();
                if (IsThrottled(list.ItemCount))
                {
                    childrenCount = QuerySubFoldersCountForLargeList(context, list, option.ParentFolderServerRelativeUrl, queryCount, ref Ids);
                    tempFolders = QueryFoldersForLargeList(context, list, option.ParentFolderServerRelativeUrl, queryItem);
                    option.PageInfo = Ids;
                }
                else
                {
                    
                    var itemCount = list.GetItems(queryCount);
                    var items = list.GetItems(queryItem);
                    context.Load(itemCount, count => count.Include(i => i.Id));
                    LoadBrowserFolderProperty(context, items);
                    context.ExecuteQuery();

                    tempFolders = items.Select(item => item.Folder).ToList();
                    childrenCount = itemCount.Count;
                    //每10个item 记录一次Id,对应browser界面 一页10个item,用于分页逻辑
                    for (int i = 1; i <= itemCount.Count; i++)
                    {
                        if (i % 10 == 0)
                        {
                            Ids = string.Format("{0},{1}", Ids, itemCount[i - 1].Id);
                        }
                    }
                }

                foreach (var temp in tempFolders)
                {
                    folders.Add(new AveFolderBrowserInfo
                    {
                        UniqueId = temp.UniqueId,
                        Name = temp.Name,
                        ServerRelativeUrl = temp.ServerRelativeUrl,
                        Url = new Uri(new Uri(this.mWebUrl), temp.ServerRelativeUrl).ToString(),
                        ParentListId = option.ParentListId,
                        ParentId = option.ParentFolderId,
                        Hidden = temp.ListItemAllFields.IsPropertyAvailable("Id"),
                        HasUniqueRoleAssignments = temp.ListItemAllFields.IsPropertyAvailable("HasUniqueRoleAssignments") ? temp.ListItemAllFields.HasUniqueRoleAssignments : false,

                    });
                }
                option.ChildrenTotalCount = childrenCount;
                option.PageInfo = Ids;
                return folders;
            }
            return folders;
        }

        private void LoadBrowserFolderProperty(ClientContext context, ListItemCollection listItemsArg)
        {
            context.Load(listItemsArg, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item.Folder.ListItemAllFields.HasUniqueRoleAssignments,
                        item => item.Folder.ListItemAllFields.Id,
                        item => item.Folder.UniqueId,
                        item => item.Folder.Name,
                        item => item.Folder.ServerRelativeUrl));
        }

        private List<Folder> QueryFoldersForLargeList(ClientContext context, List list, string folderUrl, CamlQuery query)
        {
            List<Folder> folders = new List<Folder>();
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, query);
            worker.BeforeQueryAction += LoadBrowserFolderProperty;
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                folders.Add(itemArg.Folder);
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                folders.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin browse folders in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, folderUrl);
            worker.Run();
            context.ExecuteQuery();
            mLogger.Debug("Finish browse folders in large list, {0} folders in folder {1}", folders.Count, folderUrl);
            return folders;
        }

        protected int QuerySubFoldersCountForLargeList(ClientContext context, List list, string folderUrl, CamlQuery query, ref string pageInfo)
        {
            int itemCount = 0;
            string Ids = string.Empty;
            var worker = new LargeListQueryWorker(context, list, folderUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, query);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, items => items.ListItemCollectionPosition, items => items.Include(i => i.Id));
            };
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                //每10个item 记录一次Id,对应browser界面 一页10个item
                itemCount++;
                if (itemCount % 10 == 0)
                {
                    Ids = string.Format("{0},{1}", Ids, itemArg.Id);
                }
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                itemCount = 0;
                Ids = string.Empty;
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            worker.Run();
            context.ExecuteQuery();
            pageInfo = Ids;
            return itemCount;
        }
        protected override void CreateUserProfile(string userName)
        {
            try
            {
                //经研究发现如果用site 的url创建context是无法创建出user profile的，必须使用CA的url来创建context，才能创建出user profile
                using (var context = CreateContext(AveUrlUtility.GetTenantAdminSiteUrl(mWebUrl)))
                {
                    var loader = ProfileLoader.GetProfileLoader(context);
                    loader.CreatePersonalSiteEnqueueBulk(new string[] { userName });
                    loader.Context.ExecuteQuery();
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occurred while create user profile. User name: {0}, error: {1}", userName, e.ToString());
            }
        }

        public override Dictionary<string, object> CreatePersonalSiteEnqueueBulk(string[] emailIDs, string loginName)
        {
            Dictionary<string, object> newPersonalSiteProperty = new Dictionary<string, object>();
            DateTime endTime = DateTime.Now.AddMinutes(30);  //设置时间为30分钟，如果超出时间则停止等待。
            try
            {
                using (AveClientContext context = CreateContext())
                {
                    ProfileLoader profileLoader = ProfileLoader.GetProfileLoader(context);
                    PeopleManager peopleManager = new PeopleManager(context);
                    ClientResult<string> result = null;
                    profileLoader.CreatePersonalSiteEnqueueBulk(emailIDs);
                    context.ExecuteQuery();
                    if (!string.IsNullOrEmpty(loginName))
                    {
                        do
                        {
                            System.Threading.Thread.Sleep(10000);
                            if (DateTime.Now > endTime)
                            {
                                throw new Exception("Create Site Collection timeout.");
                            }
                            result = peopleManager.GetUserProfilePropertyFor(loginName, "SPS-PersonalSiteInstantiationState");
                            context.ExecuteQuery();
                        } while (!result.Value.Equals(((int)PersonalSiteInstantiationState.Created).ToString()));
                    }
                    var userProfileProperties = peopleManager.GetPropertiesFor(loginName);
                    context.Load(userProfileProperties, property => property.PersonalUrl);
                    context.ExecuteQuery();
                    newPersonalSiteProperty["PersonalUrl"] = userProfileProperties.PersonalUrl;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to create Personal Site,  error message : {0}", e.ToString());
                newPersonalSiteProperty["ErrorMessage"] = e is ServerException ? "ServerException" + e.Message : e.Message; ;
            }
            return newPersonalSiteProperty;
        }

        public override bool HaveAddAndCustomizePagesPermission
        {
            get
            {
                if (haveAddAndCustomizePagesPermission.HasValue)
                {
                    return haveAddAndCustomizePagesPermission.Value;
                }
                using (ClientContext context = CreateContext())
                {
                    haveAddAndCustomizePagesPermission = DoesUserHavePermissions(context, PermissionKind.AddAndCustomizePages);
                }
                return haveAddAndCustomizePagesPermission.Value;
            }
        }

        private bool DoesUserHavePermissions(ClientContext context, PermissionKind permissionKind)
        {
            var permissions = new BasePermissions();
            permissions.Set(permissionKind);
            var result = context.Web.DoesUserHavePermissions(permissions);
            context.ExecuteQuery();
            return result.Value;
        }

        /// <summary>
        /// Online 支持通过API的方式来SetVersion Setting，
        /// 因此不再需要通过HttpRequest的方式更新VersionSetting，
        /// 对于Online 该Method 空跑
        /// </summary>
        /// <param name="versionLimitedProperties"></param>
        /// <param name="listProperties"></param>
        protected override void SetVersionSetting(Dictionary<string, object> versionLimitedProperties, Dictionary<string, object> listProperties)
        {
            object count;
            if (listProperties.TryGetValue("MajorVersionLimit", out count)) // O365 change this setting default value, it can not be set to 0; it must be between 1 and 50000
            {
                var versionLimitCount = (int)count;
                listProperties["MajorVersionLimit"] = versionLimitCount == 0 ? 50000 : versionLimitCount;
            }
        }

        /// <summary>
        /// Online API 支持SiteLogoUrl与SiteLogoDescription的set与get，因此当前只有Name属性需要执行HttpRequest 
        /// </summary>
        /// <param name="webProperties"></param>
        /// <returns></returns>
        protected override bool NeedUpdateWebLogo(Dictionary<string, object> webProperties)
        {
            return webProperties.ContainsKey("Name");
        }

        public override Dictionary<string, object> GetFileById(string webServerRelativeUrl, Guid fileId)
        {
            Dictionary<string, object> fileProperties = new Dictionary<string, object>();
            using (var context = CreateContext(mWebUrl))
            {
                bool fileExists = false;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var file = web.GetFileById(fileId);
                ConditionalScope fileExistScope = new ConditionalScope(context, () => file.Exists);
                using (fileExistScope.StartScope())
                {
                    using (fileExistScope.StartIfTrue())
                    {
                        SafeLoadFile(context, file);
                    }
                }
                try
                {
                    context.ExecuteQuery();
                    fileProperties["Exists"] = fileExistScope.TestResult.HasValue && fileExistScope.TestResult.Value;
                    fileExists = Convert.ToBoolean(fileProperties["Exists"]);
                }
                catch (Exception ex)
                {
                    mLogger.Debug("An error occurred while getting file.Message:{0}.", ex);
                    fileProperties["Exists"] = false;
                    fileExists = false;
                }
                if (fileExists)
                {
                    AssembleFileProperties(fileProperties, file, webServerRelativeUrl, file.ListItemAllFields);
                }
            }
            return fileProperties;
        }

        public override Dictionary<string, object> GetFolderById(string webServerRelativeUrl, Guid folderId)
        {
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            using (var context = CreateContext(mWebUrl))
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Folder fodler = web.GetFolderById(folderId);
                LoadFolderProperties(folderProperties, context, fodler, webServerRelativeUrl, folderId);
                return folderProperties;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".onmicrosoft.com should be ignored")]
        public override Dictionary<string, object> GetSiteStorageInfo()
        {
            Dictionary<string, object> storageProperties;
            try
            {
                string adminSiteUrl = mUserAccountInfo.UserName.Contains(".onmicrosoft.com") ? AveUrlUtility.GetTenantAdminSiteUrl(mWebUrl) : null;
                storageProperties = GetSiteStorageInfo(adminSiteUrl, mWebUrl);
            }
            catch (Exception e)
            {
                storageProperties = new Dictionary<string, object>();
                mLogger.Warn("An error ocurred while getting storage info.Account info:{0},WebUrl:{1},Error:{2}", mUserAccountInfo, mWebUrl, e);
            }
            return storageProperties;
        }

        public Dictionary<string, object> GetSiteStorageInfo(string adminSiteUrl, string siteUrl)
        {
            Dictionary<string, object> storageProperties = new Dictionary<string, object>();
            try
            {
                if (!string.IsNullOrEmpty(adminSiteUrl))
                {
                    using (ClientContext context = new ClientContext(adminSiteUrl))
                    {
                        SPOnlineAuthentication auth = new SPOnlineAuthentication(adminSiteUrl);
                        CookieContainer cookie = auth.Login(mUserAccountInfo.UserName, mUserAccountInfo.Password);
                        context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>((object sender, WebRequestEventArgs e) => { e.WebRequestExecutor.WebRequest.CookieContainer = cookie; });
                        Tenant tenant = new Tenant(context);
                        SiteProperties properties = GetSiteProperties(context, tenant, siteUrl, true);
                        AveObjectCopy.GetObjectBasicProperties(storageProperties, properties);
                        ConvertUnit(storageProperties);
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("An error occurred while getting storage info.AdminSiteUrl:{0},SiteUrl:{1}, Error:{2}", adminSiteUrl, siteUrl, e);
            }
            return storageProperties;
        }
        public override Dictionary<string, object> GetManagedSitecollectionData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                context.Load(tenant);
                context.ExecuteQuery();

                AveObjectCopy.GetObjectBasicProperties(data, tenant);

                //SP Online 上available的值是storage减去所有站点已使用的值
                //在这里获取所有站点已使用的值
                int startIndex = 0;
                long storageUsage = 0;
                while (startIndex != -1)
                {
                    var sitesProperties = tenant.GetSiteProperties(startIndex, true);
                    context.Load(sitesProperties, p => p.IncludeWithDefaultProperties(s => s.StorageUsage), p => p.NextStartIndex);
                    context.ExecuteQuery();
                    foreach (var siteProperties in sitesProperties)
                    {
                        storageUsage += siteProperties.StorageUsage;
                    }
                    startIndex = sitesProperties.NextStartIndex;
                }
                data["StorageUsage"] = storageUsage;
                if (mRequestCommon != null)
                {
                    mRequestCommon.GetManagedSiteCollectionData(data, mWebUrl, tenant.StorageQuota - tenant.StorageQuotaAllocated, tenant.ResourceQuota - tenant.ResourceQuotaAllocated);
                }
            }
            return data;
        }

        public override List<Dictionary<string, object>> GetGroupSiteCollectionsList(string tenantAdminSiteUrl)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter
            {
                IncludeDetail = true,
                Template = "GROUP#0",
                IncludePersonalSite = PersonalSiteFilter.Exclude,
            };
            return GetSiteCollectionsList(tenantAdminSiteUrl, speFilter);
        }
        public override List<Dictionary<string, object>> GetOneDriveSiteCollectionsList(string tenantAdminSiteUrl)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter
            {
                IncludeDetail = true,
                Template = "SPSPERS#10",
                IncludePersonalSite = PersonalSiteFilter.Include,
            };
            return GetSiteCollectionsList(tenantAdminSiteUrl, speFilter);
        }
        public override List<Dictionary<string, object>> GetAllSiteCollectionsList(string tenantAdminSiteUrl, bool inlcudeOneDriveSite, List<string> excludeTempaltes)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter
            {
                IncludeDetail = true,
                IncludePersonalSite = inlcudeOneDriveSite ? PersonalSiteFilter.Include : PersonalSiteFilter.Exclude
            };
            var collection = GetSiteCollectionsList(tenantAdminSiteUrl, speFilter);
            if (collection != null && excludeTempaltes != null)
            {
                return collection.Where(
                    element => excludeTempaltes.FirstOrDefault(
                        tempalte => tempalte.Equals(element["WebTemplateName"].ToString(), StringComparison.OrdinalIgnoreCase)) == null)
                    .ToList();
            }
            return collection;
        }

        private List<Dictionary<string, object>> GetSiteCollectionsList(string tenantAdminSiteUrl, SPOSitePropertiesEnumerableFilter filter)
        {
            try
            {
                using (AveClientContext context = InitClientObject(tenantAdminSiteUrl, mUserAccountInfo, mObj))     //mObj should be the cookieContainer we get from tenant admin site
                {
                    Tenant tenant = new Tenant(context);
                    SPOSitePropertiesEnumerable sitePropertyEnum = null;
                    List<Dictionary<string, object>> managedSiteCollections = new List<Dictionary<string, object>>();
                    string tempIndex = null;
                    do
                    {
                        filter.StartIndex = tempIndex;
                        sitePropertyEnum = tenant.GetSitePropertiesFromSharePointByFilters(filter);
                        context.Load(sitePropertyEnum);
                        context.ExecuteQuery();
                        foreach (SiteProperties siteProperty in sitePropertyEnum)
                        {
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            CopyProperty(properties, siteProperty);
                            properties.Add("SiteCollectionUrl", siteProperty.Url.TrimEnd('/'));
                            properties.Add("WebTemplateName", siteProperty.Template);
                            //properties.Add("CompatibilityLevel", siteProperty.CompatibilityLevel);
                            //properties.Add("Lcid", siteProperty.Lcid);
                            //properties.Add("LockState", siteProperty.LockState);

                            managedSiteCollections.Add(properties);
                        }
                        tempIndex = sitePropertyEnum.NextStartIndexFromSharePoint;
                    }
                    while (tempIndex != null);

                    return managedSiteCollections;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to load site collections, admin site collection url : {0}, error information : {1}", tenantAdminSiteUrl, e.ToString());
                return null;
            }
        }

        public override List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl)
        {
            try
            {
                using (AveClientContext context = InitClientObject(tenantAdminSiteUrl, mUserAccountInfo, mObj))     //mObj should be the cookieContainer we get from tenant admin site
                {
                    Tenant tenant = new Tenant(context);
                    SPOSitePropertiesEnumerable sitePropertyEnum = null;
                    List<Dictionary<string, object>> managedSiteCollections = new List<Dictionary<string, object>>();
                    int tempIndex = 0;
                    do
                    {
                        sitePropertyEnum = tenant.GetSiteProperties(tempIndex, true);
                        context.Load(sitePropertyEnum);
                        context.ExecuteQuery();
                        foreach (SiteProperties siteProperty in sitePropertyEnum)
                        {
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add("SiteCollectionUrl", siteProperty.Url.TrimEnd('/'));
                            properties.Add("CompatibilityLevel", siteProperty.CompatibilityLevel);
                            properties.Add("WebTemplateName", siteProperty.Template);
                            properties.Add("Lcid", siteProperty.Lcid);
                            managedSiteCollections.Add(properties);
                        }
                        tempIndex += sitePropertyEnum.Count;

                    }
                    while (sitePropertyEnum != null && sitePropertyEnum.Count >= 300);
                    return managedSiteCollections;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to load site collections, admin site collection url : {0}, error information : {1}", tenantAdminSiteUrl, e.ToString());
                return null;
            }
        }
        public override bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "")
        {
            try
            {
                string adminSiteUrl = string.IsNullOrEmpty(tenantAdminSiteUrl) ? AveUrlUtility.GetTenantAdminSiteUrl(siteCollectionUrl) : tenantAdminSiteUrl;
                using (AveClientContext context = InitClientObject(adminSiteUrl, mUserAccountInfo, mObj))     //mObj should be the cookieContainer we get from tenant admin site
                {
                    Tenant tenant = new Tenant(context);
                    tenant.SetSiteAdmin(siteCollectionUrl, username, true);
                    context.ExecuteQuery();
                    return true;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to add user to site collection administrators, site collection url : {0}, username : {1}, error message : {2}", siteCollectionUrl, username, e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 创建模拟site。
        /// </summary>
        /// <param name="CAUrl"></param>
        /// <param name="compatibilityLevel"></param>
        /// <param name="lcid"></param>
        /// <param name="owner"></param>
        /// <param name="storageQuota"></param>
        /// <param name="template"></param>
        /// <param name="timeZoneId"></param>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <param name="resourceQuota"></param>
        /// <returns></returns>
        private string AddSimulationSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            mWebServiceRequest.AddSite(CAUrl, compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
            return string.Empty;
        }

        public override void ApplySiteDesign(string webUrl, Guid siteDesignId)
        {
            using (AveClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                var result = tenant.ApplySiteDesign(webUrl, siteDesignId);
                context.ExecuteQuery();
            }
        }

        public override string AddSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            try
            {
                if (!string.IsNullOrEmpty(CAUrl))
                {
                    return AddSimulationSite(CAUrl, compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
                }
                using (AveClientContext context = CreateContext())
                {
                    //ADO-185210。 Office 365,需要用BLANKINTERNETCONTAINER#0创建Publishing Portal站点。
                    if (string.Equals(template, "BLANKINTERNET#0", StringComparison.OrdinalIgnoreCase))
                    {
                        mLogger.Debug("Change web template from {0} to {1}.", template, "BLANKINTERNETCONTAINER#0");
                        template = "BLANKINTERNETCONTAINER#0";
                    }
                    Tenant tenant = new Tenant(context);
                    SpoOperation ope = tenant.CreateSite(
                        new SiteCreationProperties()
                        {
                            CompatibilityLevel = compatibilityLevel,
                            Lcid = lcid,
                            Owner = owner,
                            Template = template,
                            TimeZoneId = timeZoneId,
                            Title = title,
                            Url = url,
                            StorageMaximumLevel = storageQuota,
                            UserCodeMaximumLevel = resourceQuota,
                            UserCodeWarningLevel = Math.Floor(resourceQuota * 0.85),
                            StorageWarningLevel = (long)Math.Floor(storageQuota * 0.85)
                        });
                    context.Load(ope);
                    context.ExecuteQuery();
                    if (!ope.IsComplete)
                    {
                        SiteProperties siteProperties = null;
                        bool errorOccurred = false;
                        do
                        {
                            errorOccurred = false;
                            try
                            {
                                System.Threading.Thread.Sleep(10000);
                                siteProperties = tenant.GetSitePropertiesByUrl(System.Web.HttpUtility.UrlPathEncode(url), false);
                                context.Load(siteProperties);
                                context.ExecuteQuery();
                                mLogger.Debug("Site Collection Status:{0}", siteProperties.Status);
                            }
                            catch (Exception e)
                            {
                                string message = e.Message;
                                mLogger.Warn("An error occurred while getting site properties. Error:{0}", e);
                                errorOccurred = true;
                            }
                        }
                        while (errorOccurred || (siteProperties != null && siteProperties.Status.Equals("Creating", StringComparison.OrdinalIgnoreCase)));
                    }
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to create site collection, url : {0}, error message : {1}", url, e.ToString());
                return e is ServerException ? "ServerException: " + e.Message : e.Message;
            }
        }

        public override void DeleteSiteToRecylebin(string CAUrl, string url)
        {
            DeleteSiteCore(CAUrl, url, true);
        }

        public override void DeleteSite(string CAUrl, string url)
        {
            DeleteSiteCore(CAUrl, url, false);
        }

        private void DeleteSiteCore(string CAUrl, string url, bool deleteToRecybleBin)
        {
            if (!string.IsNullOrEmpty(CAUrl))
            {
                mWebServiceRequest.DeleteSite(CAUrl, url);
                return;
            }
            string adminUrl = AveUrlUtility.GetTenantAdminSiteUrl(url);
            using (AveClientContext context = CreateContext(adminUrl))
            {
                Tenant tenant = new Tenant(context);
                tenant.RemoveSite(url);
                context.ExecuteQuery();
                //Delete Site from recycle bin.
                DeletedSiteProperties siteProperties = null;
                do
                {
                    System.Threading.Thread.Sleep(10000);
                    try
                    {
                        siteProperties = tenant.GetDeletedSitePropertiesByUrl(url);
                        context.Load(siteProperties);
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("The Site {0} is deleting.Message:{1}", url, e);
                    }
                }
                while (!siteProperties.IsPropertyAvailable("Status") && !string.IsNullOrEmpty(siteProperties.Status) &&
                       siteProperties.Status.Equals("Recycling", StringComparison.OrdinalIgnoreCase));
                if (!deleteToRecybleBin)
                {
                    tenant.RemoveDeletedSite(url);
                    context.ExecuteQuery();
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "user name suffix")]
        public override SiteStatus GetSiteStatus(string siteUrl, Func<AveBPOSAccountInfo, string, string> GetAdminUrl)
        {
            string adminSiteUrl = GetAdminUrl(mUserAccountInfo, siteUrl);
            Dictionary<string, object> storageProperties = new Dictionary<string, object>();
            SiteStatus status = SiteStatus.Normal;
            SPOnlineAuthentication auth = new SPOnlineAuthentication(adminSiteUrl);
            CookieContainer cookie = auth.Login(mUserAccountInfo.UserName, mUserAccountInfo.Password);

            using (var context = new ClientContext(adminSiteUrl))
            {
                context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>((object sender, WebRequestEventArgs e) => { e.WebRequestExecutor.WebRequest.CookieContainer = cookie; });
                Tenant tenant = new Tenant(context);
                try
                {
                    SiteProperties properties = tenant.GetSitePropertiesByUrl(siteUrl, true);
                    context.Load(properties);
                    context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    status = SiteStatus.InRecycleBin;
                    mLogger.Debug("Site:'{0}' do not exist. Message:{1}", siteUrl, ex.ToString());
                    try
                    {
                        var deleteSiteProperties = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
                        context.Load(deleteSiteProperties);
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug("Site:'{0}' do not in recycle bin. Message:{1}", siteUrl, e.ToString());
                        status = SiteStatus.Deleted;
                    }
                }
            }
            return status;
        }

        protected override void AssembleAppsProperties(string webServerRelativeUrl, ClientObjectList<AppInstance> apps, List<Dictionary<string, object>> appPropertyList)
        {
            if (apps.Count > 0)
            {
                List<Dictionary<string, object>> appsMetadata = GetInstalledApps(webServerRelativeUrl);

                foreach (AppInstance app in apps)
                {
                    Dictionary<string, object> appInstanceProperties = new Dictionary<string, object>();
                    CopyProperty(appInstanceProperties, app);
                    if (!string.IsNullOrEmpty(app.AppWebFullUrl))
                    {
                        appInstanceProperties["AppWebFullUrl"] = new Uri(app.AppWebFullUrl);
                    }
                    Dictionary<string, object> appMetadata = GetAppPropertiesById(appsMetadata, app.Id);
                    if (appMetadata == null)
                    {
                        mLogger.Debug(string.Format("Can not find app in the AppCatalog with Id:{0}.", app.Id));
                        var appProperties = new Dictionary<string, object>();
                        appProperties["ProductId"] = app.ProductId;
                        appProperties["Source"] = AveAppSource.InvalidSource;
                        appInstanceProperties["App"] = appProperties;
                    }
                    else
                    {

                        appInstanceProperties["App"] = AssembleAppProperties(appMetadata);
                    }
                    appPropertyList.Add(appInstanceProperties);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tenant"></param>
        /// <param name="siteUrl"></param>
        /// <param name="includeDetail"></param>
        /// <returns></returns>
        private SiteProperties GetSiteProperties(ClientContext context, Tenant tenant, string siteUrl, bool includeDetail)
        {
            SiteProperties properties = tenant.GetSitePropertiesByUrl(siteUrl, includeDetail);
            context.Load(properties);
            try
            {
                context.ExecuteQuery();
            }
            catch (ServerException e)
            {
                mLogger.Debug("An error occurred while getting single site properties {0},Error:{1}", siteUrl, e);
                //带有特殊字符的url，GetSitePropertiesByUrl会出错，用GetSiteProperties保证数据可以获取到
                int startIndex = 0;
                bool findSiteProperty = false;
                //tenant.GetSiteProperties默认会获取300个site collection的property，所以需要循环遍历
                while (startIndex >= 0)
                {
                    SPOSitePropertiesEnumerable allSitesProperties = tenant.GetSiteProperties(startIndex, includeDetail);
                    context.Load(allSitesProperties);
                    context.ExecuteQuery();
                    foreach (var siteProperties in allSitesProperties)
                    {
                        if (string.Equals(siteProperties.Url, siteUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            properties = siteProperties;
                            findSiteProperty = true;
                            break;
                        }
                    }
                    if (findSiteProperty)
                    {
                        break;
                    }
                    startIndex = allSitesProperties.NextStartIndex;
                }
                if (properties == null || !properties.IsPropertyAvailable("Url"))
                {
                    mLogger.Error("Failed to get site properties for {0}, throw the server exception: {1}.", siteUrl, e);
                    throw;
                }
            }
            return properties;
        }

        public override void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            using (AveClientContext context = CreateContext())
            {
                Site site = context.Site;
                Web web = site.RootWeb;
                context.Load(site, item => item.Url);
                context.ExecuteQuery();

                #region 上传solution
                string fileUrl = webServerRelativeUrl.TrimEnd('/') + "/_catalogs/solutions/" + solutionName;
                using (FileStream fileStream = new FileStream(solutionPath, FileMode.Open, FileAccess.Read))
                {
                    ClientFile.SaveBinaryDirect(context, fileUrl, fileStream, true);
                }
                var path = ResourcePath.FromDecodedUrl(fileUrl);
                ClientFile file = web.GetFileByServerRelativePath(path);
                context.Load(file.ListItemAllFields, item => item.Id);
                context.ExecuteQuery();
                #endregion

                #region 查找solution  激活solution
                using (AveWebServiceRequest aveWebServiceRequest = new AveWebServiceRequest(site.Url, mUserAccountInfo, mObj, "15"))
                {
                    aveWebServiceRequest.OperateSolution("ACT", mWebUrl, AveUrlUtility.GetServerRelativeUrl(mWebUrl), file.ListItemAllFields.Id);
                }
                var filepath = ResourcePath.FromDecodedUrl(fileUrl);
                file = web.GetFileByServerRelativePath(filepath);
                context.Load(file, f => f.ListItemAllFields);
                context.Load(site.Features, fs => fs.Include(f => f.DefinitionId));
                context.ExecuteQuery();
                Dictionary<string, object> solutionPropiesDir = file.ListItemAllFields.FieldValues;
                #endregion

                #region 激活solution  同时要激活对应的feature
                object status;
                if (solutionPropiesDir.TryGetValue("Status", out status) && status is FieldLookupValue && int.Parse((solutionPropiesDir["Status"] as FieldLookupValue).LookupValue) == 1)
                {
                    bool activeFeature = false;
                    Guid newActiveSolutionId = solutionPropiesDir.ContainsKey("SolutionId") ? (Guid)solutionPropiesDir["SolutionId"] : new Guid();
                    foreach (AveSolutionFeature feature in packageFeatures)
                    {
                        if (packageSolutionId == newActiveSolutionId && feature.Scope == AveFeatureScope.Site)
                        {
                            if (site.Features.Select(f => f.DefinitionId == feature.FeatureId) == null)
                            {
                                site.Features.Add(feature.FeatureId, false, FeatureDefinitionScope.Site);
                                activeFeature = true;
                            }
                        }
                    }
                    if (activeFeature)
                    {
                        context.ExecuteQuery();
                    }
                }
                #endregion;

                #region 应用激活solution生成的WebTemplate
                web.ApplyWebTemplate(webTemplateName);
                context.ExecuteQuery();
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">读取NinTexWorkflow文件的stream</param>
        /// <param name="publishName"></param>
        /// <param name="tenant"></param>
        /// <param name="siteServerRelativeUrl"></param>
        /// <param name="listName"></param>
        /// <param name="overWrite"></param>
        public override Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string webUrl, string listName, Guid parentListId)
        {
            var workflowId = nintexAPIProcessor.PublishNintexWorkflow(stream, publishName, webUrl, listName, parentListId);
            return new Guid(workflowId);
            //return mRequestCommon.PublishNintexWorkflow(stream, publishName, tenant, siteServerRelativeUrl, listName, overWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="workflowDefinitionId"></param>
        /// <returns></returns>
        public override Guid PublishNintexWorkflow(string webUrl, Guid workflowDefinitionId)
        {
            var workflowId = nintexAPIProcessor.PublishNintexWorkflow(webUrl, workflowDefinitionId);
            return new Guid(workflowId);
        }

        public override string ConvertNintexFormJsonObjectToXml(string webUrl, string formJsonData, string fileName)
        {
            return nintexAPIProcessor.ConvertNintexFormJsonObjectToXml(webUrl, formJsonData, fileName);
        }

        public override string ImportNintexWorkflow(System.IO.Stream stream, string publishName, string webUrl, string listTitle, Guid parentListId, bool migrate)
        {
            return nintexAPIProcessor.ImportNintexWorkflow(stream, publishName, webUrl, listTitle, parentListId, migrate);
        }

        public override void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId)
        {
            nintexAPIProcessor.SaveNintexForm(formXml, webUrl, listId, contentTypeId);
        }

        public override void PublishNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            nintexAPIProcessor.PublishNintexForm(webUrl, listId, contentTypeId);
        }
        public override Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            return nintexAPIProcessor.ExportNintexForm(webUrl, listId, contentTypeId);
        }


        public override Dictionary<string, object> GetSitePropertiesByUrl(string siteUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var siteProp = tenant.GetSitePropertiesByUrl(siteUrl, true);
                context.Load(siteProp);
                context.ExecuteQuery();
                Dictionary<string, object> sitePropDic = new Dictionary<string, object>();
                CopyProperty(sitePropDic, siteProp);
                return sitePropDic;
            };
        }

        public override void UpdateSiteBasicPropertiesByUrl(string siteUrl, Dictionary<string, object> siteProp)
        {
            using (AveClientContext context = CreateContext())
            {
                var tenant = new Tenant(context);
                var newSiteProp = tenant.GetSitePropertiesByUrl(siteUrl, true);
                AveObjectCopy.UpdateObjectBasicProperties(siteProp, newSiteProp);
                newSiteProp.Update();
                context.ExecuteQuery();
            };
        }

        public override int GetSiteCollectionsCount(string tenantAdminSiteUrl)
        {
            using (AveClientContext context = InitClientObject(tenantAdminSiteUrl, mUserAccountInfo, mObj))     //mObj should be the cookieContainer we get from tenant admin site
            {
                Tenant tenant = new Tenant(context);
                SPOSitePropertiesEnumerable sitePropertyEnum = tenant.GetSiteProperties(0, false);
                context.Load(sitePropertyEnum, sites => sites.Include(s => s.Status));
                context.ExecuteQuery();
                return sitePropertyEnum.Count;
            }
        }
        public override int GetOneDriveCount(List<string> usernames)
        {
            using (AveClientContext context = CreateContext())
            {
                int oneDriveCount = 0;
                try
                {
                    PeopleManager pm = new PeopleManager(context);
                    Dictionary<string, PersonProperties> props = new Dictionary<string, PersonProperties>();
                    int batchSize = 250;
                    foreach (string username in usernames)
                    {
                        PersonProperties prop = pm.GetPropertiesFor(string.Format("i:0#.f|membership|{0}", username));
                        context.Load(prop, p => p.PersonalUrl);
                        props.Add(username, prop);
                        if (props.Count >= batchSize && props.Count % batchSize == 0)
                        {
                            context.ExecuteQuery();
                        }
                    }
                    if (context.HasPendingRequest)
                    {
                        context.ExecuteQuery();
                    }

                    foreach (KeyValuePair<string, PersonProperties> prop in props)
                    {
                        Dictionary<string, object> oneDriveInfo = AssembleSkyDriveProProperties(prop.Value, prop.Key);
                        if (!string.IsNullOrEmpty(oneDriveInfo["PersonalSpace"].ToString()))
                        {
                            oneDriveCount++;
                        }
                    }
                    return oneDriveCount;
                }
                catch (Exception e)
                {
                    mLogger.Error("Get OneDrive count failed, error message : {0}", e);
                    throw;
                }
            }
        }

        private Dictionary<string, object> AssembleSkyDriveProProperties(PersonProperties prop, string username = null)
        {
            Dictionary<string, object> skyDriveProp = new Dictionary<string, object>();
            bool isUsernameExists = prop.ServerObjectIsNull.HasValue && prop.ServerObjectIsNull == false;
            skyDriveProp["Exists"] = isUsernameExists;
            skyDriveProp["PersonalUrl"] = isUsernameExists ? prop.PersonalUrl : string.Empty;
            if (isUsernameExists)
            {
                Uri personalUrl = new Uri(prop.PersonalUrl, UriKind.RelativeOrAbsolute);

                if ((personalUrl.IsAbsoluteUri
                    && !personalUrl.GetLeftPart(UriPartial.Path).EndsWith("Person.aspx", StringComparison.OrdinalIgnoreCase)
                    && !personalUrl.GetLeftPart(UriPartial.Path).EndsWith("PersonImmersive.aspx", StringComparison.OrdinalIgnoreCase)
                    ))
                {
                    skyDriveProp["PersonalSpace"] = prop.PersonalUrl;
                }
                else
                {
                    skyDriveProp["PersonalSpace"] = string.Empty;
                }
            }
            else
            {
                skyDriveProp["PersonalSpace"] = string.Empty;
            }
            skyDriveProp["UserName"] = username;
            skyDriveProp["Version"] = prop.Context.ServerLibraryVersion.ToString();
            return skyDriveProp;
        }

        public override void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota)
        {
            using (ClientContext context = CreateContext())
            {
                Tenant tenant = new Tenant(context);
                SiteProperties siteProperties = null;
                siteProperties = tenant.GetSitePropertiesByUrl(siteUrl, true);
                context.Load(siteProperties);
                context.ExecuteQuery();
                double rate = 0;
                if (!string.Equals(siteProperties.Template, "SPSMSITEHOST#0")) //for my site
                {
                    rate = siteProperties.StorageWarningLevel * 1.0 / siteProperties.StorageMaximumLevel * 1.0;
                    siteProperties.StorageWarningLevel = Convert.ToInt64(storageQuota * Math.Round(rate, 2));
                }
                siteProperties.StorageMaximumLevel = storageQuota;
                if (!string.Equals(siteProperties.Template, "SPSMSITEHOST#0"))
                {
                    rate = siteProperties.UserCodeMaximumLevel.Equals(0) ? 0.0 : siteProperties.UserCodeWarningLevel * 1.0 / siteProperties.UserCodeMaximumLevel * 1.0;
                    siteProperties.UserCodeWarningLevel = Convert.ToInt64(serverResourceQuota * Math.Round(rate, 2));
                }
                siteProperties.UserCodeMaximumLevel = serverResourceQuota;
                siteProperties.Update();
                context.ExecuteQuery();
            }
        }

        public override Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, string listName, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields, bool? listEnableMinorVersion)
        {
            try
            {
                string serverRelativeUrl = string.Empty;
                if (urlOfFile.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    urlOfFile.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile.Substring(WebAppName.Length);
                }
                else if (urlOfFile.StartsWith(folderServerRelativeUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    serverRelativeUrl = urlOfFile;
                }
                else
                {
                    serverRelativeUrl = folderServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                }
                using (AveClientContext context = CreateContext())
                {
                    Web parentWeb = context.Site.OpenWeb(webServerRelativeUrl);
                    var filepath = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                    ClientFile targetFile = parentWeb.GetFileByServerRelativePath(filepath);
                    ConditionalScope conditionScope = new ConditionalScope(context, () => targetFile.Exists, true);
                    using (conditionScope.StartScope())
                    {
                        using (conditionScope.StartIfTrue())
                        {
                            context.Load(targetFile);
                        }
                    }
                    context.ExecuteQuery();
                    bool exist = conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
                    bool needCheckin = false;
                    if (exist && listName != null && targetFile.CheckOutType == CheckOutType.None)
                    {
                        targetFile.CheckOut();
                        needCheckin = true;
                    }
                    //".master", ".evtx", ".cs"
                    string fileType = Path.GetExtension(serverRelativeUrl);
                    if (string.Equals(fileType, ".master", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(fileType, ".evtx", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(fileType, ".cs", StringComparison.OrdinalIgnoreCase)
                        || file.Length < WrapperConfiguration.BPOS_S.UploadLimit)
                    {
                        FileSaveBinaryInformation saveInfo = new FileSaveBinaryInformation();
                        saveInfo.ContentStream = file;
                        targetFile.SaveBinary(saveInfo);//if file is not exist, this function will create new, else update the file.
                    }
                    else
                    {
                        mLogger.Info("Upload file by slice");
                        ClientResult<long> bytesUploaded = null;

                        Guid uploadId = Guid.NewGuid();
                        using (BinaryReader br = new BinaryReader(file))
                        {
                            byte[] buffer = new byte[2 * 1024 * 1024];
                            Byte[] lastBuffer = null;
                            long fileoffset = 0;
                            long totalBytesRead = 0;
                            int bytesRead;
                            bool first = true;
                            bool last = false;

                            // Read data from filesystem in blocks 
                            while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalBytesRead = totalBytesRead + bytesRead;

                                // We've reached the end of the file
                                if (totalBytesRead == file.Length)
                                {
                                    last = true;
                                    // Copy to a new buffer that has the correct size
                                    lastBuffer = new byte[bytesRead];
                                    Array.Copy(buffer, 0, lastBuffer, 0, bytesRead);
                                }

                                if (first)
                                {
                                    using (MemoryStream contentStream = new MemoryStream())
                                    {
                                        // Add an empty file.
                                        FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
                                        fileAddParameters.Overwrite = true;
                                        var filePath = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                                        var folderPath = ResourcePath.FromDecodedUrl(folderServerRelativeUrl);
                                        targetFile = parentWeb.GetFolderByServerRelativePath(folderPath).Files.AddUsingPath(filePath, fileAddParameters, contentStream);

                                        // Start upload by uploading the first slice. 
                                        using (MemoryStream s = new MemoryStream(buffer))
                                        {
                                            // Call the start upload method on the first slice
                                            bytesUploaded = targetFile.StartUpload(uploadId, s);
                                            context.ExecuteQuery();
                                            // fileoffset is the pointer where the next slice will be added
                                            fileoffset = bytesUploaded.Value;
                                        }

                                        // we can only start the upload once
                                        first = false;
                                    }
                                }
                                else
                                {
                                    // Get a reference to our file
                                    var fileUrlPath = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                                    targetFile = parentWeb.GetFileByServerRelativePath(fileUrlPath);

                                    if (last)
                                    {
                                        // Is this the last slice of data?
                                        using (MemoryStream s = new MemoryStream(lastBuffer))
                                        {
                                            // End sliced upload by calling FinishUpload
                                            targetFile.FinishUpload(uploadId, fileoffset, s);
                                            context.ExecuteQuery();

                                            // return the file object for the uploaded file
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        using (MemoryStream s = new MemoryStream(buffer))
                                        {
                                            // Continue sliced upload
                                            bytesUploaded = targetFile.ContinueUpload(uploadId, fileoffset, s);
                                            context.ExecuteQuery();
                                            // update fileoffset for the next slice
                                            fileoffset = bytesUploaded.Value;
                                        }
                                    }
                                }

                            } // while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                        }
                    }

                    if (needCheckin)//restore checkInComment as local.
                    {
                        targetFile.CheckIn(checkInComment, listEnableMinorVersion.HasValue && listEnableMinorVersion.Value ? CheckinType.MinorCheckIn : CheckinType.MajorCheckIn);
                    }
                    if (!string.IsNullOrEmpty(listName))
                    {
                        SafeLoadFile(context, targetFile);
                    }
                    else
                    {
                        context.Load(targetFile);
                    }
                    context.ExecuteQuery();
                    Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                    fileProperties["Exists"] = true;
                    fileProperties["ListName"] = listName;
                    AssembleFileProperties(fileProperties, targetFile, webServerRelativeUrl, targetFile.ListItemAllFields);
                    return fileProperties;
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Message.Equals(mUnauthorizedMessage, StringComparison.OrdinalIgnoreCase))
                {
                    throw new AveSecurityTrimingException(mUnauthorizedMessage, webEx);
                }
                HttpWebResponse response = webEx.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.RequestUriTooLong &&
                    response.Headers != null && response.Headers["X-MSDAVEXT_Error"] != null)//Block Type File.eg. '***.ashx'
                {
                    string message = System.Web.HttpUtility.HtmlDecode(response.Headers["X-MSDAVEXT_Error"]);
                    if (message.StartsWith("589924", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(message.Substring(message.IndexOf(" ", StringComparison.OrdinalIgnoreCase) + 1).Trim());
                    }
                }
                throw;
            }
        }

        public override Stream OpenBinaryDirect(ClientRuntimeContext context, string serverRelativeUrl, object obj)
        {
            try
            {
                var path = ResourcePath.FromDecodedUrl(serverRelativeUrl);
                ClientFile file = (context as AveClientContext).Web.GetFileByServerRelativePath(path);
                ClientResult<Stream> fileStream = file.OpenBinaryStream();
                context.ExecuteQuery();
                return fileStream.Value;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get file stream failed with OpenBinaryDirect.Error:{0}", ex);
                return null;
            }
        }

        public override bool GetSiteExists(string url)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var result = Site.Exists(context, url);
                context.ExecuteQuery();
                return result.Value;
            }
        }
        public override Dictionary<string, object> GetItemByUrl(Guid webId, string itemUrl, out Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                listId = Guid.Empty;
                Dictionary<string, object> itemProp = null;
                Web web = context.Site.OpenWebById(webId);
                var path = ResourcePath.FromDecodedUrl(itemUrl);
                ListItem item = web.GetListItemUsingPath(path);
                var list = item.ParentList;
                if (item != null)
                {
                    context.Load(list, tempList => tempList.BaseType, tempList => tempList.BaseTemplate, tempList => tempList.Id);
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments);
                    context.Load(item, tempItem => tempItem.DisplayName);
                    context.ExecuteQuery();

                    listId = list.Id;
                    itemProp = new Dictionary<string, object>();
                    GetItemDic(itemProp, item);
                    if (!ItemHasVersion(list, itemProp) || !WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                    {
                        itemProp["HasVersion"] = false;
                    }
                }
                return itemProp;
            }
        }

        protected override bool UpdateLinks(ContentType contentType, Dictionary<Guid, Dictionary<string, object>> fieldLinks)
        {
            bool changed = false;
            if (fieldLinks != null)
            {
                foreach (KeyValuePair<Guid, Dictionary<string, object>> fieldlinkInterator in fieldLinks)
                {
                    FieldLink fieldLink = null;

                    foreach (var currentFieldlink in contentType.FieldLinks)
                    {
                        if (currentFieldlink.Id == fieldlinkInterator.Key)
                        {
                            fieldLink = currentFieldlink;
                        }
                    }

                    if (fieldLink == null)
                    {
                        continue;
                    }

                    if (fieldlinkInterator.Value.ContainsKey("Hidden"))
                    {
                        fieldLink.Hidden = Convert.ToBoolean(fieldlinkInterator.Value["Hidden"]);
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("Required"))
                    {
                        fieldLink.Required = Convert.ToBoolean(fieldlinkInterator.Value["Required"]);
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("DisplayName"))
                    {
                        fieldLink.DisplayName = fieldlinkInterator.Value["DisplayName"].ToString();
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("ReadOnly"))
                    {
                        fieldLink.ReadOnly = Convert.ToBoolean(fieldlinkInterator.Value["ReadOnly"]);
                        changed = true;
                    }
                    if (fieldlinkInterator.Value.ContainsKey("ShowInDisplayForm"))
                    {
                        fieldLink.ShowInDisplayForm = Convert.ToBoolean(fieldlinkInterator.Value["ShowInDisplayForm"]);
                        changed = true;
                    }
                }
            }
            return changed;
        }
        protected override void AddContentTypeFieldLink(ContentType contentType, Field field, Dictionary<string, object> fieldLinkProp)
        {
            FieldLinkCreationInformation Info = new FieldLinkCreationInformation();
            Info.Field = field;
            contentType.FieldLinks.Add(Info);
            int fieldLinksCount = contentType.FieldLinks.Count;
            FieldLink fieldLink = contentType.FieldLinks[fieldLinksCount - 1];
            if (fieldLinkProp.ContainsKey("Hidden"))
            {
                fieldLink.Hidden = bool.Parse(fieldLinkProp["Hidden"].ToString());
            }
            if (fieldLinkProp.ContainsKey("Required"))
            {
                fieldLink.Required = bool.Parse(fieldLinkProp["Required"].ToString());
            }
            if (fieldLinkProp.ContainsKey("DisplayName"))
            {
                fieldLink.DisplayName = fieldLinkProp["DisplayName"].ToString();
            }
            if (fieldLinkProp.ContainsKey("ReadOnly"))
            {
                fieldLink.ReadOnly = bool.Parse(fieldLinkProp["ReadOnly"].ToString());
            }
            if (fieldLinkProp.ContainsKey("ShowInDisplayForm"))
            {
                fieldLink.ShowInDisplayForm = bool.Parse(fieldLinkProp["ShowInDisplayForm"].ToString());
            }
        }

        protected override void LoadListCollection(AveClientContext context, ExceptionHandlingScope scope, ListCollection listCollection)
        {
            using (scope.StartScope())
            {
                using (scope.StartTry())
                {
                    context.Load(listCollection, tempListCollection => tempListCollection.IncludeWithDefaultProperties(l => l.ValidationFormula,
                                                                                                      l => l.ValidationMessage,
                                                                                                      l => l.OnQuickLaunch,
                                                                                                      l => l.IsSiteAssetsLibrary,
                                                                                                      l => l.HasUniqueRoleAssignments,
                                                                                                      l => l.DataSource,
                                                                                                      l => l.Id,
                                                                                                      l => l.ItemCount,
                                                                                                      l => l.EnableAttachments,
                                                                                                      l => l.EnableVersioning,
                                                                                                      l => l.DefaultDisplayFormUrl,
                                                                                                      l => l.EnableAssignToEmail,
                                                                                                      l => l.ExcludeFromOfflineClient,
                                                                                                      l => l.ListExperienceOptions,
                                                                                                      l => l.ReadSecurity,
                                                                                                      l => l.WriteSecurity,
                                                                                                      l => l.RootFolder,
                                                                                                      l => l.RootFolder.Properties
                                                                                                      ));
                }
                using (scope.StartCatch())
                {
                    context.Load(listCollection, tempListCollection => tempListCollection.IncludeWithDefaultProperties(
                                                                                                      l => l.HasUniqueRoleAssignments,
                                                                                                      l => l.DataSource,
                                                                                                      l => l.Id,
                                                                                                      l => l.ItemCount,
                                                                                                      l => l.EnableAttachments,
                                                                                                      l => l.EnableVersioning,
                                                                                                      l => l.RootFolder,
                                                                                                      l => l.RootFolder.Properties
                                                                                                      ));
                }
            }
        }

        protected override void SetEditorReadOnly(List list, bool readOnly) {/*do nothing.or 2013 will throw exception when add a file version.*/}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of Value")]
        protected override void SetAdvancedSetting(Dictionary<string, object> advancedSettingProp, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("DisableGridEditing"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AllowGridEditingSection$ctl02$AllowGrid"] = (bool)listProperties["DisableGridEditing"] ? "RadAllowGridNo" : "RadAllowGridYes";
                listProperties.Remove("DisableGridEditing");
            }
            if (listProperties.ContainsKey("NavigateForFormsPages"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$DialogForFormsPagesSection$ctl03$DialogForFormsPages"] = (bool)listProperties["NavigateForFormsPages"] ? "RadDialogForFormsPagesNo" : "RadDialogForFormsPagesYes";
                listProperties.Remove("NavigateForFormsPages");
            }
            if (listProperties.ContainsKey("IsSiteAssetsLibrary"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentLibrarySection$ctl02$AttachmentLibrary"] = (bool)listProperties["IsSiteAssetsLibrary"] ? "RadAttachmentLibraryYes" : "RadAttachmentLibraryNo";
                listProperties.Remove("IsSiteAssetsLibrary");
            }
            if (listProperties.ContainsKey("DefaultItemOpenUseListSetting") && !(bool)listProperties["DefaultItemOpenUseListSetting"])
            {
                advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = "RadDefaultItemOpenServerSetting";
                listProperties.Remove("DefaultItemOpenUseListSetting");
            }
            else if (listProperties.ContainsKey("DefaultItemOpen"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl01$DefaultItemOpen"] = (int)listProperties["DefaultItemOpen"] == 0 ? "RadDefaultItemOpenPreferClient" : "RadDefaultItemOpenBrowser";
                listProperties.Remove("DefaultItemOpen");
            }
            if (listProperties.ContainsKey("SendToLocationName"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationName"] = listProperties["SendToLocationName"];
                listProperties.Remove("SendToLocationName");
            }
            if (listProperties.ContainsKey("SendToLocationUrl"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl02$TxtSendToLocationUrl"] = listProperties["SendToLocationUrl"];
                listProperties.Remove("SendToLocationUrl");
            }
            if (listProperties.ContainsKey("EnableManagedIndexes"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ManagedIndexesSection$ctl02$AllowManagedIndex"] = (bool)listProperties["EnableManagedIndexes"] ? "RadManagedIndexesYes" : "RadManagedIndexesNo";
                listProperties.Remove("EnableManagedIndexes");
            }
            if (listProperties.ContainsKey("EnableAttachments"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentsSection$ctl02$DisableAttachments"] = (bool)listProperties["EnableAttachments"] ? "RadAttachmentsEnabled" : "RadAttachmentsDisabled";
                listProperties.Remove("EnableAttachments");
            }
        }

        protected override void LoadWeb(Web web, ClientContext context)
        {
            context.Load(web);
            context.Load(web, w => w.SiteLogoDescription);//该属性需要主动load，不然load不出来
            ExceptionHandlingScope memberGroupCondition = new ExceptionHandlingScope(context);
            using (memberGroupCondition.StartScope())
            {
                using (memberGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedMemberGroup, w => w.AssociatedMemberGroup.Users, w => w.AssociatedMemberGroup.Owner.Id, w => w.AssociatedMemberGroup.Owner.PrincipalType);
                }
                using (memberGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedMemberGroup);
                }
            }
            ExceptionHandlingScope ownerGroupCondition = new ExceptionHandlingScope(context);
            using (ownerGroupCondition.StartScope())
            {
                using (ownerGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedOwnerGroup, w => w.AssociatedOwnerGroup.Users, w => w.AssociatedOwnerGroup.Owner.Id, w => w.AssociatedOwnerGroup.Owner.PrincipalType);
                }
                using (ownerGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedOwnerGroup);
                }
            }
            ExceptionHandlingScope visitorGroupCondition = new ExceptionHandlingScope(context);
            using (visitorGroupCondition.StartScope())
            {
                using (visitorGroupCondition.StartTry())
                {
                    context.Load(web, w => w.AssociatedVisitorGroup, w => w.AssociatedVisitorGroup.Users, w => w.AssociatedVisitorGroup.Owner.Id, w => w.AssociatedVisitorGroup.Owner.PrincipalType);
                }
                using (visitorGroupCondition.StartCatch())
                {
                    context.Load(web, w => w.AssociatedVisitorGroup);
                }
            }
            ExceptionHandlingScope normalProperty = new ExceptionHandlingScope(context);
            using (normalProperty.StartScope())
            {
                using (normalProperty.StartTry())
                {
                    context.Load(web, w => w.CurrentUser, w => w.RootFolder, w => w.AllProperties, w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch, w => w.HasUniqueRoleAssignments, w => w.SupportedUILanguageIds, w => w.AllowDesignerForCurrentUser, w => w.AllowAutomaticASPXPageIndexing);
                }
                using (normalProperty.StartCatch())
                {
                    context.Load(web, w => w.CurrentUser, w => w.RootFolder, w => w.AllProperties, w => w.Navigation.TopNavigationBar, w => w.Navigation.QuickLaunch, w => w.HasUniqueRoleAssignments);
                }
            }
        }

        //public override ListItem InternUpdate(List list, int itemid, Dictionary<string, object> itemProperties, ExceptionHandlingScope excepScope)
        //{
        //    Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
        //    ListItem tempListItem = new ListItem(list.Context, new ObjectPathMethod(list.Context, list.Path, "GetItemById", new object[] { itemid }));
        //    bool isCurrentCheckOut = itemProperties.ContainsKey("IsCurrentCheckOut") ? (bool)itemProperties["IsCurrentCheckOut"] : false;
        //    bool changed = AveListItemRestore.SetFieldValues(tempListItem, itemFieldValues);
        //    if (changed)
        //    {
        //        string itemTitle = itemFieldValues.ContainsKey("FileLeafRef") ? itemFieldValues["FileLeafRef"] as string : string.Empty;
        //        itemFieldValues.Remove("FileLeafRef");
        //        IList<ListItemFormUpdateValue> values = new List<ListItemFormUpdateValue>();
        //        values.Add(new ListItemFormUpdateValue() { FieldName = "FileLeafRef", FieldValue = itemTitle });
        //        // ADO-169105 office文件的EnterpriseKeyword使用更新column的方法无法更新成功，需要使用ValidateUpdateListItem来更新。
        //        if (itemFieldValues.ContainsKey("TaxKeyword"))
        //        {
        //            string taxKeyword = itemFieldValues["TaxKeyword"] as string;
        //            itemFieldValues.Remove("TaxKeyword");
        //            values.Add(new ListItemFormUpdateValue() { FieldName = "TaxKeyword", FieldValue = taxKeyword });
        //        }
        //        tempListItem.SystemUpdate();
        //        list.Context.Load(tempListItem);
        //        list.Context.Load(tempListItem, it => it.HasUniqueRoleAssignments);
        //    }
        //    return tempListItem;
        //}

        public override Dictionary<string, object> GetTaxonomyGroups(Guid guid)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> groupsProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> groupsList = new List<Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(guid);
                    ExceptionHandlingScope principalCondition = new ExceptionHandlingScope(context);
                    using (principalCondition.StartScope())
                    {
                        using (principalCondition.StartTry())
                        {
                            context.Load(store.Groups);
                            context.Load(store.Groups, groupCollection => groupCollection.IncludeWithDefaultProperties(t => t.GroupManagerPrincipalNames, t => t.ContributorPrincipalNames));
                        }
                        using (principalCondition.StartCatch())
                        {
                            context.Load(store.Groups);
                        }
                    }
                    context.ExecuteQuery();
                    foreach (TermGroup group in store.Groups)
                    {
                        Dictionary<string, object> groupProp = new Dictionary<string, object>();
                        AveObjectCopy.GetObjectBasicProperties(groupProp, group);
                        List<Dictionary<string, object>> groupManagers = new List<Dictionary<string, object>>();
                        List<Dictionary<string, object>> groupContributors = new List<Dictionary<string, object>>();
                        if (group.IsPropertyAvailable("GroupManagerPrincipalNames"))
                        {
                            foreach (string principalName in group.GroupManagerPrincipalNames)
                            {
                                Dictionary<string, object> manager = new Dictionary<string, object>();
                                manager["PrincipalName"] = principalName;
                                manager["DisplayName"] = string.Empty;
                                manager["GrantRightsMask"] = (ulong)(AveTaxonomyRights.GroupManager | AveTaxonomyRights.EditTerm | AveTaxonomyRights.AddTermSetEditPermissions | AveTaxonomyRights.EditGroup | AveTaxonomyRights.EditTermSet);
                                manager["DenyRightsMask"] = (ulong)AveTaxonomyRights.None;
                                groupManagers.Add(manager);
                            }
                        }
                        if (group.IsPropertyAvailable("ContributorPrincipalNames"))
                        {
                            foreach (string principalName in group.ContributorPrincipalNames)
                            {
                                Dictionary<string, object> contributor = new Dictionary<string, object>();
                                contributor["PrincipalName"] = principalName;
                                contributor["DisplayName"] = string.Empty;
                                contributor["GrantRightsMask"] = (ulong)(AveTaxonomyRights.Contributor | AveTaxonomyRights.EditTerm | AveTaxonomyRights.EditTermSet);
                                contributor["DenyRightsMask"] = (ulong)AveTaxonomyRights.None;
                                groupContributors.Add(contributor);
                            }
                        }
                        groupProp["GroupManagers"] = groupManagers;
                        groupProp["Contributors"] = groupContributors;
                        groupsList.Add(groupProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermGroups Failed, error message:{0}", e.ToString());
                }
                groupsProp[AveObjectModelConstant.ChildrenProperties] = groupsList;
                return groupsProp;
            }
        }

        public override bool UpdateTermGroupUserInfo(TermGroup group, Dictionary<string, object> needUpdateGroupProperties)
        {
            bool change = false;
            if (needUpdateGroupProperties.ContainsKey("AddContributor"))
            {
                foreach (string principalName in needUpdateGroupProperties["AddContributor"] as List<string>)
                {
                    group.AddContributor(principalName);
                    change |= true;
                }
            }
            if (needUpdateGroupProperties.ContainsKey("AddGroupManager"))
            {
                foreach (string principalName in needUpdateGroupProperties["AddGroupManager"] as List<string>)
                {
                    group.AddGroupManager(principalName);
                    change |= true;
                }
            }
            return change;
        }

        protected override List<string> UpdateListNormalProperties
        {
            get
            {
                return new List<string> { "NoCrawl", "ReadSecurity", "WriteSecurity" };
            }
        }


        public override Stream RetryGetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source)
        {
            Stream stream = null;
            try
            {
                string tempWebServerRelativeUrl = string.Empty;
                if (source.Equals("File", StringComparison.OrdinalIgnoreCase))
                {
                    string filePath = fileServerRelativeUrl;
                    if (!fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        filePath = AveUrlUtility.CombineUrl(webServerRelativeUrl, fileServerRelativeUrl);
                    }
                    tempWebServerRelativeUrl = AveUrlUtility.CombineUrl(webServerRelativeUrl, "_layouts/15/download.aspx?SourceUrl=");
                }
                AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
                retryHelper.ShouldRetryCommonConnectionExceptions = true;
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    stream = mWebServiceRequest.GetFileStream(tempWebServerRelativeUrl, fileServerRelativeUrl, source);
                });
            }
            catch (Exception e)
            {
                mLogger.Error("Get file throught WebService failed. File:{0} Web:{1} Error:{2}", fileServerRelativeUrl, webServerRelativeUrl, e);
            }
            return stream;
        }

        protected override Stream GetFileStreamByRestApi(string webUrl, string fileServerRelativeUrl)
        {
            string methodCmd = string.Format("getfilebyserverrelativeurl('{0}')", fileServerRelativeUrl);
            string request = string.Format("{0}/_api/Web/{1}/$value", webUrl.TrimEnd('/'), methodCmd);
            mLogger.Info("Rest api request: {0}", request);

            Stream stream = null;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                stream = GetContentStream(request, "RApiFS");
            });
            return stream;
        }

        private Stream GetFileVersionStreamByRestApi(string webUrl, string fileServerRelativeUrl, int uiVersion)
        {
            string methodCmd = string.Format("getfilebyserverrelativeurl('{0}')", fileServerRelativeUrl);
            string versionCmd = string.Format("versions({0})", uiVersion);
            string request = string.Format("{0}/_api/Web/{1}/{2}/$value", webUrl, methodCmd, versionCmd);
            mLogger.Info("Large file include version request: {0}", request);

            Stream stream = null;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                               new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"),
                                                                               new KeyValuePair<string, string>("WebException", "The operation has timed out"),
                                                                               new KeyValuePair<string, string>("IOException", "Received an unexpected EOF or 0 bytes from the transport stream"));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                stream = GetContentStream(request, "FileVersionContentFS");
            });
            return stream;
        }

        private Stream GetContentStream(string cmd, string internalName)
        {
            ReconnectableHttpWebRequest webRequest = ReconnectableHttpWebRequest.CreateRequest(cmd);
            if (mObj is CookieContainer)
            {
                webRequest.CookieContainer = mObj as CookieContainer;
            }
            else if (mObj is NetworkCredential)
            {
                webRequest.Credentials = mObj as NetworkCredential;
            }
            var result = webRequest.GetResponse() as HttpWebResponse;
            AveCoordinatedStream content = new AveCoordinatedStream();
            using (Stream stream = result.GetResponseStream())
            {
                AveIOHelper.Copy(stream, content);
                content.Position = 0;
            }
            return content;
        }

        public override ListItem InternUpdateAPI(List list, ListItem item, Dictionary<string, object> itemProperties, ExceptionHandlingScope excepScope)
        {
            Dictionary<string, object> itemFieldValues = itemProperties["ChangedFieldValues"] as Dictionary<string, object>;
            bool isCurrentCheckOut = itemProperties.ContainsKey("IsCurrentCheckOut") ? (bool)itemProperties["IsCurrentCheckOut"] : false;
            bool changed = AveListItemRestore.SetFieldValues(item, itemFieldValues);
            if (changed)
            {
                item.SystemUpdate();
            }
            return item;
        }

        public override Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream, IReport report)
        {
            string oldWebUrl = string.Empty;
            if (!string.IsNullOrEmpty(info.ParentWebRelativeUrl) && !string.IsNullOrEmpty(this.mWebUrl) && this.mWebUrl.Contains("/sites"))
            {
                oldWebUrl = this.mWebUrl;
                this.mWebUrl = string.Format("{0}{1}", this.mWebUrl.Substring(0, this.mWebUrl.IndexOf("/sites", StringComparison.OrdinalIgnoreCase)), info.ParentWebRelativeUrl);
            }
            try
            {
                using (AveClientContext context = base.CreateContext())
                {
                    Site site = context.Site;
                    using (var documentRestore = new AveO365DocumentRestore(this, site, mObj, context, mServerVersion, report))
                    {
                        return documentRestore.RestoreDocument(info, fileStream);
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(oldWebUrl))
                {
                    this.mWebUrl = oldWebUrl;
                }
            }
        }
        public override Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (var listItemRestore = new AveO365ListItemRestore(this, site, context, mObj))
                {
                    return listItemRestore.RestoreListItem(data, userData);
                }
            }
        }

        public override Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (AveO365FolderRestore folderRestore = new AveO365FolderRestore(this, site, context, mObj))
                {
                    return folderRestore.RestoreFolder(data, userData);
                }
            }
        }
        public override ClientFile GetFileByAPI(Web web, string url)
        {
            // 如果url是URL encode 过的，例如包含%20（空格），使用GetFileByServerRelativePath会找不到file，需要先url decode。
            url = Uri.UnescapeDataString(url);
            var path = ResourcePath.FromDecodedUrl(url);
            return web.GetFileByServerRelativePath(path);
        }

        public override Folder GetFolderByAPI(Web web, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            return web.GetFolderByServerRelativePath(path);
        }

        public override Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            FolderCollectionAddParameters folderAddParameters = new FolderCollectionAddParameters();
            folderAddParameters.Overwrite = true;
            return folders.AddUsingPath(path, folderAddParameters);
        }

        protected override ClientFile AddFileByAPI(FileCollection files, FileCreationInformation createInfo)
        {
            FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
            fileAddParameters.Overwrite = createInfo.Overwrite;
            var filePath = ResourcePath.FromDecodedUrl(createInfo.Url);
            return files.AddUsingPath(filePath, fileAddParameters, new MemoryStream(createInfo.Content));
        }

        protected override void SetCamlQueryFolderUrl(CamlQuery camlquery, string folderUrl)
        {
            var filePath = ResourcePath.FromDecodedUrl(folderUrl);
            camlquery.FolderServerRelativePath = filePath;
        }

        protected override ListItem AddListItem(ClientContext context, List list, string folderUrl, int objectType, string leafName)
        {
            context.ValidateOnClient = false;
            var itemCrtInfo = new ListItemCreationInformationUsingPath()
            {
                FolderPath = ResourcePath.FromDecodedUrl(folderUrl),
                LeafName = ResourcePath.FromDecodedUrl(leafName),
                UnderlyingObjectType = (FileSystemObjectType)objectType,
            };
            return list.AddItemUsingPath(itemCrtInfo);
        }

        public override Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId)
        {
            try
            {
                return mWebServiceRequest.GetFileVersionStream(webServerRelativeUrl, fileServerRelativeUrl, fileVerionServerRelativeUrl, versionId);
            }
            catch (Exception e)
            {
                try
                {
                    mLogger.Warn("get file version stream by WebService failed. error message:{0}", e.ToString());
                    return GetFileVersionStreamByRestApi(AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl), fileServerRelativeUrl, versionId);
                }
                catch (Exception e1)
                {
                    mLogger.Warn("get file version stream by rest api failed. error message:{0}", e1.ToString());
                    using (AveClientContext context = CreateContext())
                    {
                        Web web = context.Site.OpenWeb(webServerRelativeUrl);
                        var path = ResourcePath.FromDecodedUrl(fileServerRelativeUrl);
                        ClientFile file = web.GetFileByServerRelativePath(path);
                        FileVersion version = file.Versions.GetById(versionId);
                        ClientResult<Stream> content = version.OpenBinaryStream();
                        context.ExecuteQuery();
                        //binary copy is required, cause ClientResult<Stream> can't be used after context is disposed
                        //MemoryStream binary = new MemoryStream((int)content.Value.Length);
                        AveCoordinatedStream binary = new AveCoordinatedStream();
                        AveIOHelper.Copy(content.Value, binary);
                        binary.Position = 0;
                        return binary;
                    }
                }
            }
        }

        public override void CopyTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, bool bOverWrite)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile file = GetFileByAPI(web, fileServerRelativeUrl);
                var path = ResourcePath.FromDecodedUrl(strNewUrl);
                file.CopyToUsingPath(path, bOverWrite);
                context.Load(file);
                context.ExecuteQuery();
            }
        }

        public override WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId)
        {
            var cache = new WorkflowStartOptionCache();
            using (var context = CreateContext(url))
            {
                var list = context.Web.Lists.GetById(listId);
                context.Load(list);
                context.Load(list.WorkflowAssociations);
                context.Load(list.ContentTypes, cts => cts.IncludeWithDefaultProperties(ct => ct.StringId, ct => ct.WorkflowAssociations));
                context.ExecuteQuery();
                Backup10ModeStartOption(WorkflowStartOptionCache.ListWorkflow, cache, list.WorkflowAssociations);
                foreach (var ct in list.ContentTypes)
                {
                    Backup10ModeStartOption(ct.StringId, cache, ct.WorkflowAssociations);
                }

                var workflowServiceManager = new WorkflowServicesManager(context, context.Web);
                context.Load(workflowServiceManager);
                context.ExecuteQuery();
                if (workflowServiceManager.IsConnected)
                {
                    var subScriptionService = workflowServiceManager.GetWorkflowSubscriptionService();
                    var subscriptions = subScriptionService.EnumerateSubscriptionsByList(list.Id);
                    context.Load(subscriptions, sub => sub.IncludeWithDefaultProperties(subscription => subscription.EventTypes, subscription => subscription.Id, subscription => subscription.ParentContentTypeId));
                    context.ExecuteQuery();
                    Backup13ModeStartOption(context, cache, subScriptionService, subscriptions);
                    context.ExecuteQuery();
                }
            }
            return cache;
        }

        private void Backup10ModeStartOption(string cacheKeyName, WorkflowStartOptionCache cache, WorkflowAssociationCollection collection)
        {
            if (collection.Count == 0)
            {
                return;
            }
            cache.SP2010ModeWorkflowAutoStartCache.Add(cacheKeyName, new List<WorkflowStartOption>());
            var listCache = cache.SP2010ModeWorkflowAutoStartCache[cacheKeyName];
            foreach (var workflow in collection)
            {
                if (workflow.Enabled && (workflow.AutoStartChange || workflow.AutoStartCreate))
                {
                    var option = new WorkflowStartOption()
                    {
                        DefinitionId = workflow.Id,
                        ItemAdded = workflow.AutoStartCreate,
                        ItemUpdated = workflow.AutoStartChange
                    };
                    listCache.Add(option);
                    mLogger.Debug("Change auto start option for 2010 mode workflow:{0}:{1},AutoStart:{2} to {3},AutoChange:{4} to {5}",
                               workflow.Name, workflow.Id, option.ItemAdded, false, option.ItemUpdated, false);
                    workflow.AutoStartChange = false;
                    workflow.AutoStartCreate = false;
                    workflow.Update();
                }
            }
        }

        private void Backup13ModeStartOption(ClientContext context, WorkflowStartOptionCache cache, WorkflowSubscriptionService service, WorkflowSubscriptionCollection collection)
        {
            if (collection.Count == 0)
            {
                return;
            }

            foreach (var workflow in collection)
            {
                if (workflow.EventTypes.Contains("ItemAdded", StringComparer.OrdinalIgnoreCase) ||
                    workflow.EventTypes.Contains("ItemUpdated", StringComparer.OrdinalIgnoreCase))
                {

                    string cacheKeyName = WorkflowStartOptionCache.ListWorkflow;
                    if (!string.IsNullOrEmpty(workflow.ParentContentTypeId))
                    {
                        cacheKeyName = workflow.ParentContentTypeId;
                    }
                    if (!cache.SP2013ModeWorkflowAutoStartCache.ContainsKey(cacheKeyName))
                    {
                        cache.SP2013ModeWorkflowAutoStartCache.Add(cacheKeyName, new List<WorkflowStartOption>());
                    }
                    var listCache = cache.SP2013ModeWorkflowAutoStartCache[cacheKeyName];

                    var option = new WorkflowStartOption()
                    {
                        DefinitionId = workflow.Id,
                        ItemAdded = workflow.EventTypes.Contains("ItemAdded", StringComparer.OrdinalIgnoreCase),
                        ItemUpdated = workflow.EventTypes.Contains("ItemUpdated", StringComparer.OrdinalIgnoreCase)
                    };
                    listCache.Add(option);
                    mLogger.Debug("Change auto start option for 2013 mode workflow:{0}:{1},AutoStart:{2} to {3},AutoChange:{4} to {5}",
                              workflow.Name, workflow.Id, option.ItemAdded, false, option.ItemUpdated, false);
                    var clonedSubscription = CloneSubscription(context, workflow);
                    var eventList = clonedSubscription.EventTypes.ToList();
                    eventList.Remove("ItemAdded");
                    eventList.Remove("ItemUpdated");
                    clonedSubscription.EventTypes = eventList.ToArray();
                    service.PublishSubscription(clonedSubscription);
                }
            }
        }

        private WorkflowSubscription CloneSubscription(ClientContext context, WorkflowSubscription subscription)
        {
            WorkflowSubscription workflowSubscription = new WorkflowSubscription(context);
            workflowSubscription.DefinitionId = subscription.DefinitionId;
            workflowSubscription.EventSourceId = subscription.EventSourceId;
            workflowSubscription.Id = subscription.Id;
            workflowSubscription.Name = subscription.Name;
            if (subscription.PropertyDefinitions != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in subscription.PropertyDefinitions)
                {
                    workflowSubscription.SetProperty(keyValuePair.Key, keyValuePair.Value);
                }
            }
            workflowSubscription.StatusFieldName = subscription.StatusFieldName;
            string eventTypeStr = string.Empty;
            List<string> eventTypes = new List<string>();
            foreach (string eventType in subscription.EventTypes)
            {
                if (!eventTypes.Contains(eventType))
                {
                    eventTypeStr += eventType + "#;";
                    eventTypes.Add(eventType);
                }
            }
            workflowSubscription.EventTypes = eventTypes.ToArray();
            return workflowSubscription;
        }

        public override void RestoreWorkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache)
        {
            using (var context = CreateContext(url))
            {
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                context.Load(list.WorkflowAssociations);
                context.Load(list.ContentTypes, cts => cts.IncludeWithDefaultProperties(ct => ct.StringId, ct => ct.WorkflowAssociations));
                context.ExecuteQuery();
                if (cache.SP2010ModeWorkflowAutoStartCache.Count > 0)
                {
                    foreach (var item in cache.SP2010ModeWorkflowAutoStartCache)
                    {
                        var workflows = list.WorkflowAssociations;
                        if (!string.Equals(item.Key, WorkflowStartOptionCache.ListWorkflow, StringComparison.OrdinalIgnoreCase))
                        {
                            var contentType = list.ContentTypes.GetById(item.Key);
                            workflows = contentType.WorkflowAssociations;
                        }
                        foreach (var cacheItem in item.Value)
                        {
                            var workflow = workflows.GetById(cacheItem.DefinitionId);
                            context.Load(workflow);
                            context.ExecuteQuery();
                            workflow.AutoStartChange = cacheItem.ItemUpdated;
                            workflow.AutoStartCreate = cacheItem.ItemAdded;
                            workflow.Update();
                            mLogger.Debug("ChangeBack auto start option for 2010 mode workflow:{0}:{1},AutoStart:{2},AutoChange:{3}",
                              workflow.Name, workflow.Id, cacheItem.ItemAdded, cacheItem.ItemUpdated);
                        }
                    }
                    context.ExecuteQuery();
                }
                if (cache.SP2013ModeWorkflowAutoStartCache.Count > 0)
                {
                    WorkflowServicesManager manager = new WorkflowServicesManager(context, web);
                    var subscriptionService = manager.GetWorkflowSubscriptionService();

                    foreach (var item in cache.SP2013ModeWorkflowAutoStartCache)
                    {
                        foreach (var cacheItem in item.Value)
                        {
                            var oldSubscription = subscriptionService.GetSubscription(cacheItem.DefinitionId);
                            context.Load(oldSubscription);
                            context.ExecuteQuery();
                            var workflow = CloneSubscription(context, oldSubscription);
                            var eventList = workflow.EventTypes.ToList();
                            bool needChange = false;
                            if (!workflow.EventTypes.Contains("ItemAdded", StringComparer.OrdinalIgnoreCase) && cacheItem.ItemAdded)
                            {
                                eventList.Add("ItemAdded");
                                needChange = true;
                            }
                            if (!workflow.EventTypes.Contains("ItemUpdated", StringComparer.OrdinalIgnoreCase) && cacheItem.ItemUpdated)
                            {
                                eventList.Add("ItemUpdated");
                                needChange = true;
                            }
                            if (needChange)
                            {
                                workflow.EventTypes = eventList.ToArray();
                            }
                            var subscriptionId = subscriptionService.PublishSubscriptionForList(workflow, listId);
                            mLogger.Debug("ChangeBack auto start option for 2013 mode workflow:{0}:{1},AutoStart:{2},AutoChange:{3},FinalId:{4}",
                             workflow.Name, workflow.Id, cacheItem.ItemAdded, cacheItem.ItemUpdated, subscriptionId.Value);
                        }
                    }
                    context.ExecuteQuery();
                }
            }
        }

        protected override void LoadItemsProperty(ClientContext context, ListItemCollection items)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                //ADO-157190 365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName时会出异常
                using (scope.StartTry())
                {
                    context.Load(items);
                    context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments, t => t.DisplayName, t => t.Properties));
                }
                using (scope.StartCatch())
                {
                    context.Load(items);
                    context.Load(items, its => its.Include(t => t.HasUniqueRoleAssignments));
                }
            }
        }

        protected override void LoadItemProperty(AveClientContext context, ListItem item)
        {
            ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
            using (scope.StartScope())
            {
                //ADO-157190 365 CommunitySite中自带的Disscussion List中的ListItem load DisplayName时会出异常
                using (scope.StartTry())
                {
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments, tempItem => tempItem.DisplayName, tempItem => tempItem.Properties);
                }
                using (scope.StartCatch())
                {
                    context.Load(item);
                    context.Load(item, tempItem => tempItem.HasUniqueRoleAssignments);
                }
            }
        }

        public override void GetItemDic(Dictionary<string, object> itemProperties, ListItem item)
        {
            var properties = new Hashtable();
            try
            {
                if (item.IsObjectPropertyInstantiated("Properties"))
                {
                    foreach (var p in item.Properties.FieldValues)
                    {
                        properties[p.Key] = p.Value;
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Error while loading item properties.Error:{0}", e);
            }
            itemProperties["Properties"] = properties;
            base.GetItemDic(itemProperties, item);
        }

        protected override void HandleMetaInfoField(AveClientContext context, ListItem item, Dictionary<string, object> itemFieldValues)
        {
            if (itemFieldValues.ContainsKey("Properties"))
            {
                LoadItemProperty(context, item);
                context.ExecuteQuery();
                var properties = itemFieldValues["Properties"] as Hashtable;
                foreach (DictionaryEntry et in properties)
                {
                    item.Properties[et.Key.ToString()] = et.Value;
                }
            }
            else
            {
                base.HandleMetaInfoField(context, item, itemFieldValues);
            }
        }

        protected override void LoadContentType(ClientContext context, ContentType contentType)
        {
            context.Load(contentType, c => c.Id, c => c.SchemaXml, c => c.FieldLinks, c => c.SchemaXmlWithResourceTokens);
        }

        protected override void LoadContentTypes(ClientContext context, ContentTypeCollection contentTypes)
        {
            context.Load(contentTypes, tempContentTypes => tempContentTypes.IncludeWithDefaultProperties(temp => temp.Id, temp => temp.Parent.Id, temp => temp.SchemaXml, temp => temp.SchemaXml, temp => temp.SchemaXmlWithResourceTokens));//cts => cts.IncludeWithDefaultProperties(ct => ct.Fields, ct => ct.FieldLinks));
        }

        public override Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties)
        {
            //using (ClientContext context = CreateContext())
            using (ClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                //Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Web web = context.Web;
                FieldCollection fields = null;
                Field field = null;
                bool changed = false;
                ContentType contentType = this.GetContentTypeWithoutFields(context, webServerRelativeUrl, listName, listId, contentTypeSource, contentTypeId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateContentTypeProperties, contentType);
                if (needUpdateContentTypeProperties.ContainsKey("AddFieldLink"))
                {
                    foreach (Dictionary<string, object> fieldLinkProp in needUpdateContentTypeProperties["AddFieldLink"] as List<Dictionary<string, object>>)
                    {
                        bool isNew = fieldLinkProp.ContainsKey("IsNew") ? (bool)fieldLinkProp["IsNew"] : false;
                        if (isNew)
                        {
                            switch (fieldLinkProp["fieldSource"].ToString())
                            {
                                case "web.fields":
                                    fields = web.Fields;
                                    break;
                                case "web.availableFields":
                                    fields = web.AvailableFields;
                                    break;
                                case "list.fields":
                                    List list = web.Lists.GetByTitle(listName);
                                    fields = list.Fields;
                                    break;
                                default:
                                    break;
                            }
                            field = fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                        }
                        else
                        {
                            ContentType newContentType = GetContentTypeWithoutFields(context, AveUrlUtility.GetServerRelativeUrl(fieldLinkProp["site"].ToString()), fieldLinkProp["ParentList"] == null ? null : fieldLinkProp["ParentList"].ToString(), Guid.Empty, fieldLinkProp["contentTypeSource"].ToString(), fieldLinkProp["Id"].ToString());
                            context.Load(newContentType, c => c.FieldLinks, c => c.Fields);
                            field = newContentType.Fields.GetById(new Guid(fieldLinkProp["FieldId"].ToString()));
                        }
                        AddContentTypeFieldLink(contentType, field, fieldLinkProp);
                        changed = true;
                        //contentType.Update(updateChildren);
                    }
                }

                changed |= UpdateFieldLinkProperties(context, contentType, needUpdateContentTypeProperties, updateChildren);
                changed |= UpdateContentTypeUserResource(contentType, needUpdateContentTypeProperties);

                int propertiesCount = Convert.ToInt32(needUpdateContentTypeProperties["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]);
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                if (changed || propertiesCount > 0)
                {
                    contentType.Update(updateChildren);
                    context.Load(contentType);
                    context.Load(contentType, c => c.Parent);
                    context.Load(contentType, c => c.SchemaXml, c => c.SchemaXmlWithResourceTokens);
                    context.ExecuteQuery();
                    this.AssembleSingleContentTypeProperties(newProp, contentType);
                }

                //ADO-201408 测试发现UpdateContentTypeAddedDocumentsByAPI 方法无法更新list level XmlDocument，增加特殊判断，list level content type使用原来的webService 的方式更新XmlDocument
                if (needUpdateContentTypeProperties.ContainsKey("AddedDocuments") && !string.Equals(contentTypeSource, "list.contentTypes", StringComparison.OrdinalIgnoreCase) && UpdateContentTypeAddedDocumentsByAPI(context, contentType, needUpdateContentTypeProperties, newProp, updateChildren))
                {
                    //ADO-182811 需要更新NewDocumentControl与RequireClientRenderingOnNew这两个属性
                    mWebServiceRequest.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, GetNeedUpdateContentTypePropertiesForWebService(needUpdateContentTypeProperties));
                }
                else
                {
                    //如果不包含AddedDocuments 或者 ResourceToken为Empty，那么使用WebService更新相关数据
                    mWebServiceRequest.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties);
                }

                return newProp;
            }
        }
        /// <summary>
        /// ADO-201408 经研究发现 API 无法更新list level xmldocuments，可以成功更新web level xmldocuments
        /// </summary>
        /// <param name="context"></param>
        /// <param name="contentType"></param>
        /// <param name="needUpdateContentTypeProperties"></param>
        /// <param name="newProp"></param>
        /// <returns>true: 使用API update 成功</returns>
        private bool UpdateContentTypeAddedDocumentsByAPI(ClientContext context, ContentType contentType, Dictionary<string, object> needUpdateContentTypeProperties, Dictionary<string, object> newProp, bool updateChildren)
        {
            if (context.HasPendingRequest)
            {
                context.ExecuteQuery();
            }
            try
            {
                var doc = new XmlDocument();
                if (!string.IsNullOrEmpty(contentType.SchemaXmlWithResourceTokens))
                {
                    doc.LoadXml(contentType.SchemaXmlWithResourceTokens);
                }
                else
                {
                    doc.LoadXml(contentType.SchemaXml);
                }

                Dictionary<string, string> XmlDocumentData = (Dictionary<string, string>)needUpdateContentTypeProperties["AddedDocuments"];

                var tags = doc.GetElementsByTagName("XmlDocuments");
                XmlNode node = null;
                if (tags.Count > 0)
                {
                    node = tags[0];
                }
                else
                {
                    node = doc.DocumentElement.AppendChild(doc.CreateElement("XmlDocuments"));
                }
                foreach (string str in XmlDocumentData.Keys)
                {
                    var element = doc.CreateElement("XmlDocument");
                    element.SetAttribute("NamespaceURI", str);
                    string str2 = XmlDocumentData[str];
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] buffer = encoding.GetBytes(str2);
                    element.InnerText = Convert.ToBase64String(buffer, 0, buffer.Length);
                    node.AppendChild(element);
                }
                contentType.SchemaXmlWithResourceTokens = doc.OuterXml;
                contentType.Update(updateChildren);
                context.ExecuteQuery();
                newProp["SchemaXmlWithResourceTokens"] = contentType.SchemaXmlWithResourceTokens;
                return true;
            }
            catch (Exception ex)
            {
                mLogger.Warn("An error occurred while update content type AddedDocuments property, content type name: {0}, error: {1}", contentType.Name, ex);
                return false;
            }




        }
        private Dictionary<string, object> GetNeedUpdateContentTypePropertiesForWebService(Dictionary<string, object> needUpdateContentTypeProperties)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            object temp;
            if (needUpdateContentTypeProperties.TryGetValue("NewDocumentControl", out temp))
            {
                properties["NewDocumentControl"] = temp;
            }
            if (needUpdateContentTypeProperties.TryGetValue("RequireClientRenderingOnNew", out temp))
            {
                properties["RequireClientRenderingOnNew"] = temp;
            }
            return properties;
        }
        public override Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ClientFile newFile = null;
                string fileType = Path.GetExtension(urlOfFile);
                if (mSpecialFileList.Contains(fileType, StringComparer.OrdinalIgnoreCase))
                {
                    Folder folder = GetFolderByAPI(web, folderServerRelativeUrl);
                    FileCreationInformation fci = new FileCreationInformation();
                    fci.Url = urlOfFile;
                    fci.Content = file;
                    fci.Overwrite = overwrite;
                    newFile = AddFileByAPI(folder.Files, fci);
                }
                else
                {
                    context.ExecuteQuery();
                    MemoryStream stream = new MemoryStream(file);
                    if (urlOfFile.StartsWith("http", StringComparison.OrdinalIgnoreCase) || urlOfFile.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        //urlOfFile = urlOfFile.Substring(WebAppName.Length);
                        Uri fileUri = new Uri(urlOfFile);
                        urlOfFile = fileUri.AbsolutePath;
                    }
                    else if (!string.IsNullOrEmpty(webServerRelativeUrl) && (string.IsNullOrEmpty(folderServerRelativeUrl) || !urlOfFile.Trim('/').StartsWith(folderServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase)) && !urlOfFile.Trim('/').StartsWith(webServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        if (urlOfFile.Trim('/').IndexOf("/", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            if (string.IsNullOrEmpty(folderServerRelativeUrl))
                            {
                                urlOfFile = webServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                            }
                            else
                            {
                                urlOfFile = folderServerRelativeUrl.TrimEnd('/') + "/" + urlOfFile.TrimStart('/');
                            }
                        }
                        else
                        {
                            urlOfFile = string.Format("{0}/{1}", webServerRelativeUrl.TrimEnd('/'), urlOfFile.TrimStart('/'));
                        }
                    }
                    newFile = web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(urlOfFile));
                    newFile.SaveBinary(new FileSaveBinaryInformation { ContentStream = stream });
                }

                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        context.Load(newFile);
                        context.Load(newFile.ListItemAllFields);
                        context.Load(newFile.CheckedOutByUser);
                    }
                    using (excepScope.StartCatch())
                    {
                        context.Load(newFile);
                    }
                }
                context.ExecuteQuery();
                if (excepScope.HasException)
                {
                    mLogger.Warn("Get AddFile CheckedOutByUser Error, newFileUrl:{0} , Error Message:{1}", urlOfFile, excepScope.ErrorMessage);
                }
                fileProperties["Exists"] = true;
                AssembleFileProperties(fileProperties, newFile, webServerRelativeUrl, newFile.ListItemAllFields);
                return fileProperties;
            }
        }
        private void LoadProperty(AveClientContext context, Action loadProperty)
        {
            ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);

            using (excepScope.StartScope())
            {
                using (excepScope.StartTry())
                {
                    loadProperty();
                }
                using (excepScope.StartCatch()) ;
            }
        }
        private void LoadAlertSpecialProperty(AveClientContext context, Alert alert)
        {
            if (alert.AlertType == AlertType.Item)
            {
                context.Load(alert, al => al.ItemID);
            }
            if (alert.AlertFrequency != AlertFrequency.Immediate)
            {
                context.Load(alert, al => al.AlertTime);
            }
        }

        public override Dictionary<string, object> GetAlerts(string webServerRelativeUrl)
        {
            List<Dictionary<string, object>> alertPropertiesList = new List<Dictionary<string, object>>();
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                context.Load(web.Alerts, alerts => alerts.IncludeWithDefaultProperties(alert => alert.ListID, alert => alert.ListUrl));
                context.ExecuteQuery();
                foreach (var alert in web.Alerts)
                {
                    LoadAlertSpecialProperty(context, alert);
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }
                foreach (var alert in web.Alerts)
                {
                    Dictionary<string, object> alertProperties = LoadAlertProprty(alert);
                    alertPropertiesList.Add(alertProperties);
                }
            }
            return new Dictionary<string, object> { { AveObjectModelConstant.ChildrenProperties, alertPropertiesList } };
        }

        private Dictionary<string, object> LoadAlertProprty(Alert alert)
        {
            Dictionary<string, object> alertProperties = new Dictionary<string, object>();
            CopyProperty(alertProperties, alert);

            #region Reset Properties 
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (var property in alert.Properties)
            {
                properties.Add(property.Key, property.Value);
            }
            alertProperties.Add("Properties" + AveObjectModelConstant.ObjectPropertySuffix, properties);
            alertProperties.Remove("Properties");
            #endregion
            return alertProperties;
        }

        private Dictionary<string, object> LoadAlertProprty(AveClientContext context, Web web, Guid alertId)
        {
            var alert = web.Alerts.GetById(alertId);
            context.Load(alert);
            context.ExecuteQuery();
            LoadAlertSpecialProperty(context, alert);
            context.ExecuteQuery();
            return LoadAlertProprty(alert);
        }

        private Dictionary<string, object> AddAlert(string webServerRelativeUrl, Guid listId, int itemId, Dictionary<string, object> data)
        {
            List<Dictionary<string, object>> alertPropertiesList = new List<Dictionary<string, object>>();
            ClientResult<Guid> alertId = null;
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                var list = web.Lists.GetById(listId);
                var item = itemId > 0 ? list.GetItemById(itemId) : null;
                var user = web.SiteUsers.GetById((int)(((Dictionary<string, object>)data["User"])["UserId"]));
                var alertCreateInfo = new AlertCreationInformation
                {
                    List = list,
                    Item = item,
                    User = user,
                    AlertType = (AlertType)(int.Parse(data["AlertType"].ToString())),
                    Title = (string)data["AlertTitle"],
                    AlertFrequency = data.ContainsKey("NotifyFreq") ? (AlertFrequency)data["NotifyFreq"] : AlertFrequency.Immediate,
                    AlertTime = data.ContainsKey("NotifyTime") ? (DateTime)data["NotifyTime"] : default(DateTime),
                    EventType = (AlertEventType)data["EventType"],
                    Status = AlertStatus.Off,
                    DeliveryChannels = (AlertDeliveryChannel)data["DeliveryChannel"],
                    Filter = data.ContainsKey("Filter") ? data["Filter"].ToString() : string.Empty,
                    Properties = (Dictionary<string, string>)data["Properties"],
                    AlertTemplateName = data.ContainsKey("AlertTemplateName") ? data["AlertTemplateName"].ToString() : string.Empty,
                };
                alertId = web.Alerts.Add(alertCreateInfo);
                context.ExecuteQuery();
                return LoadAlertProprty(context, web, alertId.Value);
            }
        }
        public override Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            return AddAlert(webServerRelativeUrl, listId, itemId, data);
        }

        public override Dictionary<string, object> UpdateAlert(string webServerRelativeUrl, Guid alertId, bool sendEmail, Dictionary<string, object> needUpdateAlertProperties)
        {
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                var alert = web.Alerts.GetById(alertId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateAlertProperties, alert);
                alert.UpdateAlert();
                context.ExecuteQuery();
                return LoadAlertProprty(context, web, alertId);
            }
        }
        public override Dictionary<string, string> GetWebUserResource(string webServerRelativeUrl, string resourceName, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext())
            {
                UserResource resource;
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = web.TitleResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = web.DescriptionResource;
                        break;
                    default:
                        throw new Exception(string.Format("resource name is invalid.{0}", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }
        public override Dictionary<string, string> GetListUserResource(string webServerRelativeUrl, Guid id, string resourceName, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext())
            {
                UserResource resource;
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                var list = web.Lists.GetById(id);
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = list.TitleResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = list.DescriptionResource;
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }
        public override Dictionary<string, string> GetContentTypeUserResource(string webServerRelativeUrl, Guid listId, string resourceName, string contentTypeResourceName, string contentTypeId, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl))
            {
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                UserResource resource;
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                ContentTypeCollection contentTypes = null;
                switch (contentTypeResourceName)
                {
                    case "web.availableContentTypes":
                        contentTypes = web.AvailableContentTypes;
                        break;
                    case "web.contentTypes":
                        contentTypes = web.ContentTypes;
                        break;
                    case "list.contentTypes":
                        List list = web.Lists.GetById(listId);
                        contentTypes = list.ContentTypes;
                        break;
                    default:
                        break;
                };
                ObjectPath path = new ObjectPathMethod(context, contentTypes.Path, "GetById", new object[] { contentTypeId });
                ContentType ct = new ContentType(context, path);
                ClientResult<string> result = new ClientResult<string>();
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = ct.NameResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = ct.DescriptionResource;
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }
        public override Dictionary<string, string> GetFieldUserResource(string webServerRelativeUrl, Guid listId, string resourceName, string fieldResourceName, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProp, List<string> cultureNames)
        {
            using (AveClientContext context = CreateContext(AveUrlUtility.GetServerUrl(mWebUrl) + webServerRelativeUrl))
            {
                Dictionary<string, ClientResult<string>> values = new Dictionary<string, ClientResult<string>>();
                UserResource resource;
                Web web = context.Web;
                FieldCollection fields = null;
                switch (fieldResourceName)
                {
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    case "web.availableFields":
                        fields = web.AvailableFields;
                        break;
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fields = list.Fields;
                        break;
                    case "contentType.fields":
                        string id = contentTypeProp["ContentTypeId"] as string;
                        string contentTypeSource = contentTypeProp["ContentTypeSource"] as string;
                        ContentType contentType = GetContentTypeWithoutFields(context, webServerRelativeUrl, string.Empty, listId, contentTypeSource, id);
                        fields = contentType.Fields;
                        break;
                    default:
                        break;
                }
                Guid fieldId = GetFieldIdFromIdentity(fieldProp["ObjectPath"].ToString());
                ObjectPath path = new ObjectPathMethod(context, fields.Path, "GetById", new object[] { fieldId });
                Field field = Activator.CreateInstance(fieldProp["FieldType"] as Type, new object[] { context, path }) as Field;

                ClientResult<string> result = new ClientResult<string>();
                switch (resourceName)
                {
                    case AveUserResourceConstants.TITLE_RESOUCE:
                        resource = field.TitleResource;
                        break;
                    case AveUserResourceConstants.DESCRIPTION_RESOUCE:
                        resource = field.DescriptionResource;
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The resource {0} is not supported.", resourceName));
                }
                foreach (string cultureName in cultureNames)
                {
                    values.Add(cultureName, resource.GetValueForUICulture(cultureName));
                }
                context.ExecuteQuery();
                return values.ToDictionary(k => k.Key, v => v.Value.Value);
            }
        }
        internal override bool UpdateWebUserResource(Web web, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(web.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(web.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }
        internal override bool UpdateListUserResource(List list, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(list.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(list.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }
        internal override bool UpdateFieldUserResource(Field field, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(field.TitleResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(field.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }

        internal override bool UpdateContentTypeUserResource(ContentType contentType, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            change |= UpdateUserResource(contentType.NameResource, AveUserResourceConstants.TITLE_RESOUCE, changeProperties);
            change |= UpdateUserResource(contentType.DescriptionResource, AveUserResourceConstants.DESCRIPTION_RESOUCE, changeProperties);
            return change;
        }
        private bool UpdateUserResource(UserResource resource, string resourceName, Dictionary<string, object> changeProperties)
        {
            bool change = false;
            object changeResourcesObj;
            if (changeProperties.TryGetValue(resourceName, out changeResourcesObj))
            {
                changeProperties.Remove(resourceName);
                var changeResources = changeResourcesObj as Dictionary<string, string>;
                if (changeResources != null)
                {
                    foreach (var item in changeResources)
                    {
                        resource.SetValueForUICulture(item.Key, item.Value);
                        change = true;
                    }
                }
            }
            return change;
        }

        public override void UpdateSupportedUICulture(Dictionary<string, object> webProperties, Web web, ref bool changed)
        {
            if (webProperties.ContainsKey("SupportedUILanguageIds"))
            {
                List<int> languages = webProperties["SupportedUILanguageIds"] as List<int>;
                foreach (var info in languages)
                {
                    web.AddSupportedUILanguage(info);
                }
                changed = true;
            }
        }

        public override void RemoveThemeFromWeb(string webServerRelativeUrl, bool deleteFiles)
        {
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                var cssFolderUrl = string.Empty;
                if (deleteFiles)
                {
                    context.Load(web);
                    context.ExecuteQuery();
                    cssFolderUrl = web.ThemedCssFolderUrl;
                }
                web.ThemedCssFolderUrl = null;
                web.Update();
                context.ExecuteQuery();
                Folder folder = null;
                if (!string.IsNullOrEmpty(cssFolderUrl))
                {
                    try
                    {
                        if (IsSharedTheme(cssFolderUrl, web))
                        {
                            context.Load(context.Site.RootWeb);
                            context.ExecuteQuery();
                            folder = context.Site.RootWeb.GetFolderByServerRelativeUrl(cssFolderUrl);
                        }
                        else
                        {
                            folder = web.GetFolderByServerRelativeUrl(cssFolderUrl);
                        }
                        context.ExecuteQuery();
                        if (folder != null && folder.Exists)
                        {
                            folder.DeleteObject();
                            context.ExecuteQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("An error occourred while deleting theme folders. Error:{0}", e);
                    }
                }
            }
        }

        public override bool GetDenyAddAndCustomizePagesStatus()
        {
            var tenantSiteUrl = string.Empty;
            try
            {
                tenantSiteUrl = GetSPOAdminUrl(mWebUrl);
                using (AveClientContext context = CreateContext(tenantSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    SiteProperties sp = tenant.GetSitePropertiesByUrl(mWebUrl, true);
                    context.Load(sp, p => p.DenyAddAndCustomizePages);
                    context.ExecuteQuery();
                    return sp.DenyAddAndCustomizePages == DenyAddAndCustomizePagesStatus.Enabled;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get site DenyAddAndCustomizePages info error . Url : {0}  Error : {1}", tenantSiteUrl, ex.ToString());
            }
            return false;
        }

        private string GetSPOAdminUrl(string siteUrl)
        {
            mLogger.Info("start to get admin url by site url {0}", siteUrl);

            Uri siteUri = new Uri(siteUrl);
            int firstDotIndex = siteUri.Host.IndexOf('.');
            string domainPrefix = siteUri.Host.Substring(0, firstDotIndex);
            if (domainPrefix.EndsWith("-my", StringComparison.OrdinalIgnoreCase)
                || domainPrefix.EndsWith("-public", StringComparison.OrdinalIgnoreCase)
                || domainPrefix.EndsWith("-admin", StringComparison.OrdinalIgnoreCase))
            {
                domainPrefix = domainPrefix.Remove(domainPrefix.LastIndexOf('-'));
            }
            string domainSuffix = siteUri.Host.Substring(firstDotIndex, siteUri.Host.Length - firstDotIndex);
            return string.Format("https://{0}-admin{1}", domainPrefix, domainSuffix);
        }

        public override AveComplianceTagInfo GetListComplianceTagProperties(string listServerRelativeUrl)
        {
            var complianceTag = new AveComplianceTagInfo();
            try
            {
                using (AveClientContext context = CreateContext(mWebUrl))
                {
                    var listComplianceTagProperties = Microsoft.SharePoint.Client.CompliancePolicy.SPPolicyStoreProxy.GetListComplianceTag(context, listServerRelativeUrl);
                    context.ExecuteQuery();
                    if (listComplianceTagProperties != null)
                    {
                        if (listComplianceTagProperties.Value == null)
                        {
                            complianceTag.ComplianceTagValue = string.Empty;
                            complianceTag.BlockEdit = false;
                            complianceTag.BlockDelete = false;
                        }
                        else
                        {
                            complianceTag.ComplianceTagValue = listComplianceTagProperties.Value.TagName;
                            complianceTag.BlockEdit = listComplianceTagProperties.Value.BlockEdit;
                            complianceTag.BlockDelete = listComplianceTagProperties.Value.BlockDelete;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Failed to get the compliance tag properties. Info error, Url : {0}  Error : {1}", listServerRelativeUrl, ex.ToString());
                return null;
            }
            return complianceTag;
        }

        public override AveComplianceTagInfo UpdateListComplianceTagProperties(string listServerRelativeUrl, AveComplianceTagInfo properties)
        {
            AveComplianceTagInfo complianceTagInfo = new AveComplianceTagInfo();
            try
            {
                using (AveClientContext context = CreateContext(mWebUrl))
                {
                    Microsoft.SharePoint.Client.CompliancePolicy.SPPolicyStoreProxy.SetListComplianceTag(
                    context,
                    listServerRelativeUrl,
                    properties.ComplianceTagValue,
                    properties.BlockEdit,
                    properties.BlockDelete,
                    false);
                    context.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to set compliance tag setting for the list. Url: {0}, exception: {1}", listServerRelativeUrl, ex));
                return null;
            }
            return complianceTagInfo;
        }

        public override Dictionary<string, object> GetListItemComplianceTag(Guid webID, Guid listID, int rowID)
        {
            try
            {
                using (var context = CreateContext())
                {
                    var web = context.Site.OpenWebById(webID);
                    var list = web.Lists.GetById(listID);
                    var item = list.GetItemById(rowID);
                    context.Load(item);
                    context.Load(item, i => i.ComplianceInfo);
                    context.LoadQuery(web.SiteUsers.IncludeWithDefaultProperties(u => u.LoginName, u => u.Id));
                    context.ExecuteQuery();
                    // 如果ComplianceTag表示item没有设置label
                    if (string.IsNullOrEmpty(item.ComplianceInfo.ComplianceTag))
                    {
                        return new Dictionary<string, object>();
                    }
                    return AssembleComplianceTagInfo(item);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to get the compliance info from list item, webID: {0}, listID: {1}, rowID:{2}. Exception: {3}", webID, listID, rowID, ex));
            }
            return new Dictionary<string, object>();
        }

        public override Dictionary<string, object> SetComplianceTag(Guid webID, Guid listID, int rowID, AveItemComplianceTagInfo complianceSettingInfo)
        {
            try
            {
                using (var context = CreateContext())
                {
                    var web = context.Site.OpenWebById(webID);
                    var list = web.Lists.GetById(listID);
                    var item = list.GetItemById(rowID);
                    //item.SetComplianceTag(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag);

                    bool blockDel = (complianceSettingInfo.ComplianceSettingFlag & 1) != 0;
                    bool blockEdit = (complianceSettingInfo.ComplianceSettingFlag & 4) != 0;
                    bool changed = (complianceSettingInfo.ComplianceSettingFlag & 2) != 0;
                    item.SetComplianceTagWithMetaInfo(complianceSettingInfo.ComplianceTag, blockDel, blockEdit, complianceSettingInfo.ComplianceWrittenDate, complianceSettingInfo.ComplianceUserLoginName, false);
                    //item.SetComplianceTagWithExplicitMetasUpdate(complianceTag, complianceSettingFlags, complianceWrittenDate, string.Empty);
                    context.Load(item);
                    context.Load(item, i => i.ComplianceInfo);
                    context.ExecuteQuery();
                    return AssembleComplianceTagInfo(item);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to set the compliance info from list item, webID: {0}, listID: {1}, rowID:{2}. Exception: {3}", webID, listID, rowID, ex));
            }
            return new Dictionary<string, object>();
        }

        private Dictionary<string, object> AssembleComplianceTagInfo(ListItem item)
        {
            Dictionary<string, object> complianceTagInfo = new Dictionary<string, object>();
            Dictionary<string, object> complianceTagInfoProperties = new Dictionary<string, object>();
            complianceTagInfoProperties.Add("ComplianceTag", item.ComplianceInfo.ComplianceTag);
            complianceTagInfoProperties.Add("TagPolicyHold", item.ComplianceInfo.TagPolicyHold);
            complianceTagInfoProperties.Add("TagPolicyRecord", item.ComplianceInfo.TagPolicyRecord);
            complianceTagInfoProperties.Add("TagPolicyEventBased", item.ComplianceInfo.TagPolicyEventBased);
            if (item.FieldValues.ContainsKey("_ComplianceFlags") && !string.IsNullOrEmpty(item["_ComplianceFlags"].ToString()))
            {
                complianceTagInfoProperties.Add("ComplianceFlags", Int32.Parse(item["_ComplianceFlags"].ToString()));
            }
            else
            {
                complianceTagInfoProperties.Add("ComplianceFlags", 0);
            }

            DateTime date = DateTime.MinValue;
            if (item.FieldValues.ContainsKey("_ComplianceTagWrittenTime"))
            {
                var dateString = item["_ComplianceTagWrittenTime"].ToString();

                if (!string.IsNullOrEmpty(dateString))
                {
                    if (!DateTime.TryParse(dateString, out date))
                    {
                        mLogger.Warn(string.Format("Failed to set the compliance apply date from list item, _ComplianceTagWrittenTime value: {0}", dateString));
                    }
                }
            }
            complianceTagInfoProperties.Add("ComplianceWrittenDate", date.ToUniversalTime());

            if (item.FieldValues.ContainsKey("_ComplianceTagUserId") && !string.IsNullOrEmpty(item["_ComplianceTagUserId"].ToString()))
            {
                complianceTagInfoProperties.Add("ComplianceTagUserId", Int32.Parse(item["_ComplianceTagUserId"].ToString()));
            }
            else
            {
                complianceTagInfoProperties.Add("ComplianceTagUserId", -1);
            }

            complianceTagInfo.Add("ComplianceTagInfo" + AveObjectModelConstant.ObjectPropertySuffix, complianceTagInfoProperties);
            return complianceTagInfo;
        }

        public override void PostRestoreModernWebpart(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo sourceSiteInfo)
        {
            new SharePointDocumentDataProcessor(site, mapping, sourceSiteInfo).PostActionImpl();
        }

        public override void ConvertUserIdInfo(Dictionary<string, object> userProperties)
        {
            if (userProperties.ContainsKey("UserId"))
            {
                var info = userProperties["UserId"] as UserIdInfo;
                if (info != null)
                {
                    userProperties["UserId" + AveObjectModelConstant.ObjectPropertySuffix] = new Dictionary<string, object>() {
                        { "NameId",info.NameId },
                        { "NameIdIssuer",info.NameIdIssuer}
                    };
                }
                userProperties.Remove("UserId");
            }
        }

        protected override void GetWebTemplate(AveWebBrowserInfo info, Web web, AveClientContext context)
        {
            info.TemplateName = web.WebTemplate + "#" + web.Configuration;

            info.TemplateTitle = GetWebTemplateTitle(web.Language, info.TemplateName, context);
        }

        private string GetWebTemplateTitle(uint language, string templateName, AveClientContext context)
        {
            var webTemplates = context.Site.GetWebTemplates(language, 15);
            var webTemplate = webTemplates.GetByName(templateName);
            context.Load(webTemplate, tempalte => tempalte.Title);
            context.ExecuteQuery();

            return webTemplate.Title;
        }

        public override string GetWebTemplateTitle(string siteUrl, uint language, string templateName)
        {
            using (AveClientContext context = CreateContext())
            {
                return GetWebTemplateTitle(language, templateName, context);
            }
        }

        public override string GetServerVersion()
        {
            using (AveClientContext context = CreateContext())
            {
                context.ExecuteQuery();
                return context.ServerVersion.ToString();
            }
        }


    }
}

