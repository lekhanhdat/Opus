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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.Archiver.Move;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using RAArchiverCommon;
using RAArchiverCommon.DisposalProgress.Impl;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using ActionType = AvePoint.RA.SharePoint.ArchiverCommon.ActionType;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using REPORTRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;


namespace AvePoint.RA.SharePoint.Archiver
{
    class KeepDataAction
    {
        private ScheduleConfiguration mConfiguration;

        private Dictionary<int, SPObjectBackup> mVaults;

        private Dictionary<int, SPObjectBackup> mBackups;

        private Dictionary<int, SPObjectBackup> mRecordManager;

        private Dictionary<int, SPObjectBackup> mEndUserSPObject;

        private Queue<string> mSecondFileHeaderCache = new Queue<string>();

        private IVaultExport vaultExport = null;

        private Dictionary<string, List<IVaultExport>> NARAMetadatas;

        private Dictionary<string, List<IVaultExport>> NAAMetadatas;

        private Dictionary<int, ArchiveApproveReport> KeepDataOnlyContainer = new Dictionary<int, ArchiveApproveReport>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);



        public KeepDataAction(ScheduleConfiguration configuration, Dictionary<int, SPObjectBackup> vaults, Dictionary<int, SPObjectBackup> backups, Dictionary<int, SPObjectBackup> recordManager, Dictionary<int, SPObjectBackup> endUserSPObject, Dictionary<string, List<IVaultExport>> NARAMetadatas,Dictionary<string, List<IVaultExport>> NAAMetadatas)
        {
            mConfiguration = configuration;
            mVaults = vaults;
            mBackups = backups;
            mRecordManager = recordManager;
            mEndUserSPObject = endUserSPObject;
            this.NARAMetadatas = NARAMetadatas;
            this.NAAMetadatas = NAAMetadatas;
        }

