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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.UniqueIdSetting.Base;
using AvePoint.RA.Common.Global;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.Common.Global.Util;
using System.Net;
using AgentUtil = AvePoint.RA.SharePoint.Common.Util;

namespace AvePoint.RA.SharePoint.UniqueIdSetting
{
    public class UniqueIdSettingInrementalProcessor : BaseUniqueIdSettingProcessor
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public UniqueIdSettingInrementalProcessor(SPTreeNodeDto siteNode, UniqueIdSettingJobMessage jobMessage) : base(siteNode, jobMessage)
        {
            
        }
        public override void Run()
        {
            ProcessSiteCollection(GetDiscoverSite());
        }
        public override void ProcessSiteCollection(AveDiscoverSite discoverSite)
        {
            base.ProcessSiteCollection(discoverSite);
            var discoverWebs = discoverSite.GetChangeWebs();
            ProgressService.IncreaseBase(discoverWebs.Count);
            foreach (var discoverWeb in discoverWebs.Values)
            {
                if (discoverWeb.ChangeType != ChangeType.Delete)
                {
                    logger.Info($"Process Web UniqueId setting {discoverWeb.FullUrl.LogBase64()}");
                    if (!CheckWebNeedSkip(discoverWeb))
                    {
                        ProcessWeb(discoverWeb);
                    }
                }
            }
        }
        public override void ProcessWeb(AveDiscoverWeb discoverWeb)
        {
            base.ProcessWeb(discoverWeb);
            var discoverLists = discoverWeb.GetChangeLists();
            ProgressService.IncreaseBase(discoverLists.Count);
            foreach (var discoverList in discoverLists.Values)
            {
                if (discoverList.ChangeType != ChangeType.Delete)
                {
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
                            var allField = list.Fields;
                            IAveField field = list.Fields.GetFieldById(DocumentIDColumnID, false);
                            if (field != null)
                            {
                                IAveView defaultView = list.DefaultView;
                                IAveViewFieldCollection viewFields = defaultView.ViewFields;
                                if (!viewFields.Exists(SPColumnConstants.DocumentIdUrl))
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
                        //暂时不支持item
                        ProcessList(discoverList, discoverWeb.WebID);
                    }
                }
            }
        }
        
        public void ProcessList(AveDiscoverList discoverList, Guid webId)
        {
            //var list = discoverList.GetListObject();
            //if (!SharePointSettingDao.GetSettingEnableInfoByScope(new Guid(groupNode.SPObjectId), new Guid(curNode.ID), discoverList.ListId))
            //{
            //    logger.Info("Process list SharePoint setting is disable {0}", discoverList.RootFolderId);
            //    JobDetailService.Commit(new JMUniqueIDSettingJobDetails()
            //    {
            //        ObjectName = discoverList.Name,
            //        SourceURL = RA.Common.Util.WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url),
            //        ColumnName = curSetting.Name,
            //        Action = I18NEntity.GetString("RM_UI_Detail_Add"),
            //        Status = JobDetailsStatus.Skipped,
            //        Comment = I18NEntity.GetString("RM_JS_JMD_DisableRecordManagement")
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
            //    return;
            //}
            //var changedItems = discoverList.GetListChangedItems(webId);
            //logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl}] for incremental UniqueId job.ChangedItems Count:[{changedItems.Count}].");
            //foreach (var changeItem in changedItems)
            //{
            //    reportManager.Increase();
            //    IAveListItem aveItem = null;
            //    Dictionary<string, object> itemChangeProperties = changeItem.Value as Dictionary<string, object>;
            //    int itemId = (int)itemChangeProperties["ItemId"];
            //    int itemChangeType = (int)itemChangeProperties["ChangeType"];
            //    Guid itemUniqueId = (Guid)itemChangeProperties["UniqueId"];
            //    logger.Info($"Process changed item:Id:{itemId}.UniqueId:{itemUniqueId}.ChangeType:{itemChangeType}.");
            //    if (itemChangeProperties.ContainsKey("Hidden") && (bool)itemChangeProperties["Hidden"])
            //    {
            //        logger.Info($"skip hidden item:{itemId}");
            //        continue;
            //    }
            //    if (itemChangeType != (int)ChangeType.Delete)
            //    {
            //        try
            //        {
            //            aveItem = list.GetItemById(itemId);
            //            if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
            //            {
            //                logger.Info($"Current list item is folder so skip it.Url:{aveItem.Url}.Id:{itemId}.");
            //                continue;
            //            }
            //            SetUniqueId(aveItem);//TO DO 
            //        }
            //        catch (Exception e)
            //        {
            //            haveErrorNode = true;
            //            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = aveItem.Name, SourceURL = aveItem.Url, ColumnName = curSetting.Name, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Failed, Comment = Util.GetExceptionMessage(e) });
            //            logger.Warn("Set Unique item unique id failed {0}.", e.ToString());
            //        }
            //    }
            //}
        }
        public override void ProcessFolder(AveDiscoverFolder discoverFolder)
        {
            List<AveDiscoverItem> changedItems = null;
            List<AveDiscoverFolder> allSubFolders = null;
            string folderFullPath = null;
            IAveList list = null;
            using (discoverFolder)
            {
                base.ProcessFolder(discoverFolder);
                list = discoverFolder.AveFolder.ParentList;
                changedItems = discoverFolder.GetChangeItems();
                allSubFolders = discoverFolder.GetChangeSubFolders();
                ProgressService.IncreaseBase(allSubFolders.Count);
                folderFullPath = discoverFolder.FullUrl;
            }

            foreach (var item in changedItems)
            {
                ProgressService.Increase();
                if (item.ChangeType != ChangeType.Delete)
                {
                    if (item.ID != null && item.ID != 0)
                    {
                        //SetUniqueId(list.GetItemById((int)item.ID));
                    }
                }
            }
            
            foreach (var subfolder in allSubFolders)
            {
                if (subfolder.ChangeType != ChangeType.Delete)
                {
                    try
                    {
                        ProcessFolder(subfolder);
                    }
                    catch (Exception e)
                    {
                        //JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = subfolder.FullUrl, SourceURL = folderFullPath, ColumnName = curSetting.Name, Action = I18NEntity.GetString("RM_UI_Detail_Add"), Status = JobDetailsStatus.Failed, Comment = Util.GetExceptionMessage(e) });
                        logger.Warn("Process folder failed {0}", e.ToString());
                    }
                }
            }
        }
        //private DateTime ModifyTime(DateTime time)
        //{
        //    if (time == DateTime.MinValue) return time;
        //    //int offsetInMinuete = 120; // default value is 2 hours
        //    //int.TryParse(RMGlobalConfiguration.AppConfig[RMAppSettingKey.UniqueIdJobRunTimeOffsetInMinute], out offsetInMinuete); //TODO
        //    int offsetInMinuete = 1;
        //    var runTime = time.AddMinutes(-offsetInMinuete);
        //    logger.Info($"Modified job run time : {runTime}");
        //    return runTime;
        //}

