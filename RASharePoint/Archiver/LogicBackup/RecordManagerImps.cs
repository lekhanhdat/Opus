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

using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.PhysicalCore.SQL;
using AvePoint.RA.Common;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Archiver.Common.ApprovalService;
using AvePoint.RA.SharePoint.Archiver.Move;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REPORTRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class SiteCollectionRecordManager : SPObjectBackup
    {
        public SiteCollectionRecordManager(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                return await BackupAsync(parent, current, entity, ruleName, subJobId, ruleLevel, mediaName, AveSender);
            }
            catch (Exception e)
            {
                mLog.Error($"Fail build cache for sc,ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                AveSPSite aveSite = null;
                if (Configuration.sharePointType == GCommon.Contract.Tree.Object.SPType.BPOS)
                {
                    aveSite = new AveSPSite(entity.LeafName, AveContextKind.ClientObjectModel, Configuration.user, null);
                }
                else
                {
                    //Online走不到当前逻辑
                    aveSite = new AveSPSite(entity.LeafName, AveContextKind.Auto, new AveBPOSAccountInfo(), null);
                }

                current.WrapperObject = aveSite;
                Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
            }
            catch (Exception ex)
            {
                Configuration.JobReportDto.summaryComments = ex.Message;
                mLog.Error("Error in RecordManager SiteCollection" + ex.ToString());
                throw;
            }
            return 0;
        }
    }

    internal class WebRecordManager : SPObjectBackup
    {
        public WebRecordManager(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                return await BackupAsync(parent, current, entity, ruleName, subJobId, ruleLevel, mediaName, AveSender);
            }
            catch (Exception e)
            {
                mLog.Error($"Fail build cache for web,ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                AveSPWeb aveWeb = new AveSPWeb(aveSite, new Guid(entity.NodeId), entity.LeafName);
                current.WrapperObject = aveWeb;
            }
            catch (Exception ex)
            {
                Configuration.JobReportDto.summaryComments = ex.Message;
                mLog.Error("Error in Web RecordManager" + ex.ToString());
                throw;
            }
            finally
            {
                Configuration.ProgressDto.UpdateProgress(true);
            }
            return 0;
        }
    }

    internal class ListRecordManager : SPObjectBackup
    {
        public ListRecordManager(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                return await BackupAsync(parent, current, entity, ruleName, subJobId, ruleLevel, mediaName, AveSender);
            }
            catch (Exception e)
            {
                mLog.Error($"Fail build cache for list,ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                if (Configuration.ArchiveJobSplitedDBInfo.IsUseSplitedDB && !Configuration.ArchiveJobSplitedDBInfo.IsLatestSplitedDB)
                {
                    Configuration.needDeleteFolder.Clear();
                }
                else
                {
                    DeleteSourceFolder();
                }
                var aveWeb = parent.WrapperObject as AveSPWeb;
                var aveList = new AveSPList(aveWeb, new Guid(entity.NodeId), entity.LeafName, true);
                current.WrapperObject = aveList;
                Configuration.folderParentList = aveList.SPList;
            }
            catch (Exception ex)
            {
                Configuration.JobReportDto.summaryComments = ex.Message;
                mLog.Error("Error in List RecordManager" + ex.ToString());
                throw;
            }
            finally
            {
                Configuration.ProgressDto.UpdateProgress(true);
            }
            return 0;
        }

        public void DisposeObj()
        {
            if (Configuration.ArchiveJobSplitedDBInfo.IsUseSplitedDB && !Configuration.ArchiveJobSplitedDBInfo.IsLatestSplitedDB)
            {
                Configuration.needDeleteFolder.Clear();
            }
            else
            {
                DeleteSourceFolder();
            }
        }

        private void DeleteSourceFolder()
        {
            while (Configuration.needDeleteFolder.Count > 0)
            {
                try
                {
                    int needDeletefolderID = Configuration.needDeleteFolder.Pop();
                    IAveListItem folderItem = Configuration.folderParentList.GetItemById(needDeletefolderID);
                    if (folderItem.Folder != null && folderItem.Folder.ItemCount > 0)
                    {
                        AveCamlQuery query = new AveCamlQuery();
                        query.FolderServerRelativeUrl = folderItem.Folder.ServerRelativeUrl;
                        IAveListItemCollection items = Configuration.folderParentList.GetItems(query);
                        if(items == null)
                        {
                            mLog.Error("Can not get item in folderParentList configuation");
                            throw new Exception("Can not get item in folderParentList configuation");
                        }
                        mLog.Info("This folder contains folders or items. folder ID is:{0},folder URL is:{1},listItem count is:{2},query item count is:{3}.", needDeletefolderID, folderItem.Url, folderItem.Folder.ItemCount, items.Count);
                        foreach (IAveListItem listItem in items)
                        {
                            mLog.Info("This folder cannot be deleted since it contains folders or items. folder ID is:{0},folder URL is:{1},listItem name is:{2}.", needDeletefolderID, folderItem.Url, listItem.Name);
                        }
                    }
                    else
                    {
                        string retentionLabel = string.Empty;
                        try
                        {
                            retentionLabel = folderItem.GetComplianceTagName();
                            DeleteComplianceTagIfEnableRemove(folderItem, new Microsoft.SharePoint.Client.ListItemComplianceInfo
                            {
                                ComplianceTag = retentionLabel,
                                TagPolicyHold = true,
                                TagPolicyRecord = true
                            });
                            folderItem.Delete();
                            mLog.Info("Delete Folder Success. folder ID is:{0},folder URL is:{1},listItem name is:{2}.", needDeletefolderID, folderItem.Url, folderItem.Name);
                        }
                        catch(Exception)
                        {
                            try
                            {
                                if (WrapperConfiguration.EnableRemoveRetentionLabel ||
                                    (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
                                {
                                    mLog.Info($@"fail delete source folder, will restore retention label");
                                    SetComplianceTagIfEnableRemove(folderItem, new Microsoft.SharePoint.Client.ListItemComplianceInfo { ComplianceTag = retentionLabel });
                                }
                            }
                            catch(Exception ex)
                            {
                                mLog.Warn($@"fail restore retention to source folder, ex:{ex}");
                            }
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Record Manager Delete Source Folder Failed.Message:{0}.", ex.ToString());
                }
            }
        }
    }


    internal class FolderRecordManager : SPObjectBackup
    {
        SPMoveFolderRestore folderRestore = new SPMoveFolderRestore();
        public FolderRecordManager(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            JobDetailsStatus res = JobDetailsStatus.Successful;
            try
            {
                //although current wrapper logic can't discover folder version but we add skip folder version for future.
                if (entity.NodeType == (int)ArchiverCommon.ItemType.FOLDER_VERSION || entity.LeafName.IndexOf(':') > 0)
                {
                    mLog.Info("Skip FOLDER_VERSION Node , Type :{0} ", entity.NodeType.ToString());
                    return 0;
                }
                //everyfolder need reset.
                Configuration.appendItemMapping.RemoveAll();
                bool isRootFolder = false;
                bool isKeepFolderStructure = Configuration.currentRule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure;
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
                    if (entity.DoDelete)
                    {
                        if (BriefScanDBOperation.GetInstance(Configuration).NodeIsFailProcessed(entity))
                        {
                            res = JobDetailsStatus.Failed;
                            mLog.Warn($"node id:{entity.NodeId} , path:{entity.FullPath} fail process, will skip delete");
                        }
                        else
                        {
                            Configuration.needDeleteFolder.Push(entity.LibRowId);
                        }
                    }
                }
                return (int)res;
            }
            catch (Exception e)
            {
                mLog.Error($"Fail build cache for folder,ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.FolderRecordManager"))
            {
                //although current wrapper logic can't discover folder version but we add skip folder version for future.
                if (entity.NodeType == (int)ArchiverCommon.ItemType.FOLDER_VERSION || entity.LeafName.IndexOf(':') > 0)
                {
                    mLog.Info("Skip FOLDER_VERSION Node , Type :{0} ", entity.NodeType.ToString());
                    return 0;
                }
                //everyfolder need reset.
                Configuration.appendItemMapping.RemoveAll();
                JobDetailsStatus status = JobDetailsStatus.Successful;
                string desUrl = Configuration.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                bool isKeepFolderStructure = Configuration.currentRule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure;
                bool isRootFolder = false;
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
                    return 0;
                }
                else
                {
                    aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                    current.WrapperObject = aveFolder;
                    if (isKeepFolderStructure && Configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Folder)
                    {
                        string errorMessage = string.Empty;
                        string realName = entity.LeafName;
                        string folderPath = Path.Combine(AveEnv.AgentJobFolder, Configuration.JobId);
                        string backupFolderPath = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(realName) + ".dat");
                        try
                        {
                            // need to do future. 多层同名folder/跟list同名folder可能会存在问题
                            //Configuration.subFolderUrl = entity.FullPath.Substring(entity.FullPath.LastIndexOf(aveFolder.AveList.SPList.RootFolder.Url) + aveFolder.AveList.SPList.RootFolder.Url.Length + 1);
                            if (entity.FullPath.Length > aveFolder.AveList.SPList.RootFolder.ServerRelativeUrl.Length + 1)
                            {
                                Configuration.subFolderUrl = entity.FullPath.Substring(aveFolder.AveList.SPList.RootFolder.ServerRelativeUrl.Length + 1);
                            }
                            else
                            {
                                mLog.Error($"Cannot substring correct sub folder url. full path:{entity.FullPath} , RootFolder.ServerRelativeUrl :{aveFolder.AveList.SPList.RootFolder.ServerRelativeUrl}");
                            }
                            
                            mLog.Info($"Current subfolder url:{Configuration.subFolderUrl}.RootFolderUrl:{aveFolder.AveList.SPList.RootFolder.Url}.FullPath:{entity.FullPath}.");
                            try
                            {
                                if (!Directory.Exists(folderPath))
                                {
                                    Directory.CreateDirectory(folderPath);
                                    mLog.Info("Create Folder : {0}", folderPath);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Error("Can not create temp folder : {0}. Reason: {1}", folderPath, ex.ToString());
                                throw;
                            }
                            #region backup
                            using (AvePerformanceScope pcbackup = new AvePerformanceScope("ArchiveBackUp.FolderRecordManager.backup"))
                            {
                                using (RecordManagerFileSender fileSender = new RecordManagerFileSender(backupFolderPath))
                                {
                                    using (IAveBackupStream exportStream = new WrapperBackupStreamV1(new FileSendWrapper(fileSender)))
                                    {
                                        SPMoveFolderExport exportor = new SPMoveFolderExport(aveFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                                        exportor.ExportSPFolder(exportStream);
                                    }
                                }
                            }
                            #endregion

                            #region Restore
                            mLog.Info("Begin to Restore Folder:FolderName:{0}.FolderVersion:{1}.FolderFullPath:{2}.", entity.LeafName, entity.UIVersion, entity.FullPath);
                            using (AvePerformanceScope pcRestore = new AvePerformanceScope("ArchiveBackUp.ItemRecordManager.Restore"))
                            {
                                using (RecordManagerFileReceiver fileReceiver = new RecordManagerFileReceiver(backupFolderPath))
                                {
                                    using (IAveRestoreStream importStream = new WrapperRestoreStreamV1(new FileReceiverWrapper(fileReceiver)))
                                    {
                                        try
                                        {
                                            folderRestore.Init(importStream, Configuration);
                                            folderRestore.RestoreParentInfo(desUrl, Configuration.subFolderUrl);
                                            using (SPMoveFolderImport importor = new SPMoveFolderImport(folderRestore.GetDestFolder(), Configuration, importStream, entity.LeafName))
                                            {
                                                importor.ImportAveSPFolder();
                                            }
                                        }
                                        //File length exceed 128 catch exception
                                        catch (PathTooLongException e)
                                        {
                                            mLog.Warn(string.Format("Filename or list URL too long. Reason: {0}.", e.ToString()));
                                            throw;
                                        }
                                        catch (SkipException)
                                        {
                                            mLog.Warn("Content Type Or Column Conflict,Skip Current Node: {0}", entity.FullPath);
                                            throw;
                                        }
                                        catch (Exception ex)
                                        {
                                            mLog.Error("Error in Move folder to Destination Library," + ex.ToString());
                                            throw;
                                        }
                                    }
                                }
                            }
                            //add folder to deletion stack when folder move success.
                            if (entity.DoDelete)
                            {
                                Configuration.needDeleteFolder.Push(entity.LibRowId);
                            }
                            mLog.Info("End to Restore Folder:FolderName:{0}.FolderVersion:{1}.FolderFullPath:{2}.", entity.LeafName, entity.UIVersion, entity.FullPath);
                            #endregion
                            //Configuration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Archived, new Guid(entity.NodeId), entity.ArchiveLevel, subJobId);
                        }
                        catch (Exception ex)
                        {
                            if(Configuration?.ArchiveJobSplitedDBInfo?.IsUseSplitedDB == true)
                            {
                                BriefScanDBOperation.GetInstance(Configuration).InsertFailProcessedNodeToDB(entity);
                            }                            
                            status = JobDetailsStatus.Failed;
                            mLog.Error("Error in Record Manager Job,Folder Name : {0},Reason: {1}", entity.LeafName, ex.ToString());
                            throw;
                        }
                        finally
                        {
                            DeleteTempFile(new List<string>() { backupFolderPath });
                            Configuration.ProgressDto.UpdateProgress(true);
                            Configuration.JobReportDto.AddRecordReport(Configuration.GetNodeFullPath(entity.FullPath), desUrl, 0, entity.CacheNodeType, status, subJobId, ruleName, errorMessage);
                        }
                    }
                    return 0;
                }
            }
        }

        private void DeleteTempFile(List<string> files)
        {
            foreach (string fileFullPath in files)
            {
                try
                {
                    File.Delete(fileFullPath);
                    mLog.Info("Delete Temp file Successful.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Error in Delete Temp File :{0},Reason :{1}", fileFullPath, ex.ToString());
                }
            }
        }
    }

    internal class ItemRecordManager : SPObjectBackup
    {
        SPMoveDocRestore restore = new SPMoveDocRestore();
        public IRecordsHistoryService RecordsHistoryTableService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        public ItemRecordManager(AveLogger log)
        {
            mLog = log;
        }

        //private CGDBReader CGDBReader
        //{
        //    get
        //    {
        //        if (Configuration.IsCGDBDiscover)
        //        {
        //            CGDBReaderInstance = CGDBReader.GetInstance(Configuration.ArchiverExtendSetting, Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId.ToString(), Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteUrl);
        //        }
        //        return CGDBReaderInstance;
        //    }
        //}

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"ItemRecordManager.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ItemRecordManager"))
            {
                if (entity.NodeType != (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    mLog.Info("Skip This Node , Type :{0} ", entity.NodeType.ToString());
                    return 0;
                }
                string desUrl = Configuration.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                bool isKeepFolderStructure = Configuration.currentRule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure;
                bool keepClassification = Configuration.currentRule.MoveToRecordCenterAndDelareSetting.KeepSourceClassification;
                //Configuration.currentRule.MoveToRecordCenterAndDelareSetting.IsMoveVersions = true;
                //Wrapper 多线程还原不会修改list setting，配置文件需改成单线程
                WrapperConfiguration.WrapperConfigurationForBPOS.IsMultiThreadRestore = false;
                long size = 0;
                JobDetailsStatus status = JobDetailsStatus.Successful;
                string errorMessage = string.Empty;
                string filePath = string.Empty;
                bool skip = false;
                bool isFirstVersion = true;
                Guid lockID = Guid.Empty;
                Record desRecord = null;
                string sourceFileFullPath = string.Empty;
                Guid sourceFileID = Guid.Empty;
                Guid desOldRecordID = Guid.Empty;
                Guid fileTermId = Guid.Empty;
                bool updateColumnFailed = false;
                bool isSkipOverwrite = false;
                try
                {
                    string realName = entity.LeafName;
                    AveSPFolder parentFolder = parent.WrapperObject as AveSPFolder;
                    IAveWeb web = parentFolder.SPFolder.ParentWeb;
                    IAveFile file = web.GetFile(parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                    mLog.Info($"Source file :{file.UniqueId}");
                    Configuration.moveSourceFileUrl = file.ServerRelativeUrl;
                    Configuration.moveSourceSiteUrl = file.ParentFolder.ParentWeb.Site.Url;
                    sourceFileID = file.UniqueId;
                    #region Add for Auto Checkout file
                    if (!file.Exists)
                    {
                        try
                        {
                            if (file.InDocumentLibrary)
                            {
                                if (IsAutoCheckOutFile(file))
                                {
                                    skip = true;
                                    status = JobDetailsStatus.Skipped;
                                    Configuration.ProgressDto.HasErrorNode = true;
                                    errorMessage = "StorageOptimization_SOARRecordManagerAutoCheckOutFile";
                                    mLog.Info(string.Format("The file is Auto Check Out file,FileName:{0}", file.UniqueId));
                                    //Configuration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Archived, new Guid(entity.NodeId), entity.ArchiveLevel, subJobId);
                                    return 0;
                                };
                            }
                            //File not Exist when file delete after scan job
                            Configuration.ProgressDto.HasErrorNode = true;
                            status = JobDetailsStatus.Failed;
                            errorMessage = "StorageOptimization_SOARRecordManagerFileNotExist";
                            mLog.Info(string.Format("This file does not exist in this list,File Name:{0}", file.UniqueId));
                            //Configuration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Archived, new Guid(entity.NodeId), entity.ArchiveLevel, subJobId);
                            return 0;
                        }
                        catch (FileNotFoundException ex)
                        {
                            mLog.Info(string.Format("TakeOverCheckOutFile Error:{0}.FileName:{1}", ex.ToString(), file.Name.ToString()));
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn(string.Format("Can not get auto check out file.Error:{0}.FileName:{1}", ex.ToString(), file.Name.ToString()));
                        }
                    }
                    #endregion
                    #region records skip check out file.
                    //Online 环境中，目前没有一个可用API 判断文件是check out 的，所以只能通过获取column value 来判断。 check out 文件的column ： {[CheckoutUser, 13;#nmg]}；  非check out文件{[CheckedOutUserId, 1;#]}
                    try
                    {
                        if (Configuration.IsILMode && file.Item != null && !ScheduleConfiguration.CheckisRecord(file.Item))
                        {
                            var values = file.Item.FieldValues;
                            string checkoutUser = values.ContainsKey("CheckoutUser") ? values["CheckoutUser"].ToString() : string.Empty;
                            if (!string.IsNullOrEmpty(checkoutUser))
                            {
                                string separator = ";#";
                                int index = checkoutUser.IndexOf(separator);
                                if (index > 0)
                                {
                                    var checkoutUserName = checkoutUser.Substring(index);
                                    if (!string.IsNullOrEmpty(checkoutUser))
                                    {
                                        skip = true;
                                        status = JobDetailsStatus.Skipped;
                                        //Configuration.ProgressDto.HasErrorNode = true;
                                        errorMessage = IsOneDriveSite(web.Site.Url) ? "RM_JS_JM_OneDriveDataCheckOut" : "StorageOptimization_SOARRecordManagerCheckOutFile";
                                        mLog.Info(string.Format("The file is Check Out file,FileName:{0}", file.UniqueId));
                                        //Configuration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Archived, new Guid(entity.NodeId), entity.ArchiveLevel, subJobId);
                                        return 0;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Debug(" Can not get Check Out User. Reason : {0}", e.ToString());
                    }
                    #endregion
                    size = file.Length;
                    string folderPath = Path.Combine(AveEnv.AgentJobFolder, Configuration.JobId);
                    filePath = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(realName) + ".dat");
                    try
                    {
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                            mLog.Info("Create Folder : {0}", folderPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error("Can not create temp folder : {0}. Reason: {1}", folderPath, ex.ToString());
                        throw;
                    }

                    #region version backup & restore.
                    if (Configuration.currentRule.MoveToRecordCenterAndDelareSetting.IsMoveVersions)
                    {
                        foreach (var version in file.Versions)
                        {
                            try
                            {
                                int UIVersion = Convert.ToInt32(version.VersionLabel.Split('.')[0]) * 512 + Convert.ToInt32(version.VersionLabel.Split('.')[1]);
                                mLog.Info("Begin to bakcup file version for ItemRecordManager.DocumentID:{0}.DocumentVersion:{1}.", entity.LibRowId, UIVersion);
                                using (AvePerformanceScope pcbackup = new AvePerformanceScope("ArchiveBackUp.ItemRecordManager.backupVersion"))
                                {
                                    using (RecordManagerFileSender fileSender = new RecordManagerFileSender(filePath))
                                    {
                                        using (IAveBackupStream exportStream = new WrapperBackupStreamV1(new FileSendWrapper(fileSender)))
                                        {
                                            //Do not need Dispost AveSPDocExport Object,Dispost by CacheNode 
                                            SPMoveDocExport exportor = new SPMoveDocExport(parentFolder, file, UIVersion);
                                            exportor.ExportSPFile(exportStream);
                                        }
                                    }
                                }
                                mLog.Info("End to bakcup file version for ItemRecordManager.DocumentId:{0}.DocumentVersion:{1}.", entity.LibRowId, UIVersion);
                                mLog.Info("Begin to Restore file version:DocumentId:{0}.DocumentVersion:{1}.", entity.LibRowId, UIVersion);
                                using (AvePerformanceScope pcRestore = new AvePerformanceScope("ArchiveBackUp.ItemRecordManager.RestoreVersion"))
                                {
                                    using (RecordManagerFileReceiver fileReceiver = new RecordManagerFileReceiver(filePath))
                                    {
                                        using (IAveRestoreStream importStream = new WrapperRestoreStreamV1(new FileReceiverWrapper(fileReceiver)))
                                        {
                                            try
                                            {
                                                if (isSkipOverwrite && !isFirstVersion)
                                                {
                                                    mLog.Info($"The first version is skipped overwrite so skip this version: {version.VersionLabel}");
                                                    throw new ConetentSkipException("StorageOptimization_SOARDocImportSkipConflictItem");
                                                }
                                                restore.Init(importStream, Configuration, isKeepFolderStructure && Configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Folder);
                                                restore.RestoreParentInfo(desUrl);
                                                using (SPMoveDocImport importor = new SPMoveDocImport(restore.aveSPFolder, restore.Record, entity.LeafName, desUrl))
                                                {
                                                    desOldRecordID = Configuration.IsILMode && isFirstVersion && Configuration.currentRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution != ContentConflictResolution.Append ? importor.GetDesExistFileRecordID() : Guid.Empty;
                                                    importor.ImportAveSPDoc(importStream, Configuration, isFirstVersion);
                                                }
                                            }
                                            #region version exception
                                            catch (ConetentSkipException contentExp)
                                            {
                                                if (contentExp.Message.Contains("StorageOptimization_SOARDocImportSkipConflictItem"))
                                                {
                                                    isSkipOverwrite = true;
                                                }
                                                skip = true;
                                                status = JobDetailsStatus.Skipped;
                                                errorMessage = contentExp.Message;
                                                mLog.Info("Content Skip: FileName: {0}", file.Name);
                                            }
                                            //File length exceed 128 catch exception
                                            catch (PathTooLongException e)
                                            {
                                                status = JobDetailsStatus.Failed;
                                                errorMessage = "StorageOptimization_SOARRecordManagerFileNameTooLong";
                                                mLog.Warn(string.Format("Filename or list URL too long. Reason: {0}.", e.ToString()));
                                            }
                                            catch (SkipException e)
                                            {
                                                skip = true;
                                                status = JobDetailsStatus.Failed;
                                                errorMessage = e.Message;
                                                mLog.Warn("Content Type Or Column Conflict,Skip Current Node: {0}", entity.FullPath);
                                            }
                                            catch (Exception ex)
                                            {
                                                status = JobDetailsStatus.Failed;
                                                errorMessage = ex.Message;
                                                mLog.Error("Error in Move to Destination Library," + ex.ToString());
                                            }
                                            #endregion
                                        }
                                    }
                                }
                                mLog.Info("End to Restore file version:DocumentId:{0}.DocumentVersion:{1}.", entity.LibRowId, UIVersion);
                                isFirstVersion = false;
                            }
                            finally
                            {
                                Configuration.JobReportDto.AddRecordReport(Configuration.GetNodeFullPath(entity.FullPath) + ":" + version.VersionLabel, desUrl, size, (int)CacheNodeType.ItemVersion, status, subJobId, ruleName, errorMessage);
                            }
                        }
                    }
                    #endregion

                    #region current backup & restore.
                    using (AvePerformanceScope pcbackup = new AvePerformanceScope("ArchiveBackUp.ItemRecordManager.backup"))
                    {
                        using (RecordManagerFileSender fileSender = new RecordManagerFileSender(filePath))
                        {
                            using (IAveBackupStream exportStream = new WrapperBackupStreamV1(new FileSendWrapper(fileSender)))
                            {
                                //Do not need Dispost AveSPDocExport Object,Dispost by CacheNode 
                                SPMoveDocExport exportor = new SPMoveDocExport(parentFolder, file, file.UIVersion);
                                exportor.ExportSPFile(exportStream);
                            }
                        }
                    }
                    mLog.Info("Begin to Restore Document:DocumentId:{0}.DocumentVersion:{1}.", entity.LibRowId, entity.UIVersion);
                    using (AvePerformanceScope pcRestore = new AvePerformanceScope("ArchiveBackUp.ItemRecordManager.Restore"))
                    {
                        using (RecordManagerFileReceiver fileReceiver = new RecordManagerFileReceiver(filePath))
                        {
                            using (IAveRestoreStream importStream = new WrapperRestoreStreamV1(new FileReceiverWrapper(fileReceiver)))
                            {
                                try
                                {
                                    if (isSkipOverwrite && !isFirstVersion)
                                    {
                                        mLog.Info("The first version is skipped overwrite so skip current version");
                                        throw new ConetentSkipException("StorageOptimization_SOARDocImportSkipConflictItem");
                                    }
                                    restore.Init(importStream, Configuration, isKeepFolderStructure && Configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Folder);
                                    restore.RestoreParentInfo(desUrl);
                                    using (SPMoveDocImport importor = new SPMoveDocImport(restore.aveSPFolder, restore.Record, entity.LeafName, desUrl))
                                    {
                                        desOldRecordID = Configuration.IsILMode && isFirstVersion && Configuration.currentRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution != ContentConflictResolution.Append ? importor.GetDesExistFileRecordID() : Guid.Empty;
                                        importor.ImportAveSPDoc(importStream, Configuration, isFirstVersion);
                                    }
                                }
                                catch (ConetentSkipException contentExp)
                                {
                                    skip = true;
                                    status = JobDetailsStatus.Skipped;
                                    errorMessage = contentExp.Message;
                                    mLog.Info("Content Skip: FileName: {0}", file.Name);
                                }
                                //File length exceed 128 catch exception
                                catch (PathTooLongException e)
                                {
                                    mLog.Warn(string.Format("Filename or list URL too long. Reason: {0}.", e.ToString()));
                                    throw;
                                }
                                catch (SkipException)
                                {
                                    mLog.Warn("Content Type Or Column Conflict,Skip Current Node: {0}", entity.FullPath);
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    mLog.Error("Error in Move to Destination Library," + ex.ToString());
                                    throw;
                                }
                            }
                        }
                    }
                    mLog.Info("End to Restore Document:Document:{0}.DocumentVersion:{1}.", entity.LibRowId, entity.UIVersion);
                    #endregion

                    #region Declare & link xml.
                    if (status != JobDetailsStatus.Skipped)
                    {
                        if (Configuration.IsILMode && keepClassification)
                        {
                            if (IsOneDriveSite(web.Site.Url))
                            {
                                Guid sourceRecordID = ScheduleConfiguration.GetRecordId(web.Site.ID, sourceFileID);
                                fileTermId = GetOneDriveDataTermId(web.Site.ID, sourceRecordID);
                            }
                            else
                            {
                                fileTermId = GetFileTermId(file);
                            }
                        }
                        string desFileName = file.Name;
                        if (Configuration.currentRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution == ContentConflictResolution.Append
                                && Configuration.appendItemMapping.ContainsKeyAppendName(desFileName))
                        {
                            desFileName = Configuration.appendItemMapping.GetValueAppendName(desFileName);
                        }
                        if (Configuration.IsILMode)//to handle RA old data update from 102, currently use false to declare record,true to undeclare.
                        {
                            desRecord = restore.GetDesFileRecord(desFileName);
                            if (keepClassification)
                            {
                                if (desRecord.SourceFlag == (int)SOSourceFlag.SharePoint || desRecord.SourceFlag == (int)SOSourceFlag.Teams)
                                {
                                    if (fileTermId != Guid.Empty)
                                    {
                                        string updateError = restore.UpdateClassificationColumn(desFileName, fileTermId);
                                        if (string.IsNullOrWhiteSpace(updateError))
                                        {
                                            desRecord.TermId = fileTermId;
                                        }
                                        else if (updateError.Equals(ArchiverErrorMessage.NotUnderTermScopeString))
                                        {
                                            var result = restore.UpdateClassificationColumnWithDestination(desFileName);
                                            desRecord.TermId = result.Item1;
                                            desRecord.TermName = result.Item2;
                                            updateColumnFailed = true;
                                            errorMessage = updateError;
                                        }
                                        else
                                        {
                                            updateColumnFailed = true;
                                            errorMessage = updateError;
                                        }
                                    }
                                    else
                                    {
                                        mLog.Info("File term id is null, will not keep classification.");
                                        var result = restore.UpdateClassificationColumnWithDestination(desFileName, true);
                                        desRecord.TermId = result.Item1;
                                        desRecord.TermName = result.Item2;
                                    }
                                }
                                else
                                {
                                    mLog.Info("Destination is not sharepoint, will not keep classification.");
                                }
                            }
                            else
                            {
                                if (desRecord.SourceFlag == (int)SOSourceFlag.SharePoint || desRecord.SourceFlag == (int)SOSourceFlag.Teams)
                                {
                                    //更新为目的端library default term value
                                    var result = restore.UpdateClassificationColumnWithDestination(desFileName);
                                    desRecord.TermId = result.Item1;
                                    desRecord.TermName = result.Item2;
                                }
                                else
                                {
                                    //不keep源端term，则将目的端数据term置为空，等待data sync job赋值term
                                    desRecord.TermId = Guid.Empty;
                                    desRecord.TermName = string.Empty;
                                }
                            }
                            sourceFileFullPath = new Uri(web.Site.Url).Scheme + @"://" + new Uri(web.Site.Url).Authority + file.ServerRelativeUrl.Replace("\\", "/");
                            if (Configuration.currentRule.MoveToRecordCenterAndDelareSetting.DelaredRecord)
                            {
                                mLog.Info("Records Online job and option is not declared records ");
                                //keep source retention Status(Block delete).
                                if (ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(file.Item))
                                {
                                    restore.ModifySiteRecordDeclarationSetting();
                                    restore.DeclareItem(desFileName);
                                    restore.RevertSiteRecordDeclarationSetting();
                                }
                                //Keep source Block Edit & Block delete Status.
                                else if (ArchiverCommonStaticMethod.CheckIsRecordOnly(file.Item))
                                {
                                    desRecord.DeclareAsRecord = true;
                                    restore.DeclareItem(desFileName);
                                }
                            }
                            else
                            {
                                desRecord.DeclareAsRecord = true;
                                restore.DeclareItem(desFileName);
                            }
                        }
                        else
                        {
                            restore.DeclareItem(desFileName);
                            //if (Configuration.currentRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution == ContentConflictResolution.Append
                            //    && Configuration.appendItemMapping.ContainsKeyAppendName(desFileName))
                            //{
                            //    restore.DeclareItem(desFileName);
                            //}
                            //else
                            //{
                            //    restore.DeclareItem(desFileName);
                            //}
                        }
                        //isRestoreXml = true  means Need Restore XML file
                        if (Configuration.isRestoreXml)
                        {
                            mLog.Info("Move action begin generate field xml.");
                            List<FieldDataInfo> fields = GetFieldVaule(file);
                            RecordManagerUtility rmUtility = new RecordManagerUtility();
                            string xmlString = rmUtility.ConvertToXML(fields, web.Url + "/" + file.Url);
                            restore.RestoreFileXML(desFileName + "_" + GetDateTimeToFolderName() + ".xml", xmlString);
                        }
                    }
                    #endregion

                    #region Delete

                    //If content not skip ,delete source file
                    if (!skip)
                    {
                        //Delete Source File
                        Delete(file);
                    }

                    #endregion

                    if (status != JobDetailsStatus.Skipped)
                    {
                        //Guid sourcePathMD5 = new Guid(HashCodeHelper.ToMD5HashCode(sourceFileFullPath.ToLowerInvariant()));
                        Guid sourceRecordID = ScheduleConfiguration.GetRecordId(web.Site.ID, sourceFileID);
                        UpdateMoveActionExploreDB(web.Site.ID, sourceRecordID, desOldRecordID, sourceFileFullPath, desRecord, updateColumnFailed, fileTermId, keepClassification);
                    }

                    if (updateColumnFailed)
                    {
                        throw new Exception(errorMessage);
                    }
                    //if (Configuration.IsCGDBDiscover && CGDBReader != null)
                    //{
                    //    CGDBReader.UpdateStatus(Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId, sourceFileID, JobDetailsStatus.Successful);
                    //}
                    //Configuration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Archived, new Guid(entity.NodeId), entity.ArchiveLevel, subJobId);
                }
                catch (CheckOutDocumentDeleteException exception)
                {
                    //if (Configuration.IsCGDBDiscover && CGDBReader != null)
                    //{
                    //    CGDBReader.UpdateStatus(Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId, sourceFileID, JobDetailsStatus.Failed);
                    //}
                    status = JobDetailsStatus.Failed;
                    Configuration.ProgressDto.HasErrorNode = true;
                    errorMessage = "StorageOptimization_SOARRecordManagerCheckOutDocumentDeleteFailed";
                    mLog.Error("[CheckOutDocumentDeleteException]Error in Record Manager Job,Item Name : {0},Reason: {1}", entity.LeafName, exception.ToString());
                }
                catch (DocumentSetContentTypeFileDeclareException exception)
                {
                    //if (Configuration.IsCGDBDiscover && CGDBReader != null)
                    //{
                    //    CGDBReader.UpdateStatus(Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId, sourceFileID, JobDetailsStatus.Failed);
                    //}
                    status = JobDetailsStatus.Failed;
                    Configuration.ProgressDto.HasErrorNode = true;
                    errorMessage = "StorageOptimization_SOARRecordManagerDocumentSetContentTypeFileDeclareFailed";
                    mLog.Error("[DocumentSetContentTypeFileDeclareException]Error in Record Manager Job,Item Name : {0},Reason: {1}", entity.LeafName, exception.ToString());
                }
                catch (CheckOutDocumentDeclareException exception)
                {
                    //if (Configuration.IsCGDBDiscover && CGDBReader != null)
                    //{
                    //    CGDBReader.UpdateStatus(Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId, sourceFileID, JobDetailsStatus.Failed);
                    //}
                    status = JobDetailsStatus.Failed;
                    Configuration.ProgressDto.HasErrorNode = true;
                    errorMessage = "StorageOptimization_SOARRecordManagerCheckOutDocumentDeclareFailed";
                    mLog.Error("[CheckOutDocumentDeclareException]Error in Record Manager Job,Item Name : {0},Reason: {1}", entity.LeafName, exception.ToString());
                }
                catch (LabelDocumentDeleteException exception)
                {
                    //if (Configuration.IsCGDBDiscover && CGDBReader != null)
                    //{
                    //    CGDBReader.UpdateStatus(Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId, sourceFileID, JobDetailsStatus.Failed);
                    //}
                    status = JobDetailsStatus.Failed;
                    Configuration.ProgressDto.HasErrorNode = true;
                    errorMessage = "StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed";
                    mLog.Error("[LabelDocumentDeleteException]Error in Record Manager Job,Item Name : {0},Reason: {1}", entity.LeafName, exception.ToString());
                }
                catch (Exception exception)
                {
                    //if (Configuration.IsCGDBDiscover && CGDBReader != null)
                    //{
                    //    CGDBReader.UpdateStatus(Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId, sourceFileID, JobDetailsStatus.Failed);
                    //}
                    Configuration.JobReportDto.summaryComments = exception.Message;
                    errorMessage = exception.Message;
                    if (exception is SkipException)
                    {
                        status = JobDetailsStatus.Failed;
                    }
                    else if (exception is PathTooLongException)
                    {

                        status = JobDetailsStatus.Failed;
                        errorMessage = "StorageOptimization_SOARRecordManagerFileNameTooLong";
                        mLog.Error("Error in Record Manager Job,Item Name : {0},Reason: {1}", entity.LeafName, exception.ToString());
                    }
                    else
                    {
                        mLog.Error("Error in Record Manager Job,Item Name : {0},Reason: {1}", entity.LeafName, exception.ToString());
                        status = JobDetailsStatus.Failed;
                    }
                    throw;
                }
                finally
                {
                    DeleteTempFile(new List<string>() { filePath });
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                    //JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, status);
                    Configuration.JobReportDto.AddRecordReport(Configuration.GetNodeFullPath(entity.FullPath), desUrl, size, entity.CacheNodeType, status, subJobId, ruleName, errorMessage);
                }
                return 0;
            }
        }

        private bool IsOneDriveSite(string siteUrl)
        {
            var daoSite = Configuration.IsILMode ? Configuration.GetRemoteSiteCollectionByRecords(siteUrl) : Configuration.GetRemoteSiteCollectionByDAO(siteUrl);
            return daoSite != null && daoSite.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro;

        }

        private Guid GetFileTermId(IAveFile file)
        {
            Guid termId = Guid.Empty;
            var siteInfo = Configuration.GetDestinationColumnSetting(file.ParentFolder.ParentWeb.Site.Url);
            if (siteInfo != null)
            {
                if (file.Item.Fields.ContainsField(siteInfo.ColumnName))
                {
                    var termObj = file.Item[siteInfo.ColumnName];
                    if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
                    {
                        var valueString = termObj.ToString().Split('|');
                        if (valueString.Length > 1)
                        {
                            termId = new Guid(valueString[1]);
                        }
                        else
                        {
                            mLog.Info($"{file.Url} invalid term format:{valueString}");
                        }

                    }
                }
            }

            return termId;
        }

        private Guid GetOneDriveDataTermId(Guid sourceSiteID, Guid sourceRecordID)
        {
            if (Configuration.ExplorerDao != null)
            {
                Record sourceRecord = Configuration.ExplorerDao.ReadById(sourceSiteID, sourceRecordID);
                if (sourceRecord != null)
                {
                    return sourceRecord.TermId;
                }
                else
                {
                    return Guid.Empty;
                }
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// source  dest	 behaviors
        /// exists	exists   update des to source, delete source.
        /// non	    exists   delete des.(wait for next sync job sync the dest item into explorer db)
        /// exists	non      insert new des,delete source.
        /// non	    non	     do nothing, wait for next sync job sync the dest item into explorer db.
        /// </summary>
        private void UpdateMoveActionExploreDB(Guid sourceSiteID, Guid sourceRecordID, Guid desOldRecordID, string sourcePath, Record desRecord, bool updateColumnFailed, Guid fileTermId, bool keepSourceClassfication)
        {
            if (Configuration.IsILMode && Configuration.ExplorerDao != null)
            {
                //SP Move 源端文件不一定在ExplorerDB存在，需要先判断源端在不在ExplorerDB中.
                //目的端是SP，NodeID需要记录Move之前的File ID.
                Record sourceRecord = Configuration.ExplorerDao.ReadById(sourceSiteID, sourceRecordID);
                if (sourceRecord != null)
                {
                    var sourceFlag = sourceRecord.SourceFlag;
                    sourceRecord.FullPath = sourcePath;
                    desRecord = CopySourceNonSPRecordPropertyToDesRecord(desRecord, sourceRecord, updateColumnFailed, fileTermId, keepSourceClassfication);
                    desRecord.ContainerId = Configuration.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ContainerId;
                    desRecord.AppendMetaInfoForMovedData();
                    if (sourceFlag != desRecord.SourceFlag || desRecord.TermId == Guid.Empty)
                    {
                        desRecord.RuleId = Guid.Empty;
                        desRecord.DisposalDueDate = -2;
                        desRecord.RecordOwner = string.Empty;
                        desRecord.RecordOwner_Array = null;
                    }
                    if (Configuration.ExplorerDao.ReadById(desRecord.ScopeId, desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID) != null)
                    {
                        //源端和目的端都存在，则添加新的记录到ExploreDB中，删除源端和目的端原有记录，由于源端和目的端RecordID已经变了，不能再次使用。
                        Configuration.ExplorerDao.Add(desRecord);
                        //RECO-3552 对于Move 操作，Report 要求将原端数据更新成 4 = Moved 状态，不进行删除Explorer 数据操作
                        var rec = Configuration.ExplorerDao.QueryAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID).FirstOrDefault();
                        if (rec != null)
                        {
                            Configuration.ExplorerDao.UpdateAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID && s.RecordStatus == 1, r => { r.RecordStatus = 4; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                        }
                        //Configuration.explorerDao.Delete(sourceSiteID, sourceRecordID);
                        //Configuration.explorerDao.Delete(desRecord.ScopeId, desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID);
                        Configuration.ExplorerDao.UpdateAll(s => s.ScopeId == desRecord.ScopeId && s.Id == (desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID), r => { r.RecordStatus = 5; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                        if (desRecord.HoldStatus)
                        {
                            RecordsDBOperation.UpdateRMRecordAlliancesTableRecordsId(sourceRecordID, desRecord.Id);
                        }
                    }
                    else
                    {
                        Configuration.ExplorerDao.Add(desRecord);
                        //RECO-3552 对于Move 操作，Report 要求将原端数据更新成 4 = Moved 状态，不进行删除Explorer 数据操作
                        var rec = Configuration.ExplorerDao.QueryAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID).FirstOrDefault();
                        if (rec != null)
                        {
                            Configuration.ExplorerDao.UpdateAll(s => s.ScopeId == sourceSiteID && s.Id == sourceRecordID && s.RecordStatus == 1, r => { r.RecordStatus = 4; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                        }
                        //Configuration.explorerDao.Delete(sourceSiteID, sourceRecordID);
                        if (desRecord.HoldStatus)
                        {
                            RecordsDBOperation.UpdateRMRecordAlliancesTableRecordsId(sourceRecordID, desRecord.Id);
                        }
                    }
                }
                else
                {
                    if (Configuration.ExplorerDao.ReadById(desRecord.ScopeId, desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID) != null)
                    {
                        //Configuration.explorerDao.Delete(desRecord.ScopeId, desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID);
                        Configuration.ExplorerDao.UpdateAll(s => s.ScopeId == desRecord.ScopeId && s.Id == (desOldRecordID == Guid.Empty ? desRecord.Id : desOldRecordID), r => { r.RecordStatus = 5; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                    }
                    else
                    {
                        // don't do anything.
                    }
                }
            }
        }

        //此方法建议修改成将desRecord 中已经取到的属性，赋值给source， 然后返回SourceRecord 对象。这样能避免后期添加source属性造成bug
        private Record CopySourceNonSPRecordPropertyToDesRecord(Record desRecord, Record sourceRecord, bool updateColumnFailed, Guid fileTermId, bool keepSourceClassfication)
        {
            #region Copy Source non SP Record property to Des Record
            desRecord.CollectTime = DateTime.UtcNow.Ticks;
            desRecord.CreateDate = sourceRecord.CreateDate;
            desRecord.DeclaredBy = sourceRecord.DeclaredBy;
            desRecord.DestroyedTime = sourceRecord.DestroyedTime;
            desRecord.DisposalDueDate = sourceRecord.DisposalDueDate;
            desRecord.ExtensionForFile = sourceRecord.ExtensionForFile;
            desRecord.Extsion1 = sourceRecord.Extsion1;
            desRecord.HoldBy = sourceRecord.HoldBy;
            desRecord.HoldId = sourceRecord.HoldId;
            desRecord.HoldReleaseTime = sourceRecord.HoldReleaseTime;
            desRecord.HoldStatus = sourceRecord.HoldStatus;
            desRecord.AppendHolds_Array = sourceRecord.AppendHolds_Array;
            desRecord.HoldByUsers = sourceRecord.HoldByUsers;
            desRecord.HoldUntilTimes = sourceRecord.HoldUntilTimes;
            desRecord.MetaInfo = sourceRecord.MetaInfo;
            //RECO - 3615, RECO-3616 当前版本，Move行为仍然不去管所有属性，依赖后期data sync行为。所以create by modified by 还从sourceRecord 获取。
            desRecord.ModifiedBy = sourceRecord.ModifiedBy;
            desRecord.CreatedBy = sourceRecord.CreatedBy;
            desRecord.NodeType = sourceRecord.NodeType;

            desRecord.PredictTermId = sourceRecord.PredictTermId;
            desRecord.PredictTime = sourceRecord.PredictTime;
            desRecord.MLUnderReview = sourceRecord.MLUnderReview;
            desRecord.MLClassificationType = sourceRecord.MLClassificationType;
            desRecord.MLReviewer = sourceRecord.MLReviewer;
            desRecord.MLApprovalStatus = sourceRecord.MLApprovalStatus;
            desRecord.MLEscalateFrom = sourceRecord.MLEscalateFrom;
            desRecord.MLEscalatedComment = sourceRecord.MLEscalatedComment;
            desRecord.TrainingScope = sourceRecord.TrainingScope;
            desRecord.TrainingTermId = sourceRecord.TrainingTermId;
            desRecord.TrainingAddType = sourceRecord.TrainingAddType;
            desRecord.TrainingModelId = sourceRecord.TrainingModelId;
            desRecord.PredictTermScore = sourceRecord.PredictTermScore;

            AddRecordHistory(desRecord, sourceRecord);
            desRecord.RecordOwner = sourceRecord.RecordOwner;
            desRecord.RecordsId = sourceRecord.RecordsId;
            desRecord.RecordStatus = sourceRecord.RecordStatus;
            desRecord.RelatedRecords = sourceRecord.RelatedRecords;
            desRecord.RelatedRecordsCount = sourceRecord.RelatedRecordsCount;
            desRecord.RuleId = sourceRecord.RuleId;
            desRecord.RuleLevel = sourceRecord.RuleLevel;
            //desRecord.SourceFlag = sourceRecord.SourceFlag;
            if ((desRecord.SourceFlag == (int)SOSourceFlag.SharePoint || desRecord.SourceFlag == (int)SOSourceFlag.Teams) && !updateColumnFailed)
            {
                if (keepSourceClassfication)
                {
                    desRecord.TermName = sourceRecord.TermName;
                }
            }
            //目的端为OneDrive时，TermId直接使用源端TermId
            if (desRecord.SourceFlag == (int)SOSourceFlag.OneDrive)
            {
                if (keepSourceClassfication)
                {
                    desRecord.TermId = fileTermId == Guid.Empty ? sourceRecord.TermId : fileTermId;
                }
            }
            #endregion
            return desRecord;
        }

        private string AddRecordHistory(Record desRecord, Record sourceRecord)
        {
            string recordHistory = string.Empty;
            try
            {
                var action = $"RM_Explorer_RecordHistorySuccessfulInformation{I18NEntity.Separator}{sourceRecord.FullPath}{I18NEntity.Separator}{desRecord.FullPath}";
                RecordsHistoryTableService.AddRecordsHistory(new List<Guid> { sourceRecord.Id }, action);
                
                //string actionResult = string.Empty;
                ////actionResult = string.Format("Failed to move the object from '{0}' to '{1}'.", sourceRecord.FullPath, desRecord.FullPath);
                //actionResult = string.Format("Successfully moved the object from '{0}' to '{1}'.", sourceRecord.FullPath, desRecord.FullPath);
                ////actionResult = string.Format("Skipped moving the object from '{0}' to '{1}'.", sourceRecord.FullPath, desRecord.FullPath);
                //if (!string.IsNullOrEmpty(sourceRecord.RecordHistory))
                //{
                //    var old = XmlUtil.GetXmlObject<RecordHistoryXml>(sourceRecord.RecordHistory);
                //    old.HistoryList.Add(new RecordHistory() { Action = actionResult, User = Configuration.runJobUser, TimeUTC = DateTime.UtcNow.Ticks });
                //    recordHistory = XmlUtil.GetXmlString(old);
                //}
                //else
                //{
                //    RecordHistoryXml historyXML = new RecordHistoryXml
                //    {
                //        HistoryList = new List<RecordHistory>()
                //    };
                //    historyXML.HistoryList.Add(new RecordHistory() { Action = actionResult, User = Configuration.runJobUser, TimeUTC = DateTime.UtcNow.Ticks });
                //    recordHistory = XmlUtil.GetXmlString(historyXML);
                //}
            }
            catch (Exception ex)
            {
                recordHistory = string.Empty;
                mLog.Info(string.Format("GetRecordHistory Error:{0}.", ex.ToString()));
            }
            return recordHistory;
        }

        private void Delete(IAveFile file)
        {
            //AutoCheckOut
            if (!file.Exists)
            {
                try
                {
                    if (file.InDocumentLibrary)
                    {
                        if (!TakeOverCheckOutFile(file))
                        {
                            mLog.Info(string.Format("TakeOverCheckOutFile Success,FileName:{0}", file.UniqueId.ToString()));
                            return;
                        };
                    }
                }
                catch (FileNotFoundException ex)
                {
                    mLog.Info(string.Format("TakeOverCheckOutFile Error:{0}.FileName:{1}", ex.ToString(), file.Name.ToString()));
                    return;
                }
            }
            //only records delete system file.
            if (!Configuration.IsILMode
                &&
                (Configuration.BackgroundSettings.SkipExtentionName.Exists(f => file.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)
                 || (file.ParentFolder.Name.Equals("Forms", StringComparison.OrdinalIgnoreCase) && !file.ParentFolder.GetType().IsVisible))))
            {
                mLog.Info(string.Format("File don't need Delete ,It is system File.FileName:{0}", file.UniqueId.ToString()));
                return;
            }
            try
            {
                file.Delete();
                mLog.Info("Delete file Successful.");
            }
            catch (Exception ex)
            {
                mLog.Info(string.Format("File Delete Error: {0} error message: {1}", file.Name, ex.ToString()));
                //support office 365 declare file ADO-168747
                if (ScheduleConfiguration.CheckisRecord(file.Item) || restore.IsLockFileByRecordLabel(file.Item))
                {
                    try
                    {
                        mLog.Info("Current file is Declare file");
                        restore.UnDeclareItem(file.Item);
                        if (restore.IsLockFileByRecordLabel(file.Item))
                        {
                            mLog.Info($"Current file {file.ServerRelativeUrl} is lock by record label");
                            restore.DeleteRecordLabel(file.Item);
                        }
                        file.Delete();
                        mLog.Info("Delete Declare file or lock by record file Successful.");
                        return;
                    }
                    catch (Exception dx)
                    {
                        mLog.Info("Delete Declare file or lock by record file Failed,file Name:{0},Exception:{1}.", file.Name, dx.ToString());
                    }
                }
                else if (file.CheckedOutByUser != null)
                {
                    mLog.Info("Start CheckOutByUser Operation.");
                    DeleteCheckOutFile(file);
                    return;
                }
                else if (ex.InnerException != null && ex.InnerException.Message.Contains("is checked out for editing by"))
                {
                    mLog.Info("Current file is check out file.Message:{0}.", ex.InnerException.Message);
                    throw new CheckOutDocumentDeleteException("");
                }
                else if (ex.InnerException != null && ex.InnerException.Message.Contains("The label that's applied to this item prevents it from being edited or deleted"))
                {
                    mLog.Info("Current file is label file.Message:{0}.", ex.InnerException.Message);
                    if (Configuration.IsILMode
                                && file.Item.Fields.ContainsField("Retention label")
                                && RecordsDBOperation.RMEXOLabels.Where(
                                    x => x.LabelName == file.Item["Retention label"].ToString()
                                    && x.Status == 1 && x.Type == 1).FirstOrDefault() != null)
                    {
                        mLog.Info("Current file is label file and Records remove label and delete.FileName:{0}.", file.UniqueId);
                        //file.Item.SetComplianceTag(string.Empty, false, false, false, false, false);
                        file.Item.SetComplianceTagOnBulkItems(string.Empty);
                        file = file.Web.GetFile(file.UniqueId, file.ServerRelativeUrl);
                        file.Delete();
                        mLog.Info("Delete label file success.File name:{0}", file.UniqueId);
                        return;
                    }
                    else
                    {
                        throw new LabelDocumentDeleteException("");
                    }
                }
                throw;
            }
        }
        /// <summary>
        /// 用于删除系统用户上传并被其他用户checkout的文件
        /// 非系统用户上传并被非系统用户checkout的document的删除不能用此方法
        /// </summary>
        /// <param name="listItem"></param>
        private void DeleteCheckOutFile(IAveFile file)
        {
            int checkOutUserId = file.CheckedOutByUser.ID;
            if (checkOutUserId > 0)
            {
                if (!file.CheckOutStatus.Equals(AveCheckOutStatus.None) || (Configuration.recordManagerRestoreOMFactory != null && Configuration.recordManagerRestoreOMFactory.ContextKind == AveContextKind.ClientObjectModel))
                {
                    try
                    {
                        file.UndoCheckOut();//对Check Out的File进行并且check out User被删除的文件，需要先check in 
                    }
                    catch (Exception e)
                    {
                        mLog.Warn(string.Format("UndoCheckOut Failed. Error:{0}", e.ToString()));
                    }
                }
            }
            if (!file.Exists)
            {
                return;
            }
            file.Delete();
            mLog.Info(string.Format("CheckOutFile Success delete"));
        }

        private bool IsAutoCheckOutFile(IAveFile file)
        {
            IAveDocumentLibrary docList = file.ParentFolder.ParentList as IAveDocumentLibrary;
            IList<IAveCheckedOutFile> checkOutFiles = docList.CheckedOutFiles;
            bool isCheckOutFile = false;
            foreach (IAveCheckedOutFile cofile in checkOutFiles)
            {
                if (cofile.LeafName.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!file.Exists)
                    {
                        isCheckOutFile = true;
                        break;
                    }
                }
            }
            return isCheckOutFile;
        }

        private bool TakeOverCheckOutFile(IAveFile file)
        {
            try
            {
                IAveDocumentLibrary docList = file.ParentFolder.ParentList as IAveDocumentLibrary;
                IList<IAveCheckedOutFile> checkOutFiles = docList.CheckedOutFiles;
                int count = 0;
                foreach (IAveCheckedOutFile cofile in checkOutFiles)
                {
                    if (cofile.LeafName.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        cofile.TakeOverCheckOut();
                        if (!file.Exists)
                        {
                            return false;
                        }
                        break;
                    }
                    count++;
                }
                if (count >= checkOutFiles.Count)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                mLog.Info("Take Over Check Out File Error: {0}", e.ToString());
                return false;
            }
            return true;
        }

        private void DeleteTempFile(List<string> files)
        {
            foreach (string fileFullPath in files)
            {
                try
                {
                    File.Delete(fileFullPath);
                    mLog.Info("Delete Temp file Successful");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Error in Delete Temp File :{0},Reason :{1}", fileFullPath, ex.ToString());
                }
            }
        }

        private string GetDateTimeToFolderName()
        {
            string name = string.Empty;
            DateTime date = DateTime.UtcNow;
            name = date.Year + "_" + date.Month + "_" + date.Day + "_" + date.Hour + "_" + date.Minute + "_" + date.Second;
            return name;
        }

        private FieldDataInfo GetFiledVaule(IAveListItem item, IAveField field)
        {
            try
            {
                string fieldType = field.Type.ToString();
                string fieldValue = string.Empty;
                if (item.Fields.Contains(field.ID))
                {
                    fieldValue = GetFiledVaule(field, item[field.ID]);
                }
                //Add code for O365 :Mata Column has  "|" and Lookup Column has ";#",we need to split it - ADO-134022
                if ("Invalid".Equals(fieldType, StringComparison.OrdinalIgnoreCase))
                {
                    int indexInvalid = fieldValue.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (indexInvalid > 0)
                    {
                        fieldValue = fieldValue.Substring(0, indexInvalid);
                    }
                }
                if ("Lookup".Equals(fieldType, StringComparison.OrdinalIgnoreCase))
                {
                    int indexLookup = fieldValue.IndexOf(";#", StringComparison.OrdinalIgnoreCase);
                    if (indexLookup > 0)
                    {
                        fieldValue = fieldValue.Substring(0, indexLookup);
                    }
                }
                FieldDataInfo fieldDataInfo = new FieldDataInfo()
                {
                    DisplayName = field.Title,
                    InternalName = field.InternalName,
                    Value = fieldValue,
                    FieldType = fieldType,
                };
                if (field is IAveFieldUser)
                {
                    fieldDataInfo.Value = ProcessFieldUser(fieldDataInfo.Value);
                }
                if (field is IAveFieldDateTime)
                {
                    fieldDataInfo.Value = item[field.ID].ToString();
                }
                return fieldDataInfo;
            }
            catch (Exception e)
            {
                mLog.Warn("Get field Error, field Title: {0}, Reason: {1}", field.Title, e.ToString());
                return null;
            }
        }

        private string ProcessFieldUser(string value)
        {
            string result = value;
            int index = value.IndexOf(';');
            if (index > 0)
            {
                try
                {
                    string IDStr = value.Substring(0, index);
                    int id = 0;
                    if (int.TryParse(IDStr, out id))
                    {
                        //IAveUser user = _currentWeb.SiteUsers.GetByID(id);
                        //result = user.LoginName;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info("Error in ProcessFieldUser: value is {0}, Reason : {1}", value, ex.ToString());
                }
            }
            return result;
        }


        private string GetFiledVaule(IAveField field, object fieldValue)
        {
            return field.GetFieldValueAsText(fieldValue);
        }

        private List<FieldDataInfo> GetFieldVaule(IAveFile file)
        {
            List<FieldDataInfo> result = new List<FieldDataInfo>();
            if (file.Item == null)
            {
                return null;
            }
            IEnumerable<IAveField> fieldMapping = file.ParentFolder.ParentList.Fields.Where(f => !f.Hidden);
            return fieldMapping.Select(f => GetFiledVaule(file.Item, f)).Where(f => f != null).ToList();
        }

        public void DisposeObj()
        {
            restore.Dispose();
        }
    }


    internal class AttachmentRecordManager : SPObjectBackup
    {
        public AttachmentRecordManager(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"AttachmentRecordManager.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }


        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            return 0;
        }
    }
}
