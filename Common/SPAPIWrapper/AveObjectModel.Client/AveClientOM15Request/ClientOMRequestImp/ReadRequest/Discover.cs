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
namespace AvePoint.ObjectModel.ClientOM
{

    using AvePoint.Common.Portal;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using ClientFolder = Microsoft.SharePoint.Client.Folder;
    using SPChangeType = Microsoft.SharePoint.Client.ChangeType;

    public partial class AveClientOM2013Request
    {
        #region IAveDiscoverQuery

        public int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache)
        {
            using (var context = CreateRetryContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                query.File = false;
                query.View = false;
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;1;" + siteId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;1;" + siteId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate;
                //changeCache的初始化操作，不能放入while(true)中，否则每一次循环开始，都会把已经获取的数据清空
                changeCache["ChangedSiteCache"] = new Dictionary<Guid, object>();
                changeCache["ChangedWebCache"] = new Dictionary<Guid, object>();
                changeCache["ChangedListCache"] = new Dictionary<Guid, object>();
                //changeCache["ChangedItemsCache"] = new Dictionary<string, object>();
                //Dictionary<string, object> changedItemsCache = changeCache["ChangedItemsCache"] as Dictionary<string, object>;
                //changedItemsCache["ChangedFolderCache"] = new Dictionary<Guid, object>();
                //changedItemsCache["ChangedFileCache"] = new Dictionary<Guid, object>();
                //changedItemsCache["ChangedItemCache"] = new Dictionary<string, object>();
                using (var changeEventLogger = new IBChangeReporter(siteId))
                {
                    while (true)
                    {
                        ChangeCollection changedCollection = context.Site.GetChanges(query);
                        context.Load(changedCollection);
                        context.ExecuteQuery();
                        WriteChangeReport(changedCollection, changeEventLogger);
                        ConvertToContainerChangeObject(changedCollection, changeCache);
                        if (changedCollection.Count < 1000)
                        {
                            break;
                        }
                        query.ChangeTokenStart = changedCollection[999].ChangeToken;
                    }
                }
                if ((changeCache["ChangedSiteCache"] as Dictionary<Guid, object>).Count > 0)
                {
                    return 2;
                }
                return 0;
            }
        }
        public int GetListChangedForRecords(Guid webId, Guid listId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache)
        {
            using (var context = CreateRetryContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                query.Item = true;
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;3;" + listId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;3;" + listId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate;
                //changeCache的初始化操作，不能放入while(true)中，否则每一次循环开始，都会把已经获取的数据清空
                //changeCache["ChangedSiteCache"] = new Dictionary<Guid, object>();
                //changeCache["ChangedWebCache"] = new Dictionary<Guid, object>();
                //changeCache["ChangedListCache"] = new Dictionary<Guid, object>();
                changeCache["ChangedItemsCache"] = new Dictionary<string, object>();
                Dictionary<string, object> changedItemsCache = changeCache["ChangedItemsCache"] as Dictionary<string, object>;
                changedItemsCache["ChangedFolderCache"] = new Dictionary<Guid, object>();
                changedItemsCache["ChangedFileCache"] = new Dictionary<Guid, object>();
                changedItemsCache["ChangedItemCache"] = new Dictionary<string, object>();
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                //context.Load(web, w => w.Url);
                //context.Load(list, l => l.Id, l => l.BaseType,
                //    l => l.RootFolder.ServerRelativeUrl,
                //    l => l.ParentWeb.Id, l => l.Id);
                context.ExecuteQuery();
                using (var changeEventLogger = new IBChangeReporter(listId))
                {
                    while (true)
                    {
                        ChangeCollection changedCollection = list.GetChanges(query);
                        context.Load(changedCollection);
                        context.ExecuteQuery();
                        WriteChangeReport(changedCollection, changeEventLogger);
                        ConvertToChangeObject(changedCollection, changeCache);
                        if (changedCollection.Count < 1000)
                        {
                            break;
                        }
                        query.ChangeTokenStart = changedCollection[999].ChangeToken;
                    }
                }
                //if ((changeCache["ChangedSiteCache"] as Dictionary<Guid, object>).Count > 0)
                //{
                //    return 2;
                //}
                return 0;
            }
        }

