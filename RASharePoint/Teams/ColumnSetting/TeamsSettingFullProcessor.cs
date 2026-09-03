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
using System.Threading.Tasks;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Discovery;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.Wrapper.Common;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Contract.Object.JobMessage;

namespace AvePoint.RA.SharePoint.Teams.ColumnSetting
{
    public class TeamsSettingFullProcessor : SPSettingFullProcessor
    {
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private static readonly RALogger logger = RALogger.GetInstance(typeof(TeamsSettingFullProcessor));
        private ITeamsSettingDao mTeamsSettingDao;
        private Guid TeamsId = Guid.Empty;
        private Guid CurrentTeamsSettingId = Guid.Empty;
        protected ITeamsSettingDao TeamsSettingDao
        {
            get
            {
                if (mTeamsSettingDao == null)
                {
                    mTeamsSettingDao = new TeamsSettingDao();
                }
                return mTeamsSettingDao;
            }
        }
        public TeamsSettingFullProcessor(RMTeamsSetting setting, SPTreeNodeDto nodeInfo, long settingTime, BaseJobDto jobDto, SPOLabelUtility labelUtility, Guid teamsId, bool supportLockedSite, bool enableLifecycleManagementForSharePointLists) : base(SPSettingsUtility.ConvertTeamSettingToSharePointSetting(setting), nodeInfo, settingTime, jobDto, labelUtility, supportLockedSite, enableLifecycleManagementForSharePointLists)
        {

            TeamsId = teamsId;
            CurrentTeamsSettingId = setting.TeamsId;
            curNodeInfo = nodeInfo;
            SPSettingsUtility.sourceType = RMBrowseTreeNodeSourceType.Teams;
            SPSettingsUtility.teamsId = teamsId;
            SPSettingsUtility.currentSiteCollectionLevel = GetNodeLevel(nodeInfo);
        }

        public override async Task RunAsync()
        {
            switch (curNodeInfo.Level)
            {
                case NodeLevel.SiteCollection:
                    await ProcessSiteCollectionAsync();
                    break;
                case NodeLevel.Site:
                    await ProcessWebAsync();
                    break;
                case NodeLevel.List:
                    await ProcessListAsync();
                    break;
                case NodeLevel.Folder:
                    await ProcessFolderAsync();
                    break;
            }
            AddSiteScope();
            StringBuilder errorMessage = new StringBuilder();
            JobState status = JobState.Finished;
            if (isFailedAddBCS || isFailedAddContainer || isFailedEnablePhysical || isFailedEnableApp)
            {
                status = JobState.FinishedException;
                errorMessage.Append("RM_TS_SS_Summary");
            }
            try
            {
                await TeamsSettingDao.SetSettingJobTimeAsync(curSetting.ScopeId, CurrentTeamsSettingId, curSetting.SiteId, isFailedAddBCS, isFailedAddContainer);
            }
            catch (Exception e)
            {
                logger.Warn("Update status error {0}", e.ToString());
            }
            try
            {
                if (mLabelUtility != null && mLabelUtility.LabelApplied)
                {
                    await mLabelUtility.AddLabelHistoryAsync();
                }
            }
            catch (Exception e)
            {
                logger.Warn("Error occurred while updating label history {0}", e.ToString());
            }

            try
            {
                RMMachineLearningDataSyncManager.Commit();
            }
            catch (Exception e)
            {
                logger.Warn($"An error while commit ai sync data, message: {e}");
            }


            //JobContext.Current.Cleanup();
            //JobContext.Current.JobSummaryService.NotifyManager(status, errorMessage.ToString());
            logger.Info($"TeamsSettingFullProcessor finish processing [{curNodeInfo.FullPath}]");
        }

