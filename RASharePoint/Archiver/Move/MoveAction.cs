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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using AvePoint.Wrapper.Restore;
using RAArchiverCommon;
using RAArchiverCommon.DisposalProgress.Impl;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Web;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.Archiver.Move
{
    public class MoveAction : IDisposable
    {
        private ScheduleConfiguration mConfiguration;
        private Dictionary<int, SPObjectBackup> mVaults;
        private Dictionary<int, SPObjectBackup> mBackups;
        private Dictionary<int, SPObjectBackup> mRecordManager;

        AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Queue<string> mSecondFileHeaderCache = new Queue<string>();


        public MoveAction(ScheduleConfiguration configuration)
        {
            mConfiguration = configuration;
            //InitVaulters();
            //InitBackupers();
            InitRecordManager();
            //InitEndUserBackupers();
        }

        public async System.Threading.Tasks.Task MoveActionFunAsync(string jobId, string subJobId, IEnumerable<ArchiveApproveReport> reader)
        {
            int errorType = int.MaxValue;
            string ruleName = string.Empty;
            SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            try
            {
                using SiteStateTransitionScope siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfiguration);
                var isUrlAvailable = await HandleSharePointSite();
                if (!isUrlAvailable)
                {
                    return;
                }
                ruleName = mConfiguration.currentRule.Name;
                //int ruleLevel = (int)mConfiguration.BackupRequest.Rules[ruleId].PolicyLevel;
                using (IBackwardDependencyNodeCache<CacheNode> cacheSPObjs = new BackwardDependenceNodeCache<CacheNode>())
                {
                    InitBackupers(null, cacheSPObjs, mSecondFileHeaderCache);

                    IBackupController backupController = new MultiBackupController(null,
                                                   mConfiguration.BackgroundSettings.TotalMultiBackupThreadNumber,
                                                   mConfiguration.BackgroundSettings.EnableMultiBackup,
                                                   mConfiguration.BackgroundSettings.TotalTransferQueueNumber);
                    JobExecutionProgressStatisticExecutor.Instance.StartProgressForOther();
                    foreach (var entity in reader)
                    {
                        using (new CheckJobStopScope()) { }
                        #region get partSiteCollectionURL
                        if (entity.CacheNodeType == (int)CacheNodeType.SiteCollection)
                        {
                            try
                            {
                                mConfiguration.siteUrlSchemeAndHost = new Uri(entity.FullPath).Scheme + @"://" + new Uri(entity.FullPath).Authority;
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Can not get Site Collection URL while Init Keep Data Arguments." + ex.ToString());
                            }
                        }
                        #endregion
                        try
                        {
                            #region errorType
                            if (entity.CacheNodeType > errorType)
                            {
                                mLog.Info("Current item:{0} CacheNodeType:{1} large than errorType:{2} so UpdateStatus to Failed.NodeId:{3}.", entity.FullPath, entity.CacheNodeType, errorType, entity.NodeId);
                                if (entity.CacheNodeType != (int)CacheNodeType.ItemVersion)
                                {
                                    //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, jobId);
                                }
                                continue;
                            }
                            else
                            {
                                errorType = int.MaxValue;
                            }
                            #endregion
                            SPObjectBackup backup = GetBackupObject(entity);
                            CacheNode cacheNode = new CacheNode()
                            {
                                Sender = null,//backup.AveSender,
                            };
                            cacheNode.DoDelete = entity.DoDelete;

                            var backupNodeParameters = new BackupNodeParameters()
                            {
                                CacheSPObjs = cacheSPObjs,
                                Node = entity,
                                BackupObj = backup,
                                CacheNode = cacheNode,
                                RuleName = ruleName,
                                SubJobId = subJobId,
                                RuleLevel = 1,
                                MediaName = string.Empty,
                                Sender = null,
                                Configuration = mConfiguration
                            };
                            await backupController.ProcessAsync(backupNodeParameters);
                            mConfiguration.ProgressDto.HasCompleteNode = true;
                        }
                        #region
                        catch (Exception e)
                        {
                            errorType = entity.CacheNodeType;
                            mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, e.ToString());
                            mConfiguration.ProgressDto.HasErrorNode = true;
                            //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, subJobId);
                            //mConfiguration.JobReportDto.summaryRecordManagerComments = "StorageOptimization13_SOARSORecordManagerErrorComment";
                            //this.error = new CompletedWithExceptionException();
                        }
                        finally
                        {
                            try
                            {
                                SOProgressScAndFileStatistic.Instance()?.IncreaseFileCount(1, entity?.NodeType ?? 0);
                            }
                            catch (Exception e)
                            {
                                mLog.Warn($@"A error in finally of move action try catch, ex:{e}");
                            }
                            try
                            {
                                bool isSupportVersion = mConfiguration.currentRule.spMoveOption is not null && mConfiguration.currentRule.spMoveOption.MoveDestination is not null && mConfiguration.currentRule.spMoveOption.MoveDestination.IsMoveVersions;
                                if (entity.CacheNodeType == (int)CacheNodeType.Item || entity.CacheNodeType == (int)CacheNodeType.HSMItem
                                    || ((entity.CacheNodeType == (int)CacheNodeType.ItemVersion || entity.CacheNodeType == (int)CacheNodeType.HSMItemVersion) && isSupportVersion))
                                {
                                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherActions();
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("IncreaseOtherActions error:{0}.", ex.ToString());
                            }
                        }
                        #endregion
                    }
                    backupController.Finish();
                }
            }
            catch (JobStopException jse)
            {
                mLog.Warn(jse.ToString());
                throw;
            }
            catch (AveSkipLockSiteException ex)
            {
                mConfiguration.JobReportDto.AddDetailOnly(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, JobDetailsStatus.Failed, mConfiguration.currentRule.Name, ex.Message, string.Empty);
                mConfiguration.ProgressDto.HasErrorNode = true;
            }
            catch (Exception e)
            {
                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiveBackupError, e.ToString());
                mConfiguration.ProgressDto.HasErrorNode = true;
                if (mConfiguration.JobReportDto.summaryComments == null)
                {
                    mConfiguration.JobReportDto.summaryComments = e.Message;  //SAAS-13233 更改failed的comment,增加error message
                }
                ////会有问题，因为这个地方需要更新的是子子Job 的信息，所以应该是RuleID + subjobid
                //mConfiguration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Failed, jobId);
            }
            finally
            {
                //reader.DisposeApprovalReportProxy();
            }
            //if (mConfiguration.ProgressDto.HasErrorNode && !hasErrorNode) hasErrorNode = mConfiguration.ProgressDto.HasErrorNode; //SAAS-12584 记录下执行完一个rule有没有Error Node。
        }

        private async System.Threading.Tasks.Task<bool> HandleSharePointSite()
        {
            string listUrl = mConfiguration.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
            listUrl = HttpUtility.UrlDecode(listUrl);
            GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByListUrl(listUrl);
            if (remoteSiteCollection == null)
            {
                throw new Exception("RM_SO_MoveAction_DestinationSiteNotExist");
            }
            mConfiguration.siteUrl = remoteSiteCollection.url;
            mConfiguration.user = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
            mConfiguration.recordManagerRestoreOMFactory = MultiAppUtil.CreateAveObjectModelFactory(mConfiguration.siteUrl, mConfiguration.user, AveContextKind.ClientObjectModel);

            var recordUrlAvailable = GetcorrectRecordDesUrl(listUrl);
            if (string.IsNullOrEmpty(recordUrlAvailable))
            {
                mLog.Info("CheckRecordDesUrl is illegal ,this error form MoveAction calss MoveActionFun function");
                mConfiguration.JobReportDto.HasCompleteNode = false;
                mConfiguration.JobReportDto.HasErrorNode = true;
                return false;
            }

            mConfiguration.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url = recordUrlAvailable;
            mLog.Info($"GetcorrectRecordDesUrl: {recordUrlAvailable}");

            return true;
        }

        private void InitBackupers(BackupInfoSender sender, IBackwardDependencyNodeCache<CacheNode> cacheSPObjs, Queue<string> secondHeaderQueue = null)
        {
            //foreach (SPObjectBackup backupObj in mBackups.Values)
            //{
            //    backupObj.CacheSPObjs = cacheSPObjs;
            //}
            //foreach (SPObjectBackup backupObj in mVaults.Values)
            //{
            //    //backupObj.AveSender = sender;
            //    backupObj.CacheSPObjs = cacheSPObjs;
            //}
            foreach (SPObjectBackup backupObj in mRecordManager.Values)
            {
                //backupObj.AveSender = sender;
                backupObj.CacheSPObjs = cacheSPObjs;
            }

        }
        
        public async System.Threading.Tasks.Task<(SharePointLocationDto, AveBPOSAccountInfo)> GetSharePointLibraryAndAccount()
        {
            try
            {
                if (mConfiguration.currentRule.MoveToRecordCenterAndDelareSetting is null)
                {
                    return (null,null);
                }
                string listUrl = mConfiguration.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                listUrl = HttpUtility.UrlDecode(listUrl);
                GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByListUrl(listUrl);
                if (remoteSiteCollection == null)
                {
                    throw new Exception("RM_SO_MoveAction_DestinationSiteNotExist");
                }
                var siteUrl = remoteSiteCollection.url;
                var user = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
                var recordManagerRestoreOMFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);


                var recordUrlAvailable = GetCorrectRecordDesUrl(listUrl, recordManagerRestoreOMFactory, siteUrl);
                if (string.IsNullOrEmpty(recordUrlAvailable))
                {
                    mLog.Info("CheckRecordDesUrl is illegal ,this error form MoveAction calss MoveActionFun function");
                    mConfiguration.JobReportDto.HasCompleteNode = false;
                    mConfiguration.JobReportDto.HasErrorNode = true;
                    return (null,null);
                }

                listUrl = recordUrlAvailable;
                SharePointLocationDto result = new();
                var site = recordManagerRestoreOMFactory.CreateSite(siteUrl);
                var web = site.OpenWeb(site.GetWebServerRelativeUrl(listUrl));
                var list = web.GetList(listUrl);
                var currentIAveFolder = web.GetFolder(list.RootFolder.ServerRelativeUrl);
                if (!currentIAveFolder.Exists)
                {
                    throw new Exception(string.Format("Folder Not Exists :{0}", currentIAveFolder.Name));
                }

                var jobFolder = currentIAveFolder.ServerRelativeUrl + "/" + mConfiguration.JobId;
                var newJobFolder = currentIAveFolder.Folders.Add(jobFolder);
                result.ParentFolderId = newJobFolder.UniqueId;
                result.JobFolder = newJobFolder;
                result.ParentWebUrl = web.ServerRelativeUrl;
                result.SiteUrl = siteUrl;
                return (result, user);
            }
            catch (Exception ex)
            {
                mLog.Error($"{ex.Message}");
            }
            return (null,null);
        }

        private SPObjectBackup GetBackupObject(ArchiveApproveReport entity)
        {
            //检查JOB是否被停止。
            bool isWeb = entity.CacheNodeType >= (int)CacheNodeType.Web && entity.CacheNodeType < (int)CacheNodeType.APP;
            bool isFolder = false;
            bool isFolderChild = false;
            bool isApp = entity.CacheNodeType == (int)CacheNodeType.APP;
            int x = entity.CacheNodeType / 1000;
            int y = entity.CacheNodeType % 1000;
            isFolder = (x > 1 && x < 10) || (x == 1 && y > 0);
            isFolderChild = x >= 10;

            bool isVersion = entity.CacheNodeType == (int)CacheNodeType.ItemVersion;
            int nodeType = entity.CacheNodeType;
            if (isWeb)
            {
                nodeType = (int)CacheNodeType.Web;
            }
            else if (isFolder)
            {
                nodeType = (int)CacheNodeType.Folder;
            }
            else if (isVersion)
            {
                nodeType = (int)CacheNodeType.Item;
            }
            else if (isApp)
            {
                nodeType = (int)CacheNodeType.APP;
            }
            if (mConfiguration.currentRule.MoveToRecordCenterAndDelareSetting != null)
            {
                return mRecordManager[nodeType];
            }
            else if (mConfiguration.VaultRulesCollection.ContainsKey(mConfiguration.currentRule.Id))
            {
                return mVaults[nodeType];
            }
            else
            {
                return mBackups[nodeType];
            }
        }

        private string GetcorrectRecordDesUrl(string listUrl)
        {
            string returnValue = string.Empty;
            listUrl = HttpUtility.UrlDecode(listUrl);
            try
            {
                using (IAveSite restoreSite = mConfiguration.recordManagerRestoreOMFactory.CreateSite(mConfiguration.siteUrl))
                {
                    try
                    {
                        Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
                        if (restoreSite.Features[mRecordFeatureId] == null)
                        {
                            restoreSite.Features.Add(mRecordFeatureId, true);
                            using (IAveSite checkSite = mConfiguration.recordManagerRestoreOMFactory.CreateSite(mConfiguration.siteUrl))
                            {
                                ArchiverCommonStaticMethod.UpdateSiteRecordDeclarationSettings(checkSite, ScheduleConfiguration.BlockDeleteEdit);
                            }
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                        mConfiguration.JobReportDto.summaryComments = "StorageOptimization_SOARSORecordManagerNoInPlaceRecrdFeature";
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
                        throw;
                    }
                    try
                    {
                        var webUrl = restoreSite.GetWebServerRelativeUrl(listUrl);
                        using (IAveWeb restoreWeb = restoreSite.OpenWeb(webUrl))
                        {
                            IAveList restoreList;
                            if (listUrl.Contains("#/"))
                            {
                                restoreList = restoreWeb.GetListFromUrl(listUrl.Substring(listUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                            }
                            else
                            {
                                restoreList = restoreWeb.GetList(listUrl);
                            }
                            //int listTemplate = (int)restoreList.BaseTemplate;
                            if (!(restoreList.BaseTemplate == AveListTemplateType.DocumentLibrary
                                || restoreList.BaseTemplate == AveListTemplateType.RecordLib
                                || restoreList.BaseTemplate == AveListTemplateType.OneDriveDocumentLibrary))
                            {
                                mLog.Error("List Template Error :{0}", restoreList.BaseTemplate.ToString());
                                throw new Exception("List Template Error");
                            }
                            returnValue = restoreList.FullUrl();
                            mLog.Info("List Auto Check Out Property is:{0}", restoreList.ForceCheckout.ToString());
                        }
                    }
                    catch (Exception listException)
                    {
                        mLog.Error("Check List Url error,Message:{0}", listException.ToString());
                        mConfiguration.JobReportDto.summaryComments = "StorageOptimization13_SOARSORecordManagerLibraryNotExist";
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Can not get destination Site, Des url : {0}, Reason: {1}", listUrl, ex.ToString());
            }

            return returnValue;
        }
        
        private string GetCorrectRecordDesUrl(string listUrl, AveObjectModelFactory recordManagerRestoreOMFactory, string siteUrl)
        {
            string returnValue = string.Empty;
            listUrl = HttpUtility.UrlDecode(listUrl);
            try
            {
                using (IAveSite restoreSite = recordManagerRestoreOMFactory.CreateSite(siteUrl))
                {
                    try
                    {
                        Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
                        if (restoreSite.Features[mRecordFeatureId] == null)
                        {
                            restoreSite.Features.Add(mRecordFeatureId, true);
                            using (IAveSite checkSite = recordManagerRestoreOMFactory.CreateSite(siteUrl))
                            {
                                ArchiverCommonStaticMethod.UpdateSiteRecordDeclarationSettings(checkSite, ScheduleConfiguration.BlockDeleteEdit);
                            }
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                        mConfiguration.JobReportDto.summaryComments = "StorageOptimization_SOARSORecordManagerNoInPlaceRecrdFeature";
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
                        throw;
                    }
                    try
                    {
                        var webUrl = restoreSite.GetWebServerRelativeUrl(listUrl);
                        using (IAveWeb restoreWeb = restoreSite.OpenWeb(webUrl))
                        {
                            IAveList restoreList;
                            if (listUrl.Contains("#/"))
                            {
                                restoreList = restoreWeb.GetListFromUrl(listUrl.Substring(listUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                            }
                            else
                            {
                                restoreList = restoreWeb.GetList(listUrl);
                            }
                            //int listTemplate = (int)restoreList.BaseTemplate;
                            if (!(restoreList.BaseTemplate == AveListTemplateType.DocumentLibrary
                                || restoreList.BaseTemplate == AveListTemplateType.RecordLib
                                || restoreList.BaseTemplate == AveListTemplateType.OneDriveDocumentLibrary))
                            {
                                mLog.Error("List Template Error :{0}", restoreList.BaseTemplate.ToString());
                                throw new Exception("List Template Error");
                            }
                            returnValue = restoreList.FullUrl();
                            mLog.Info("List Auto Check Out Property is:{0}", restoreList.ForceCheckout.ToString());
                        }
                    }
                    catch (Exception listException)
                    {
                        mLog.Error("Check List Url error,Message:{0}", listException.ToString());
                        mConfiguration.JobReportDto.summaryComments = "StorageOptimization13_SOARSORecordManagerLibraryNotExist";
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Can not get destination Site, Des url : {0}, Reason: {1}", listUrl, ex.ToString());
            }

            return returnValue;
        }

        //private void InitVaulters()
        //{
        //    mVaults = new Dictionary<int, SPObjectBackup>();
        //    mVaults.Add((int)CacheNodeType.SiteCollection, new SiteCollectionVault(mLog) { Configuration = mConfiguration });
        //    mVaults.Add((int)CacheNodeType.Web, new WebVault(mLog) { Configuration = mConfiguration });
        //    mVaults.Add((int)CacheNodeType.List, new ListVault(mLog) { Configuration = mConfiguration });
        //    mVaults.Add((int)CacheNodeType.Folder, new FolderVault(mLog) { Configuration = mConfiguration });
        //    mVaults.Add((int)CacheNodeType.Item, new ItemVault(mLog) { Configuration = mConfiguration });
        //    mVaults.Add((int)CacheNodeType.Attachment, new AttachmentVault(mLog) { Configuration = mConfiguration });
        //}


        //private void InitBackupers()
        //{
        //    mBackups = new Dictionary<int, SPObjectBackup>();
        //    mBackups.Add((int)CacheNodeType.SiteCollection, new SiteCollectionBackup() { Configuration = mConfiguration, VaultExport = new SiteCollectionVault() { Configuration = mConfiguration } });
        //    mBackups.Add((int)CacheNodeType.Web, new WebBackup() { Configuration = mConfiguration, VaultExport = new WebVault() { Configuration = mConfiguration } });
        //    mBackups.Add((int)CacheNodeType.List, new ListBackup() { Configuration = mConfiguration, VaultExport = new ListVault() { Configuration = mConfiguration } });
        //    mBackups.Add((int)CacheNodeType.Folder, new FolderBackup() { Configuration = mConfiguration, VaultExport = new FolderVault() { Configuration = mConfiguration } });
        //    mBackups.Add((int)CacheNodeType.Item, new ItemBackup() { Configuration = mConfiguration, VaultExport = new ItemVault() { Configuration = mConfiguration } });
        //    mBackups.Add((int)CacheNodeType.Attachment, new AttachmentBackup() { Configuration = mConfiguration, VaultExport = new AttachmentVault() { Configuration = mConfiguration } });
        //    mBackups.Add((int)CacheNodeType.APP, new AppDefinitionBackup() { Configuration = mConfiguration });
        //}

        //private void InitEndUserBackupers()
        //{
        //    mEndUserSPObject = new Dictionary<int, SPObjectBackup>();
        //    mEndUserSPObject.Add((int)CacheNodeType.SiteCollection, new EndUserSiteCollectionBackup(mLog) { Configuration = mConfiguration, VaultExport = new SiteCollectionVault(mLog) { Configuration = mConfiguration } });
        //    mEndUserSPObject.Add((int)CacheNodeType.Web, new EndUserWebBackup(mLog) { Configuration = mConfiguration, VaultExport = new WebVault(mLog) { Configuration = mConfiguration } });
        //    mEndUserSPObject.Add((int)CacheNodeType.List, new EndUserListBackup(mLog) { Configuration = mConfiguration, VaultExport = new ListVault(mLog) { Configuration = mConfiguration } });
        //    mEndUserSPObject.Add((int)CacheNodeType.Folder, new EndUserFolderBackup(mLog) { Configuration = mConfiguration, VaultExport = new FolderVault(mLog) { Configuration = mConfiguration } });
        //    mEndUserSPObject.Add((int)CacheNodeType.Item, new EndUserItemBackup(mLog) { Configuration = mConfiguration, VaultExport = new ItemVault(mLog) { Configuration = mConfiguration } });
        //    mEndUserSPObject.Add((int)CacheNodeType.Attachment, new EndUserAttachmentBackup(mLog) { Configuration = mConfiguration, VaultExport = new AttachmentVault(mLog) { Configuration = mConfiguration } });
        //}

        private void InitRecordManager()
        {
            mRecordManager = new Dictionary<int, SPObjectBackup>();
            mRecordManager.Add((int)CacheNodeType.SiteCollection, new SiteCollectionRecordManager() { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Web, new WebRecordManager() { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.List, new ListRecordManager() { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Folder, new FolderRecordManager() { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Item, new ItemRecordManager() { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Attachment, new AttachmentRecordManager() { Configuration = mConfiguration });
        }

        public void Dispose()
        {
            if (mRecordManager != null && mRecordManager.Values != null)
            {
                foreach (SPObjectBackup backupObj in mRecordManager.Values)
                {
                    if (backupObj is AvePoint.RA.SharePoint.Archiver.Move.ListRecordManager)
                    {
                        (backupObj as ListRecordManager).DisposeObj();
                    }
                    else if (backupObj is AvePoint.RA.SharePoint.Archiver.Move.ItemRecordManager)
                    {
                        //(backupObj as ItemRecordManager).DisposeObj();
                    }
                    else if (backupObj is IDisposable)
                        using (backupObj) { }
                }
            }
        }
    }
}