        //public override bool Run()
        //{
        //    var runTime = ModifyTime(DateTime.UtcNow);
        //    bool isEnableRecordManagement = false;
        //    bool errorNode = false;
        //    try
        //    {
        //        var bposInfo = GetBPOSInfo();
        //        var mfactory = AveObjectModelFactory.CreateObjectModelFactory(curNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
        //        curSite = mfactory.CreateSite(curNode.FullPath);
        //        long startTime = SiteInformationDic[curNode.FullPath].LastScanTime;
        //        AveDiscoverSite tmpDiscoverSite = null;
        //        var siteIsEnableRecordsManagement = SiteEnableSettings != null && SiteEnableSettings.Any(o => o.GroupId == new Guid(curNode.Parent.SPObjectId) && o.SiteId == new Guid(curNode.SPObjectId) && o.EnableRecordsManagement);
        //        if (siteIsEnableRecordsManagement)
        //        {
        //            isEnableRecordManagement = true;
        //            //InitSearchContext(bposInfo, curSite.Url);  // init context for search
        //            EnableFeatureAndUpdateBeginID();
        //            if (startTime == DateTime.MinValue.Ticks)
        //            {
        //                logger.Info("need start full unique id setting job :{0}", curNode.FullPath);
        //                UniqueIdSettingFullProcessor fullProcessor = new UniqueIdSettingFullProcessor(curNode, curSetting, currentClientContext, searchSiteColumnFileName);
        //                tmpDiscoverSite = new AveDiscoverSite(curSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
        //                fullProcessor.ProcessSiteCollection(tmpDiscoverSite);
        //                errorNode = fullProcessor.haveErrorNode;
        //            }
        //            else
        //            {
        //                tmpDiscoverSite = new AveDiscoverSite(curSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, new DateTime(startTime, DateTimeKind.Utc), runTime);
        //                this.ProcessSiteCollection(tmpDiscoverSite);
        //                errorNode = this.haveErrorNode;
        //            }
        //        }
        //        else
        //        {
        //            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = curNode.Name, SourceURL = curNode.FullPath, ColumnName = curSetting.Name, Status = JobDetailsStatus.Skipped, Comment = "RM_JS_JMD_DisableRecordManagement" });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn("Set Unique Id error {0}", e.ToString());
        //        JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = curNode.Name, SourceURL = curNode.FullPath, ColumnName = curSetting.Name, Status = JobDetailsStatus.Failed, Comment = Util.GetExceptionMessage(e) });
        //        errorNode = true;
        //    }
        //    finally
        //    {
        //        //currentClientContext?.Dispose(); //TODO
        //        using (curSite)
        //        { }
        //        if (isEnableRecordManagement)
        //        {
        //            //TODO
        //            //RMNodeFlagDao.AddSiteFlagInfo(new RMNodeFlag()
        //            //{
        //            //    NodeId = new Guid(curNode.SPObjectId),
        //            //    Title = curNode.Name,
        //            //    FullPath = curNode.FullPath,
        //            //    CollectionTime = runTime.Ticks,
        //            //    GroupId = new Guid(curNode.Parent.ID),
        //            //    IsRemoved = false,
        //            //    NodeFlagType = (int)NodeFlagType.UniqueId
        //            //});
        //        }
        //        if (!errorNode)
        //        { 
        //            errorNode = !isEnableRecordManagement;
        //        }
        //    }
        //    return errorNode;
        //}
    }
}