        public override async Task<bool> ProcessWebAsync(AveDiscoverWeb discoverWeb, bool browserSub = true)
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessWeb2", $"RMSPSettingFullProcessor.ProcessWeb2.{discoverWeb.FullUrl}", true))
            {
                //using (discoverWeb)
                //{
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var webSetting = TeamsSettingDao.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), TeamsId, new Guid(siteNode.ID), discoverWeb.WebID);
                        if (webSetting != null)
                        {
                            logger.Info("Web {0} is a break node which has custom setting", discoverWeb.FullUrl);
                            return true;
                        }
                        base.DoSettingAction(discoverWeb.AveWeb, curSiteProperties);
                        var allDiscoverLists = discoverWeb.GetLists().Values;
                        if (allDiscoverLists != null && allDiscoverLists.Count > 0)
                        {
                            ReportManager.IncreaseBase(allDiscoverLists.Count);
                        }
                        //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                        ArgumentNullException.ThrowIfNull(allDiscoverLists);
                        foreach (var list in allDiscoverLists)
                        {
                            ReportManager.Increase();
                            await ProcessListAsync(list);
                        }
                        if (browserSub)
                        {
                            var allSubWebs = discoverWeb.GetSubWebs(true).Values;
                            //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                            foreach (var subWeb in allSubWebs)
                            {
                                await ProcessWebAsync(subWeb);
                            }
                            try
                            {
                                allSubWebs.FirstOrDefault(w => w.AveWeb.IsRootWeb)?.Dispose();
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Dispose root web error:{e}");
                            }
                        }
                        //await base.ProcessWebAsync();
                        return false;
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error("Process web Error {0}:{1}", discoverWeb.FullUrl, e.ToString());
                    return false;
                }
                finally
                {
                    if (!discoverWeb.AveWeb.IsRootWeb)
                    {
                        discoverWeb.Dispose();
                        logger.Info($"Dispose web {discoverWeb.FullUrl}");
                    }
                    else
                    {
                        logger.Info($"This web is root web, we dispose the web when process all webs.");
                    }
                }
                //}

            }
        }

        protected override List<string> GetNeedRemoveValueFolders(IAveList list, string parentFolderPath = "")
        {
            logger.Info($"Get need remove default value folders for list:{list.RootFolder.ServerRelativeUrl}");
            var needRemoveDefaultValueFolders = new List<string>();
            var folderSettingsUnderList = TeamsSettingDao.GetFolderSettingUnderList(list.ID, new Guid(siteNode.ID), TeamsId).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
            var foldersWithDefault = SPSettingsUtility.GetFoldersWithDefaultValue(list, GetColumnInternalName(list), parentFolderPath);
            foreach (var fWithDefault in foldersWithDefault)
            {
                if (!folderSettingsUnderList.Contains(fWithDefault))
                {
                    logger.Info($"Need remove default value folder:{fWithDefault}");
                    needRemoveDefaultValueFolders.Add(fWithDefault);
                }
            }
            return needRemoveDefaultValueFolders;
        }

        public override async Task ProcessListAsync(AveDiscoverList list)
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessList2", $"RMSPSettingFullProcessor.ProcessList2.{list.Title}", true))
            {
                try
                {
                    #region debug code 
                    //DateTime startTime = DateTime.UtcNow.AddHours(-1);
                    //DateTime endTime = DateTime.UtcNow;
                    //var debugSite = discoverFactory.CreateDiscoverSite(curSite, DiscoverModule.Archive, AveDiscoveryKind.API, mfactory, startTime, endTime);
                    //var debugList = discoverFactory.CreateDiscoverList(curSite, curSite.RootWeb.ID, list.RootFolderUrl, startTime, endTime, DiscoverModule.Archive, AveDiscoveryKind.API, mfactory);
                    //var debugFolder = debugList.GetChangeRootFolder();
                    //foreach (var discoverItem in debugFolder.GetChangeItems())
                    //{
                    //    logger.Info("Item name {0}", discoverItem.FullUrl);
                    //}
                    #endregion
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (list.Title == "{System Folder}")
                        {
                            return;
                        }
                        IAveList aveList = list.GetListObject();
                        if (CheckIsDesignList(aveList))
                        {
                            logger.Info("Skip the system list {0}", list.RootFolderUrl);
                            //****ProgressService.Increase();
                            return;
                        }
                        else if (CheckIsDesignListAgain(list))
                        {
                            logger.Info("Skip the system list 2 {0}", list.RootFolderUrl);
                        }
                        if (aveList.Hidden)
                        {
                            logger.Info("Skip the hidden list {0}", list.RootFolderUrl);
                            return;
                        }
                        if (ShouldSkipSharePointList(aveList))
                        {
                            logger.Info("Skip list {0} in scope {1} because lifecycle management for SharePoint Lists is disabled", list.RootFolderUrl, curNodeInfo.FullPath);
                            return;
                        }
                        logger.Info($"Process list {list.Title}, list root folder name:{aveList.RootFolder.Name}, list base template:{(int)aveList.BaseTemplate}, " +
                            $"discover list root folder url:{list.RootFolderUrl}, server template:{list.ServerTemplate}");
                        var listSetting = TeamsSettingDao.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), TeamsId, new Guid(siteNode.ID), list.ListId);
                        if (listSetting != null)
                        {
                            logger.Info("List {0} is a break node which has custom setting", list.RootFolderUrl);
                            //*****ProgressService.Increase();
                            //TO DO Detail
                            return;
                        }
                        await base.DoSettingActionAsync(aveList);
                        if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                        {
                            if (!IsKeepSPDefaultValue(curSetting))
                            {
                                List<string> folders = GetNeedRemoveValueFolders(aveList);
                                folders.ForEach(f => SPSettingsUtility.RemoveFolderDefaultValue(aveList, f, GetColumnInternalName(aveList)));
                                logger.Info($"Remove the folders default value.");
                            }
                        }
                        #region 不再按照Folder结构处理Apply Existing和Auto Job
                        //if (aveList.BaseType == AveBaseType.DocumentLibrary)
                        //{
                        //    bool isEnableDocumentLevelSetting = curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                        //                                    (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn));
                        //    bool isApplyExistingJob = curSetting.NeedCheckDefaultValue && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm;
                        //    bool isAutoJob = (DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification && aveList.BaseType == AveBaseType.DocumentLibrary;
                        //    if (isEnableDocumentLevelSetting)
                        //    {
                        //        if (isApplyExistingJob || isAutoJob)
                        //        {
                        //            var rootFolder = list.GetRootFolder();
                        //            var subFolders = rootFolder.GetSubFolders();

                        //            if (subFolders != null && subFolders.Count > 0)
                        //            {
                        //                ReportManager.IncreaseBase(subFolders.Count);
                        //            }

                        //            foreach (var discoverFolder in subFolders)
                        //            {
                        //                ReportManager.Increase();
                        //                if (!discoverFolder.IsSystemObject)
                        //                {
                        //                    ProcessFolder(discoverFolder);
                        //                }
                        //            }
                        //        }
                        //        else
                        //        {
                        //            logger.Info($"Current setting does not contain ApplyExisting or AutoJob, so skip folders under list.");
                        //        }
                        //    }
                        //}
                        #endregion
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error("Process List Error {0}:{1}", list.Name, e.ToString());
                }
                //finally
                //{
                //    await base.ProcessListAsync();
                //}
            }
        }

        private NodeLevel GetNodeLevel(SPTreeNodeDto nodeInfo)
        {
            try
            {
                var node = RMRemoteNodeDao.GetRemoteNodeById(new Guid(SPTreeNodeManagement.GetSiteCollectionNode(nodeInfo).SPObjectId));
                return (NodeLevel)node.NodeLevel;
            }
            catch (Exception e)
            {
                logger.Error($"Get node level has errors: {e}");
                return NodeLevel.O365GroupSites;
            }
        }
    }
}