        public int GetListChangedCount(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (var context = CreateRetryContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                query.Item = true;
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;3;" + listId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;3;" + listId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate;
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                int listChangeCount = 0;
                context.ExecuteQuery();
                while (true)
                {
                    ChangeCollection changedCollection = list.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();
                    listChangeCount += changedCollection.Count;
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }
                return listChangeCount;
            }
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (var context = CreateRetryContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                query.Item = true;
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;3;" + listId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;3;" + listId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate;
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);

                context.ExecuteQuery();
                Dictionary<string, object> changedItemCache = new Dictionary<string, object>();
                while (true)
                {
                    ChangeCollection changedCollection = list.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();
                    changedItemCache.AddRange(GetChangeItemObject(changedCollection));
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }

                return changedItemCache;
            }
        }

        public Dictionary<string, object> GetListDeletedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (var context = CreateRetryContext())
            {
                ChangeQuery query = new ChangeQuery(true, true)
                {
                    Item = true
                };
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;3;" + listId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;3;" + listId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate;
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                context.ExecuteQuery();

                Dictionary<string, object> deletedItemCache = new Dictionary<string, object>();
                while (true)
                {
                    ChangeCollection changedCollection = list.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();
                    var allItems = GetChangeItemObject(changedCollection);
                    foreach (var kv in allItems)
                    {
                        if (kv.Value is Dictionary<string, object> itemProps && itemProps.TryGetValue("ChangeType", out var changeTypeObj))
                        {
                            int changeTypeInt;
                            if (changeTypeObj is int i)
                            {
                                changeTypeInt = i;
                            }
                            else if (changeTypeObj is AvePoint.Wrapper.Common.ChangeType ct)
                            {
                                changeTypeInt = (int)ct;
                            }
                            else
                            {
                                try
                                {
                                    changeTypeInt = Convert.ToInt32(changeTypeObj);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                            // 4 corresponds to Delete
                            if (changeTypeInt == 4)
                            {
                                deletedItemCache[kv.Key] = kv.Value;
                            }
                        }
                    }
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }
                return deletedItemCache;
            }
        }

        public Dictionary<string, object> GetFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime)
        {
            using (var context = CreateRetryContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                query.Item = true;
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;3;" + listId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;3;" + listId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate;
                var web = context.Site.OpenWebById(webId);
                //var list = web.Lists.GetById(listId);
                var folder = web.GetFolderById(folderId);
                context.ExecuteQuery();
                Dictionary<string, object> changedItemCache = new Dictionary<string, object>();
                while (true)
                {
                    ChangeCollection changedCollection = folder.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();
                    changedItemCache.AddRange(GetChangeItemObject(changedCollection));
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }

                return changedItemCache;
            }
        }

        public Dictionary<string, object> GetFolderAndSubFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime)
        {
            using (var context = CreateRetryContext())
            {
                ChangeQuery query = new ChangeQuery(true, true);
                query.Item = true;
                ChangeToken startToken = new ChangeToken();
                ChangeToken endToken = new ChangeToken();
                startToken.StringValue = "1;3;" + listId.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
                endToken.StringValue = "1;3;" + listId.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
                query.ChangeTokenStart = startToken;
                query.ChangeTokenEnd = endToken;
                query.SystemUpdate = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeSystemUpdate;
                var web = context.Site.OpenWebById(webId);
                //var list = web.Lists.GetById(listId);
                var folder = web.GetFolderById(folderId);
                context.ExecuteQuery();
                Dictionary<string, object> changedItemCache = new Dictionary<string, object>();
                while (true)
                {
                    ChangeCollection changedCollection = folder.GetChanges(query);
                    context.Load(changedCollection);
                    context.ExecuteQuery();
                    changedItemCache.AddRange(GetChangeItemObject(changedCollection));
                    if (changedCollection.Count < 1000)
                    {
                        break;
                    }
                    query.ChangeTokenStart = changedCollection[999].ChangeToken;
                }

                // Get changes for subfolders
                var subFolders = folder.Folders;
                context.Load(subFolders);
                context.ExecuteQuery();

                foreach (var subFolder in subFolders)
                {
                    var subFolderChanges = GetFolderAndSubFolderChangedItems(webId, listId, subFolder.UniqueId, startTime, endTime);
                    foreach (var item in subFolderChanges)
                    {
                        changedItemCache[item.Key] = item.Value;
                    }
                }

                return changedItemCache;
            }
        }

        class IBChangeReporter : IDisposable
        {
            private static GCommon.AveLogger mLogger = GCommon.AveLogger.GetInstance(typeof(IBChangeReporter));
            protected StreamWriter ReportWriter { get; set; }
            public IBChangeReporter(Guid siteId)
            {
                try
                {
                    if (!string.IsNullOrEmpty(WrapperConfiguration.JobDir) && WrapperConfiguration.JobDir != WrapperConfiguration.JobDirDefaultValue)
                    {
                        string changeReportPath = Path.Combine(WrapperConfiguration.JobDir, string.Format("{0}_DetailChangeReport_{1}_{2}.csv", Process.GetCurrentProcess().ProcessName, siteId, DateTime.Now.ToString("yyyyMMddHHmmss")));
                        ReportWriter = new StreamWriter(changeReportPath);

                        ReportWriter.AutoFlush = true;
                        WriteLine("ChangeObject,ChangeType,ChangeTime,WebId,ListId,UniqueId,ItemId,Extension1");

                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("Init change report logger failed.Error:{0}", e);
                    ReportWriter = null;
                }
            }

            public void WriteLine(string format, params object[] param)
            {
                if (ReportWriter != null)
                {
                    ReportWriter.WriteLine(format, param);
                }
            }

            public void Write(string format, params object[] param)
            {
                if (ReportWriter != null)
                {
                    ReportWriter.Write(format, param);
                }
            }

            protected void FinishWrite()
            {
                if (ReportWriter != null)
                {
                    ReportWriter.Close();
                }
            }

            public void Dispose()
            {
                FinishWrite();
            }
        }

        private void WriteChangeReport(ChangeCollection changes, IBChangeReporter writer)
        {
            try
            {
                foreach (Change cObj in changes)
                {
                    writer.Write("{0},{1},{2},", cObj.GetType(), cObj.ChangeType, cObj.Time);
                    switch (cObj.GetType().ToString())
                    {
                        case "Microsoft.SharePoint.Client.ChangeWeb":
                            var web = cObj as ChangeWeb;
                            writer.WriteLine("{0},{1},{2},{3},{4}", web.WebId, "", web.WebId, "", "");
                            break;
                        case "Microsoft.SharePoint.Client.ChangeList":
                            var list = cObj as ChangeList;
                            writer.WriteLine("{0},{1},{2},{3},{4}", list.WebId, list.ListId, list.ListId, "", "");
                            break;
                        case "Microsoft.SharePoint.Client.ChangeItem":
                            var item = cObj as ChangeItem;
                            writer.WriteLine("{0},{1},{2},{3},{4}", item.WebId, item.ListId, item.UniqueId, item.ItemId, "");
                            break;
                        case "Microsoft.SharePoint.Client.ChangeView":
                            var view = cObj as ChangeView;
                            writer.WriteLine("{0},{1},{2},{3},{4}", view.WebId, view.ListId, view.ViewId, "", "");
                            break;
                        case "Microsoft.SharePoint.Client.ChangeFile":
                            var file = cObj as ChangeFile;
                            writer.WriteLine("{0},{1},{2},{3},{4}", file.WebId, "", file.UniqueId, "", "");
                            break;
                        case "Microsoft.SharePoint.Client.ChangeFolder":
                            var folder = cObj as ChangeFolder;
                            writer.WriteLine("{0},{1},{2},{3},{4}", folder.WebId, "", folder.UniqueId, "", "");
                            break;
                        case "Microsoft.SharePoint.Client.ChangeGroup":
                            var group = cObj as ChangeGroup;
                            writer.WriteLine("{0},{1},{2},{3},{4}", "", "", "", "", group.GroupId);
                            break;
                        case "Microsoft.SharePoint.Client.ChangeUser":
                            var user = cObj as ChangeUser;
                            writer.WriteLine("{0},{1},{2},{3},{4}", "", "", "", "", user.UserId);
                            break;
                        case "Microsoft.SharePoint.Client.ChangeField":
                            var field = cObj as ChangeField;
                            writer.WriteLine("{0},{1},{2},{3},{4}", field.WebId, "", "", "", field.FieldId);
                            break;
                        case "Microsoft.SharePoint.Client.ChangeContentType":
                            var ct = cObj as ChangeContentType;
                            writer.WriteLine("{0},{1},{2},{3},{4}", ct.WebId, "", "", "", ct.ContentTypeId);
                            break;
                        case "Microsoft.SharePoint.Client.ChangeAlert":
                            var alert = cObj as ChangeAlert;
                            writer.WriteLine("{0},{1},{2},{3},{4}", alert.WebId, "", "", "", alert.AlertId);
                            break;
                        default:
                            writer.WriteLine("{0},{1},{2},{3},{4}", "", "", "", "", "");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("write change report failed, error:{0}", e);
            }
        }

        [Obsolete]
        public Dictionary<int, object> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            return null;
        }

        public Dictionary<Guid, object> QueryWebForIB(Dictionary<Guid, object> changedWebsInfo)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<Guid, object> changedWebsProperties = new Dictionary<Guid, object>();
                foreach (KeyValuePair<Guid, object> pair in changedWebsInfo)
                {
                    Dictionary<string, object> change = pair.Value as Dictionary<string, object>;
                    if (change != null)
                    {
                        Dictionary<string, object> webProp = new Dictionary<string, object>();
                        if (!changedWebsProperties.ContainsKey(pair.Key))
                        {
                            AveChangeType changeType = (AveChangeType)change["ChangeType"];
                            webProp["ChangeType"] = (int)changeType;
                            webProp["WebID"] = pair.Key;
                            webProp["EventTime"] = change["Time"];
                            if (change.ContainsKey("NavigationChanged"))
                            {
                                webProp["NavigationChanged"] = change["NavigationChanged"];
                            }
                            if (changeType != AveChangeType.Delete)
                            {
                                try
                                {
                                    Web web = context.Site.OpenWebById(pair.Key);
                                    string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                                    GetWebPropertiesForIB(web, context.Url, siteServerRelativeUrl, false, webProp);
                                }
                                catch (ServerException se)
                                { //SAAS-21467 如果获取的异常为file not found则将change type置为delete
                                    if (string.Equals(se.ServerErrorTypeName, "System.IO.FileNotFoundException"))
                                    {
                                        mLogger.Error("Query Web ID has an error,Web ID:{0} ChangeType:{1} error details:{2}", pair.Key.ToString(), webProp["ChangeType"], se.ToString());
                                        webProp["ChangeType"] = (int)AveChangeType.Delete;
                                    }
                                }
                            }
                            changedWebsProperties.Add(pair.Key, webProp);
                        }
                    }
                }
                return changedWebsProperties;
            }
        }

        public Dictionary<string, object> QueryRootWeb(Guid siteId)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.RootWeb;
                context.Load(web, w => w.Id, w => w.Title, w => w.ServerRelativeUrl, w => w.AppInstanceId);
                context.ExecuteQuery();
                string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                Dictionary<string, object> webDictionary = new Dictionary<string, object>();
                AssembleDiscoverWebProperties(webDictionary, web, siteServerRelativeUrl);
                return webDictionary;
            }
        }

        public Dictionary<string, object> QueryWeb(Guid webId)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWebById(webId);
                context.Load(web, w => w.Id, w => w.Title, w => w.ServerRelativeUrl, w => w.AppInstanceId);
                context.ExecuteQuery();
                string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                Dictionary<string, object> webDictionary = new Dictionary<string, object>();
                AssembleDiscoverWebProperties(webDictionary, web, siteServerRelativeUrl);
                return webDictionary;
            }
        }

        public Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<Guid, object> webProperties = new Dictionary<Guid, object>();
                Web web = context.Site.OpenWebById(parentWebId);
                WebCollection webs = web.Webs;
                context.Load(webs, collection => collection.Include(w => w.Id, w => w.Title, w => w.ServerRelativeUrl, w => w.AppInstanceId));
                context.Load(context.Site, site => site.ServerRelativeUrl);
                context.ExecuteQuery();
                foreach (Web subWeb in webs)
                {
                    Dictionary<string, object> subWebProperty = new Dictionary<string, object>();
                    AssembleDiscoverWebProperties(subWebProperty, subWeb, context.Site.ServerRelativeUrl);
                    webProperties.Add(subWeb.Id, subWebProperty);
                }
                return webProperties;
            }
        }

        public Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<Guid, object> changedListCache)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<Guid, object> lists = new Dictionary<Guid, object>();
                Web web = null;
                if (changedListCache.Count > 0)
                {
                    web = context.Site.OpenWebById(webId);
                }
                foreach (KeyValuePair<Guid, object> pair in changedListCache)
                {
                    Dictionary<string, object> change = pair.Value as Dictionary<string, object>;
                    if (change != null)
                    {
                        if (change.ContainsKey("WebId"))
                        {
                            Guid id = new Guid(change["WebId"].ToString());
                            if (id == webId)
                            {
                                Dictionary<string, object> listProp = new Dictionary<string, object>();
                                AveChangeType changeType = (AveChangeType)change["ChangeType"];
                                listProp["ChangeType"] = (int)changeType;
                                listProp["ListId"] = pair.Key;
                                if (changeType != AveChangeType.Delete)
                                {
                                    List list = web?.Lists.GetById(pair.Key);
                                    if (list == null)
                                    {
                                        continue;
                                    }
                                    context.Load(list);
                                    context.Load(list.RootFolder);
                                    context.ExecuteQuery();
                                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                                    CopyProperty(listProp, list);
                                    CopyProperty(rootFolderProp, list.RootFolder);
                                    long flag = 0;
                                    if (list.EnableVersioning)
                                        flag |= 0x0000000000000080;
                                    if (!list.EnableAttachments)
                                        flag |= 0x0000000000000008;
                                    listProp["Flag"] = flag;
                                    listProp["Name"] = listProp["Title"];
                                    listProp["Type"] = listProp["BaseType"];
                                    listProp["RootFolderUrl"] = rootFolderProp["ServerRelativeUrl"];
                                    listProp["ServerTemplate"] = listProp["BaseTemplate"];
                                    if (rootFolderProp.ContainsKey("UniqueId"))
                                    {
                                        listProp["RootFolderId"] = rootFolderProp["UniqueId"];
                                    }
                                    else
                                    {
                                        listProp["RootFolderId"] = Guid.Empty;
                                    }
                                }
                                lists.Add(pair.Key, listProp);
                            }
                        }
                    }
                }
                return lists;
            }
        }

        public Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<string, object> changedCache, DateTime startTime, DateTime endTime)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<Guid, object> lists = new Dictionary<Guid, object>();
                Web web = context.Site.OpenWebById(webId);
                context.Load(web);
                context.Load(web.Lists);
                context.Load(web.Lists, ls => ls.Include(l => l.RootFolder));
                context.ExecuteQuery();
                //webRootFolder

                //list
                Dictionary<Guid, object> changedListCache = changedCache["ChangedListCache"] as Dictionary<Guid, object>;
                //Dictionary<string, object> changedItemsCache = changedCache["ChangedItemsCache"] as Dictionary<string, object>;
                //Dictionary<Guid, object> changedFileCache = changedItemsCache["ChangedFileCache"] as Dictionary<Guid, object>;
                Dictionary<Guid, AveChangeType> changeListDic = GetChangeListFormChangeListCache(context, web, changedListCache, lists);
                foreach (List list in web.Lists)
                {
                    bool isListChanged = false;
                    AveChangeType changeType = changeListDic.ContainsKey(list.Id) ? changeListDic[list.Id] : AveChangeType.None;
                    if (changeType == AveChangeType.Add || changeType == AveChangeType.Restore)
                    {
                        lists[list.Id] = AssembleChangeListProperties(list, changeType);
                        continue;
                    }
                    if (WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView)
                    {
                        isListChanged = GetListFileChanged(context, list, null, startTime, endTime);
                    }
                    if (isListChanged || changeType != AveChangeType.None)
                    {
                        lists[list.Id] = AssembleChangeListProperties(list, changeType);
                    }
                }
                return lists;
            }
        }

        public Dictionary<Guid, object> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<Guid, object> views = new Dictionary<Guid, object>();
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                ViewCollection viewColl = list.Views;
                //context.Load(viewColl, vc => vc.Include(v => v.Id, v => v.PersonalView, v => v.BaseViewId, v => v.Title, v => v.ServerRelativeUrl));
                context.Load(viewColl);
                context.ExecuteQuery();
                foreach (View view in viewColl)
                {
                    Dictionary<string, object> viewPro = new Dictionary<string, object>();
                    ClientFile file = web.GetFileByServerRelativePath(view.ServerRelativePath);
                    ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                    using (exceptionScope.StartScope())
                    {
                        using (exceptionScope.StartTry())
                        {
                            context.Load(file, f => f.ETag, f => f.Name, f => f.ListItemAllFields, f => f.UIVersion, f => f.TimeLastModified, f => f.Level);
                        }
                        using (exceptionScope.StartCatch())
                        {
                            context.Load(file, f => f.ETag, f => f.Name, f => f.UIVersion, f => f.TimeLastModified, f => f.Level);
                        }
                    }
                    try
                    {
                        context.ExecuteQuery();
                        if (exceptionScope.HasException)
                        {
                            mLogger.Warn("query list view failed.view url:{0}. error message:{1}.", view.ServerRelativeUrl, exceptionScope.ErrorMessage);
                        }
                        AssembleDiscoverViewProperties(viewPro, view, file);
                        viewPro["CheckoutUserId"] = (int?)null;
                        views[view.Id] = viewPro;
                    }
                    catch (ServerException ex)
                    {
                        mLogger.Warn("query list view failed.view url:{0},view hidden:{1}. exception:{2}.", view.ServerRelativeUrl, view.Hidden, ex);
                    }
                }
                return views;
            }
        }

        public Dictionary<string, object> QueryListRootFolder(Guid siteId, Guid webId, Guid mlistId)
        {
            {
                Dictionary<string, object> folderPro = new Dictionary<string, object>();
                var folderServerRelativeUrl = string.Empty;
                try
                {
                    using var context = CreateRetryContext();
                    Web web = context.Site.OpenWebById(webId);
                    List list = web.Lists.GetById(mlistId);
                    Folder folder = list.RootFolder;
                    context.Load(folder, f => f.ServerRelativeUrl);
                    context.ExecuteQuery();
                    folderServerRelativeUrl = folder.ServerRelativeUrl;
                }
                catch (ServerUnauthorizedAccessException ex)
                {
                    mLogger.Debug($"Load root folder failed. {ex.Message}");
                    
                    using var context = CreateRetryContext();
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(web, w => w.ServerRelativeUrl);
                    context.Load(
                        web.Lists,
                        ls => ls.Include(
                            l => l.BaseTemplate
                        ).Where(l => l.Id == mlistId));
                    context.ExecuteQuery();
                    var list = web.Lists.FirstOrDefault();
                    if (list?.BaseTemplate == (int)AveListTemplateType.UserInformation)
                    {
                        folderServerRelativeUrl = $"{web.ServerRelativeUrl}/_catalogs/users";
                    }
                    else
                    {
                        throw;
                    }
                }
                

                if (!string.IsNullOrEmpty(folderServerRelativeUrl))
                {
                    string serverRelativeUrl = folderServerRelativeUrl.Trim('/');
                    if (serverRelativeUrl.Contains("/"))
                    {
                        int index = serverRelativeUrl.LastIndexOf('/');
                        folderPro["DirName"] = serverRelativeUrl.Substring(0, index);
                        folderPro["LeafName"] = serverRelativeUrl.Substring(index + 1);
                    }
                    else
                    {
                        folderPro["DirName"] = "";
                        folderPro["LeafName"] = serverRelativeUrl;
                    }
                    folderPro["FullUrl"] = folderServerRelativeUrl;
                }
                else
                {
                    throw new MissingMemberException("there is no ServerRelativeUrl");
                }
                folderPro["Size"] = 0;    //Can not get this property.
                #region there is no following parameters in web root folder, so we set them as default value
                folderPro["Type"] = Convert.ToByte(2);
                folderPro["Level"] = Convert.ToByte(1);
                folderPro["ID"] = null;
                folderPro["DocID"] = Guid.Empty;
                folderPro["CheckoutUserId"] = (int?)null;
                folderPro["Hidden"] = (bool?)true;
                folderPro["UIVersion"] = 512;
                folderPro["DocFlags"] = 0;
                folderPro["HasStream"] = 0;
                folderPro["ParentID"] = Guid.Empty;
                folderPro["TimeLastModified"] = DateTime.MinValue;
                folderPro["IsCurrentVersion"] = (bool?)true;
                folderPro["QueryType"] = 2;
                #endregion

                return folderPro;
            }
        }

        public Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> itemPro = null;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(web, w => w.ServerRelativeUrl);
                context.Load(list, l => l.BaseType, l => l.EnableMinorVersions, l => l.EnableVersioning, l => l.BaseTemplate);
                ListItem item = null;
                if (id == Guid.Empty)
                {
                    //当传入的item id为空时，采用dirName和leafName来找该item
                    item = GetListItemByDirName(context, list, "/" + dirName.TrimStart('/'), leafName);
                }
                else
                {
                    item = GetListItemByUniqueId(context, list, id);
                }
                if (item != null)
                {
                    itemPro = new Dictionary<string, object>();
                    itemPro["Attachments"] = new List<Dictionary<string, object>>();
                    AssembleDiscoverItemProperties(itemPro, item);
                    itemPro["RbsId"] = null;
                    if (Convert.ToInt16(itemPro["Type"]) == 1)
                    {//Folder
                        itemPro["ObjType"] = 4;
                    }
                    else
                    {
                        itemPro["ObjType"] = 1; //Item
                    }
                    itemPro["CheckoutUserId"] = (int?)null;
                    if (list.BaseType == BaseType.DocumentLibrary && Convert.ToInt16(itemPro["Type"]) == 0)//Item有可能是Folder或者File，当Item是File的时候才需要load File的信息
                    {
                        if (Convert.ToInt16(itemPro["Type"]) != 1)
                        {//Document
                            itemPro["ObjType"] = 2;
                        }
                        ClientFile file = list.ParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(itemPro["FullUrl"].ToString()));

                        ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                        using (excepScope.StartScope())
                        {
                            using (excepScope.StartTry())
                            {
                                context.Load(file, f => f.CheckedOutByUser);
                            }
                            using (excepScope.StartCatch())
                            {
                                context.Load(file);
                            }
                        }
                        context.ExecuteQuery();
                        if (excepScope.HasException)
                        {
                            mLogger.Warn("Get File CheckedOutByUser Error, FileUrl:{0} , Error Message:{1}", file.ServerRelativeUrl, excepScope.ErrorMessage);
                        }
                        if (file.IsObjectPropertyInstantiated("CheckedOutByUser") && file.IsPropertyAvailable("Id"))
                        //if (!file.CheckedOutByUser.ServerObjectIsNull.Value)
                        {
                            itemPro["CheckoutUserId"] = (int?)file.CheckedOutByUser.Id;
                        }
                    }

                    itemPro["Versions"] = LoadVersionsForItem(web, listId, item, itemPro);
                }
                return itemPro;
            }
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId)
        {
            using (var context = CreateRetryContext())
            {
                DateTime time = DateTime.MinValue;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType);
                ListItem item = GetListItemBytpGuid(context, list, tp_Guid);
                if (item != null)
                {
                    time = (DateTime)item.FieldValues["Modified"];
                    docId = (Guid)item.FieldValues["UniqueId"];
                }
                return time;
            }
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
        {
            using (var context = CreateRetryContext())
            {
                DateTime time = DateTime.MinValue;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType);
                ListItem item = GetListItemByUniqueId(context, list, id);
                if (item != null)
                {
                    time = (DateTime)item.FieldValues["Modified"];
                }
                return time;
            }
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            using (var context = CreateRetryContext())
            {
                DateTime time = DateTime.MinValue;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseType);
                context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                ListItem item = GetListItemByDirName(context, list, dirName, leafName);
                if (item != null)
                {
                    time = (DateTime)item.FieldValues["Modified"];
                    docId = (Guid)item.FieldValues["UniqueId"];
                }
                else//得到listrootfolder下的系统文件
                {
                    Dictionary<string, object> viewProperty = GetViewItem(web, list.RootFolder.ServerRelativeUrl, (list.BaseType.Equals(BaseType.GenericList)), dirName, leafName);
                    if (viewProperty != null)
                    {
                        time = (DateTime)viewProperty["TimeLastModified"];
                        docId = (Guid)viewProperty["DocID"];
                    }
                }
                return time;
            }
        }

        [Obsolete]
        public Dictionary<Guid, object> QueryListAlertForIB(Guid siteId, Guid webId, Guid mlistId)
        {
            return null;
        }

        [Obsolete]
        public Dictionary<Guid, object> QueryListViewForIB(Guid siteId, Guid webId, Guid mlistId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, object> QueryWebListForFB(Guid siteId, Guid webId, bool throwException = false)
        {
            Dictionary<Guid, object> lists = new Dictionary<Guid, object>();
            try
            {
                Dictionary<string, object> allListProperties = GetListsLightly(webId);
                var listPropertiesList = allListProperties.GetChildren();
                foreach (var listProperties in listPropertiesList)
                {
                    Dictionary<string, object> list = new Dictionary<string, object>();
                    list.Add("ListId", listProperties["Id"]);
                    list.Add("Name", listProperties["Title"]);
                    list.Add("Title", listProperties["Title"]);
                    list.Add("Type", listProperties["BaseType"]);
                    list.Add("ItemCount", listProperties["ItemCount"]);
                    list.Add("Flag", listProperties["Flag"]);    //Can not get this property.
                    Dictionary<string, object> rootFolder = listProperties["RootFolderObject"] as Dictionary<string, object>;
                    list.Add("RootFolderUrl", rootFolder["ServerRelativeUrl"]);
                    list.Add("Hidden", listProperties["Hidden"]);
                    list.Add("ServerTemplate", listProperties["BaseTemplate"]);
                    if (rootFolder.ContainsKey("UniqueId"))
                    {
                        list.Add("RootFolderId", rootFolder["UniqueId"]);
                    }
                    else
                    {
                        list.Add("RootFolderId", Guid.Empty);
                    }
                    list.Add("ListTemplate", (int)listProperties["BaseTemplate"]);
                    lists[(Guid)listProperties["Id"]] = list;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Query Lists For Web:{0} failed.Error Message:{1}", webId, ex.ToString());
                if (throwException)
                {
                    throw;
                }
            }
            return lists;
        }

        public Dictionary<string, object> QueryCurrentFolder(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, string listUrl)
        {
            return QueryListItemForFB(siteId, webId, listId, folderId, folderUrl, true, true);
        }

        [Obsolete]
        public Dictionary<int, object> GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId)
        {
            return null;
        }

        public Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId)
        {
            using (var context = CreateRetryContext())
            {
                Guid id = Guid.Empty;
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseTemplate);
                context.ExecuteQuery();
                int listTemplate = list.BaseTemplate;
                if (listTemplate != (int)ListTemplateType.Survey)
                {
                    CamlQuery query = new CamlQuery();
                    query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq></Where></Query></View>", tp_Guid);
                    ListItemCollection itemColl = list.GetItems(query);
                    context.Load(itemColl);
                    context.ExecuteQuery();
                    if (itemColl != null && itemColl.Count > 0)
                    {
                        id = (Guid)itemColl[0]["UniqueId"];
                    }
                }
                else
                {
                    ListItem item = list.GetItemById(rowId);
                    context.Load(item);
                    context.ExecuteQuery();
                    if (item != null)
                    {
                        id = (Guid)item["UniqueId"];
                    }
                }
                return id;
            }
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return this.GetListItemGuid(webId, listId, tp_Guid, rowId);
        }

        public bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
        {
            using (var context = CreateRetryContext())
            {
                try
                {
                    Web web = context.Site.OpenWebById(webId);
                    List list = web.Lists.GetById(listId);
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = string.Format(
                                "<View Scope=\"RecursiveAll\"><Query><Where><And><Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{0}</Value></Eq><Eq><FieldRef Name=\"FileLeafRef\"/><Value Type=\"Lookup\">{1}</Value></Eq></And></Where></Query></View>",
                                dirName, leafName);
                    ListItemCollection itemColl = list.GetItems(camlQuery);
                    context.Load(itemColl, ic => ic.Include(i => i.Id));
                    context.ExecuteQuery();
                    if (itemColl.Count > 0)
                    {
                        return true;
                    }
                }
                catch (ServerException ex)
                {
                    //SAAS-37461, the same issue CI:SAAS-37336,SAAS-37415
                    if (ex.ServerErrorCode == AveSPErrorCode.ERROR_SHARING_BUFFER_EXCEEDED)
                    {
                        mLogger.Info($"Current list:{listId} itemcount is greater than limititaion, webId:{webId},listId:{listId},dirName:{dirName},leafName:{leafName}.");
                        ListItem listItem = FindItemInLargeListV1(context, webId, listId, dirName, leafName);
                        if (listItem != null)
                        {
                            return true;
                        }
                        else
                        {
                            mLogger.Info($"Can not findItemInLargeList.");
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return false;
        }

        /*private ListItem FindItemInLargeList(ClientContext context, Guid webId, Guid listId, string dirName, string leafName)
        {
            ListItem resultItem = null;
            try
            {
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list);
                context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                context.ExecuteQuery();

                var parentFolderPath = ResourcePath.FromDecodedUrl(dirName);
                if (list.ItemCount > context.Site.MaxItemsPerThrottledOperation)
                {
                    int index = 0;
                    int batchQueryCount = 2000;
                    int totalCount = list.ItemCount;
                    do
                    {
                        CamlQuery camlQuery = new CamlQuery();
                        camlQuery.DatesInUtc = true;
                        camlQuery.ViewXml = string.Format(
                            "<View Scope='RecursiveAll'>" +
                            "<Query><Where><And><Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query>" +
                            "<RowLimit>{2}</RowLimit>" +
                            "</View>", index, index + batchQueryCount, batchQueryCount);
                        int lastIndex = index;
                        camlQuery.FolderServerRelativePath = parentFolderPath;
                        ListItemCollection items = list.GetItems(camlQuery);
                        context.Load(items, its => its.Include(it => it["FileLeafRef"], it => it["FileDirRef"]));
                        context.ExecuteQuery();
                        resultItem = items.Where(it => it["FileDirRef"].ToString().EndsWith(dirName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(it => string.Equals(it["FileLeafRef"].ToString(), leafName, StringComparison.OrdinalIgnoreCase));
                        index = lastIndex + batchQueryCount < index ? index : lastIndex + batchQueryCount;
                        totalCount -= items.Count;

                    } while (totalCount > 0 && resultItem == null);
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when FindItemInLargeList, webid:{0},listId:{1},dirName:{2},leafName:{3}. ERROR:{4}", webId, listId, dirName, leafName, e);
            }
            return resultItem;
        }*/

        private ListItem FindItemInLargeListV1(ClientContext context, Guid webId, Guid listId, string dirName, string leafName)
        {
            ListItem resultItem = null;
            try
            {
                if (!dirName.StartsWith("/"))
                {
                    //for this format: sites/***/doc1/folder1
                    dirName = $"/{dirName.TrimStart('/')}";
                }
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list);
                context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                context.ExecuteQuery();
                int retryQueryTimes = 0;
                if (list.ItemCount > context.Site.MaxItemsPerThrottledOperation)
                {
                    int batchQueryCount = 2000;
                    ListItemCollectionPosition itemPosition = null;
                    do
                    {
                        CamlQuery camlQuery = new CamlQuery();
                        camlQuery.DatesInUtc = true;
                        camlQuery.ViewXml = string.Format(
                        "<View Scope=\"RecursiveAll\">" +
                        "<QueryOptions><QueryThrottleMode>Override</QueryThrottleMode></QueryOptions>" +
                        "<ViewFields><FieldRef Name ='ID' /><FieldRef Name ='FileLeafRef' /><FieldRef Name ='FileDirRef' /></ViewFields>" +
                        "<RowLimit>{0}</RowLimit>" +
                        "</View>", batchQueryCount);
                        camlQuery.ListItemCollectionPosition = itemPosition;
                        ListItemCollection items = list.GetItems(camlQuery);
                        context.Load(items, its => its.Include(it => it["FileLeafRef"], it => it["FileDirRef"]));
                        context.Load(items, it => it.ListItemCollectionPosition);
                        context.ExecuteQuery();
                        resultItem = items.Where(it => it["FileDirRef"].ToString().EndsWith(dirName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(it => string.Equals(it["FileLeafRef"].ToString(), leafName, StringComparison.OrdinalIgnoreCase));
                        itemPosition = items.ListItemCollectionPosition;
                        if (resultItem == null)
                        {
                            mLogger.Info($"Current list:{listId} items total count is {list.ItemCount}, batch query count is {batchQueryCount}, current retry query times is {retryQueryTimes}.");
                        }
                        else
                        {
                            mLogger.Info($"Success to find this item in large list:{listId}_{list.ItemCount}, current retry query times is {retryQueryTimes}.");
                            break;
                        }
                        retryQueryTimes++;
                    } while (itemPosition != null);
                }
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when FindItemInLargeList, webid:{0},listId:{1},dirName:{2},leafName:{3}. ERROR:{4}", webId, listId, dirName, leafName, e);
            }
            return resultItem;
        }

        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list, l => l.BaseTemplate);
                context.ExecuteQuery();
                int listTemplate = list.BaseTemplate;
                if (listTemplate != (int)ListTemplateType.Survey)
                {
                    CamlQuery query = new CamlQuery();
                    query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq></Where></Query></View>", tpGuid);
                    ListItemCollection itemColl = list.GetItems(query);
                    context.Load(itemColl);
                    context.ExecuteQuery();
                    if (itemColl != null && itemColl.Count > 0)
                    {
                        return true;
                    }
                }
                else
                {
                    ListItem item = list.GetItemById(rowId);
                    context.Load(item);
                    context.ExecuteQuery();
                    if (item != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        ///<summary>
        /// 将list下需要备份的item/folder填充并缓存，使备份时无需再次进行GetItem操作。Note: 缓存只对当前request有效
        /// </summary>
        public Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool loadSubFolders, bool loadSubItems, bool includeSystemFolder = false)
        {
            string logUrl = string.IsNullOrEmpty(folderUrl) ? string.Empty : folderUrl;
            mLogger.Info("start query folder Items,Folder url:{0}, loadFolders:{1}, loadItems:{2}", logUrl, loadSubFolders, loadSubItems);
            mIsLoadDFolderId = true;
            Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
            //needLoadFields.Add("ID", "Counter");
            //needLoadFields.Add("GUID", "Guid");
            needLoadFields.Add("_Level", "Integer");
            //needLoadFields.Add("_IsCurrentVersion", "Boolean");
            needLoadFields.Add("_UIVersion", "Integer");
            Dictionary<string, object> folder = this.PutCurrentVersionIntoVersions(siteId, webId, listId, folderId, folderUrl, loadSubFolders, loadSubItems, includeSystemFolder);
            mIsLoadDFolderId = false;
            mLogger.Info("finished query folder Items");
            return folder;
        }

        public Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changedItemsCache)
        {
            mLogger.Info("start query folder Items for IB,Folder url:{0}", folderUrl);
            Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
            //needLoadFields.Add("ID", "Counter");
            //needLoadFields.Add("GUID", "Guid");
            //needLoadFields.Add("_Level", "Integer");
            //needLoadFields.Add("_IsCurrentVersion", "Boolean");
            needLoadFields.Add("_UIVersion", "Integer");
            Dictionary<string, object> folder = new Dictionary<string, object>();
            folder["Items"] = new List<Dictionary<string, object>>();
            folder["Folders"] = new List<Dictionary<string, object>>();
            GetChangeItemsFromChangeCache(folder, webId, listId, folderUrl, changedItemsCache);
            List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)folder["Items"];
            string webUrl = folder.ContainsKey("WebServerRelativeUrl") ?
                folder["WebServerRelativeUrl"].ToString() : this.GetWeb(webId)["ServerRelativeUrl"].ToString();
            List<DelegateTask> getItemVersionTasks = new List<DelegateTask>();
            mLogger.Info("start query list item versions for IB.");
            foreach (Dictionary<string, object> item in items)
            {
                if (item.ContainsKey("ChangeType") && (AvePoint.Wrapper.Common.ChangeType)item["ChangeType"] == AvePoint.Wrapper.Common.ChangeType.Delete)
                {
                    item["Versions"] = new List<Dictionary<string, object>>();
                    continue;
                }
                //better to user batch query instead of multi-thread //TODO_LONG
                if (item.ContainsKey("Versions") && WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance)
                {
                    getItemVersionTasks.Add(() =>
                    {
                        List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)item["Versions"];
                        //Dictionary<string, object> allVersionProperties = mWebServiceRequest.GetItemVersionsWithMultiRequest(webUrl, "", listId.ToString(), (int)item["Id"], "", needLoadFields);
                        var allVersionProperties = GetItemVersions(webUrl, "", listId.ToString(), (int)item["Id"], "", null, needLoadFields, true);
                        var versionProperties = allVersionProperties.GetChildren();
                        foreach (var version in versionProperties)
                        {
                            version["ID"] = (int)item["ID"];
                            version["GUID"] = new Guid(item["GUID"].ToString());
                            version["Size"] = 0;
                            version["ObjType"] = item["ObjType"];
                            version["TimeLastModified"] = version["Modified"];
                            int versionId = (int)version["VersionId"];
                            version["Level"] = (byte)1;
                            version["UIVersion"] = version["VersionId"];
                            version["UserDataGuid"] = version["GUID"];
                            version["IsCurrentVersion"] = versionId == (int)item["UIVersion"] ? true : false;
                            versions.Add(version.ToDictionary());
                        }
                    });
                }
                else
                {// list enable version is false, we just add current version here
                    List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                    Dictionary<string, object> version = new Dictionary<string, object>();
                    version["ID"] = (int)item["ID"];
                    //SAAS-22971,survey list中的item，没有GUID
                    object guid;
                    if (item.TryGetValue("GUID", out guid))
                    {
                        version["GUID"] = new Guid(guid.ToString());
                        version["UserDataGuid"] = guid;
                    }
                    version["Size"] = 0;
                    version["ObjType"] = item["ObjType"];
                    version["TimeLastModified"] = item["TimeLastModified"];
                    version["UIVersion"] = item["UIVersion"];
                    version["IsCurrentVersion"] = item["_IsCurrentVersion"];
                    version["Level"] = item["Level"];
                    versions.Add(version);
                    item["Versions"] = versions;
                }
            }
            if (getItemVersionTasks.Count > 0)
            {
                using (var taskExecutor = new CountableTaskExecutor(WrapperConfiguration.WrapperConfigurationForBPOS.MaximumThreadsGettingVersions))
                {
                    taskExecutor.Execute(getItemVersionTasks, true);
                }
            }
            mLogger.Info("finished query folder Items for IB");
            return folder;
        }

        public Dictionary<byte[], object> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<byte[], object> contentTypes = new Dictionary<byte[], object>();
                Web web = context.Site.OpenWebById(webId);
                ContentTypeCollection ctColl = web.ContentTypes;
                context.Load(ctColl, collection => collection.Include(ct => ct.Id, ct => ct.Name, ct => ct.SchemaXml, ct => ct.Scope));
                context.ExecuteQuery();
                foreach (ContentType ct in ctColl)
                {
                    Dictionary<string, object> contentTye = new Dictionary<string, object>();
                    byte[] id = Encoding.UTF8.GetBytes(ct.Id.ToString());
                    contentTye["ContentTypeId"] = id;
                    contentTye["Name"] = ct.Name;
                    contentTye["SchemaXml"] = ct.SchemaXml;
                    contentTye["Scope"] = ct.Scope;
                    Dictionary<string, object> folder = GetContentTypeRelatedFolder(context, ct.SchemaXml, web, ct.Scope);
                    contentTye["RelatedFolder"] = folder;
                    contentTypes.Add(id, contentTye);
                }
                return contentTypes;
            }
        }

        public Dictionary<byte[], object> QueryListContentTypeForFB(Guid siteId, Guid webId, Guid listId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<byte[], object> contentTypes = new Dictionary<byte[], object>();
                Web web = context.Site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                ContentTypeCollection ctColl = list.ContentTypes;
                context.Load(ctColl, collection => collection.Include(ct => ct.Id, ct => ct.Name, ct => ct.SchemaXml, ct => ct.Scope));
                context.ExecuteQuery();
                foreach (ContentType ct in ctColl)
                {
                    Dictionary<string, object> contentTye = new Dictionary<string, object>();
                    byte[] id = Encoding.UTF8.GetBytes(ct.Id.ToString());
                    contentTye["ContentTypeId"] = id;
                    contentTye["Name"] = ct.Name;
                    contentTye["SchemaXml"] = ct.SchemaXml;
                    contentTye["Scope"] = ct.Scope;
                    contentTypes.Add(id, contentTye);
                }
                return contentTypes;
            }
        }

        public Dictionary<string, object> QueryWebRootFolder(Guid webId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> folder = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                Folder rootFolder = web.RootFolder;
                context.Load(rootFolder);
                context.ExecuteQuery();
                folder.Add("DocID", Guid.Empty);   //Can not get Guid of root folder.
                folder.Add("DirName", rootFolder.ServerRelativeUrl.Substring(0, rootFolder.ServerRelativeUrl.Length - (rootFolder.Name.Length + 1)).TrimStart('/'));
                folder.Add("LeafName", rootFolder.Name);
                folder.Add("ID", null);  //Can not get ID of root folder.
                folder.Add("UIVersion", 512);    //Can not get this property.
                folder.Add("DocFlags", null);    //Can not get this property.
                folder.Add("TimeLastModified", DateTime.MinValue);    //Can not get this property.
                folder.Add("Level", Convert.ToByte(1));    //Can not get this property. default value: Published
                folder.Add("Type", Convert.ToByte(1));    //Can not get this property.  default value: Folder
                folder.Add("Size", 0);    //Can not get this property.
                folder.Add("ParentID", Guid.Empty);    //Can not get this property.
                folder.Add("FullUrl", rootFolder.ServerRelativeUrl);
                folder.Add("CheckoutUserId", (int?)null);
                folder.Add("Hidden", (bool?)true);
                return folder;
            }
        }

        public virtual Dictionary<string, object> GetWebChangesByQuery(string webServerRelativeUrl, IDictionary<string, object> queryProps)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> changesProps = new Dictionary<string, object>();
                Web targetWeb = context.Site.OpenWeb(webServerRelativeUrl);
                ChangeQuery query = GenerateChangeQuery(queryProps);
                ChangeCollection changeCollection = targetWeb.GetChanges(query);
                context.Load(changeCollection);
                context.ExecuteQuery();
                var changePropsList = new List<IDictionary<string, object>>();
                foreach (Change tempChange in changeCollection)
                {
                    Dictionary<string, object> changeProps = new Dictionary<string, object>();
                    CopyProperty(changeProps, tempChange);
                    changeProps["ChangeType"] = (int)tempChange.ChangeType;
                    changeProps["ChangeObjectType"] = tempChange.GetType().ToString();
                    changeProps.Remove("ChangeToken");
                    changeProps["ChangeToken" + AveObjectModelConstant.ObjectPropertySuffix] = tempChange.ChangeToken.StringValue;
                    changePropsList.Add(changeProps);
                }
                changesProps.AddChildren(changePropsList);
                return changesProps;
            }
        }

        public virtual Dictionary<string, object> GetListChangesByQuery(string webServerRelativeUrl, Guid listId, string listTitle, IDictionary<string, object> queryProps)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> changesProps = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List targetList = web.Lists.GetById(listId);
                ChangeQuery query = GenerateChangeQuery(queryProps);
                ChangeCollection changeCollection = targetList.GetChanges(query);
                context.Load(changeCollection);
                context.ExecuteQuery();
                var changePropsList = new List<IDictionary<string, object>>();
                foreach (Change tempChange in changeCollection)
                {
                    Dictionary<string, object> changeProps = new Dictionary<string, object>();
                    CopyProperty(changeProps, tempChange);
                    changeProps["ChangeType"] = (int)tempChange.ChangeType;
                    changeProps["ChangeObjectType"] = tempChange.GetType().ToString();
                    changeProps.Remove("ChangeToken");
                    changeProps["ChangeToken" + AveObjectModelConstant.ObjectPropertySuffix] = tempChange.ChangeToken.StringValue;
                    changePropsList.Add(changeProps);
                }
                changesProps.AddChildren(changePropsList);
                return changesProps;
            }
        }

        public bool CheckSiteChanged(string siteUrl, long startTime, AveQueryOption option)
        {
            mLogger.Info("Check site changed. SiteUrl:{0}. StartTime:{1}", siteUrl, startTime.ToString());
            //对于没有startTime的, 认为是之前没有跑过job的
            if (startTime <= 0) return true;

            using (var context = CreateRetryContext(siteUrl))
            {
                context.Load(context.Site, site => site.CurrentChangeToken, site => site.Id);
                context.ExecuteQuery();
                ChangeQuery query = new ChangeQuery();
                query.InitChangeQuery(option);
                query.ChangeTokenStart = new ChangeToken();
                query.ChangeTokenStart.StringValue = string.Format("1;1;{0};{1};-1", context.Site.Id.ToString(), startTime);
                query.ChangeTokenEnd = context.Site.CurrentChangeToken;
                var changes = context.Site.GetChanges(query);
                context.Load(changes);
                context.ExecuteQuery();
                return changes.Count > 0;
            }

        }

        public void RemoveFolderCache(string folderServerRelativeUrl)
        {
            if (!string.IsNullOrEmpty(folderServerRelativeUrl)
                && this.mCurrentList != null
                && this.mCurrentList.Items != null)
            {
                //remove items under the folder
                if (this.mCurrentList.FoldersToSubItemIds.ContainsKey(folderServerRelativeUrl))
                {
                    IList<int> subitemIds = this.mCurrentList.FoldersToSubItemIds[folderServerRelativeUrl];
                    foreach (int itemId in subitemIds)
                    {
                        this.mCurrentList.Items.Remove(itemId);
                    }
                    this.mCurrentList.FoldersToSubItemIds.Remove(folderServerRelativeUrl);
                }

                //remove files under the folder
                if (this.mCurrentList.FoldersToSubFiles.ContainsKey(folderServerRelativeUrl))
                {
                    IList<string> subFiles = this.mCurrentList.FoldersToSubFiles[folderServerRelativeUrl];
                    foreach (string fileRelativeUrl in subFiles)
                    {
                        this.mCurrentList.Files.Remove(fileRelativeUrl);
                    }
                    this.mCurrentList.FoldersToSubFiles.Remove(folderServerRelativeUrl);
                }

                //remove items guid under the folder
                if (this.mCurrentList.FoldersToSubItemUniqueIds.ContainsKey(folderServerRelativeUrl))
                {
                    this.mCurrentList.FoldersToSubItemUniqueIds.Remove(folderServerRelativeUrl);
                }

                //remove items last access time under the folder
                if (this.mCurrentList.FoldersToSubItemLastAccessTime.ContainsKey(folderServerRelativeUrl))
                {
                    if (!this.mCurrentList.FoldersToSubItemLastAccessTime.TryRemove(folderServerRelativeUrl, out Dictionary<string, long> removeResult))
                    {
                        mLogger.Info("Remove LAT Cache for folder {0}, failed", folderServerRelativeUrl);
                    }
                }
                //这里不能删除Folders, 因为discover是把所有的folder都缓存了之后才backup， 如果在parent执行了remove，后面获取subfolder信息就会重新获取

            }
        }

        public void ClearItemCache()
        {
            if (mCurrentList != null)
            {
                mCurrentList.Items.Clear();
            }
        }

        public void RemoveFolderCache(List<int> folderIds)
        {
            if (mCurrentList != null)
            {
                foreach (var id in folderIds)
                {
                    if (mCurrentList.Folders.ContainsKey(id))
                    {
                        mCurrentList.Folders.Remove(id);
                    }
                }
            }
        }

        public Dictionary<string, object> QueryFolderForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool includeSystemFolder = false)
        {
            string logUrl = string.IsNullOrEmpty(folderUrl) ? string.Empty : folderUrl;
            mLogger.Info("start query folder folders,Folder url:{0}", logUrl);
            mIsLoadDFolderId = true;

            //Dictionary<string, object> folder = this.PutFolderCurrentVersionIntoVersions(siteId, webId, listId, folderId, folderUrl, includeSystemFolder);
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            using (var context = CreateRetryContext())
            {
                Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
                //needLoadFields.Add("ID", "Counter");
                //needLoadFields.Add("GUID", "Guid");
                needLoadFields.Add("Author", "User");
                needLoadFields.Add("_Level", "Integer");
                //needLoadFields.Add("_IsCurrentVersion", "Boolean");
                needLoadFields.Add("_UIVersion", "Integer");
                Web web = context.Site.OpenWebById(webId);
                parentFolder["Items"] = new List<Dictionary<string, object>>();
                parentFolder["Folders"] = new List<Dictionary<string, object>>();
                //parentFolder["Attachments"] = new List<Dictionary<string, object>>();
                //parentFolder["Versions"] = new List<Dictionary<string, object>>();
                List list = null;
                if (listId != Guid.Empty) // for system folder, we skip it now, to do it later
                {
                    ArgumentCheck.CheckNotNull(folderUrl);
                    string folderServerRelativeUrl = "/" + folderUrl?.TrimStart('/');
                    Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                    context.Load(folder, f => f.ItemCount, f => f.Properties, f => f.ListItemAllFields);
                    list = web.Lists.GetById(listId);
                    //需要优化，只需要获取要用的属性
                    context.Load(list,
                        l => l.BaseType, l => l.EnableVersioning, l => l.EnableMinorVersions, l => l.EnableAttachments,
                        l => l.EnableFolderCreation, l => l.EnableModeration, l => l.BaseTemplate,
                        l => l.Id, l => l.Title, l => l.Created, l => l.ItemCount);
                    context.Load(list.RootFolder, r => r.ServerRelativeUrl, r => r.ItemCount);
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl,
                                      tmpWeb => tmpWeb.WebTemplate);
                    context.ExecuteQuery();
                    GetSubFoldersFromFolder(context, web, list, folder, folderServerRelativeUrl, parentFolder, context.Site.MaxItemsPerThrottledOperation);
                }
                else
                {
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                    context.ExecuteQuery();
                    List<Dictionary<string, object>> webFolders = parentFolder["Folders"] as List<Dictionary<string, object>>;
                    ArgumentCheck.CheckNotNull(folderUrl);
                    Dictionary<string, object> folders = GetFolders(web.ServerRelativeUrl, null, Guid.Empty, folderUrl != "/" ? "/" + folderUrl?.TrimStart('/') : "/", includeSystemFolder);
                    foreach (var folder in folders.GetChildren())
                    {
                        webFolders.Add(folder.ToDictionary());
                    }
                }
                parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                if (!listId.Equals(Guid.Empty))
                {
                    //GetFolderOrItemVersions(parentFolder, webId, listId, needLoadFields, "Items");
                    if (list?.BaseTemplate == (int)AveListTemplateType.DiscussionBoard && WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance)
                    {
                        GetFolderOrItemVersions(parentFolder, webId, listId, needLoadFields, "Folders");
                    }
                }
            }


            mIsLoadDFolderId = false;
            mLogger.Info("finished query folder Items");
            return parentFolder;
        }

        public Dictionary<string, object> QueryItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, ref string pageInfo, bool includeSystemFolder = false)
        {
            mLogger.Info("start query folder Items,Folder url:{0}", folderUrl);
            mIsLoadDFolderId = true;
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            using (var context = CreateRetryContext())
            {
                Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
                needLoadFields.Add("Author", "User");
                needLoadFields.Add("_Level", "Integer");
                needLoadFields.Add("_UIVersion", "Integer");
                Web web = context.Site.OpenWebById(webId);
                parentFolder["Items"] = new List<Dictionary<string, object>>();
                parentFolder["Folders"] = new List<Dictionary<string, object>>();
                //parentFolder["Attachments"] = new List<Dictionary<string, object>>();
                //parentFolder["Versions"] = new List<Dictionary<string, object>>();
                List list = null;
                if (listId != Guid.Empty) // for system folder, we skip it now, to do it later
                {
                    string folderServerRelativeUrl = "/" + folderUrl.TrimStart('/');
                    //SAAS-27651 支持特殊字符（%，#）
                    Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    context.Load(folder, f => f.ItemCount, f => f.Properties, f => f.ListItemAllFields);
                    list = web.Lists.GetById(listId);
                    //需要优化，只需要获取要用的属性
                    context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                    context.Load(list,
                        l => l.BaseType, l => l.EnableVersioning, l => l.EnableMinorVersions, l => l.EnableAttachments,
                        l => l.EnableFolderCreation, l => l.EnableModeration, l => l.BaseTemplate,
                        l => l.Id, l => l.Title, l => l.Created, l => l.ItemCount);
                    context.Load(list.RootFolder, r => r.ServerRelativeUrl, r => r.ItemCount);
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl,
                                      tmpWeb => tmpWeb.WebTemplate);
                    context.ExecuteQuery();
                    //if (list.ItemCount > 5000)
                    //{
                    GetSubItemsFromFolder(context, web, list, folder, folderServerRelativeUrl, parentFolder, context.Site.MaxItemsPerThrottledOperation, ref pageInfo);
                    //}
                    //else
                    //{
                    //    //如果list item count < 5000就直接都load出来
                    //    GetItemsFromFolder(context, web, list, folder, folderServerRelativeUrl, parentFolder);
                    //}
                }
                else
                {
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                    context.ExecuteQuery();
                    List<Dictionary<string, object>> webItems = parentFolder["Items"] as List<Dictionary<string, object>>;
                    Dictionary<string, object> files = GetFiles(web.ServerRelativeUrl, null, folderUrl != "/" ? "/" + folderUrl.TrimStart('/') : "/");
                    foreach (var item in files.GetChildren())
                    {
                        webItems.Add(item.ToDictionary());
                    }
                }
                parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                if (!listId.Equals(Guid.Empty))
                {
                    if (WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance)
                    {
                        GetFolderOrItemVersions(parentFolder, webId, listId, needLoadFields, "Items");
                    }
                    //if (list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard)
                    //{
                    //    GetFolderOrItemVersions(parentFolder, webId, listId, needLoadFields, "Folders");
                    //}
                }
                else
                {
                    List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)parentFolder["Items"];
                    items.ForEach((item) =>
                    {
                        List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                        AssembleWebItemVersionProperty(item, versions);
                        item["HasVersion"] = false;
                    });
                }
            }
            mIsLoadDFolderId = false;
            mLogger.Info("finished query folder Items");
            return parentFolder;
        }

        public Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> webpartsProperties = new Dictionary<string, object>();
                try
                {
                    AvePersonalizationScope scope = AvePersonalizationScope.Shared;
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(web);
                    List list = web.Lists.GetById(listId);
                    context.Load(list);
                    context.Load(list, l => l.Views.IncludeWithDefaultProperties(v => v.HtmlSchemaXml));
                    context.ExecuteQuery();
                    foreach (View view in list.Views)
                    {
                        ClientFile file = web.GetFileByServerRelativePath(view.ServerRelativePath);
                        context.Load(file, f => f.ServerRelativeUrl, f => f.ETag);
                        context.ExecuteQuery();
                        string fileDocId = string.Empty;
                        if (!string.IsNullOrEmpty(file.ETag))
                        {
                            int index = file.ETag.IndexOf(',');
                            fileDocId = file.ETag.Substring(1, index - 1);
                            fileDocId = new Guid(fileDocId).ToString();
                        }
                        AvePersonalizationScope personalizationScope = view.PersonalView ? AvePersonalizationScope.User : AvePersonalizationScope.Shared;
                        Dictionary<string, object> webpartManagerProperties = GetLimitedWebPartManager(web.ServerRelativeUrl, view.ServerRelativeUrl, (int)scope);
                        webpartManagerProperties = webpartManagerProperties["WebParts" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                        var webpartProperties = webpartManagerProperties.GetChildren();
                        foreach (var webpartProperty in webpartProperties)
                        {
                            webpartProperty["Id"] = webpartProperty.ContainsKey("ID") ? new Guid(webpartProperty["ID"].ToString()) : Guid.Empty;
                            webpartProperty["DisplayName"] = webpartProperty.ContainsKey("Title") ? webpartProperty["Title"].ToString() : string.Empty;
                            webpartProperty["ZoneId"] = webpartProperty.ContainsKey("ZoneId") ? webpartProperty["ZoneId"].ToString() : string.Empty;
                            webpartProperty["Flags"] = 0;
                            webpartProperty["AllUsersProperties"] = null;
                            webpartProperty["PerUserProperties"] = null;
                            webpartProperty["IsIncluded"] = false;
                            webpartProperty["PartOrder"] = webpartProperty.ContainsKey("ZoneIndex") ? (int)webpartProperty["ZoneIndex"] : 0;
                            webpartProperty["View"] = Encoding.UTF8.GetBytes(view.HtmlSchemaXml);
                        }
                        webpartsProperties.Add(fileDocId, webpartProperties);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get item webparts failed,docid:{0},error:{1}", itemDocId, e.ToString());
                }
                return webpartsProperties;
            }
        }

        public Dictionary<string, object> GetItemsByCamlQueryWithAttachments(string webServerRelativeUrl, Guid listId, string[] camlQueryNode)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> itemsProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                List list = web.Lists.GetById(listId);
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = camlQueryNode[3];
                bool loadAllItems = true;
                if (!string.IsNullOrEmpty(camlQueryNode[4]))
                {
                    camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(camlQueryNode[4]);
                }
                ListItemCollectionPosition licp = null;
                if (!string.IsNullOrEmpty(camlQueryNode[5]))
                {
                    licp = new ListItemCollectionPosition
                    {
                        PagingInfo = camlQueryNode[5]
                    };
                }
                if (!string.IsNullOrEmpty(camlQueryNode[6]))
                {
                    camlQuery.DatesInUtc = Convert.ToBoolean(camlQueryNode[6]);
                }
                if (!string.IsNullOrEmpty(camlQueryNode[7]))
                {
                    loadAllItems = Convert.ToBoolean(camlQueryNode[7]);
                }
                List<IDictionary<string, object>> itemList = new List<IDictionary<string, object>>();
                do
                {
                    camlQuery.ListItemCollectionPosition = licp;
                    ListItemCollection items = list.GetItems(camlQuery);
                    ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
                    using (ehScope.StartScope())
                    {
                        using (ehScope.StartTry())
                        {
                            context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                            context.Load(items);
                            context.Load(items, its => its.ListItemCollectionPosition,
                                                its => its.Include(t => t.HasUniqueRoleAssignments, t => t.DisplayName));
                        }
                        using (ehScope.StartCatch())
                        {
                            context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                            context.Load(items);
                            context.Load(items, its => its.ListItemCollectionPosition,
                                                    its => its.Include(t => t.HasUniqueRoleAssignments));//SAAS-6084 DisplayName not support discussion board
                        }
                    }

                    context.ExecuteQuery();
                    if (ehScope.HasException)
                    {
                        mLogger.Warn("load item failed due to: {0}", ehScope.ErrorMessage);
                    }
                    foreach (ListItem item in items)
                    {
                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                        GetItemDic(itemProperties, item);
                        itemList.Add(itemProperties);
                    }
                    licp = items.ListItemCollectionPosition;
                    if (items.ListItemCollectionPosition != null)
                    {
                        itemsProperties["PageInfo"] = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        itemsProperties["PageInfo"] = null;
                    }
                }
                while (licp != null && loadAllItems);
                itemList.ForEach(item =>
                {
                    var attachments = GetAttachments(context, list, item);
                    if (attachments.Count > 0)
                    {
                        item["Attachments"] = attachments;
                    }
                });
                itemsProperties.AddChildren(itemList);
                return itemsProperties;
            }
        }

        #endregion IAveDiscoverQuery




        #region Discovery Query

        private List<Dictionary<string, object>> GetAttachments(ClientContext clientContext, List list, IDictionary<string, object> item)
        {
            object needGetAttachments = false;
            List<Dictionary<string, object>> attachments = new List<Dictionary<string, object>>();
            object itemId = 0;
            if (!item.TryGetValue("Id", out itemId) || Convert.ToInt32(itemId) <= 0)
            {
                return attachments;
            }
            if (item.TryGetValue("Attachments" + AveObjectModelConstant.ObjectPropertySuffix, out needGetAttachments) && Convert.ToBoolean(needGetAttachments))
            {
                string attachmentFolderUrl = list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Attachments/" + itemId;
                ClientFolder attachmentFolder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(attachmentFolderUrl));
                clientContext.Load(attachmentFolder, a => a.ServerRelativeUrl, a => a.Files);
                FileCollection files = attachmentFolder.Files;
                ExceptionHandlingScope handleAuthorNotExist = new ExceptionHandlingScope(clientContext);
                using (handleAuthorNotExist.StartScope())
                {
                    using (handleAuthorNotExist.StartTry())
                    {
                        clientContext.Load(files, fs => fs.IncludeWithDefaultProperties(file => file.Author, file => file.ModifiedBy));
                    }
                    using (handleAuthorNotExist.StartCatch())
                    {
                        clientContext.Load(files, fs => fs.IncludeWithDefaultProperties());
                    }
                }
                clientContext.ExecuteQuery();
                if (handleAuthorNotExist.HasException)
                {
                    mLogger.Warn("Get Files Author Error, attachmentFolderUrl:{0} , Error Message:{1}", attachmentFolderUrl, handleAuthorNotExist.ErrorMessage);
                }
                string attachmentFolderServerRelativeUrl = attachmentFolder.ServerRelativeUrl;
                foreach (ClientFile attachment in attachmentFolder.Files)
                {
                    Dictionary<string, object> attachmentPro = new Dictionary<string, object>();
                    string eTag = attachment.ETag.Trim('"');
                    string[] pros = eTag.Split(',');
                    if (!handleAuthorNotExist.HasException && !attachment.Author.ServerObjectIsNull.Value)
                    {
                        attachmentPro["Author" + AveObjectModelConstant.ObjectPropertySuffix] = attachment.Author.LoginName;
                    }
                    attachmentPro["DocID"] = new Guid(pros[0]);
                    attachmentPro["DirName"] = attachmentFolderServerRelativeUrl;
                    attachmentPro["Name"] = attachmentPro["LeafName"] = attachment.Name;
                    attachmentPro["UIVersion"] = attachment.UIVersion;//统一为UIVersion
                    attachmentPro["DocFlags"] = (int?)null;//cannot get this property
                    attachmentPro["TimeLastModified"] = attachment.TimeLastModified;
                    attachmentPro["TimeCreated"] = attachment.TimeCreated;//SAAS-1049
                    attachmentPro["Level"] = (byte)attachment.Level;
                    attachmentPro["Type"] = (byte)FileSystemObjectType.File;
                    attachmentPro["Size"] = 0; //cannot get this property
                    attachmentPro["Length"] = attachment.Length;//SAAS-1053
                    attachmentPro["ParentID"] = Guid.Empty;
                    attachmentPro["FullUrl"] = attachmentFolderServerRelativeUrl.TrimEnd('/') + "/" + attachmentPro["LeafName"];
                    attachmentPro["CheckoutUserId"] = (int?)null;
                    attachmentPro["HasStream"] = true;
                    attachmentPro["RbsId"] = null;
                    attachmentPro["ServerRelativeUrl"] = attachment.ServerRelativeUrl;
                    attachmentPro["ID"] = (int?)itemId;
                    attachments.Add(attachmentPro);
                }
            }
            return attachments;
        }



        protected void GetSystemFoldersAndFiles(ClientContext context, List<Dictionary<string, object>> folders, List<Dictionary<string, object>> items, List list, Folder folder, string webServerRelativeUrl, string folderServerRelativeUrl, bool isExceedListViewThreshold)
        {
            //base.GetSystemFolders(context, folders, webServerRelativeUrl, folderServerRelativeUrl);
            //Folder folder =  list.ParentWeb.GetFolderByServerRelativeUrl(folderServerRelativeUrl);
            //context.Load(folder);
            if (folder.ListItemAllFields.ServerObjectIsNull.Value)
            {
                mLogger.Info($"Discover_GetSystemFoldersAndFiles to load Folder:[{folderServerRelativeUrl}] Sub folders and files,ExceedListViewThreshold:[{isExceedListViewThreshold}]");
                if (!folder.Folders.AreItemsAvailable)
                {
                    if (!isExceedListViewThreshold)
                    {
                        context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ListItemAllFields, f => f.Properties).Where(f => f.ListItemAllFields.ServerObjectIsNull.Value));
                        context.ExecuteQuery();
                        foreach (Folder subFolder in folder.Folders)
                        {
                            if (subFolder.ListItemAllFields.ServerObjectIsNull.Value && !subFolder.Name.Equals("Attachments", StringComparison.OrdinalIgnoreCase) && !subFolder.Name.Equals("Forms", StringComparison.OrdinalIgnoreCase))
                            {
                                Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                                //GetItemDic(itemProperty, subFolder.ListItemAllFields);
                                itemProperty["ObjType"] = 4;
                                itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = subFolder.ListItemAllFields.FieldValues.ContainsKey("Attachments") ? subFolder.ListItemAllFields.FieldValues["Attachments"] : false;
                                //if (subFolder.ListItemAllFields.FieldValues.ContainsKey("FileRef") && !string.IsNullOrEmpty(subFolder.ListItemAllFields.FieldValues["FileRef"].ToString()))
                                //{
                                //    this.mCurrentList.FoldersToItemIds[subFolder.ListItemAllFields["FileRef"].ToString()] = subFolder.ListItemAllFields.Id;
                                //}
                                itemProperty["FullUrl"] = subFolder.ServerRelativeUrl;
                                itemProperty["ServerRelativeUrl"] = subFolder.ServerRelativeUrl;
                                itemProperty["LeafName"] = subFolder.Name;
                                itemProperty["Items"] = new List<Dictionary<string, object>>();
                                itemProperty["Folders"] = new List<Dictionary<string, object>>();
                                itemProperty["Attachments"] = new List<Dictionary<string, object>>();
                                itemProperty["ItemId"] = itemProperty["Id"] = null;
                                itemProperty["Hidden"] = true; //(itemProperty["Id"] == null) ? true : false;
                                itemProperty["UniqueId"] = subFolder.UniqueId;
                                itemProperty["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = new Hashtable();
                                itemProperty["IsSystemFile"] = true;
                                if (subFolder.Properties.FieldValues != null && subFolder.Properties.FieldValues.Count > 0)
                                {
                                    Hashtable hashtable = new Hashtable();
                                    foreach (KeyValuePair<string, object> pair in subFolder.Properties.FieldValues)
                                    {
                                        hashtable[pair.Key] = pair.Value;
                                    }
                                    itemProperty["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = hashtable;
                                }
                                //GetAttachmentsFromItem(context, list, itemProperty, list.RootFolder.ServerRelativeUrl);
                                folders.Add(itemProperty);
                            }
                        }
                    }
                    else
                    {
                        mLogger.Warn($"Discover_GetSystemFoldersAndFiles Skip to load folder:[{folderServerRelativeUrl}].Folders");
                    }
                }

                bool isRootFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
                bool isForms = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
                if (isRootFolder || isForms)
                {
                    return;
                }
                //context.Load(folder.Files);
                //context.ExecuteQuery();

                if (folder.ItemCount == 0)
                {
                    context.Load(folder.Files);
                    context.ExecuteQuery();
                    if (folder.Files.Count > 0)
                    {
                        foreach (Microsoft.SharePoint.Client.File file in folder.Files)
                        {
                            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                            AssembleViewFileProperties(itemProperty, file);
                            itemProperty["ObjType"] = 2;//set default value to 2.
                            itemProperty["IsSystemFile"] = true;
                            items.Add(itemProperty);
                        }
                    }
                }
            }
        }

        private void AssembleWebItemVersionProperty(Dictionary<string, object> item, List<Dictionary<string, object>> versions)
        {
            Dictionary<string, object> version = new Dictionary<string, object>();
            version["ID"] = item.ContainsKey("ID") ? (int)item["ID"] : default(int);
            if (item.ContainsKey("GUID"))  //Survey List item没有GUID
            {
                version["GUID"] = new Guid(item["GUID"].ToString());
                version["UserDataGuid"] = item["GUID"];
            }
            else if (item.ContainsKey("UniqueId"))
            {
                version["GUID"] = new Guid(item["UniqueId"].ToString());
                version["UserDataGuid"] = item["UniqueId"];
            }
            version["Size"] = 0;
            version["ObjType"] = 2;
            version["TimeLastModified"] = item["TimeLastModified"];
            version["UIVersion"] = item["UIVersion"];
            version["IsCurrentVersion"] = true;
            version["Level"] = item["Level"];
            versions.Add(version);
            item["Versions"] = versions;
        }

        private Dictionary<string, object> PutCurrentVersionIntoVersions(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool loadSubFolders, bool loadSubItems, bool includeSystemFolder)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> parentFolder = new Dictionary<string, object>();
                Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
                //needLoadFields.Add("ID", "Counter");
                //needLoadFields.Add("GUID", "Guid");
                needLoadFields.Add("Author", "User");
                needLoadFields.Add("_Level", "Integer");
                //needLoadFields.Add("_IsCurrentVersion", "Boolean");
                needLoadFields.Add("_UIVersion", "Integer");
                Web web = context.Site.OpenWebById(webId);
                parentFolder["Items"] = new List<Dictionary<string, object>>();
                parentFolder["Folders"] = new List<Dictionary<string, object>>();
                //parentFolder["Attachments"] = new List<Dictionary<string, object>>();
                //parentFolder["Versions"] = new List<Dictionary<string, object>>();
                List list = null;
                if (listId != Guid.Empty) // for system folder, we skip it now, to do it later
                {
                    string folderServerRelativeUrl = "/" + folderUrl.TrimStart('/');
                    Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    context.Load(folder, f => f.ItemCount, f => f.Properties, f => f.ListItemAllFields);
                    list = web.Lists.GetById(listId);
                    //需要优化，只需要获取要用的属性
                    context.Load(list,
                        l => l.BaseType, l => l.EnableVersioning, l => l.EnableMinorVersions, l => l.EnableAttachments,
                        l => l.EnableFolderCreation, l => l.EnableModeration, l => l.BaseTemplate,
                        l => l.Id, l => l.Title, l => l.Created, l => l.ItemCount);
                    context.Load(list.RootFolder, r => r.ServerRelativeUrl, r => r.ItemCount);
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl,
                                      tmpWeb => tmpWeb.WebTemplate);
                    context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                    context.ExecuteQuery();
                    mLogger.Info($"Discover_PutCurrentVersionIntoVersions Folder:[{folderServerRelativeUrl}],SubItemCount:[{folder.ItemCount}],MaxItemsPerThrottledOperation:[{context.Site.MaxItemsPerThrottledOperation}],ExceedListViewThreshold:[{folder.ItemCount > context.Site.MaxItemsPerThrottledOperation}]");
                    GetItemsFromFolder(context, web, list, folder, folderServerRelativeUrl, parentFolder, loadSubFolders, loadSubItems, folder.ItemCount > context.Site.MaxItemsPerThrottledOperation);
                }
                else
                {
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                    context.ExecuteQuery();
                    List<Dictionary<string, object>> webItems = parentFolder["Items"] as List<Dictionary<string, object>>;
                    List<Dictionary<string, object>> webFolders = parentFolder["Folders"] as List<Dictionary<string, object>>;
                    Dictionary<string, object> files = GetFiles(web.ServerRelativeUrl, null, folderUrl != "/" ? "/" + folderUrl.TrimStart('/') : "/");
                    Dictionary<string, object> folders = GetFolders(web.ServerRelativeUrl, null, Guid.Empty, folderUrl != "/" ? "/" + folderUrl.TrimStart('/') : "/", includeSystemFolder);
                    foreach (var item in files.GetChildren())
                    {
                        webItems.Add(item.ToDictionary());
                    }
                    foreach (var folder in folders.GetChildren())
                    {
                        webFolders.Add(folder.ToDictionary());
                    }
                }
                parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                if (!listId.Equals(Guid.Empty))
                {
                    GetFolderOrItemVersions(parentFolder, webId, listId, needLoadFields, "Items");
                    if (list?.BaseTemplate == (int)AveListTemplateType.DiscussionBoard)
                    {
                        GetFolderOrItemVersions(parentFolder, webId, listId, needLoadFields, "Folders");
                    }
                }
                else
                {
                    List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)parentFolder["Items"];
                    items.ForEach((item) =>
                    {
                        List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                        AssembleWebItemVersionProperty(item, versions);
                        item["HasVersion"] = false;
                    });
                }
                return parentFolder;
            }
        }

        private void GetFolderOrItemVersions(Dictionary<string, object> parentFolder, Guid webId, Guid listId, Dictionary<string, string> needLoadFields, string ItemsOrFolder)
        {
            List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)parentFolder[ItemsOrFolder];
            string webUrl = parentFolder.ContainsKey("WebServerRelativeUrl") ?
                parentFolder["WebServerRelativeUrl"].ToString() : this.GetWeb(webId)["ServerRelativeUrl"].ToString();
            List<DelegateTask> getItemVersionTasks = new List<DelegateTask>();
            items.ForEach((item) =>
            {
                if (item.ContainsKey("Versions") && WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance)
                {
                    var versions = (List<Dictionary<string, object>>)item["Versions"];

                    if (versions.Count == 0)
                    {
                        getItemVersionTasks.Add(() => { this.GetListItemVersion(item, webUrl, listId, needLoadFields); });
                    }
                }
                else if (item.ContainsKey("ID"))
                {// list enable version is false, we just add current version here
                    List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                    this.AssembleItemVersionProperty(item, versions);
                    item["HasVersion"] = false;
                }
            });
            if (getItemVersionTasks.Count > 0)
            {
                using (var taskExecutor = new CountableTaskExecutor(WrapperConfiguration.WrapperConfigurationForBPOS.MaximumThreadsGettingVersions))
                {
                    taskExecutor.Execute(getItemVersionTasks, true);
                }
            }
        }

        private void GetListItemVersion(Dictionary<string, object> item, string webUrl, Guid listId, Dictionary<string, string> needLoadFields)
        {
            List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)item["Versions"];
            //Dictionary<string, object> allVersionProperties = mWebServiceRequest.GetItemVersions(webUrl, "", listId.ToString(), (int)item["Id"], "", null, needLoadFields, false);
            var allVersionProperties = GetItemVersions(webUrl, "", listId.ToString(), (int)item["Id"], "", null, needLoadFields, false);
            if (allVersionProperties.ContainsKey("HasVersion") && !Convert.ToBoolean(allVersionProperties["HasVersion"]))
            {
                AssembleItemVersionProperty(item, versions);
                item["HasVersion"] = false;
            }
            else
            {
                var versionProperties = allVersionProperties.GetChildren();
                foreach (var version in versionProperties)
                {
                    version["ID"] = (int)item["Id"];
                    version["GUID"] = new Guid(item["GUID"].ToString());
                    version["Size"] = 0;
                    version["ObjType"] = item["ObjType"];
                    version["TimeLastModified"] = version["Modified"];
                    int versionId = (int)version["VersionId"];
                    version["Level"] = (byte)version["Level"];
                    version["UIVersion"] = version["VersionId"];
                    version["UserDataGuid"] = version["GUID"];
                    version["IsCurrentVersion"] = versionId == (int)item["UIVersion"] ? true : false;
                    versions.Add(version.ToDictionary());
                }
            }
        }

        private void AssembleItemVersionProperty(Dictionary<string, object> item, List<Dictionary<string, object>> versions)
        {
            Dictionary<string, object> version = new Dictionary<string, object>();
            version["ID"] = (int)item["ID"];
            if (item.ContainsKey("GUID"))  //Survey List item没有GUID
            {
                version["GUID"] = new Guid(item["GUID"].ToString());
                version["UserDataGuid"] = item["GUID"];
            }
            version["Size"] = 0;
            version["ObjType"] = item["ObjType"];
            version["TimeLastModified"] = item["TimeLastModified"];
            version["UIVersion"] = item["UIVersion"];
            version["IsCurrentVersion"] = item["_IsCurrentVersion"];
            version["Level"] = item["Level"];
            versions.Add(version);
            item["Versions"] = versions;
        }

        private void GetChangeItemsFromChangeCache(Dictionary<string, object> changedItems, Guid webId, Guid listId, string folderUrl, Dictionary<string, object> changeCache)
        {
            using (var context = CreateRetryContext())
            {
                //List<Dictionary<string, object>> changeFiles = changedItems["ChangeFile"] as List<Dictionary<string, object>>;
                List<Dictionary<string, object>> changeItems = changedItems["Items"] as List<Dictionary<string, object>>;
                List<Dictionary<string, object>> changeFolders = changedItems["Folders"] as List<Dictionary<string, object>>;
                Dictionary<string, object> tempFolders = new Dictionary<string, object>();
                //SAAS-22856，用于缓存uniqueId，判断是否有重复的id
                HashSet<object> uniqueIdCache = new HashSet<object>();
                Site site = context.Site;
                Web web = site.OpenWebById(webId);
                List list = web.Lists.GetById(listId);
                context.Load(list.RootFolder, folder => folder.ServerRelativeUrl);
                context.Load(list, l => l.EnableVersioning, l => l.EnableMinorVersions, l => l.ParentWebUrl, l => l.BaseTemplate, l => l.BaseType);
                context.ExecuteQuery();
                foreach (string key in changeCache.Keys)
                {
                    try
                    {
                        switch (key)
                        {
                            case "ChangedFolderCache":
                                Dictionary<Guid, object> foldersInCache = changeCache[key] as Dictionary<Guid, object>;
                                foreach (KeyValuePair<Guid, object> tempFolder in foldersInCache)
                                {
                                    Dictionary<string, object> changedProperties = tempFolder.Value as Dictionary<string, object>;
                                    Guid parentWebId = new Guid(changedProperties["WebId"].ToString());
                                    if (!parentWebId.Equals(webId))
                                    {
                                        continue;
                                    }
                                    mLogger.Info("unexpected change event");
                                    foreach (KeyValuePair<string, object> changedKV in changedProperties)
                                    {
                                        mLogger.Info("key: {0}, value: {1}", changedKV.Key, changedKV.Value);
                                    }
                                }
                                break;
                            case "ChangedFileCache":
                                Dictionary<Guid, object> filesInCache = changeCache[key] as Dictionary<Guid, object>;
                                Dictionary<Guid, object> files = filesInCache.ContainsKey(listId) ? filesInCache[listId] as Dictionary<Guid, object> : new Dictionary<Guid, object>();
                                string attachmentFolderUrl = list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Attachments/";
                                foreach (KeyValuePair<Guid, object> filePair in files)
                                {
                                    Dictionary<string, object> changedProperties = filePair.Value as Dictionary<string, object>;
                                    bool isDeleteView = false;
                                    Dictionary<string, object> properties = new Dictionary<string, object>();
                                    if (changedProperties.ContainsKey("SPChangeType"))
                                    {
                                        properties["SPChangeType"] = changedProperties["SPChangeType"].ToString();
                                    }
                                    if (changedProperties.ContainsKey("ChangeObjectType") && (ChangeObjectType)changedProperties["ChangeObjectType"] == ChangeObjectType.View)
                                    {
                                        //view item query for item
                                        ClientFile file = GetFileByViewGuid(context, list, new Guid(changedProperties["ViewId"].ToString()));
                                        if (file == null && (AveChangeType)changedProperties["ChangeType"] == AveChangeType.Delete && changedProperties.ContainsKey("ViewId"))
                                        {
                                            isDeleteView = true;
                                            properties["ViewId"] = new Guid(changedProperties["ViewId"].ToString());
                                            properties["ServerRelativeUrl"] = folderUrl.TrimEnd('/') + "/" + changedProperties["ViewId"].ToString();
                                            properties["FullUrl"] = properties["ServerRelativeUrl"];
                                            properties["ChangeTime"] = changedProperties["Time"];
                                        }
                                        else if (file == null)
                                        {
                                            mLogger.Debug("View is Invalide.");
                                            continue;
                                        }
                                        else
                                        {
                                            if ((AveChangeType)changedProperties["ChangeType"] == AveChangeType.Delete)
                                            {
                                                mLogger.Debug("The event is delete view ,but the view is exist now");
                                                continue;
                                            }
                                            AssembleFileProperties(properties, file as ClientFile, list.ParentWebUrl, null);
                                            properties["ID"] = properties["DocLibRowId"];
                                            properties["GUID"] = Guid.Empty;
                                            properties["_IsCurrentVersion"] = true;
                                        }
                                    }
                                    else if (changedProperties.ContainsKey("ChangeObjectType") && (ChangeObjectType)changedProperties["ChangeObjectType"] == ChangeObjectType.File)
                                    {
                                        if ((AveChangeType)changedProperties["ChangeType"] == AveChangeType.Delete)
                                        {
                                            continue;
                                        }
                                        ClientFile file = null;
                                        try
                                        {
                                            file = web.GetFileById(filePair.Key);
                                            context.Load(file);
                                            context.ExecuteQuery();
                                            if (file.ServerRelativeUrl.StartsWith(attachmentFolderUrl))
                                            {
                                                continue;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mLogger.Warn("Cannot Get file by FileGuid:{0},error:{1}", filePair.Key, e.ToString());
                                            file = null;
                                        }
                                        if (file != null)
                                        {
                                            AssembleFileProperties(properties, file, list.ParentWebUrl, null);
                                            properties["ID"] = properties["DocLibRowId"];
                                            properties["GUID"] = Guid.Empty;
                                            properties["_IsCurrentVersion"] = true;
                                        }
                                        else
                                        {
                                            mLogger.Info("unexpected change event");
                                            foreach (KeyValuePair<string, object> changedKV in changedProperties)
                                            {
                                                mLogger.Info("key: {0}, value: {1}", changedKV.Key, changedKV.Value);
                                            }
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        mLogger.Info("unexpected change event");
                                        foreach (KeyValuePair<string, object> changedKV in changedProperties)
                                        {
                                            mLogger.Info("key: {0}, value: {1}", changedKV.Key, changedKV.Value);
                                        }
                                        continue;
                                    }
                                    properties["ChangeType"] = changedProperties["ChangeType"];
                                    properties["ChangeTime"] = changedProperties["Time"];
                                    if (isDeleteView)
                                    {
                                        properties["ObjType"] = ItemType.View;
                                        changeItems.Add(properties);
                                    }
                                    else
                                    {
                                        properties["ObjType"] = ItemType.Document;
                                        //SAAS-22856,add view 会产生changeView和changeFile两种change，properties完全一样。
                                        object uniqueId;
                                        if ((!properties.TryGetValue("UniqueId", out uniqueId)) || uniqueIdCache.Add(uniqueId))
                                        {
                                            changeItems.Add(properties);
                                        }
                                    }
                                }
                                break;
                            case "ChangedItemCache":
                                Dictionary<string, object> itemsInCache = changeCache[key] as Dictionary<string, object>;
                                foreach (KeyValuePair<string, object> tempItem in itemsInCache)
                                {
                                    Dictionary<string, object> itemChangeProperties = tempItem.Value as Dictionary<string, object>;
                                    AveChangeType changeType = (AveChangeType)itemChangeProperties["ChangeType"];
                                    int itemId = (int)itemChangeProperties["ItemId"];
                                    try
                                    {
                                        if (!tempItem.Key.Equals(listId + ";" + itemId.ToString()))
                                        {
                                            continue;
                                        }
                                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                                        if (itemChangeProperties.ContainsKey("SPChangeType"))
                                        {
                                            itemProperties["SPChangeType"] = itemChangeProperties["SPChangeType"].ToString();
                                        }
                                        if (changeType == AveChangeType.Delete)
                                        {// because bpos cannot check type on delete object, "folder, item or document" will all consider as item here                      
                                            itemProperties["LeafName"] = itemId + "_.000";
                                            itemProperties["DoclibRowId"] = itemId;
                                            itemProperties["ObjType"] = ItemType.Item;
                                            itemProperties["Id"] = itemId;
                                            itemProperties["ServerRelativeUrl"] = folderUrl.TrimEnd('/') + "/" + itemId + "_.000";
                                            itemProperties["FullUrl"] = itemProperties["ServerRelativeUrl"];
                                            itemProperties["ChangeType"] = itemChangeProperties["ChangeType"];
                                            itemProperties["ChangeTime"] = itemChangeProperties["Time"];
                                            if (itemChangeProperties.ContainsKey("UniqueId"))
                                            {
                                                itemProperties["UniqueId"] = itemChangeProperties["UniqueId"];
                                            }
                                            changeItems.Add(itemProperties);
                                            continue;
                                        }
                                        if (itemChangeProperties.ContainsKey("IsRenamed") && itemChangeProperties["IsRenamed"].ToString().Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase))
                                        {
                                            Dictionary<string, object> renameProperties = new Dictionary<string, object>();
                                            if (itemChangeProperties.ContainsKey("SPChangeType"))
                                            {
                                                renameProperties["SPChangeType"] = itemChangeProperties["SPChangeType"].ToString();
                                            }
                                            renameProperties["LeafName"] = itemId + "_.000";
                                            renameProperties["DoclibRowId"] = itemId;
                                            renameProperties["ObjType"] = ItemType.Item;
                                            renameProperties["Id"] = itemId;
                                            renameProperties["ServerRelativeUrl"] = folderUrl.TrimEnd('/') + "/" + itemId + "_.000";
                                            renameProperties["FullUrl"] = renameProperties["ServerRelativeUrl"];
                                            renameProperties["ChangeType"] = (int)AveChangeType.Delete;
                                            changeItems.Add(renameProperties);
                                        }
                                        ListItem item = list.GetItemById(itemId);

                                        context.Load(item);
                                        context.Load(item, i => i.HasUniqueRoleAssignments, i => i.DisplayName);
                                        context.ExecuteQuery();

                                        GetItemDic(itemProperties, item);
                                        if (ItemHasVersion(list, itemProperties))
                                        {
                                            itemProperties["Versions"] = new List<Dictionary<string, object>>();
                                        }

                                        itemProperties["FullUrl"] = itemProperties["ServerRelativeUrl"];
                                        itemProperties["ChangeType"] = itemChangeProperties["ChangeType"];
                                        itemProperties["ChangeTime"] = itemChangeProperties["Time"];
                                        itemProperties["LeafName"] = itemProperties.ContainsKey("LeafName") ? itemProperties["LeafName"] : itemProperties.ContainsKey("Name") ? itemProperties["Name"] : item.DisplayName;
                                        itemProperties["HasStream"] = false;
                                        string fullUrl = itemProperties["FullUrl"].ToString();
                                        string parentFolderUrl = "/" + fullUrl.Substring(0, fullUrl.LastIndexOf('/')).Trim('/');
                                        if (!parentFolderUrl.Trim('/').Equals(folderUrl.Trim('/')))
                                        {
                                            mLogger.Debug("The item is not in the current parent folder.ItemUrl:{0}\t\rParentFolderUrl:{1}", fullUrl, folderUrl);
                                            if (!parentFolderUrl.Trim('/').StartsWith(folderUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                                            {
                                                continue;
                                            }
                                            if (!tempFolders.ContainsKey(parentFolderUrl))
                                            {
                                                Folder parentFolder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(parentFolderUrl));
                                                AddParentFolderToCache(context, list, parentFolder, tempFolders, changeFolders);
                                            }
                                        }
                                        itemProperties["Attachments"] = new List<Dictionary<string, object>>();
                                        GetAttachmentsFromItem(context, list, itemProperties, list.RootFolder.ServerRelativeUrl);
                                        Guid uniqueId = new Guid(itemProperties["UniqueId"].ToString());
                                        if (item.FileSystemObjectType == FileSystemObjectType.Folder)
                                        {
                                            itemProperties["ObjType"] = ItemType.Folder;
                                            tempFolders[fullUrl] = itemProperties;
                                            changeFolders.Add(itemProperties);
                                        }
                                        else
                                        {

                                            if (itemProperties.ContainsKey("Length") && Convert.ToInt32(itemProperties["Length"]) > 0)
                                            {
                                                itemProperties["ObjType"] = ItemType.Document;
                                                itemProperties["HasStream"] = true;
                                                itemProperties["Size"] = itemProperties["Length"] = Convert.ToInt32(itemProperties["Length"]);
                                                changeItems.Add(itemProperties);
                                            }
                                            else
                                            {
                                                itemProperties["ObjType"] = ItemType.Item;
                                                changeItems.Add(itemProperties);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        mLogger.Warn($"An error occured when process one changed item, Id:{itemId},changeType:{changeType.ToString()},webId:{webId.ToString()},listId:{listId.ToString()},folderUrl:{folderUrl}, Error:{ex.Message}, StackTrace:{ex.StackTrace}");
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("Get one change item failed,key:{0},error:{1}", key, e.ToString());
                    }
                }
                tempFolders.Clear();
            }
        }

        private ClientFile GetFileByViewGuid(ClientContext context, List list, Guid viewGuid)
        {
            try
            {
                //need to be optimised, to reduce the request count.
                context.Load(list, tempList => tempList.ParentWebUrl);
                View view = list.GetView(viewGuid);
                context.Load(view, tempView => tempView.ServerRelativeUrl);
                context.ExecuteQuery();
                ClientFile file = list.ParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(view.ServerRelativeUrl));
                context.Load(file);
                context.ExecuteQuery();
                return file;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Cannot Get file by viewGuid:{0},error:{1}", viewGuid, ex.ToString());
                return null;
            }
        }

        private void GetItemsFromFolder(ClientContext context, Web web, List list, Folder folder, string folderServerRelativeUrl, Dictionary<string, object> parentFolder, bool loadSubFolders, bool loadSubItems, bool isExceedListViewThreshold)
        {
            List<Dictionary<string, object>> items = parentFolder["Items"] as List<Dictionary<string, object>>;
            List<Dictionary<string, object>> folders = parentFolder["Folders"] as List<Dictionary<string, object>>;
            //Query Item
            string rootFolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            int totalItemCount = folder.ItemCount;//list.RootFolder.ItemCount;            
            int subfolderCount = folder.Properties.FieldValues.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folder.Properties.FieldValues["vti_foldersubfolderitemcount"]) : 0;
            this.SwitchListContext(list);
            IList<int> subItemIds = new List<int>(totalItemCount);
            IList<string> subItemUniqueIds = new List<string>(totalItemCount);
            this.mCurrentList.FoldersToSubItemIds[folderServerRelativeUrl] = subItemIds;
            this.mCurrentList.FoldersToSubItemUniqueIds[folderServerRelativeUrl] = subItemUniqueIds;
            if (totalItemCount > 0)
            {
                List<Dictionary<string, object>> listItems = null;

                if (loadSubItems)
                {
                    listItems = GetItemsByCamlIncludeRequestedFields(context, list, web.ServerRelativeUrl, folderServerRelativeUrl, totalItemCount, subItemIds);

                    foreach (Dictionary<string, object> item in listItems)
                    {
                        if (ItemHasVersion(list, item))
                        {
                            item["Versions"] = new List<Dictionary<string, object>>();
                        }
                        item["Attachments"] = new List<Dictionary<string, object>>();
                        item["RbsId"] = null;
                        if (list.BaseType != BaseType.DocumentLibrary)
                        {
                            GetAttachmentsFromItem(context, list, item, rootFolderServerRelativeUrl);
                        }
                        if (item.ContainsKey("UniqueId"))
                        {
                            subItemUniqueIds.Add(item["UniqueId"].ToString());
                        }
                        items.Add(item);
                    }
                }
                if (subfolderCount > 0 && loadSubFolders)
                {
                    //Query Folder                                            
                    listItems = GetFoldersByCamlIncludeRequestedFields(context, list, web.ServerRelativeUrl, folderServerRelativeUrl, subfolderCount, subItemIds);

                    foreach (Dictionary<string, object> item in listItems)
                    {
                        item["Items"] = new List<Dictionary<string, object>>();
                        item["Folders"] = new List<Dictionary<string, object>>();
                        item["Attachments"] = new List<Dictionary<string, object>>();
                        if (ItemHasVersion(list, item))
                        {
                            item["Versions"] = new List<Dictionary<string, object>>();
                        }
                        item["ItemId"] = item["Id"];
                        item["Hidden"] = (item["Id"] == null) ? true : false;
                        GetAttachmentsFromItem(context, list, item, rootFolderServerRelativeUrl);
                        folders.Add(item);
                    }
                }
            }
            GetSystemFoldersAndFiles(context, folders, items, list, folder, web.ServerRelativeUrl, folderServerRelativeUrl, isExceedListViewThreshold);
            //Add to Query View Item by Client API
            AddViewItems(context, list, folderServerRelativeUrl, items, folders, isExceedListViewThreshold);
        }

        private void AddViewItems(ClientContext context, List list, string folderServerRelativeUrl, List<Dictionary<string, object>> items, List<Dictionary<string, object>> folders, bool isExceedListViewThreshold)
        {
            bool isRootFolder = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
            bool isForms = folderServerRelativeUrl.TrimEnd('/').Equals(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
            if (!isRootFolder && !isForms)
            {
                return;
            }
            Folder folder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
            mLogger.Info($"Discover_AddViewItems to load List:[{list.BaseType}] ,SubFolder:[{folderServerRelativeUrl}] ,ExceedListViewThreshold:[{isExceedListViewThreshold}]");
            if (list.BaseType != BaseType.DocumentLibrary)
            {
                if (!isRootFolder || !WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView)
                {
                    return;
                }
                if (isExceedListViewThreshold)
                {
                    context.Load(list.Views);
                    context.ExecuteQuery();
                    foreach (View view in list.Views)
                    {
                        if (!string.IsNullOrEmpty(view.ServerRelativeUrl) && view.ServerRelativeUrl.StartsWith(folderServerRelativeUrl.TrimEnd('/') + '/'))
                        {
                            ClientFile viewFile = list.ParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(view.ServerRelativeUrl));
                            context.Load(viewFile);
                            context.ExecuteQuery();
                            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                            AssembleViewFileProperties(itemProperty, viewFile);
                            itemProperty["ObjType"] = 2;//set default value to 2.
                            itemProperty["IsSystemFile"] = true;
                            this.mCurrentList.Files[viewFile.ServerRelativeUrl] = itemProperty;
                            items.Add(itemProperty);
                        }
                    }
                }
                else
                {
                    context.Load(folder);
                    context.Load(folder.Files);
                    context.ExecuteQuery();
                    foreach (ClientFile viewFile in folder.Files)
                    {
                        Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                        AssembleViewFileProperties(itemProperty, viewFile);
                        itemProperty["ObjType"] = 2;//set default value to 2.
                        itemProperty["IsSystemFile"] = true;
                        this.mCurrentList.Files[viewFile.ServerRelativeUrl] = itemProperty;
                        items.Add(itemProperty);
                    }
                }
                //if (folder.ItemCount < 5000)
                //{
                //    foreach (ClientFile viewFile in folder.Files)
                //    {
                //        Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                //        AssembleViewFileProperties(itemProperty, viewFile);
                //        itemProperty["ObjType"] = 2;//set default value to 2.
                //        itemProperty["IsSystemFile"] = true;
                //        this.mCurrentList.Files[viewFile.ServerRelativeUrl] = itemProperty;
                //        items.Add(itemProperty);
                //    }
                //}
                //else
                //{
                //    foreach (View view in list.Views)
                //    {
                //        if (!string.IsNullOrEmpty(view.ServerRelativeUrl) && view.ServerRelativeUrl.StartsWith(folderServerRelativeUrl.TrimEnd('/') + '/'))
                //        {
                //            ClientFile viewFile = list.ParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(view.ServerRelativeUrl));
                //            context.Load(viewFile);
                //            context.ExecuteQuery();
                //            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                //            AssembleViewFileProperties(itemProperty, viewFile);
                //            itemProperty["ObjType"] = 2;//set default value to 2.
                //            itemProperty["IsSystemFile"] = true;
                //            this.mCurrentList.Files[viewFile.ServerRelativeUrl] = itemProperty;
                //            items.Add(itemProperty);
                //        }
                //    }
                //}
            }
            else
            {
                if (isRootFolder)
                {
                    try
                    {
                        Folder formsFolder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl + "/Forms"));
                        context.Load(formsFolder);
                        context.ExecuteQuery();
                        Dictionary<string, object> itemPro = new Dictionary<string, object>();
                        itemPro["Items"] = new List<Dictionary<string, object>>();
                        itemPro["Folders"] = new List<Dictionary<string, object>>();
                        itemPro["Attachments"] = new List<Dictionary<string, object>>();
                        itemPro["Versions"] = new List<Dictionary<string, object>>();
                        AssembleViewFolderProperties(itemPro, formsFolder);
                        itemPro["IsSystemFile"] = true;
                        itemPro["ObjType"] = 4;  //Folder
                        itemPro["ItemId"] = itemPro["Id"];
                        //this.mCurrentList.Folders[formsFolder.ServerRelativeUrl] = itemPro;
                        folders.Add(itemPro);
                    }
                    catch (Exception e)
                    {
                        mLogger.Info($"Discover Root folder {e.ToString()}");
                    }
                }
                else if (isForms && WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView)
                {
                    context.Load(folder, f => f.Files);
                    context.ExecuteQuery();
                    foreach (ClientFile viewFile in folder.Files)
                    {
                        //if (WrapperConfiguration.BPOS_S.IncludeListView)
                        {
                            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                            AssembleViewFileProperties(itemProperty, viewFile);
                            itemProperty["ObjType"] = 2;//set default value to 2.
                            itemProperty["IsSystemFile"] = true;
                            this.mCurrentList.Files[viewFile.ServerRelativeUrl] = itemProperty;
                            items.Add(itemProperty);
                        }
                    }
                }
            }
        }

        private bool ItemHasVersion(List list, Dictionary<string, object> item)
        {
            return list.BaseTemplate != 0x70 && (list.EnableMinorVersions || list.EnableVersioning) && (list.BaseType == BaseType.DocumentLibrary || Convert.ToInt32(item["UIVersion"]) > 512);
            //0x70 means user info list
            //return list.BaseTemplate != 0x70 && (list.BaseType == BaseType.DocumentLibrary || Convert.ToInt32(item["UIVersion"]) > 512);
        }

        private List<Dictionary<string, object>> LoadVersionsForItem(Web web, Guid listId, ListItem item, Dictionary<string, object> itemProperties)
        {
            var versions = new List<Dictionary<string, object>>();
            try
            {
                string webUrl = web.ServerRelativeUrl ?? "/";
                object fullUrlObj;
                string itemUrl = itemProperties.TryGetValue("FullUrl", out fullUrlObj) ? fullUrlObj as string : string.Empty;
                var versionResponse = GetItemVersions(webUrl, string.Empty, listId.ToString(), item.Id, itemUrl ?? string.Empty, null, BuildVersionLoadFields(), true);
                if (versionResponse != null && (!versionResponse.ContainsKey("HasVersion") || Convert.ToBoolean(versionResponse["HasVersion"])))
                {
                    var versionChildren = versionResponse.GetChildren();
                    if (versionChildren != null)
                    {
                        versions = ConvertVersionPropertiesForItem(versionChildren, itemProperties, item.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Failed to load versions for item {0} in list {1}. {2}", item.Id, listId, ex);
            }

            return versions;
        }

        private static Dictionary<string, string> BuildVersionLoadFields()
        {
            return new Dictionary<string, string>
            {
                {"_Level", "Integer"},
                {"_UIVersion", "Integer"},
                {"_IsCurrentVersion", "Boolean"},
                {"GUID", "Guid"}
            };
        }

        private List<Dictionary<string, object>> ConvertVersionPropertiesForItem(IEnumerable<IDictionary<string, object>> rawVersions, Dictionary<string, object> itemProperties, int itemId)
        {
            var versions = new List<Dictionary<string, object>>();
            if (rawVersions == null)
            {
                return versions;
            }

            Guid itemGuid = ExtractItemGuid(itemProperties);
            object objType;
            if (!itemProperties.TryGetValue("ObjType", out objType))
            {
                objType = 1;
            }
            object uiVersionObj;
            if (!itemProperties.TryGetValue("UIVersion", out uiVersionObj))
            {
                uiVersionObj = 0;
            }
            int currentVersion = Convert.ToInt32(uiVersionObj);
            object timeLastModified;
            if (!itemProperties.TryGetValue("TimeLastModified", out timeLastModified))
            {
                timeLastModified = DateTime.MinValue;
            }

            foreach (var version in rawVersions)
            {
                var versionDic = version.ToDictionary();
                versionDic["ID"] = itemId;
                versionDic["GUID"] = itemGuid;
                versionDic["UserDataGuid"] = itemGuid;
                versionDic["ObjType"] = objType;
                AssignVersionSize(versionDic);
                versionDic["TimeLastModified"] = versionDic.ContainsKey("Modified") ? versionDic["Modified"] : timeLastModified;
                int versionId = versionDic.ContainsKey("VersionId") ? Convert.ToInt32(versionDic["VersionId"]) : currentVersion;
                versionDic["Level"] = versionDic.ContainsKey("Level") ? versionDic["Level"] : (byte)1;
                versionDic["UIVersion"] = versionId;
                versionDic["IsCurrentVersion"] = versionDic.ContainsKey("IsCurrentVersion") ? versionDic["IsCurrentVersion"] : (object)(versionId == currentVersion);
                versions.Add(versionDic);
            }

            return versions;
        }

        private static Guid ExtractItemGuid(Dictionary<string, object> itemProperties)
        {
            object guidObj;
            if (itemProperties.TryGetValue("GUID", out guidObj))
            {
                if (guidObj is Guid guid)
                {
                    return guid;
                }
                Guid parsedGuid;
                if (Guid.TryParse(guidObj.ToString(), out parsedGuid))
                {
                    return parsedGuid;
                }
            }
            return Guid.Empty;
        }

        private static void AssignVersionSize(Dictionary<string, object> versionDic)
        {
            object lengthObj;
            if (versionDic.TryGetValue("Length", out lengthObj))
            {
                versionDic["Size"] = Convert.ToInt64(lengthObj);
            }
            else if (!versionDic.ContainsKey("Size"))
            {
                versionDic["Size"] = 0L;
            }
        }

        private List<Dictionary<string, object>> GetItemsByCamlIncludeRequestedFields(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, int totalItemCount, IList<int> subitemIds)
        {
            if (list.BaseType == BaseType.DocumentLibrary)
            {
                if (mCurrentList.ListId.Equals(list.Id) && mCurrentList.Loaded)
                {
                    return GetListItemsByCamlIncludeRequestedFields(context, list, webServerRelativeUrl, folderUrl, totalItemCount, subitemIds);
                }
                else
                {
                    try
                    {
                        return GetFilesByCamlIncludeRequestedFields(context, list, webServerRelativeUrl, folderUrl, totalItemCount, subitemIds);
                    }
                    /*review-qlluo*/
                    catch (ServerException e)
                    {
                        if (e.ServerErrorCode == -2147024860)
                        {
                            if (this.mCurrentList != null)
                            {
                                this.mCurrentList.Items.Clear();
                                this.mCurrentList.Loaded = false;
                                this.mCurrentList.ExceedListViewThreshold = true;
                            }
                            mLogger.Warn("the items under a folder exceed the listviewthreshold.", e.ToString());
                            return GetListItemsByCamlIncludeRequestedFields(context, list, webServerRelativeUrl, folderUrl, totalItemCount, subitemIds);
                        }
                        throw;
                    }
                }
            }
            else
            {
                return GetListItemsByCamlIncludeRequestedFields(context, list, webServerRelativeUrl, folderUrl, totalItemCount, subitemIds);
            }
        }

        private List<Dictionary<string, object>> GetListItemsByCamlIncludeRequestedFields(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, int totalItemCount, IList<int> subitemIds)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            SwitchListContext(list);
            if (!this.mCurrentList.Loaded)
            {
                if (list.BaseTemplate == (int)AveListTemplateType.UserInformation)
                {
                    CacheListItemsInSmallList(context, list, webServerRelativeUrl, folderUrl, totalItemCount, subitemIds);
                }
                else
                {
                    CacheAllListItemsInLargeList(context, list, webServerRelativeUrl, folderUrl, totalItemCount, subitemIds);
                }
                this.mCurrentList.Loaded = true;
            }

            foreach (KeyValuePair<int, Dictionary<string, object>> item in this.mCurrentList.Items)
            {
                if (item.Value.ContainsKey("FileDirRef") && folderUrl.Equals(item.Value["FileDirRef"]))
                {
                    subitemIds.Add((int)item.Value["Id"]);
                    results.Add(item.Value);
                }
            }
            return results;
        }

        private void CacheAllListItemsInLargeList(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, int totalItemCount, IList<int> subitemIds)
        {
            var performanceTimer = Stopwatch.StartNew();
            Stopwatch timer = new Stopwatch();
            timer.Start();

            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            ListItemCollection listItems = null;
            int index = 0;
            int totalCount = list.ItemCount;
            int objectType = list.BaseType == BaseType.DocumentLibrary ? 2 : 1;
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                            "<View Scope=\"RecursiveAll\">" +
                            "<Query><Where><And><Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query>" +
                            "<RowLimit>{2}</RowLimit>" +
                            "</View>", index, index + RowIdStep, RowIdStep);
                int lastIndex = index;
                listItems = list.GetItems(camlQuery);
                //context.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                        items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus));
                }
                else
                {
                    context.Load(listItems, items => items.ListItemCollectionPosition,
                        items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                }
                context.ExecuteQuery();

                for (int i = 0; i < listItems.Count; i++)
                {
                    if (Convert.ToInt32(listItems[i]["FSObjType"]) == (int)FileSystemObjectType.Folder)
                    {
                        continue;
                    }
                    Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                    GetItemDic(itemProperty, listItems[i]);
                    if (list.BaseType == BaseType.DocumentLibrary)
                    {
                        itemProperty["CustomizedPageStatus"] = (int)listItems[i].File.CustomizedPageStatus;
                        itemProperty["ObjType"] = 2;
                        if (listItems[i].FieldValues.ContainsKey("File_x0020_Size")) //for RP file.Length
                        {
                            itemProperty["Length"] = long.Parse(listItems[i]["File_x0020_Size"].ToString());
                        }
                    }
                    else
                    {
                        itemProperty["ObjType"] = 1;
                    }
                    itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = listItems[i].FieldValues.ContainsKey("Attachments") ? listItems[i].FieldValues["Attachments"] : false;
                    this.mCurrentList.Items[listItems[i].Id] = itemProperty;
                    //subitemIds.Add(listItems[i].Id);
                    results.Add(itemProperty);
                    index = index < listItems[i].Id ? listItems[i].Id : index;
                }
                index = lastIndex + RowIdStep < index ? index : lastIndex + RowIdStep;
                totalCount -= listItems.Count;
                if (listItems.Count > 0)
                {
                    timer.Reset();
                    timer.Start();
                }
                if (timer.ElapsedMilliseconds > CACHE_TIME_OUT)
                {
                    mLogger.Warn("Timeout when caching items under list : {0}", folderUrl);
                    break;
                }
            }
            while (totalCount > 0);
            timer.Stop();
            performanceTimer.Stop();

            mLogger.Info("load all items under list:{0} under web:{1} takes {2}", list.Title, webServerRelativeUrl, performanceTimer.Elapsed);

            EnsureParentThreadId(list, results);
        }

        private void CacheListItemsInSmallList(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, int totalItemCount, IList<int> subitemIds)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            ListItemCollection listItems = null;
            int totalCount = list.ItemCount;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(folderUrl);
            listItems = list.GetItems(camlQuery);
            if (list.BaseType == BaseType.DocumentLibrary)
            {
                context.Load(listItems, items => items.ListItemCollectionPosition,
                    items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus).Where(item => (string)item["FSObjType"] == "0"));
            }
            else
            {
                context.Load(listItems, items => items.ListItemCollectionPosition,
                    items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "0"));
            }
            context.ExecuteQuery();

            for (int i = 0; i < listItems.Count; i++)
            {
                Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                GetItemDic(itemProperty, listItems[i]);
                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    itemProperty["CustomizedPageStatus"] = (int)listItems[i].File.CustomizedPageStatus;
                    itemProperty["ObjType"] = 2;
                    if (listItems[i].FieldValues.ContainsKey("File_x0020_Size")) //for RP file.Length
                    {
                        itemProperty["Length"] = long.Parse(listItems[i]["File_x0020_Size"].ToString());
                    }
                }
                else
                {
                    itemProperty["ObjType"] = 1;
                }
                itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = listItems[i].FieldValues.ContainsKey("Attachments") ? listItems[i].FieldValues["Attachments"] : false;
                this.mCurrentList.Items[listItems[i].Id] = itemProperty;
                //subitemIds.Add(listItems[i].Id);
                results.Add(itemProperty);
            }

            EnsureParentThreadId(list, results);
        }

        private List<Dictionary<string, object>> GetFilesByCamlIncludeRequestedFields(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, int totalItemCount, IList<int> subitemIds)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

            SwitchListContext(list);

            ListItemCollection listItems = null;
            //Dictionary<string, ClientFile> filesMap = new Dictionary<string, ClientFile>();

            //FileCollection files = list.ParentWeb.GetFolderByServerRelativeUrl(folderUrl).Files;

            //context.Load(files);
            //context.ExecuteQuery();

            //foreach (ClientFile file in files)
            //{
            //    filesMap[file.ServerRelativeUrl] = file;
            //}

            int totalCount = totalItemCount;
            //IList<string> filesets = new List<string>(files.Count);
            ListItemCollectionPosition itemPosition = null;
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                            "<View Scope=\"FilesOnly\">" +
                            "<RowLimit>{0}</RowLimit>" +
                            "</View>", 500);
                camlQuery.ListItemCollectionPosition = itemPosition;
                camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(folderUrl);

                listItems = list.GetItems(camlQuery);

                context.Load(listItems, items => items.ListItemCollectionPosition,
                    items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus));
                context.ExecuteQuery();

                for (int i = 0; i < listItems.Count; i++)
                {
                    if (listItems[i].FileSystemObjectType == FileSystemObjectType.File)
                    {
                        //RECO-4751 客户环境取不到Author
                        if (!listItems[i].FieldValues.ContainsKey("Author"))
                        {
                            mLogger.Info($"Reload item to get Author proprty. ItemId:[{listItems[i].Id}]");
                            context.Load(listItems[i]);
                            context.ExecuteQuery();
                        }

                        Dictionary<string, object> itemProperty = new Dictionary<string, object>();

                        GetItemDic(itemProperty, listItems[i]);
                        //itemProperty["ObjType"] = 1;
                        itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = listItems[i].FieldValues.ContainsKey("Attachments") ? listItems[i].FieldValues["Attachments"] : false;

                        if (listItems[i].FieldValues.ContainsKey("FileRef") && !string.IsNullOrEmpty(listItems[i]["FileRef"] as string))// && filesMap.ContainsKey(listItems[i]["FileRef"] as string))
                        {
                            string fileRelativeUrl = listItems[i]["FileRef"] as string;
                            //ClientFile file = filesMap[fileRelativeUrl];
                            itemProperty["ServerRelativeUrl"] = fileRelativeUrl;
                            //Dictionary<string, object> fileProperty = new Dictionary<string, object>();
                            //AssembleBasicFileProperties(fileProperty, file, webServerRelativeUrl);
                            //itemProperty["File" + AveObjectModelConstant.ObjectPropertySuffix] = fileProperty;

                            //this.mCurrentList.Files[fileRelativeUrl] = fileProperty;
                            //filesets.Add(fileRelativeUrl);
                        }
                        itemProperty["ObjType"] = 2;
                        itemProperty["CustomizedPageStatus"] = (int)listItems[i].File.CustomizedPageStatus;
                        if (listItems[i].FieldValues.ContainsKey("File_x0020_Size")) //for RP  file.Length
                        {
                            itemProperty["Length"] = long.Parse(listItems[i]["File_x0020_Size"].ToString());
                        }

                        this.mCurrentList.Items[listItems[i].Id] = itemProperty;
                        subitemIds.Add(listItems[i].Id);
                        results.Add(itemProperty);
                    }
                }
                itemPosition = listItems.ListItemCollectionPosition;
            }
            while (listItems.ListItemCollectionPosition != null);
            //this.mCurrentList.FoldersToSubFiles[folderUrl] = filesets;
            //filesMap.Clear();
            return results;
        }

        private void SwitchListContext(List list)
        {
            if (list.Id != this.mCurrentList.ListId)
            {
                this.mCurrentList.Clear();
                this.mCurrentList.ListId = list.Id;
                this.mCurrentList.ListTitle = list.Title;
                this.mCurrentList.SiteMaxItemsPerThrottleOperation = this.maxItemsPerThrottledOperation;
            }
        }

        private void SwitchListContext(List list, string folderServerRelativeUrl)
        {
            SwitchListContext(list);
            if (!string.Equals(this.mCurrentList.FolderPageInfo.ServerRelativeUrl, folderServerRelativeUrl))
            {
                this.mCurrentList.FolderPageInfo.ServerRelativeUrl = folderServerRelativeUrl;
                this.mCurrentList.FolderPageInfo.StartIndex = 0;
                this.mCurrentList.FolderPageInfo.EndIndex = 0;
                this.mCurrentList.FolderPageInfo.SurplusCount = 0;
                this.mCurrentList.FolderPageInfo.QueryRange = 4999;
                this.mCurrentList.FolderPageInfo.QueryTimer.Reset();
                this.mCurrentList.FolderPageInfo.QueryTimer.Start();
            }
        }

        private void EnsureParentThreadId(List list, List<Dictionary<string, object>> results)
        {
            if (list.BaseTemplate != (int)AveListTemplateType.DiscussionBoard)
            {
                return;
            }
            for (int i = results.Count - 1; i >= 0; i--)
            {
                Dictionary<string, object> currentItemProperties = results[i]["FieldValues"] as Dictionary<string, object>;
                try
                {
                    bool parentFound = false;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string currentThreadIndex = currentItemProperties["ThreadIndex"].ToString();

                        Dictionary<string, object> tempItemProperties = results[j]["FieldValues"] as Dictionary<string, object>;
                        if (currentThreadIndex.StartsWith(tempItemProperties["ThreadIndex"].ToString()))
                        {
                            currentItemProperties["#ThreadIndexParentId"] = tempItemProperties["ID"];
                            parentFound = true;
                            break;
                        }
                    }
                    if (!parentFound)
                    {
                        mLogger.Warn("Can not get ParentItemID with {0}, make it as: {1}", currentItemProperties["ID"], currentItemProperties["ParentFolderId"]);
                        currentItemProperties["#ThreadIndexParentId"] = currentItemProperties["ParentFolderId"];
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Can not find item's parent thread index item. Using ParentFolderId instead of ThreadIndexParentId.Error:{0}", ex.ToString());
                    currentItemProperties["#ThreadIndexParentId"] = currentItemProperties["ParentFolderId"];
                }
            }
        }

        private List<Dictionary<string, object>> GetFoldersWithRequestedProperties(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, int subFolderCount)
        {
            var performanceTimer = Stopwatch.StartNew();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            ListItemCollection listItems = null;
            context.Load(list, l => l.ItemCount);
            int index = 0;
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                            "<View>" +
                                "<Query><Where><And>" +
                                    "<And>" +
                                        "<Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt>" +
                                        "<Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq>" +
                                    "</And>" +
                                    "<Eq><FieldRef Name=\"FSObjType\"/><Value Type=\"Integer\">{2}</Value></Eq>" +
                                "</And></Where></Query>" +
                                "<RowLimit>{3}</RowLimit>" +
                            "</View>",
                            index, index + RowIdStep, (int)FileSystemObjectType.Folder, RowIdStep);
                camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(folderUrl);
                listItems = list.GetItems(camlQuery);
                context.Load(listItems, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                int lastIndex = index;
                context.ExecuteQuery();
                if (listItems.Count > 0)
                {
                    for (int i = 0; i < listItems.Count; i++)
                    {
                        Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                        GetItemDic(itemProperty, listItems[i]);
                        itemProperty["ObjType"] = 4;
                        itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = listItems[i].FieldValues.ContainsKey("Attachments") ? listItems[i].FieldValues["Attachments"] : false;
                        results.Add(itemProperty);
                        index = index < listItems[i].Id ? listItems[i].Id : index;
                    }
                }
                index = lastIndex + RowIdStep < index ? index : lastIndex + RowIdStep;
                subFolderCount -= listItems.Count;
                if (listItems.Count > 0)
                {
                    timer.Reset();
                    timer.Start();
                }
                mLogger.Info($"Query list item count:{listItems.Count}, currentIndex:{index}");
                if (timer.ElapsedMilliseconds > CACHE_TIME_OUT)
                {
                    mLogger.Warn("Timeout when caching items under list : {0}", folderUrl);
                    break;
                }
            }
            while (subFolderCount > 0);


            performanceTimer.Stop();
            mLogger.Info("load sub folders under folder:{0} under web:{1} takes {2}", folderUrl, webServerRelativeUrl, performanceTimer.Elapsed);

            return results;
        }

        protected List<Dictionary<string, object>> GetFoldersByCaml(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, IList<int> subitemIds)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            if (list.Id != this.mCurrentList.ListId)
            {
                this.mCurrentList.Clear();
                this.mCurrentList.ListId = list.Id;
                this.mCurrentList.ListTitle = list.Title;
                //this.mCurrentList.List = list;
                this.mCurrentList.SiteMaxItemsPerThrottleOperation = this.maxItemsPerThrottledOperation;
            }
            ListItemCollection listItems = null;
            CamlQuery camlQuery = new CamlQuery();
            StringBuilder queryXml = new StringBuilder();
            queryXml.Append("<View><RowLimit>");
            queryXml.Append(this.maxItemsPerThrottledOperation);
            queryXml.Append("</RowLimit></View>");
            camlQuery.ViewXml = queryXml.ToString();
            camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(folderUrl);
            ListItemCollectionPosition pos = null;
            do
            {
                camlQuery.ListItemCollectionPosition = pos;
                listItems = list.GetItems(camlQuery);
                context.Load(listItems, items => items.ListItemCollectionPosition,
                                        items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                    item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "1"));
                context.ExecuteQuery();
                if (listItems.Count > 0)
                {
                    foreach (ListItem item in listItems)
                    {
                        if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor")) //for community site discussion list
                        {
                            context.Load(item);
                            context.ExecuteQuery();
                        }
                        Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                        GetItemDic(itemProperty, item);
                        itemProperty["ObjType"] = 4;
                        itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
                        this.mCurrentList.Items[item.Id] = itemProperty;
                        //if (item.FieldValues.ContainsKey("FileRef") && !string.IsNullOrEmpty(item.FieldValues["FileRef"].ToString()))
                        //{
                        //    this.mCurrentList.FoldersToItemIds[item["FileRef"].ToString()] = item.Id;
                        //}
                        results.Add(itemProperty);
                    }
                }
                pos = listItems.ListItemCollectionPosition;
            }
            while (pos != null);
            if (results.Count > 0)
            {
                Folder folder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderUrl));
                context.Load(folder, f => f.Folders.IncludeWithDefaultProperties(tempFolder => tempFolder.ParentFolder.ServerRelativeUrl, tempFolder => tempFolder.Properties, tempFolder => tempFolder.ListItemAllFields.Id));
                context.ExecuteQuery();
                //IList<string> subFolders = new List<string>(folder.Folders.Count);
                foreach (Folder tempFolder in folder.Folders)
                {
                    Dictionary<string, object> folderProperty = new Dictionary<string, object>();
                    AssembleFolderProperties(webServerRelativeUrl, tempFolder, tempFolder.ServerRelativeUrl, folderProperty);
                    //this.mCurrentList.Folders[tempFolder.ServerRelativeUrl] = folderProperty;
                    //subFolders.Add(tempFolder.ServerRelativeUrl);
                }
                //this.mCurrentList.FoldersToSubFolders[folderUrl] = subFolders;
            }
            return results;
        }

        private List<Dictionary<string, object>> GetFoldersByCamlIncludeRequestedFields(ClientContext context, List list, string webServerRelativeUrl, string folderUrl, int subfolderCount, IList<int> subitemIds)
        {
            if (list.Id != this.mCurrentList.ListId)
            {
                this.mCurrentList.Clear();
                this.mCurrentList.ListId = list.Id;
                this.mCurrentList.ListTitle = list.Title;
                //this.mCurrentList.List = list;
                this.mCurrentList.SiteMaxItemsPerThrottleOperation = this.maxItemsPerThrottledOperation;
            }
            List<Dictionary<string, object>> results = null;

            if (mCurrentList.ExceedListViewThreshold)
            {
                results = GetFoldersWithRequestedProperties(context, list, webServerRelativeUrl, folderUrl, subfolderCount);
            }
            else
            {
                try
                {
                    return GetFoldersByCaml(context, list, webServerRelativeUrl, folderUrl, subitemIds);
                }
                catch (ServerException se)
                {
                    if (se.ServerErrorCode == -2147024860)
                    {
                        //if (this.mCurrentList != null)
                        //{
                        //    this.mCurrentList.Clear();
                        //}
                        mCurrentList.ExceedListViewThreshold = true;
                        mLogger.Warn("the items under a folder exceed the listviewthreshold.", se.ToString());
                        results = GetFoldersWithRequestedProperties(context, list, webServerRelativeUrl, folderUrl, subfolderCount);
                    }
                    else
                    {
                        mLogger.Warn("Get the items under folder failed.Error:{0}", se.ToString());
                        throw;
                    }
                }
            }
            if (results == null)
            {
                mLogger.Info("Can not get the items under folder");
            }
            //foreach (Dictionary<string, object> folderProp in results)
            //{
            //    this.mCurrentList.Items[Convert.ToInt32(folderProp["Id"])] = folderProp;
            //    if (folderProp.ContainsKey("FileRef") && !string.IsNullOrEmpty(folderProp["FileRef"].ToString()))
            //    {
            //        this.mCurrentList.FoldersToItemIds[folderProp["FileRef"].ToString()] = Convert.ToInt32(folderProp["Id"]);
            //    }
            //}

            return results;
        }

        private void GetAttachmentsFromItem(ClientContext context, List list, Dictionary<string, object> item, string rootFolderServerRelativeUrl)
        {
            if (item.ContainsKey("Id") && item.ContainsKey("Attachments" + AveObjectModelConstant.ObjectPropertySuffix)
                && Convert.ToBoolean(item["Attachments" + AveObjectModelConstant.ObjectPropertySuffix]))
            {
                int id = (int)item["Id"];
                string attachmentFolderUrl = rootFolderServerRelativeUrl.TrimEnd('/') + "/Attachments/" + id;
                List<Dictionary<string, object>> attachments = item["Attachments"] as List<Dictionary<string, object>>;
                Folder attachmentFolder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(attachmentFolderUrl));
                //SAAS-37935                
                ConditionalScope attachmentFolderExistScope = new ConditionalScope(context, () => attachmentFolder.Exists);
                ExceptionHandlingScope handleAuthorNotExist = new ExceptionHandlingScope(context);
                using (attachmentFolderExistScope.StartScope())
                {
                    using (attachmentFolderExistScope.StartIfTrue())
                    {
                        context.Load(attachmentFolder, a => a.ServerRelativeUrl);
                        using (handleAuthorNotExist.StartScope())
                        {
                            using (handleAuthorNotExist.StartTry())
                            {
                                context.Load(attachmentFolder.Files, fs => fs.IncludeWithDefaultProperties(file => file.Author, file => file.ModifiedBy));
                            }
                            using (handleAuthorNotExist.StartCatch())
                            {
                                context.Load(attachmentFolder.Files, fs => fs.IncludeWithDefaultProperties());
                            }
                        }
                    }
                }
                context.ExecuteQuery();

                if (!Convert.ToBoolean(attachmentFolderExistScope.TestResult.Value))
                {
                    mLogger.Warn("Attachment folder does not exist any more: {0}", attachmentFolderUrl);
                    return;
                }
                if (handleAuthorNotExist.HasException)
                {
                    mLogger.Warn("Get Files Author Error, attachmentFolderUrl:{0} , Error Message:{1}", attachmentFolderUrl, handleAuthorNotExist.ErrorMessage);
                }
                mLogger.Info($"Attachment folder exists, url:{attachmentFolderUrl}, sub files count:{attachmentFolder.Files.Count()}");
                string attachmentFolderServerRelativeUrl = attachmentFolder.ServerRelativeUrl;
                foreach (ClientFile attachment in attachmentFolder.Files)
                {
                    Dictionary<string, object> attachmentPro = new Dictionary<string, object>();
                    string eTag = attachment.ETag.Trim('"');
                    string[] pros = eTag.Split(',');
                    if (!handleAuthorNotExist.HasException && !attachment.Author.ServerObjectIsNull.Value)
                    {
                        attachmentPro["Author" + AveObjectModelConstant.ObjectPropertySuffix] = attachment.Author.LoginName;
                    }
                    attachmentPro["DocID"] = new Guid(pros[0]);
                    attachmentPro["DirName"] = attachmentFolderServerRelativeUrl;
                    attachmentPro["Name"] = attachmentPro["LeafName"] = attachment.Name;
                    attachmentPro["UIVersion"] = attachment.UIVersion;//统一为UIVersion
                    attachmentPro["DocFlags"] = (int?)null;//cannot get this property
                    attachmentPro["TimeLastModified"] = attachment.TimeLastModified;
                    attachmentPro["TimeCreated"] = attachment.TimeCreated;//SAAS-1049
                    attachmentPro["Level"] = (byte)attachment.Level;
                    attachmentPro["Type"] = (byte)FileSystemObjectType.File;
                    attachmentPro["Size"] = 0; //cannot get this property
                    attachmentPro["Length"] = attachment.Length;//SAAS-1053
                    attachmentPro["ParentID"] = Guid.Empty;
                    attachmentPro["FullUrl"] = attachmentFolderServerRelativeUrl.TrimEnd('/') + "/" + attachmentPro["LeafName"];
                    attachmentPro["CheckoutUserId"] = (int?)null;
                    attachmentPro["HasStream"] = true;
                    attachmentPro["RbsId"] = null;
                    attachmentPro["ServerRelativeUrl"] = attachment.ServerRelativeUrl;
                    attachmentPro["ID"] = (int?)id;
                    this.mCurrentList.Files[attachment.ServerRelativeUrl] = attachmentPro;
                    attachments.Add(attachmentPro);
                }
            }
        }

        private ChangeQuery GenerateChangeQuery(IDictionary<string, object> queryProps)
        {
            bool allChangeObjectTypes = queryProps.ContainsKey("allChangeObjectTypes") ? (bool)queryProps["allChangeObjectTypes"] : false;
            bool allChangeTypes = queryProps.ContainsKey("allChangeTypes") ? (bool)queryProps["allChangeTypes"] : false;
            ChangeQuery query = new ChangeQuery(allChangeObjectTypes, allChangeTypes);
            if (queryProps.ContainsKey("ChangeTokenStart"))
            {
                query.ChangeTokenStart = new ChangeToken()
                {
                    StringValue = queryProps["ChangeTokenStart"].ToString()
                };
            }
            if (queryProps.ContainsKey("ChangeTokenEnd"))
            {
                query.ChangeTokenEnd = new ChangeToken()
                {
                    StringValue = queryProps["ChangeTokenEnd"].ToString()
                };
            }
            AveObjectCopy.UpdateObjectBasicProperties(queryProps, query);
            return query;
        }

        private void GetWebPropertiesForIB(Web web, string siteUrl, string siteServerRelativeUrl, bool webLoaded, Dictionary<string, object> webProperties)
        {
            if (!webLoaded)
            {
                web.Context.Load(web);
                web.Context.ExecuteQuery();
            }
            webProperties["Title"] = web.Title;
            //string Url = string.Empty;
            //if (web.ServerRelativeUrl.Equals("/"))
            //{
            //    Url = this.WebAppName;
            //}
            //else
            //{
            //    Url = siteUrl.Replace(siteServerRelativeUrl, web.ServerRelativeUrl);
            //}
            webProperties["FullUrl"] = web.Url;
            string Name = ".";
            if (!web.ServerRelativeUrl.Equals(siteServerRelativeUrl))
            {
                int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                Name = web.ServerRelativeUrl.Substring(lastSlashIndex + 1);
            }
            webProperties["Name"] = Name;
        }

        private Dictionary<string, object> AssembleChangeListProperties(List list, AveChangeType changeType)
        {
            Dictionary<string, object> listProp = new Dictionary<string, object>();
            Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
            CopyProperty(listProp, list);
            CopyProperty(rootFolderProp, list.RootFolder);
            long flag = 0;
            if (list.EnableVersioning)
                flag |= 0x0000000000000080;
            if (!list.EnableAttachments)
                flag |= 0x0000000000000008;
            listProp["ChangeType"] = (int)changeType;
            listProp["ListId"] = list.Id;
            listProp["Flag"] = flag;
            listProp["Name"] = listProp["Title"];
            listProp["Type"] = listProp["BaseType"];
            listProp["RootFolderUrl"] = rootFolderProp["ServerRelativeUrl"];
            listProp["ServerTemplate"] = listProp["BaseTemplate"];
            if (rootFolderProp.ContainsKey("UniqueId"))
            {
                listProp["RootFolderId"] = rootFolderProp["UniqueId"];
            }
            else
            {
                listProp["RootFolderId"] = Guid.Empty;
            }
            return listProp;
        }

        private Dictionary<Guid, AveChangeType> GetChangeListFormChangeListCache(ClientContext context, Web web, Dictionary<Guid, object> changedListCache, Dictionary<Guid, object> lists)
        {
            Dictionary<Guid, AveChangeType> changeListDic = new Dictionary<Guid, AveChangeType>();
            foreach (KeyValuePair<Guid, object> pair in changedListCache)
            {
                Dictionary<string, object> change = pair.Value as Dictionary<string, object>;
                if (change != null && change.ContainsKey("WebId"))
                {
                    Guid id = new Guid(change["WebId"].ToString());
                    if (id == web.Id)
                    {
                        AveChangeType changeType = (AveChangeType)change["ChangeType"];
                        if (changeType == AveChangeType.Delete)
                        {
                            Dictionary<string, object> listProp = new Dictionary<string, object>();
                            listProp["ChangeType"] = (int)changeType;
                            listProp["ListId"] = pair.Key;
                            lists[pair.Key] = listProp;
                        }
                        else
                        {
                            changeListDic.Add(pair.Key, changeType);
                        }
                    }
                }
            }
            return changeListDic;
        }

        private bool GetListFileChanged(ClientContext context, List list, Dictionary<Guid, object> changedFileCache, DateTime startTime, DateTime endTime)
        {
            bool isListChanged = false;
            ChangeQuery listquery = new ChangeQuery(false, true);
            listquery.File = true;
            listquery.View = true;
            ChangeToken liststartToken = new ChangeToken();
            ChangeToken listendToken = new ChangeToken();
            liststartToken.StringValue = "1;3;" + list.Id.ToString() + ";" + startTime.Ticks.ToString() + ";-1";
            listendToken.StringValue = "1;3;" + list.Id.ToString() + ";" + endTime.Ticks.ToString() + ";-1";
            listquery.ChangeTokenStart = liststartToken;
            listquery.ChangeTokenEnd = listendToken;
            Dictionary<Guid, object> fileCache = new Dictionary<Guid, object>();
            bool viewLoaded = false;
            while (true)
            {
                ChangeCollection listChangeCollection = list.GetChanges(listquery);
                context.Load(listChangeCollection);
                context.ExecuteQuery();
                if (!viewLoaded && NeedLoadView(listChangeCollection))
                {
                    context.Load(list.Views);
                    context.ExecuteQuery();
                    viewLoaded = true;
                }
                ConvertChangedFileToObject(context, list, listChangeCollection, fileCache, list.Id.ToString());
                if (listChangeCollection.Count < 1000)
                {
                    break;
                }
                listquery.ChangeTokenStart = listChangeCollection[999].ChangeToken;
            }
            if (fileCache.Count > 0)
            {
                if (changedFileCache != null)
                {
                    changedFileCache[list.Id] = fileCache;
                }
                isListChanged = true;
            }
            return isListChanged;
        }

        private bool NeedLoadView(ChangeCollection changes)
        {
            return changes.Any(t => t.GetType() == typeof(ChangeView));
        }

        private void ConvertChangedFileToObject(ClientContext context, List list, ChangeCollection changeCollection, Dictionary<Guid, object> changedFileCache, string listId)
        {
            foreach (Change changeObject in changeCollection)
            {
                Dictionary<string, object> objectProperties = new Dictionary<string, object>();
                CopyProperty(objectProperties, changeObject);
                var aveChangeType = GetFileChangeType((SPChangeType)objectProperties["ChangeType"]);
                objectProperties["ChangeType"] = (int)aveChangeType;
                objectProperties["ListId"] = listId;
                switch (changeObject.GetType().ToString())
                {
                    case "Microsoft.SharePoint.Client.ChangeFile":
                        Guid uniqueId = new Guid(objectProperties["UniqueId"].ToString());
                        if (!changedFileCache.ContainsKey(uniqueId))
                        {
                            try
                            {
                                if (context.HasPendingRequest)
                                {
                                    context.ExecuteQuery();
                                }
                                var file = list.ParentWeb.GetFileById(uniqueId);
                                ExceptionHandlingScope fileNotExist = new ExceptionHandlingScope(context);
                                using (fileNotExist.StartScope())
                                {
                                    using (fileNotExist.StartTry())
                                    {
                                        context.Load(file, f => f.ServerRelativeUrl, f => f.Exists);
                                    }
                                    using (fileNotExist.StartCatch())
                                    {
                                        context.Load(file, f => f.Exists);
                                    }
                                }
                                context.ExecuteQuery();
                                if (file.Exists && file.ServerRelativeUrl.StartsWith(list.RootFolder.ServerRelativeUrl))
                                {
                                    objectProperties["ChangeObjectType"] = ChangeObjectType.File;
                                    changedFileCache[uniqueId] = objectProperties;
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Convert change file failed,Guid:{0},Error:{1}", uniqueId, ex);
                            }
                        }
                        break;
                    case "Microsoft.SharePoint.Client.ChangeView":
                        Guid viewId = new Guid(objectProperties["ViewId"].ToString());
                        if (!changedFileCache.ContainsKey(viewId))
                        {
                            var view = list.Views.FirstOrDefault(t => t.Id == viewId);
                            string listFormsFolderServerRelativeUrl =
                                list.BaseType == BaseType.DocumentLibrary ?
                                list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Forms"
                                : list.RootFolder.ServerRelativeUrl;
                            if (view != null)
                            {
                                if (view.ServerRelativeUrl.StartsWith(listFormsFolderServerRelativeUrl))
                                {
                                    objectProperties["ChangeObjectType"] = ChangeObjectType.View;
                                    changedFileCache[viewId] = objectProperties;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private AveChangeType GetFileChangeType(SPChangeType currentChangeType)
        {
            AveChangeType fileChangeType = AveChangeType.None;
            switch (currentChangeType)
            {
                case SPChangeType.Add:
                    fileChangeType = AveChangeType.Add;
                    break;
                case SPChangeType.Update:
                case SPChangeType.SystemUpdate:
                case SPChangeType.Rename:
                    fileChangeType = AveChangeType.Edit;
                    break;
                case SPChangeType.DeleteObject:
                    fileChangeType = AveChangeType.Delete;
                    break;
                case SPChangeType.Restore:
                    fileChangeType = AveChangeType.Restore;
                    break;
                default:
                    mLogger.Info("Get file changed type : {0} ", currentChangeType.ToString());
                    break;
            }
            return fileChangeType;
        }

        /// <summary>
        /// 得到list下系统的view item;
        /// 可以得到指定的forms folder object；
        /// 可以得到指定的list下rootfolder的系统file；
        /// </summary>
        /// <param name="web"></param>
        /// <param name="listRootFolderUrl"></param>
        /// <param name="isGenericList"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetViewItem(Web web, string listRootFolderUrl, bool isGenericList, string dirName, string leafName)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> itemProperty = null;
                bool isRootFolder = ("/" + dirName.TrimStart('/')).Equals(listRootFolderUrl, StringComparison.OrdinalIgnoreCase);
                bool isForms = ("/" + dirName.TrimStart('/')).Equals(listRootFolderUrl.TrimEnd('/') + "/Forms", StringComparison.OrdinalIgnoreCase);
                if (!isRootFolder && !isForms)
                {
                    return null;
                }
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl("/" + dirName.TrimStart('/')));
                context.Load(folder);
                if (isRootFolder && !isGenericList)
                {
                    context.Load(folder.Folders);
                    context.ExecuteQuery();
                    foreach (var tempFolder in folder.Folders)
                    {
                        if (tempFolder.Name.Equals("Forms", StringComparison.OrdinalIgnoreCase) &&
                            leafName.Equals("Forms", StringComparison.OrdinalIgnoreCase))
                        {
                            Dictionary<string, object> property = new Dictionary<string, object>();
                            AssembleViewFolderProperties(property, tempFolder);
                            property["ObjType"] = 4;
                            property["ItemId"] = property["ID"];
                            itemProperty = property;
                            break;
                        }
                    }
                }
                else
                {
                    context.Load(folder.Files);
                    context.ExecuteQuery();
                    foreach (ClientFile viewFile in folder.Files)
                    {
                        if (viewFile.Name.Equals(leafName, StringComparison.OrdinalIgnoreCase))
                        {
                            Dictionary<string, object> property = new Dictionary<string, object>();
                            AssembleViewFileProperties(property, viewFile);
                            property["ObjType"] = 2;
                            itemProperty = property;
                            break;
                        }
                    }
                }
                return itemProperty;
            }
        }

        private ListItem GetListItemByUniqueId(ClientContext context, List list, Guid id)
        {
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\">" +
                "<Query><Where>" +
                "<Eq><FieldRef Name=\"UniqueId\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                "</Where></Query></View>",
                id.ToString());
            ListItemCollection listItems = list.GetItems(camlQuery);
            ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
            using (ehScope.StartScope())
            {
                using (ehScope.StartTry())
                {
                    context.Load(listItems);
                    context.Load(listItems, its => its.Include(t => t.HasUniqueRoleAssignments, t => t.DisplayName));
                }
                using (ehScope.StartCatch())
                {
                    context.Load(listItems);
                    context.Load(listItems, its => its.Include(t => t.HasUniqueRoleAssignments));//SAAS-6084 DisplayName not support discussion board
                }
            }
            context.ExecuteQuery();
            if (ehScope.HasException)
            {
                mLogger.Warn("load item failed due to: {0}", ehScope.ErrorMessage);
            }
            if (listItems.Count != 0)
            {
                return listItems[0];
            }
            throw new ArgumentException("Item does not exist. It may have been deleted by another user.");
        }

        private ListItem GetListItemByDirName(ClientContext context, List list, string dirName, string leafName)
        {
            ListItem item = null;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"Default\">" +
                "<Query><Where><And>" +
                "<Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                "<Eq><FieldRef Name=\"FileLeafRef\"/><Value Type=\"Lookup\">{1}</Value></Eq>" +
                "</And></Where></Query></View>",
                dirName, leafName);
            camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(dirName);
            ListItemCollection listItems = list.GetItems(camlQuery);
            context.Load(listItems);
            context.ExecuteQuery();
            if (listItems.Count == 1)
            {
                item = listItems[0];
            }
            return item;
        }

        private ListItem GetListItemBytpGuid(ClientContext context, List list, Guid tp_Guid)
        {
            ListItem item = null;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\">" +
                "<Query><Where>" +
                "<Eq><FieldRef Name=\"GUID\"/><Value Type=\"Guid\">{0}</Value></Eq>" +
                "</Where></Query></View>",
                tp_Guid.ToString());
            ListItemCollection listItems = list.GetItems(camlQuery);
            context.Load(listItems);
            context.ExecuteQuery();
            if (listItems.Count == 1)
            {
                item = listItems[0];
            }
            return item;
        }

        private Dictionary<string, object> GetContentTypeRelatedFolder(ClientContext context, string schema, Web web, string scope)
        {
            Dictionary<string, object> folderPro = null;
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(schema);
            XmlNode node = xDoc.DocumentElement.SelectSingleNode("Folder");
            if (node != null)
            {
                string folderName = node.Attributes["TargetName"].Value;
                Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(scope + "/" + folderName));
                context.Load(folder);
                context.ExecuteQuery();
                folderPro = new Dictionary<string, object>();
                folderPro.Add("DocID", Guid.Empty);   //Can not get Guid of root folder.
                folderPro.Add("DirName", folder.ServerRelativeUrl.Substring(0, folder.ServerRelativeUrl.Length - (folder.Name.Length + 1)).TrimStart('/'));
                folderPro.Add("LeafName", folder.Name);
                folderPro.Add("ID", null);  //Can not get ID of root folder.
                folderPro.Add("Uiversion", 512);    //Can not get this property.
                folderPro.Add("DocFlags", null);    //Can not get this property.
                folderPro.Add("TimeLastModified", DateTime.MinValue);    //Can not get this property.
                folderPro.Add("Level", Convert.ToByte(1));    //Can not get this property. default value: Published
                folderPro.Add("Type", Convert.ToByte(1));    //Can not get this property.  default value: Folder
                folderPro.Add("Size", 0);    //Can not get this property.
                folderPro.Add("ParentID", Guid.Empty);    //Can not get this property.
                folderPro.Add("FullUrl", folder.ServerRelativeUrl);
                folderPro.Add("CheckoutUserId", (int?)null);
                folderPro.Add("Hidden", (bool?)true);
            }
            return folderPro;
        }


        #endregion
    }
}
