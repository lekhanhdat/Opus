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
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Backup;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAExportCommon;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Runtime.ConstrainedExecution;
using AvePoint.RA.SharePoint.Archiver.Common.ApprovalService;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2013/10/11",
    "dong.xie@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
            {
                CodeReviewConstants.CHECK_LIST_ID_FA_1,
                CodeReviewConstants.CHECK_LIST_ID_FA_10,
                CodeReviewConstants.CHECK_LIST_ID_LOG_1,
                CodeReviewConstants.CHECK_LIST_ID_LOG_2,
                CodeReviewConstants.CHECK_LIST_ID_LOG_3,
                CodeReviewConstants.CHECK_LIST_ID_LOG_4,
            },
    "ADO-92003",
    true
    )]

    //Media is Useless in this file,but we add it because of the struture

    //sitecollection level do no Vault ,we only Updata VaultReport
    internal class SiteCollectionVault : SPObjectBackup
    {
        public SiteCollectionVault(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info($"not need repeat export processed sc node:{entity.NodeId}");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender fileSender = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.SiteCollectionVault"))
            {
                try
                {
                    AveSPSite aveSite;
                    aveSite = new AveSPSite(entity.LeafName, AveContextKind.ClientObjectModel, Configuration.user, null);

                    current.WrapperObject = aveSite;
                    this.VaultExport(aveSite, entity, subJobId, ruleName, mediaName);

                    #region current.BackupStatus = FileHeaderStatus.Complete; only for Metlife Vault
                    current.BackupStatus = FileHeaderStatus.Complete;
                    current.IsSiteLevel = true;
                    #endregion

                }
                catch (Exception ex)
                {
                    mLog.Error("Error in Vault SiteCollection" + ex.ToString());
                    throw;
                }
                return 0;
            }
        }

        internal void VaultExport(AveSPSite aveSite, ArchiveApproveReport entity, string subJobId, string ruleName, string mediaName)
        {
            ExportStatus vaultState = null;
            if (Configuration.IsUpgradedVEOV3 && Configuration.currentRule != null 
                && Configuration.currentRule.PolicyLevel == PolicyLevel.SiteCollection && Configuration.currentRule.ExportType == ExportTypeValue.VEO)
            {
                try
                {
                    SiteCollectionLevelPathGeneratorInfo siteExportInfo = new SiteCollectionLevelPathGeneratorInfo()
                    {
                        JobId = Configuration.JobId,
                        Site = aveSite
                    };
                    VaultExportInfo exportInfo = VaultBeforeArcInfo.VaultExportPathGenerator.GeneratSiteCollectionExportInfo(siteExportInfo);
                    vaultState = VaultBeforeArcInfo.VaultExport.ExportSite(aveSite, exportInfo);
                    if (vaultState == null)
                    {
                        vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNull };
                        mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, entity.FullPath));
                        throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                    }
                    if (vaultState.State != ExportState.Succeed)
                    {
                        mLog.Info(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, entity.FullPath));
                        throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                    }
                    else
                    {
                        vaultState.ErrorMessage = string.Empty;
                    }
                    return;
                }
                catch (Exception ex)
                {
                    vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                    mLog.Error("Error in ExportSite" + ex.ToString());
                    throw;
                }
                finally
                {
                    if (vaultState != null)
                    {
                        BackupSize = vaultState.ExportSize;
                        Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(entity.FullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                    }
                }
            }

            vaultState = new ExportStatus() { State = ExportState.Succeed, ErrorMessage = string.Empty };
            BackupSize = vaultState.ExportSize;
            Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(entity.FullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
            return;
        }
    }

    internal class WebVault : SPObjectBackup
    {
        public WebVault(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info($"not need repeat export processed web node:{entity.NodeId}");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender fileSender = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.WebVault"))
            {
                try
                {
                    var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                    AveSPWeb aveWeb = new AveSPWeb(aveSite, new Guid(entity.NodeId), entity.LeafName);
                    current.WrapperObject = aveWeb;
                    this.VaultExport(aveWeb, entity, subJobId, ruleName, mediaName);
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in WebVaultBackup" + ex.ToString());
                    throw;
                }
                return 0;
            }
        }

        internal void VaultExport(AveSPWeb aveWeb, ArchiveApproveReport entity, string subJobId, string ruleName, string mediaName)
        {
            ExportStatus vaultState = null;
            try
            {
                if (!Configuration.IsUpgradedVEOV3)
                {
                    vaultState = this.VaultBeforeArcInfo.VaultExport.ExportWeb(aveWeb, new VaultExportInfo());
                }
                else if (Configuration.currentRule != null && Configuration.currentRule.PolicyLevel <= PolicyLevel.Site
                    && Configuration.currentRule.ExportType == ExportTypeValue.VEO && !aveWeb.SPWeb.IsRootWeb)
                {
                    WebLevelPathGeneratorInfo webExportInfo = new WebLevelPathGeneratorInfo()
                    {
                        JobId = this.Configuration.JobId,
                        Web = aveWeb
                    };

                    VaultExportInfo exportInfo = this.VaultBeforeArcInfo.VaultExportPathGenerator.GeneratWebExportInfo(webExportInfo);
                    vaultState = this.VaultBeforeArcInfo.VaultExport.ExportWeb(aveWeb, exportInfo);
                }
                else
                {
                    vaultState = new ExportStatus() { State = ExportState.Succeed, ErrorMessage = string.Empty };
                }

                if (vaultState == null)
                {
                    vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNull };
                    mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, entity.FullPath));
                    throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                }
                if (vaultState.State != ExportState.Succeed)
                {
                    mLog.Info(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, entity.FullPath));
                    throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                }
                else 
                {
                    vaultState.ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                mLog.Error("Error in ExportWeb" + ex.ToString());
                throw;
            }
            finally
            {
                BackupSize = vaultState.ExportSize;
                Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(entity.FullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
            }
        }
    }

    internal class ListVault : SPObjectBackup
    {
        public ListVault(AveLogger log)
        {
            mLog = log;
        }


        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info($"not need repeat export processed list node:{entity.NodeId}");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender fileSender = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ListVault"))
            {
                try
                {
                    var aveWeb = parent.WrapperObject as AveSPWeb;
                    //释放并且重新获取Web对象 ADO-111892
                    try
                    {
                        Guid webId = aveWeb.SPWeb.ID;
                        string webName = aveWeb.Name;
                        aveWeb.Dispose();
                        var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                        aveWeb = new AveSPWeb(aveSite, webId, webName);
                        parent.WrapperObject = aveWeb;
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Error in get web, list is " + entity.LeafName + ex.ToString());
                    }
                    var aveList = new AveSPList(aveWeb, new Guid(entity.NodeId), entity.LeafName, true);
                    current.WrapperObject = aveList;
                    VaultExport(aveList, entity, subJobId, ruleName, mediaName);
                    //current.BackupStatus = FileHeaderStatus.Complete; only for updateProperty for Metlife
                    current.BackupStatus = FileHeaderStatus.Complete;
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in ListVaultBackup" + ex.ToString());
                    throw;
                }
                return 0;
            }
        }

        internal void VaultExport(AveSPList aveList, ArchiveApproveReport entity, string subJobId, string ruleName, string mediaName)
        {
            ExportStatus vaultState = null;
            if (entity.NodeType == (int)NodeType.MyProfileList)
            {
                mLog.Error("MyList Do not Vault Export");
                return;
            }
            if (aveList.SPList != null && aveList.SPList.BaseTemplate.ToString().Equals("ExternalList", StringComparison.OrdinalIgnoreCase))
            {
                vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = "ExternalList do not supported" };
                throw new Exception("Do not supported ExternalList VaultExport");
            }

            try
            {
                if (!aveList.IsSystemList && !aveList.SPList.Hidden)
                {
                    if (Configuration.currentRule != null
                        && Configuration.currentRule.PolicyLevel == PolicyLevel.Document
                        && Configuration.currentRule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO
                        && !ScanDBOperationFactory.GetScanDBOperation(Configuration).CheckListOrFolderHasFitRuleFile(aveList.Id, Guid.Empty.ToString(), Configuration.currentRule.Id))
                    {
                        mLog.Info($"VaultExport ExportTypeValue.VEO skip current list:ID:{aveList.Id},URL:{aveList.ServerRelativeUrl} due to list root folder does not have file fit rule.");
                        //vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                    }
                    else
                    {
                        if (Configuration.IsUpgradedVEOV3 && Configuration.currentRule != null && Configuration.currentRule.PolicyLevel > PolicyLevel.List
                            && Configuration.currentRule.ExportType == ExportTypeValue.VEO)
                        {
                            vaultState = new ExportStatus() { State = ExportState.Succeed, ErrorMessage = string.Empty };
                            return;
                        }

                        ListLevelPathGeneratorInfo listExportInfo = new ListLevelPathGeneratorInfo()
                        {
                            JobId = this.Configuration.JobId,
                            List = aveList
                        };

                        VaultExportInfo exportInfo = this.VaultBeforeArcInfo.VaultExportPathGenerator.GeneratListExportInfo(listExportInfo);
                        vaultState = this.VaultBeforeArcInfo.VaultExport.ExportList(aveList, exportInfo);

                        if (vaultState == null)
                        {
                            vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNull };
                            mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, entity.FullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        if (vaultState.State != ExportState.Succeed)
                        {
                            mLog.Info(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, entity.FullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        else
                        {
                            vaultState.ErrorMessage = string.Empty;
                        }
                    }
                }
                else
                {
                    vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                }
            }
            catch (Exception ex)
            {
                vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                mLog.Error("Error in ExportList" + ex.ToString());
                throw;
            }
            finally
            {
                if (vaultState != null) 
                {
                    BackupSize = vaultState.ExportSize;
                    Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(entity.FullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                }
            }
        }
    }

    internal class FolderVault : SPObjectBackup
    {
        public FolderVault(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info($"not need repeat export processed folder node:{entity.NodeId}");
            return 0;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender fileSender = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.FolderVault"))
            {
                bool isRootFolder = false;
                var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                AveSPWeb aveWeb = null;
                if (CacheSPObjs.ValueInCacheOfLevel(500) != default(object))
                {
                    aveWeb = (CacheSPObjs.ValueInCacheOfLevel(500) as CacheNode).WrapperObject as AveSPWeb;
                }
                else if (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) != default(object))
                {
                    aveWeb = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as CacheNode).WrapperObject as AveSPWeb;
                }
                if (((CacheNode)parent).WrapperObject is AveSPList)
                {
                    isRootFolder = true;
                }
                AveSPFolder aveFolder = null;
                if (isRootFolder)
                {
                    aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPList);
                    current.IsRootFolder = true;
                    current.WrapperObject = aveFolder;
                }
                else
                {
                    aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                    current.WrapperObject = aveFolder;
                }

                try
                {
                    this.VaultExport(aveFolder, (int)entity.CacheNodeType, entity.FullPath, subJobId, isRootFolder, ruleName, mediaName);
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in FolderVaultBackup" + ex.ToString());
                    throw;
                }
                return 0;
            }
        }

        internal void VaultExport(AveSPFolder aveFolder, int cacheNodeType, string fullPath, string subJobId, bool isRootFolder, string ruleName, string mediaName)
        {
            ExportStatus vaultState = null;
            try
            {
                if (isRootFolder)
                {
                    return;
                }
                if (aveFolder.AveItem.SPListItem != null && !aveFolder.AveList.SPList.Hidden)
                {
                    if (Configuration.IsUpgradedVEOV3 && Configuration.currentRule != null && Configuration.currentRule.PolicyLevel > PolicyLevel.Folder
                            && Configuration.currentRule.ExportType == ExportTypeValue.VEO)
                    {
                        vaultState = new ExportStatus() { State = ExportState.Succeed, ErrorMessage = string.Empty };
                    }
                    else if (Configuration.currentRule != null
                        && Configuration.currentRule.PolicyLevel == PolicyLevel.Document
                        && Configuration.currentRule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO
                        && !ScanDBOperationFactory.GetScanDBOperation(Configuration).CheckListOrFolderHasFitRuleFile(aveFolder.AveList.Id, aveFolder.Id.ToString(), Configuration.currentRule.Id))
                    {
                        mLog.Info($"VaultExport ExportTypeValue.VEO skip current folder:ID:{aveFolder.Id},URL:{aveFolder.ServerRelativeUrl} due to folder does not have file fit rule.");
                        //vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                    }
                    else
                    {
                        FolderLevelPathGeneratorInfo folderPathInfo = new FolderLevelPathGeneratorInfo()
                        {
                            Item = aveFolder.AveItem,
                            JobId = this.Configuration.JobId,
                        };
                        VaultExportInfo exportInfo = this.VaultBeforeArcInfo.VaultExportPathGenerator.GenerateFolderExportInfo(folderPathInfo);
                        vaultState = this.VaultBeforeArcInfo.VaultExport.ExportFolder(aveFolder, exportInfo, isRootFolder);
                        if (vaultState == null)
                        {
                            vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNull };
                            mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, fullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        if (vaultState.State == ExportState.Failed)//由于vault只对特殊的folder type 进行export ，所以有很多情况是skip的，所以skip状态应该继续Archiver,故这里的
                        {
                            mLog.Info(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, fullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        else if (vaultState.State == ExportState.Succeed)
                        {
                            vaultState.ErrorMessage = string.Empty;
                        }
                    } 
                }
                else
                {
                    vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                }
            }
            catch (Exception ex)
            {
                vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                mLog.Error("Error in ExportFolder" + ex.ToString());
                throw;
            }
            finally
            {
                if (!isRootFolder)
                {
                    if (vaultState != null)
                    {
                        BackupSize = vaultState.ExportSize;
                        Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(fullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, cacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                    }
                }
            }
        }
    }

    internal class ItemVault : SPObjectBackup
    {
        public ItemVault(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"ItemVault.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender fileSender = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ItemVault"))
            {
                try
                {
                    switch (entity.NodeType)
                    {
                        case (int)ArchiverCommon.ItemType.DOCUMENT:
                        case (int)ArchiverCommon.ItemType.DOCUMENT_VER:
                            {
                                VaultDocumentOrDocumentVersion(parent, current, entity, ruleName, subJobId, ruleLevel, mediaName);
                                break;
                            }
                        case (int)ArchiverCommon.ItemType.ITEM_TYPE:
                        case (int)ArchiverCommon.ItemType.ITEM_VERSION:
                            {
                                VaultItemOrItemVersion(parent, current, entity, ruleName, subJobId, ruleLevel, mediaName);
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in ItemVaultBackup" + ex.ToString());
                    throw;
                }
                return 0;
            }
        }

        private void VaultDocumentOrDocumentVersion(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ItemVault.VaultDocumentOrDocumentVersion"))
            {
                try
                {
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER && entity.CacheNodeType != (int)CacheNodeType.ItemVersion)
                    {
                        throw new Exception(LOGRESOURCE.StorageOptimization13_SOARBackupImpsItemBackupException);
                    }
                    if(entity.DocumentSize == 0)
                    {
                        ExportStatus vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = "RM_JM_Detail_SkipBackup0KBFile" };
                        BackupSize = vaultState.ExportSize;
                        Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(entity.FullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseExportedFiles(BackupSize);
                        return;
                    }
                    string realName = entity.LeafName;
                    int index = realName.IndexOf(':');
                    if (index >= 0)
                    {
                        realName = realName.Substring(0, index);
                    }
                    AveSPFolder parentFolder = null;
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                    {
                        parentFolder = parent.WrapperObject as AveSPFolder;
                    }
                    else
                    {
                        parentFolder = CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder;
                    }
                    var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection).WrapperObject as AveSPSite;
                    var aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                    current.WrapperObject = aveDoc;
                    if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.DocumentVersion)//version rule 不需要备份current version
                    {
                        current.IsCurrentVersion = true;
                        return;
                    }
                    if (aveDoc.AveSPItem.SPListItem != null && !aveDoc.AveSPItem.AveSPList.SPList.Hidden)//skip all system items
                    {
                        aveDoc.AveSPItem.UserDataCache = aveDoc.AveSPItem.GetUserData();
                        ExportVaultDocument(aveDoc, (int)entity.CacheNodeType, entity.FullPath, subJobId, ruleName, mediaName);
                    }
                    else
                    {
                        ExportStatus vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                        BackupSize = vaultState.ExportSize;
                        Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(entity.FullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseExportedFiles(BackupSize);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in Export Document and DocumentVersion" + ex.ToString());
                    throw;
                }
            }
        }

        internal void ExportVaultDocument(AveSPDoc aveDoc, int cacheNodeType, string fullPath, string subJobId, string ruleName, string mediaName, bool isEndUserExport = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ItemVault.ExportVaultDocument"))
            {
                ExportStatus vaultState = null;
                try
                {
                    if (aveDoc.AveSPItem.SPListItem != null && !aveDoc.AveSPItem.AveSPList.SPList.Hidden)//skip all system items
                    {
                        ItemLevelPathGeneratorInfo docPathInfo = null;
                        VaultExportInfo exportInfo = new VaultExportInfo() { ContentFilePath = string.Empty, FolderPath = string.Empty };
                        docPathInfo = new ItemLevelPathGeneratorInfo()
                        {
                            Item = aveDoc.AveSPItem,
                            JobId = this.Configuration.JobId,
                            PhysicalDeviceDtoId = string.Empty,
                        };
                        exportInfo = this.VaultBeforeArcInfo.VaultExportPathGenerator.GeneratDocExportInfo(docPathInfo);
                        vaultState = this.VaultBeforeArcInfo.VaultExport.ExportDocOrDocVersion(aveDoc, exportInfo);
                        OutputExportVaultDocumentInfo(exportInfo, aveDoc.AveSPItem);
                        mLog.Info("vaultState ExportSize is:{0},ID is:{1},Status is:{2}.", vaultState == null ? "00000" : vaultState.ExportSize.ToString(), aveDoc.AveSPItem.Id, vaultState == null ? ExportState.Failed.ToString() : vaultState.State.ToString());
                        if (vaultState == null)
                        {
                            vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNull };
                            mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, fullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        if (vaultState.State == ExportState.Failed)
                        {
                            mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, fullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        else if (vaultState.State == ExportState.Succeed)
                        {
                            vaultState.ErrorMessage = string.Empty;
                        }
                    }
                    else
                    {
                        vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in Export Vault Document" + ex.ToString());
                    vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                    throw;
                }
                finally
                {
                    if(vaultState != null)
                    {
                        BackupSize = vaultState.ExportSize;
                        if (isEndUserExport)
                        {
                            JobDetail detail = new JobDetail()
                            {
                                SubJobId = Configuration.JobId,
                                Type = cacheNodeType.ToString(),
                                SrcURL = Configuration.GetNodeFullPath(fullPath),
                                Size = vaultState.ExportSize,
                                Status = (int)vaultState.State,
                                Remark12 = "Export",
                                Message = vaultState.ErrorMessage
                            };
                            Configuration.relativeDataJobReportOperation.AddDetail(detail);
                        }
                        else
                        {
                            Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(fullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, cacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                        }
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseExportedFiles(BackupSize);
                    }
                }
            }
        }

        private void OutputExportVaultDocumentInfo(VaultExportInfo exportInfo, AveSPItem item)
        {
            try
            {
                mLog.Info($"NAAGeneratDocExportInfo." +
                    $"AveSPItem.ID:{item.RowId}." +
                    $"AveSPItem.Name:{Convert.ToBase64String(Encoding.UTF8.GetBytes(item.SPListItem.Name))}." +
                    $"Info.FolderPath:{Convert.ToBase64String(Encoding.UTF8.GetBytes(exportInfo.FolderPath))}." +
                    $"info.ContentFilePath:{Convert.ToBase64String(Encoding.UTF8.GetBytes(exportInfo.ContentFilePath))}.");
            }
            catch (Exception ex)
            {
                mLog.Info($"Error in OutputExportVaultDocumentInfo:{ex}.");
            }
        }

        private void VaultItemOrItemVersion(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ItemVault.VaultItemOrItemVersion"))
            {
                try
                {
                    AveSPFolder parentFolder = null;
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                    {
                        parentFolder = parent.WrapperObject as AveSPFolder;
                    }
                    else
                    {
                        parentFolder = CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder;
                    }
                    var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection).WrapperObject as AveSPSite;
                    var aveListItem = new AveSPListItem(parentFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                    current.WrapperObject = aveListItem;
                    if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.ItemVersion)//version rule 不需要备份current version
                    {
                        current.IsCurrentVersion = true;
                        return;
                    }
                    if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.Attachment)
                    { //判断是什么rule,attachement rule 不备份item
                        current.IsCurrentVersion = true;
                        return;
                    }
                    if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.Item)
                    {
                        current.IsCurrentVersion = true;
                        return;
                    }
                    if (aveListItem.AveSPItem.SPListItem != null && !aveListItem.AveSPItem.AveSPList.SPList.Hidden)
                    {
                        VaultExportItem(aveListItem, (int)entity.CacheNodeType, entity.FullPath, subJobId, ruleName, mediaName);
                    }
                    else
                    {
                        ExportStatus vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                        BackupSize = vaultState.ExportSize;

                        Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(entity.FullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseExportedFiles(BackupSize);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in VaultItemOrItemVersion" + ex.ToString());
                    throw;
                }
            }
        }

        internal void VaultExportItem(AveSPListItem aveListItem, int cacheNodeType, string fullPath, string subJobId, string ruleName, string mediaName, bool isEndUserExport = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ItemVault.VaultExportItem"))
            {
                ExportStatus vaultState = null;
                try
                {
                    if (aveListItem.AveSPItem.SPListItem != null && !aveListItem.AveSPItem.AveSPList.SPList.Hidden)
                    {
                        ItemLevelPathGeneratorInfo itemPathInfo = null;
                        VaultExportInfo exportInfo = null;
                        itemPathInfo = new ItemLevelPathGeneratorInfo()
                        {
                            Item = aveListItem.AveSPItem,
                            JobId = this.Configuration.JobId,
                            PhysicalDeviceDtoId = string.Empty,
                        };
                        exportInfo = this.VaultBeforeArcInfo.VaultExportPathGenerator.GenerateItemExportInfo(itemPathInfo);
                        vaultState = this.VaultBeforeArcInfo.VaultExport.ExportItemOrItemVersion(aveListItem, exportInfo);
                        if (vaultState == null)
                        {
                            vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNull };
                            mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, fullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        if (vaultState.State == ExportState.Failed)
                        {
                            mLog.Info(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, fullPath));
                            throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                        }
                        else if (vaultState.State == ExportState.Succeed)
                        {
                            vaultState.ErrorMessage = string.Empty;
                        }
                    }
                    else
                    {
                        vaultState = new ExportStatus() { State = ExportState.Skipped, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOArchiveIsSystemObject };
                    }
                }
                catch (Exception ex)
                {
                    vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                    mLog.Error("Error in Export Item and ItemVersion" + ex.ToString());
                    throw;
                }
                finally
                {
                    BackupSize = vaultState.ExportSize;
                    if (isEndUserExport)
                    {
                        JobDetail detail = new JobDetail()
                        {
                            SubJobId = Configuration.JobId,
                            Type = cacheNodeType.ToString(),
                            SrcURL = Configuration.GetNodeFullPath(fullPath),
                            Size = vaultState.ExportSize,
                            Status = (int)vaultState.State,
                            Remark12 = "Export",
                            Message = vaultState.ErrorMessage
                        };
                        Configuration.relativeDataJobReportOperation.AddDetail(detail);
                    }
                    else
                    {
                        Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(fullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, cacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                    }
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseExportedFiles(BackupSize);
                }
            }
        }
    }

    internal class AttachmentVault : SPObjectBackup
    {
        public AttachmentVault(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"AttachmentVault.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender fileSender = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.AttachmentVault"))
            {
                AveSPItem parentNode = null;
                string itemTitle = string.Empty;
                string fullPath = string.Empty;
                bool isFolder = false;
                try
                {
                    if (parent.WrapperObject is AveSPListItem)
                    {
                        var parentItem = parent.WrapperObject as AveSPListItem;
                        parentNode = parentItem.AveSPItem;
                    }
                    else if (parent.WrapperObject is AveSPFolder)
                    {
                        var parentItem = parent.WrapperObject as AveSPFolder;
                        parentNode = parentItem.AveItem;
                    }
                    //Office 365 Must give ServerRelativeUrl to CreeateAveSPAttachment
                    #region GetAttachmentServerRelativeUrl
                    int index = entity.LeafName.IndexOf(':');
                    int id = 0;
                    string realName = string.Empty;
                    if (index >= 0)
                    {
                        id = Convert.ToInt32(entity.LeafName.Substring(0, entity.LeafName.IndexOfAny(new char[] { '_', '.' })));
                        realName = entity.LeafName.Substring(index + 1);
                    }
                    var aveList = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.List).WrapperObject as AveSPList;
                    string serverUrl = aveList.ServerRelativeUrl + "/Attachments/" + id + "/" + realName;
                    #endregion

                    var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection).WrapperObject as AveSPSite;
                    var aveAttachemnt = new AveSPAttachment(CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder, new Guid(entity.NodeId), entity.LeafName, serverUrl, parentNode);
                    VaultExport(entity.CacheNodeType, aveAttachemnt, serverUrl, subJobId, ruleName, mediaName);
                }
                catch (Exception ex)
                {
                    mLog.Error("Error in AttachmentVaultBackup" + ex.ToString());
                    throw;
                }
                return 0;
            }
        }

        internal void VaultExport(int cacheNodeType, AveSPAttachment aveAttachment, string fullPath, string subJobId, string ruleName, string mediaName, bool isEndUserExport = false)
        {
            ExportStatus vaultState = null;
            try
            {
                AttachmentLevelPathGeneratorInfo attachmentPathInfo = null;
                VaultExportInfo exportInfo = null;
                attachmentPathInfo = new AttachmentLevelPathGeneratorInfo()
                {
                    Attachment = aveAttachment,
                    JobId = this.Configuration.JobId,
                    PhysicalDeviceDtoId = string.Empty,
                };
                exportInfo = this.VaultBeforeArcInfo.VaultExportPathGenerator.GenerateAttachmentExportInfo(attachmentPathInfo);
                vaultState = this.VaultBeforeArcInfo.VaultExport.ExportAttachment(aveAttachment, exportInfo);
                if (vaultState == null)
                {
                    vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNull };
                    mLog.Warn(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArReturnNullLog, fullPath));
                    throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                }
                if (vaultState.State == ExportState.Failed)
                {
                    mLog.Info(string.Format(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedErrorLog, fullPath));
                    throw new VaultFailedException(LOGRESOURCE.StorageOptimization13_SOARSOVaultBefArFailedError);
                }
                else if (vaultState.State == ExportState.Succeed)
                {
                    vaultState.ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                vaultState = new ExportStatus() { State = ExportState.Failed, ExportSize = 0, ErrorMessage = ex.Message };
                mLog.Error("Error in ExportAttachment" + ex.ToString());
                throw;
            }
            finally
            {
                BackupSize = vaultState.ExportSize;
                if (isEndUserExport)
                {
                    JobDetail detail = new JobDetail()
                    {
                        SubJobId = Configuration.JobId,
                        Type = cacheNodeType.ToString(),
                        SrcURL = Configuration.GetNodeFullPath(fullPath),
                        Size = vaultState.ExportSize,
                        Status = (int)vaultState.State,
                        Remark12 = "Export",
                        Message = vaultState.ErrorMessage
                    };
                    Configuration.relativeDataJobReportOperation.AddDetail(detail);
                }
                else
                {
                    Configuration.JobReportDto.AddVaultReport(Configuration.GetNodeFullPath(fullPath), vaultState.ExportSize, (JobDetailsStatus)vaultState.State, cacheNodeType, subJobId, ruleName, mediaName, vaultState.ErrorMessage);
                }
                JobExecutionProgressStatisticExecutor.Instance.IncreaseExportedFiles(BackupSize);
            }
        }
    }
}