        public async System.Threading.Tasks.Task KeepDataOnlyActionAsync(string ruleId, string jobId, string subJobId, IEnumerable<ArchiveApproveReport> reader)
        {
            SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            bool hasErrorNode= false;
            int errorType = int.MaxValue;
            string ruleName = string.Empty;
            try
            {
                using SiteStateTransitionScope siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfiguration);
                InitExportType(ruleId);
                using (IBackwardDependencyNodeCache<CacheNode> cacheSPObjs = new BackwardDependenceNodeCache<CacheNode>())
                {
                    InitBackupers(null, cacheSPObjs, mSecondFileHeaderCache);
                    using (IMultiDeleteController deleteController = new MultiDeleteController(mConfiguration,
                                             mConfiguration.BackgroundSettings.TotalMultiDeleteThreadNumber,
                                             mConfiguration.BackgroundSettings.EnableMultiBackup
                                             ))
                    {
                        IBackupController backupController = new MultiBackupController(null,
                                                           mConfiguration.BackgroundSettings.TotalMultiBackupThreadNumber,
                                                           mConfiguration.BackgroundSettings.EnableMultiBackup,
                                                           mConfiguration.BackgroundSettings.TotalTransferQueueNumber);
                        ArchiverDeletion mDeletion = new ArchiverDeletion(mConfiguration);
                        JobExecutionProgressStatisticExecutor.Instance.StartProgressForOther();
                        foreach (ArchiveApproveReport entity in reader)
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
                                        //Configuration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, jobId);
                                    }
                                    continue;
                                }
                                else
                                {
                                    errorType = int.MaxValue;
                                }
                                #endregion
                                if (mConfiguration.actionType == ActionType.ExportBeforeKeepDataOnly)
                                {
                                    SPObjectBackup backup = mVaults[GetCacheNodeType(entity.CacheNodeType)];
                                    CacheNode cacheNode = new CacheNode()
                                    {
                                        Sender = null,//backup.AveSender,
                                        Configuration = mConfiguration,
                                        Node = entity
                                    };
                                    cacheNode.DoDelete = entity.DoDelete;
                                    RegisterSecondHeaderEventHandler(cacheNode);
                                    var backupNodeParameters = new BackupNodeParameters()
                                    {
                                        CacheSPObjs = cacheSPObjs,
                                        Node = entity,
                                        BackupObj = backup,
                                        CacheNode = cacheNode,
                                        RuleName = ruleName,
                                        SubJobId = subJobId,
                                        RuleLevel = (int)mConfiguration.currentRule.PolicyLevel,
                                        MediaName = string.Empty,
                                        Sender = null,
                                        Configuration = mConfiguration
                                    };
                                    await backupController.ProcessAsync(backupNodeParameters);
                                }
                                GetKeepDataOnlyContainerObject(entity);
                                bool isVersion = false;
                                string message = GetDeletionNodeMessage(entity, ref isVersion);
                                if (!isVersion)
                                {
                                    DeletionNode deletionNode = new DeletionNode(message);
                                    //mLog.Info("GetDeletionNodeMessage:{0}.", message);  //cloud archiver也注释掉
                                    deleteController.Process(deletionNode, mDeletion);
                                    mConfiguration.ProgressDto.HasCompleteNode = true;
                                }
                            }
                            #region
                            catch (Exception e)
                            {
                                errorType = entity.CacheNodeType;
                                //mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, e.ToString());
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, subJobId);
                                //AddBackupCommons(entity.CacheNodeType);
                                //this.error = new CompletedWithExceptionException();
                            }
                            #endregion
                            mConfiguration.JobReportDto.UpdateProgress();
                            SOProgressScAndFileStatistic.Instance()?.IncreaseFileCount(1, entity.NodeType);
                        }
                        backupController.Finish();
                        deleteController.WaitForFinish();
                        CacheNARANAAExportMetadata(subJobId);
                    }
                }
            }
            catch (JobStopException)
            {
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
                //if (mConfiguration.JobReportDto.summaryComments == null)
                //{
                //    mConfiguration.JobReportDto.summaryCommentsDetails = e.Message;  //SAAS-13233 更改failed的comment,增加error message
                //}
                ////会有问题，因为这个地方需要更新的是子子Job 的信息，所以应该是RuleID + subjobid
                //mConfiguration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Failed, jobId);
            }
            finally
            {
                //reader.DisposeApprovalReportProxy();
            }
            if (mConfiguration.ProgressDto.HasErrorNode && !hasErrorNode) hasErrorNode = mConfiguration.ProgressDto.HasErrorNode; //SAAS-12584 记录下执行完一个rule有没有Error Node。
        }
        public void DisposeVEOExportMetadata()
        {
            if (vaultExport != null)
            {
                //针对EDRM，参数0 string：rule name ，参数1 int：分xml的个个数，
                if (mConfiguration.currentRule.ExportType == ExportTypeValue.VEO)
                {
                    RMRunningJobRuleMappingDao.AddJobMappingsForVEOMerge(TenantLocalValue.LogonGroupId, mConfiguration.MainJobId);
                    mLog.Info("Begin DisposeVEOExportMetadata.");
                    vaultExport.ExtensionMethod(mConfiguration.currentRule.Name, mConfiguration.BackgroundSettings.ManifestXmlSize);
                    vaultExport.Dispose();
                    mLog.Info("End DisposeVEOExportMetadata.");
                }
            }
        }
        private void InitBackupers(BackupInfoSender sender, IBackwardDependencyNodeCache<CacheNode> cacheSPObjs, Queue<string> secondHeaderQueue = null)
        {
            foreach (SPObjectBackup backupObj in mBackups.Values)
            {
                backupObj.CacheSPObjs = cacheSPObjs;
            }
            foreach (SPObjectBackup backupObj in mVaults.Values)
            {
                //backupObj.AveSender = sender;
                backupObj.CacheSPObjs = cacheSPObjs;
            }
            foreach (SPObjectBackup backupObj in mRecordManager.Values)
            {
                //backupObj.AveSender = sender;
                backupObj.CacheSPObjs = cacheSPObjs;
            }

        }


        private void InitExportType(string ruleId)
        {
            vaultExport = null;
            ExportpathGeneratorBase generator = null;
            if (mConfiguration.currentRule.ExportType != ExportTypeValue.None)
            {
                InitVaultState(ref generator, ruleId);
                foreach (SPObjectBackup backup in mBackups.Values)
                {
                    backup.VaultBeforeArcInfo = new VaultBefArcInfo()
                    {
                        VaultExport = vaultExport,
                        VaultExportPathGenerator = generator
                    };
                }
                foreach (SPObjectBackup vault in mVaults.Values)
                {
                    vault.VaultBeforeArcInfo = new VaultBefArcInfo()
                    {
                        VaultExport = vaultExport,
                        VaultExportPathGenerator = generator
                    };
                }
                foreach (SPObjectBackup EndUserExport in mEndUserSPObject.Values)
                {
                    EndUserExport.VaultBeforeArcInfo = new VaultBefArcInfo()
                    {
                        VaultExport = vaultExport,
                        VaultExportPathGenerator = generator
                    };
                }
            }
            else
            {
                foreach (SPObjectBackup backup in mBackups.Values)
                {
                    backup.VaultBeforeArcInfo = null;
                }
                foreach (SPObjectBackup vault in mVaults.Values)
                {
                    vault.VaultBeforeArcInfo = null;
                }
                foreach (SPObjectBackup EndUserExport in mEndUserSPObject.Values)
                {
                    EndUserExport.VaultBeforeArcInfo = null;
                }
            }
        }

        private string GetDeletionNodeMessage(ArchiveApproveReport entity, ref bool isVersion)
        {
            string message = string.Empty;
            XmlDocument doc = new XmlDocument();
            XmlElement fileHeaderXml = doc.CreateElement("FileHeader");
            fileHeaderXml.SetAttribute(KeyWord.PATH, entity.LeafName);
            fileHeaderXml.SetAttribute(KeyWord.TYPE, GetObjectType(entity.CacheNodeType, entity.NodeType));
            fileHeaderXml.SetAttribute(KeyWord.NODEGUID, entity.NodeId);
            fileHeaderXml.SetAttribute(KeyWord.LEVEL, entity.Level.ToString());
            fileHeaderXml.SetAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            fileHeaderXml.SetAttribute(KeyWord.ID, entity.NodeId);
            fileHeaderXml.SetAttribute(KeyWord.RowId, entity.LibRowId.ToString());
            isVersion = IsVersion(entity.NodeType, entity.LeafName);
            fileHeaderXml.SetAttribute(KeyWord.ISVERSION, isVersion.ToString());
            fileHeaderXml.SetAttribute(KeyWord.URL, entity.FullPath);
            fileHeaderXml.SetAttribute(KeyWord.RULENAME, mConfiguration.currentRule.Name);
            fileHeaderXml.SetAttribute(KeyWord.SUBJOBID, mConfiguration.JobId);
            fileHeaderXml.SetAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            fileHeaderXml.SetAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            fileHeaderXml.SetAttribute(KeyWord.FULLPATH, entity.FullPath);
            fileHeaderXml.SetAttribute(KeyWord.SIZE, entity.DocumentSize.ToString());
            fileHeaderXml.SetAttribute(KeyWord.DoDelete, entity.DoDelete.ToString());
            fileHeaderXml.SetAttribute(KeyWord.DeleteRelatedRecords, entity.DeleteRelatedRecords.ToString());
            fileHeaderXml.SetAttribute(KeyWord.SiteUrl, KeepDataOnlyContainer[(int)CacheNodeType.SiteCollection].SiteUrl);
            fileHeaderXml.SetAttribute(KeyWord.WebId, KeepDataOnlyContainer.ContainsKey((int)CacheNodeType.Web) ? KeepDataOnlyContainer[(int)CacheNodeType.Web].WebID.ToString() : string.Empty);
            fileHeaderXml.SetAttribute(KeyWord.ListId, KeepDataOnlyContainer.ContainsKey((int)CacheNodeType.List) ? KeepDataOnlyContainer[(int)CacheNodeType.List].NodeId.ToString() : string.Empty);
            fileHeaderXml.SetAttribute(KeyWord.IsRepeatProcess, entity.IsRepeatProcess.ToString());
            doc.AppendChild(fileHeaderXml);
            message = doc.InnerXml.ToString();
            return message;
        }

        private string GetObjectType(int cacheNodeType, int nodeType)
        {
            string objectType = string.Empty;
            if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                objectType = AveConstants.TYPE_SITE.ToString();
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web && cacheNodeType < (int)CacheNodeType.List)
            {
                objectType = AveConstants.TYPE_WEB.ToString();
            }
            else if (cacheNodeType == (int)CacheNodeType.APP)
            {
                objectType = AveConstants.TYPE_APP.ToString();
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                objectType = AveConstants.TYPE_LIST.ToString();
            }
            else if (cacheNodeType > (int)CacheNodeType.List && cacheNodeType < (int)CacheNodeType.Item)
            {
                objectType = AveConstants.TYPE_FOLDER.ToString();
            }
            else if (cacheNodeType == (int)CacheNodeType.Item)
            {
                if (nodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                {
                    objectType = AveConstants.TYPE_LISTITEM.ToString();
                }
                else if (nodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    objectType = AveConstants.TYPE_DOCUMENT.ToString();
                }
            }
            else if (cacheNodeType == (int)CacheNodeType.ItemVersion)
            {
                if (nodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER)
                {
                    objectType = AveConstants.TYPE_VERSION.ToString();
                }
                else if (nodeType == (int)ArchiverCommon.ItemType.ITEM_VERSION)
                {
                    objectType = AveConstants.TYPE_LISTITEMVERSION.ToString();
                }
            }
            else if (cacheNodeType == (int)CacheNodeType.Attachment)
            {
                objectType = AveConstants.TYPE_ATTACHMENTS.ToString();
            }
            return objectType;
        }

        private bool IsVersion(int nodeType, string leafName)
        {
            bool isVersion = false;
            if (nodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER || nodeType == (int)ArchiverCommon.ItemType.ITEM_VERSION)
            {
                isVersion = true;
            }
            return isVersion;
        }

        private void GetKeepDataOnlyContainerObject(ArchiveApproveReport entity)
        {
            switch (entity.CacheNodeType)
            {
                case (int)CacheNodeType.SiteCollection:
                case (int)CacheNodeType.Web:
                case (int)CacheNodeType.List:
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
                    if (!entity.FullPath.StartsWith(mConfiguration.siteUrlSchemeAndHost))
                    {
                        entity.FullPath = mConfiguration.siteUrlSchemeAndHost + entity.FullPath;
                    }
                    if (KeepDataOnlyContainer.ContainsKey(entity.CacheNodeType))
                    {
                        KeepDataOnlyContainer[entity.CacheNodeType] = entity;
                    }
                    else
                    {
                        KeepDataOnlyContainer.Add(entity.CacheNodeType, entity);
                    }
                    break;
                case (int)CacheNodeType.Item:
                    if (!entity.FullPath.StartsWith(mConfiguration.siteUrlSchemeAndHost))
                    {
                        entity.FullPath = (mConfiguration.siteUrlSchemeAndHost + entity.FullPath).Replace('\\', '/');
                    }
                    break;
                default:
                    //subsite CacheNodeType between 4~999
                    if (entity.CacheNodeType > (int)CacheNodeType.Web && entity.CacheNodeType < (int)CacheNodeType.List)
                    {
                        if (!entity.FullPath.StartsWith(mConfiguration.siteUrlSchemeAndHost))
                        {
                            entity.FullPath = mConfiguration.siteUrlSchemeAndHost + entity.FullPath;
                        }
                        if (KeepDataOnlyContainer.ContainsKey((int)CacheNodeType.Web))
                        {
                            KeepDataOnlyContainer[(int)CacheNodeType.Web] = entity;
                        }
                        else
                        {
                            KeepDataOnlyContainer.Add((int)CacheNodeType.Web, entity);
                        }
                    }
                    break;
            }
        }

        private int GetCacheNodeType(int cacheNodeType)
        {
            int nodeType = 0;
            if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                nodeType = (int)CacheNodeType.SiteCollection;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web && cacheNodeType < (int)CacheNodeType.List)
            {
                nodeType = (int)CacheNodeType.Web;
            }
            else if (cacheNodeType == (int)CacheNodeType.APP)
            {
                nodeType = (int)CacheNodeType.APP;
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                nodeType = (int)CacheNodeType.List;
            }
            else if (cacheNodeType > (int)CacheNodeType.List && cacheNodeType < (int)CacheNodeType.Item)
            {
                nodeType = (int)CacheNodeType.Folder;
            }
            else if (cacheNodeType == (int)CacheNodeType.Item)
            {
                nodeType = (int)CacheNodeType.Item;
            }
            else if (cacheNodeType == (int)CacheNodeType.ItemVersion)
            {
                nodeType = (int)CacheNodeType.Item;
            }
            else if (cacheNodeType == (int)CacheNodeType.Attachment)
            {
                nodeType = (int)CacheNodeType.Attachment;
            }
            return nodeType;
        }

        private void RegisterSecondHeaderEventHandler(CacheNode cacheNode)
        {
            cacheNode.CustomizedDisposeAction = () =>
            {
                try
                {
                    CacheSecondHeader(cacheNode.GenerateSecondFileHeader());
                }
                catch (Exception ex)
                {
                    mLog.Error($"Failed to save header, Message:{ex}");
                    //TODO:Logging
                }
            };
        }

        private void CacheSecondHeader(string tempHeader)
        {
            if (string.IsNullOrEmpty(tempHeader))
            {
                mLog.Info("Current second Header IsNullOrEmpty.");
                return;
            }

            mLog.Info(string.Format("Cache second Header for {0}", tempHeader));
            mSecondFileHeaderCache.Enqueue(tempHeader);
        }

        /// <summary>
        /// 检查是否需要退出
        /// </summary>
        /// <returns></returns>
    /*    private bool NeedStopCurrentJob()
        {


            return false;
        }*/

        private void CacheNARANAAExportMetadata(string subJobId)
        {
            if (mConfiguration.currentRule.ExportType == ExportTypeValue.NAA)
            {
                mLog.Info("Cache NAA Data {0}", subJobId);
                if (!NAAMetadatas.ContainsKey(subJobId))
                {
                    List<IVaultExport> exportObjs = new List<IVaultExport>();
                    exportObjs.Add(vaultExport);
                    NAAMetadatas.Add(subJobId, exportObjs);
                }
                else
                {
                    NAAMetadatas[subJobId].Add(vaultExport);
                }
            }
            if (mConfiguration.currentRule.ExportType == ExportTypeValue.NARA)
            {
                mLog.Info("Cache NARA Data {0}", subJobId);
                if (!NARAMetadatas.ContainsKey(subJobId))
                {
                    List<IVaultExport> exportObjs = new List<IVaultExport>();
                    exportObjs.Add(vaultExport);
                    NARAMetadatas.Add(subJobId, exportObjs);
                }
                else
                {
                    NARAMetadatas[subJobId].Add(vaultExport);
                }
            }
        }

        private void InitVaultState(ref ExportpathGeneratorBase generator, string ruleId)
        {
            VautlExportfactory factory = new VautlExportfactory();
            ExportTypeValue vaultExportType = mConfiguration.currentRule.ExportType;
            PhysicalDeviceDto physicalDto = mConfiguration.currentRule.PhysicalDeviceDto;
            SharePointLocationDto spoDto = null;
            AveBPOSAccountInfo accountInfoOfDestinationSpo = null;
            if (physicalDto == null)
            {
                mLog.Info("Using export to sharepoint library.");
                var (spoLibrary, accountInfo) = new MoveAction(mConfiguration).GetSharePointLibraryAndAccount().GetAwaiter().GetResult();
                spoDto = spoLibrary;
                accountInfoOfDestinationSpo = accountInfo;
            }
            mLog.Info("Vault Export Type is: {0}.", vaultExportType.ToString());
            byte[] exportEncryptionKeyBytes = null;
            byte[] exportEncryptionIVBytes = null;
            if (physicalDto != null  || spoDto != null)
            {
                if (vaultExportType == ExportTypeValue.VEO && mConfiguration.IsUpgradedVEOV3 && !string.IsNullOrEmpty(mConfiguration.BackgroundSettings.VEOV3Type))
                {
                    mLog.Info("Export Type will change to :{0}Export.", mConfiguration.BackgroundSettings.VEOV3Type);
                    byte[] contentVEO = mConfiguration.currentRule.VEOContent;
                    byte[] historyVEO = mConfiguration.currentRule.VEOHistory;
                    vaultExport = physicalDto != null
                        ? factory.Create(physicalDto, mConfiguration.JobId, mConfiguration.BackgroundSettings.VEOV3Type, (int)mConfiguration.currentRule.PolicyLevel, mConfiguration.currentRule.ArchiverSetting, contentVEO, historyVEO, mConfiguration.currentRule.ExportDataEncryptionKey)
                        : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.JobId, mConfiguration.BackgroundSettings.VEOV3Type, (int)mConfiguration.currentRule.PolicyLevel, mConfiguration.currentRule.ArchiverSetting, contentVEO, historyVEO, mConfiguration.currentRule.ExportDataEncryptionKey);
                    generator = new VEOV3ExportPathGenerator(mConfiguration.TeamsAddress);
                    mLog.Info($"created VEO V3 export path generator. TeamsAddress: [{mConfiguration.TeamsAddress}]");
                    return;
                }

                if (vaultExportType == ExportTypeValue.VEO && !string.IsNullOrEmpty(mConfiguration.BackgroundSettings.VEOType))
                {
                    mLog.Info("Export Type will change to :{0}Export.", mConfiguration.BackgroundSettings.VEOType);
                    byte[] fileVEO = mConfiguration.currentRule.FileVEO;
                    byte[] recordVEO = mConfiguration.currentRule.RecordVEO;
                    byte[] manifestVEO = mConfiguration.currentRule.ManifestVEO;
                    var recordsEncryptionKey = mConfiguration.currentRule.ExportDataEncryptionKey;
                    var recordsEncryptionIV = mConfiguration.currentRule.ExportDataEncryptionIV;
                    if (mConfiguration.IsILMode && !string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                    {
                        mLog.Info("Export data encryption is enabled.");
                        exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                        exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                    }
                    vaultExport = physicalDto != null 
                            ? factory.Create(physicalDto, mConfiguration.JobId, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), mConfiguration.BackgroundSettings.VEOType, true), fileVEO, recordVEO, manifestVEO, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                            : factory.Create(spoDto, accountInfoOfDestinationSpo , spoDto.SiteUrl, mConfiguration.JobId, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), mConfiguration.BackgroundSettings.VEOType, true), fileVEO, recordVEO, manifestVEO, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                    generator = new VEOExportPathGenerator();
                }
                else if (vaultExportType == ExportTypeValue.NAA)
                {
                    var recordsEncryptionKey = mConfiguration.currentRule.ExportDataEncryptionKey;
                    var recordsEncryptionIV = mConfiguration.currentRule.ExportDataEncryptionIV;

                    if (mConfiguration.IsILMode && !string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                    {
                        mLog.Info("Export data encryption is enabled.");
                        exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                        exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                    }
                    vaultExport = physicalDto != null 
                            ? factory.Create(physicalDto, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NAAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                            : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NAAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                    generator = new NAAExportPathGenerator(string.Empty, physicalDto?.Location, GetGlobalSettingColumnName(), mConfiguration.TeamsAddress);
                }
                else if (vaultExportType == ExportTypeValue.NARA)
                {
                    var recordsEncryptionKey = mConfiguration.currentRule.ExportDataEncryptionKey;
                    var recordsEncryptionIV = mConfiguration.currentRule.ExportDataEncryptionIV;
                    if (mConfiguration.IsILMode && !string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                    {
                        mLog.Info("Export data encryption is enabled.");
                        exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                        exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                    }
                    vaultExport = physicalDto != null
                            ? factory.Create(physicalDto, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NARAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                            : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NARAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                    generator = new NARAExportPathGenerator(string.Empty, physicalDto?.Location, GetGlobalSettingColumnName(), mConfiguration.TeamsAddress);
                }
            }
            else
            {
                mLog.Info("The Vault Before Archiver is false.");
            }
        }

        #region for RA
        /// <summary>
        /// one job -> one sharepoint group
        /// one sharepoint site group -> one column
        /// </summary>
        /// <returns></returns>
        private string GetGlobalSettingColumnName()
        {
            var rule = mConfiguration.currentRule;
            string columnName = string.Empty;
            if (rule != null && rule.SOFilters != null)
            {
                foreach (var filter in rule.SOFilters)
                {
                    if (filter.Rule is AvePoint.GCommon.Contract.CommonFilter.TermRule)
                    {
                        columnName = filter.Rule.Value1;
                        mLog.Info("get gobal setting columnName:{0}", columnName);
                        break;
                    }
                }
            }
            return columnName;
        }
        #endregion
    }
}
