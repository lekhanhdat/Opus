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
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.UniqueIdSetting.Base;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Contract.Global.JobMessage;
using System.Net;
using AgentUtil = AvePoint.RA.SharePoint.Common.Util;

namespace AvePoint.RA.SharePoint.UniqueIdSetting
{
    public class UniqueIdSettingFullProcessor : BaseUniqueIdSettingProcessor
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public UniqueIdSettingFullProcessor(SPTreeNodeDto siteNode, ClientContext clientContext, string searchSiteColumnFileName, UniqueIdSettingJobMessage jobMessage) : base(siteNode, jobMessage)
        {
            var bposInfo = GetBPOSInfo();
            var mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
            curSite = mfactory.CreateSite(siteNode.FullPath);
            currentClientContext = clientContext;
            this.searchSiteColumnFileName = searchSiteColumnFileName;
        }

        public override void Run()
        {
            ProcessSiteCollection(GetDiscoverSite());
        }
        public override void ProcessSiteCollection(AveDiscoverSite discoverSite)
        {
            base.ProcessSiteCollection(discoverSite);
            var discoverWebs = discoverSite.GetWebs();
            foreach (var discoverWeb in discoverWebs.Values)
            {
                logger.Info("Process Web UniqueId setting {0}", discoverWeb.FullUrl.LogBase64());
                if (!CheckWebNeedSkip(discoverWeb))
                {
                    ProcessWeb(discoverWeb);
                }
            }
        }
        public override void ProcessWeb(AveDiscoverWeb discoverWeb)
        {
            base.ProcessWeb(discoverWeb);
            var discoverLists = discoverWeb.GetLists();
            ProgressService.IncreaseBase(discoverLists.Count);
            foreach (var discoverList in discoverLists.Values)
            {
                if (discoverList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                logger.Info("Process list UniqueId setting {0}", discoverList.RootFolderUrl.LogBase64());
                var list = discoverList.GetListObject();
                if (list.BaseType == AveBaseType.DocumentLibrary)
                {
                    try
                    {
                        if (list.Hidden)
                        {
                            logger.Info("Skip the hidden list {0}", discoverList.RootFolderUrl.LogBase64());
                            continue;
                        }
                        if (CheckIsDesignList(list))
                        {
                            logger.Info("Skip the system list {0}", discoverList.RootFolderUrl.LogBase64());
                            continue;
                        }
                        IAveField field = list.Fields.GetFieldById(DocumentIDColumnID, false);
                        if (field != null)
                        {
                            IAveView defaultView = list.DefaultView;
                            IAveViewFieldCollection viewFields = defaultView.ViewFields;
                            if (!viewFields.Exists(RA.Common.Global.SPColumnConstants.DocumentIdUrl))
                            {
                                viewFields.Add(field);
                                defaultView.Update();
                                JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = "Document ID", Action = "RM_UI_Detail_Add", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Successful, Comment = string.Empty });

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Config Document ID column failed {0}", e.ToString());
                        JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = "Document ID", Action = "RM_UI_Detail_Add", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(e) });
                        haveErrorNode = true;
                    }
                }
                else
                {
                    continue;
                    //暂时不支持item
                    ProcessList(discoverList);
                }
            }
        }
        public override void ProcessList(AveDiscoverList discoverList)
        {
            //if (discoverList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
            //{
            //    return;
            //}
            //var list = discoverList.GetListObject();
            //if (!SharePointSettingDao.GetSettingEnableInfoByScope(new Guid(groupNode.SPObjectId), new Guid(curNode.ID), discoverList.ListId))
            //{
            //    logger.Info("Process list SharePoint setting is disable {0}", discoverList.RootFolderId);
            //    reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails()
            //    {
            //        ObjectName = discoverList.Name,
            //        SourceURL = RA.Common.Util.WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url),
            //        ColumnName = curSetting.Name,
            //        Action = "RM_UI_Detail_Add",
            //        Status = JobDetailsStatus.Skipped,
            //        Comment = "RM_JS_JMD_DisableRecordManagement"
            //    });
            //    return;
            //}
            //base.ProcessList(discoverList);
            
            //if (list.Hidden)
            //{
            //    logger.Info("Skip the hidden list {0}", discoverList.RootFolderUrl);
            //    return;
            //}
            //if (CheckIsDesignList(list))
            //{
            //    logger.Info("Skip the system list {0}", discoverList.RootFolderUrl);
            //    return;
            //}
            //var discoverRootFolder = discoverList.GetRootFolder();

            //bool needQueryNext = false;
            //int maxItemId = GetLastItemId(list, list.RootFolder);
            //int startIdx = 0;
            //int lastIdx = 0;
            //AveCamlQuery query = GetQuery(list.RootFolder, startIdx, startIdx + MaxItemsPerThrottledOperation, MaxItemsPerThrottledOperation);
            //logger.Info($"Get items under [{list.RootFolder.ServerRelativeUrl}]");
            //do
            //{
            //    logger.Info($"StartIndex:[{startIdx}] LastIndex:[{lastIdx}] MaxItemId:[{maxItemId}]");
            //    var items = list.GetItemsForRecords(query);
            //    if (items.Count > 0)
            //    {
            //        ProcessItems(items);
            //        int curIdx = items.Max(i => i.ID);
            //        startIdx = curIdx;
            //    }
            //    else
            //    {
            //        startIdx = lastIdx;
            //    }
            //    int endIdx = startIdx + MaxItemsPerThrottledOperation;
            //    lastIdx = endIdx;
            //    needQueryNext = startIdx < maxItemId;
            //    if (needQueryNext)
            //    {
            //        logger.Info($"Query Next");
            //        query.ViewXml = GetQueryXml(startIdx, endIdx, MaxItemsPerThrottledOperation);
            //    }
            //    else
            //    {
            //        logger.Info($"Query finished.");
            //    }
            //}
            //while (needQueryNext);
        }
        public void ProcessItems(IAveListItemCollection items)
        {
            logger.Info($"Process item count:{items.Count}");
            //reportManager.IncreaseBase(items.Count);
            foreach (var item in items)
            {
                try
                {
                    //reportManager.Increase();
                    SetUniqueId(item);
                }
                catch (Exception e)
                {
                    haveErrorNode = true;
                    //JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = item.LeafName, SourceURL = discoverFolder.FullUrl, ColumnName = curSetting.Name, Action = string.Empty, Status = JobDetailsStatus.Failed, Comment = e.Message });
                    logger.Error("Set Unique item unique id failed {0}", e.ToString());
                }
            }
        }

        /// <summary>
        /// 注意：这个方法有时获取出来的是folder的最大ID
        /// </summary>
        /// <returns></returns>
        public string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastItemQueryXml:{result}");
            return result;
        }

        public string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastFileQueryXml:{result}");
            return result;
        }

        public int InnerGetLastItemId(IAveList list, IAveFolder folder, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            //query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ViewXml = queryXml;
            var itemCollection = list.GetItemsForRecords(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }
        public int GetLastItemId(IAveList list, IAveFolder folder)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(list, folder, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(list, folder, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }

        public string GetQueryXml(int startIdx, int endIdx, int rowLimit)
        {
            string queryXml = $@"
                <View Scope='RecursiveAll'>
                    <Query>
                        <Where>
                            <And>
                                <Gt><FieldRef Name='ID'/><Value Type='Integer'>{startIdx}</Value></Gt>
                                <Leq><FieldRef Name='ID'/><Value Type='Integer'>{endIdx}</Value></Leq>
                            </And>
                        </Where>
                    </Query>
                    <RowLimit>{rowLimit}</RowLimit>
                </View>";
            logger.Info($"ApplyExisting query xml: {queryXml.LogBase64()}");
            return queryXml;
        }

        public AveCamlQuery GetQuery(IAveFolder folder, int startIndex, int endIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            //query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ListItemCollectionPosition = new AveItemCollectionPosition();
            query.ViewXml = GetQueryXml(startIndex, endIndex, rowLimit);
            return query;
        }
    }
}
