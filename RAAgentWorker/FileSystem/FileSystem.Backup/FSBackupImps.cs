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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Media.Storage;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Diagnostics.CodeAnalysis;
using RAFileSystem.FileSystem.Backup;
using RAFileSystem.FileSystem.FileSystem.Backup;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.RA.Contract.Explorer;
using LS.SPWorkflowProcessor;
using AvePoint.RA.FileSystem.Collect;
using RAFileSystem.FileSystem.Common;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Utils;
using AvePoint.RA.FileSystem.Core;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Media.Object;
using RAFileSystem.FileSystem.Common.Extension;

namespace AvePoint.StorageOptimization.Schedule.Archiver
{
    //internal class ConnectionInfoBackup : FSObjectBackup
    //{
    //    private AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    //    public FSBackupSender AveSender { get; set; }
    //    public ScheduleConfiguration Configuration { get; set; }
    //    public override int Backup(FSAzureTableEntityDto dto)
    //    {
    //        using (AvePerformanceScope pc = new AvePerformanceScope("ConnectionInfoBackup"))
    //        {
    //            mLog.Debug("Start Backup Connection,connection name: {0}.", dto.FullPath);
    //            string errorMessage = string.Empty;
    //            BackupRestoreStatus status = BackupRestoreStatus.Succeed;
    //            FileAtrributeInfo info = new FileAtrributeInfo();
    //            try
    //            {
    //                //current.StorageObject = Configuration.physicalDevice;// just backup the path info and other settings.
    //                ConnectionBackup aveConnection = new ConnectionBackup(Configuration.physicalDevice);
    //                mLog.Debug("Start Backup Connection Header,connection name: {0}.", dto.FullPath);
    //                AveSender.BackupConnectionHeader(Configuration.physicalDevice, Configuration.UNCPath, dto, AveSender.BackupStream.StreamTransfered, ruleName, subJobid, mediaName);
    //                //current.PermissionScopeId = entity.PermissionScopeId;
    //                //current.FileHeader = AveSender.BackupHeader(dto.FullPath);
    //                var stream = AveSender.BackupStream;
    //                stream.BeginWriteMetadata();
    //                try
    //                {
    //                    mLog.Debug("Start Export Collection Base Info,connection Path: {0}.", dto.FullPath);
    //                    aveConnection.ExportBaseConnectionInfo(stream);
    //                    mLog.Debug("Start Export Connection Full Text Index,Collection Path: {0}.", dto.FullPath);
    //                    aveConnection.ExportFullTextIndex(stream);
    //                    mLog.Debug("Start Export Connection Permission metadata, Collection Path : {0}", dto.FullPath);
    //                    aveConnection.ExportFSConnectionPermission(stream);
    //                }
    //                catch (Exception e)
    //                {
    //                    mLog.Error("Error in backup connection metadata, reason : {0}", e.ToString());
    //                    status = BackupRestoreStatus.Failed;
    //                    throw;
    //                }
    //                finally
    //                {
    //                    AveSender.BackupStream.EndWriteMetadata();
    //                    AveSender.BackupStream.FlushMetadata(0);
    //                    AveSender.BackupTail(status == BackupRestoreStatus.Succeed);
    //                    //current.BackupStatus = FileHeaderStatus.Complete;
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                mLog.Error("Backup Connection Error: {0}", e.ToString());
    //                errorMessage = e.Message.ToString();
    //                status = BackupRestoreStatus.Failed;
    //                //current.BackupStatus = FileHeaderStatus.Failed;
    //                throw;
    //            }
    //            finally
    //            {
    //                mLog.Debug("Start sending the job progress...Archiver level: {0}", entity.ArchiveLevel.ToString());
    //                //JobReportInfo reportInfo = new JobReportInfo() { jobReportType = ArchiveJobReportType.FSArchiver, SourceURL = entity.Path, status = (int)status, cacheNodeType = entity.CacheNodeType, jobID = Configuration.JobId, dataSize = AveSender.BackupStream.StreamTransfered, errorMessage = errorMessage, ruleName = ruleName, mediaName = mediaName };
    //                Configuration.JobReportDto.AddReport(reportInfo);
    //                //current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
    //            }
    //        }
    //        return 0;
    //    }
    //}

    //internal class FSFolderBackup : FSObjectBackup
    //{
    //    private AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    //    public FSBackupSender AveSender { get; set; }
    //    public ScheduleConfiguration Configuration { get; set; }
    //    [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
    //    public override int Backup(FSAzureTableEntityDto dto)
    //    {
    //        using (AvePerformanceScope pc = new AvePerformanceScope("FSFolderBackup"))
    //        {
    //            mLog.Debug("Start Backup FSFolder,Folder Path: {0}.", dto.FullPath);
    //            string errorMessage = string.Empty;
    //            BackupRestoreStatus status = BackupRestoreStatus.Succeed;
    //            try
    //            {
    //                XDirectoryInfo dirInfo = Configuration.physicalDevice.OpenDirectory(new StorageInfo(dto.HighName, dto.LowName), System.IO.FileMode.Open);
    //                //current.StorageObject = dirInfo;// just backup the path info and other settings.

    //                FSFolderBackupIMP aveFolder = new FSFolderBackupIMP(dirInfo, Configuration.physicalDevice);
    //                mLog.Debug("Start Backup folder name: {0}.", entity.FullPath);
    //                entity.PermissionScopeId = parent.PermissionScopeId;
    //                AveSender.BackupFSFolderHeader(dirInfo, entity, Configuration.UNCPath, ruleName, subJobid, mediaName, AveSender.BackupStream.StreamTransfered);
    //                //current.FileHeader = AveSender.BackupHeader(dto.FullPath);
    //                //current.PermissionScopeId = dto.PermissionScopeId;
    //                var stream = AveSender.BackupStream;
    //                stream.BeginWriteMetadata();
    //                try
    //                {
    //                    mLog.Debug("Start Export folder Base Info,name: {0}.", dto.FullPath);
    //                    aveFolder.ExportBaseFolderInfo(stream);
    //                    mLog.Debug("Start Export folder Full Text Index,folder Url: {0}.", dto.FullPath);
    //                    Dictionary<string, object> fullText = new Dictionary<string, object>();
    //                    foreach (var tag in Configuration.tagInfoCollection)
    //                    {
    //                        try
    //                        {
    //                            fullText[tag.Key] = tag.Value;
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            mLog.Warn("Get folder full text failed. error:{0}", ex);
    //                        }
    //                    }
    //                    aveFolder.ExportFullTextIndex(stream, fullText);
    //                    mLog.Debug("Start Export folder permission metadata : {0}", dto.FullPath);
    //                    aveFolder.ExportFSFolderPermission(stream);
    //                }
    //                catch (Exception e)
    //                {
    //                    mLog.Error("Error in backup Folder metadata, reason : {0}", e.ToString());
    //                    status = BackupRestoreStatus.Failed;
    //                    throw;
    //                }
    //                finally
    //                {
    //                    AveSender.BackupStream.EndWriteMetadata();
    //                    AveSender.BackupStream.FlushMetadata(0);
    //                    AveSender.BackupTail(aveFolder.GetTailInfo(), status == BackupRestoreStatus.Succeed);
    //                    current.BackupStatus = FileHeaderStatus.Complete;
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                mLog.Error("Backup folder Error: {0}", e.ToString());
    //                errorMessage = SOArchiverInternationalString.StorageOptimization_FSABackUpFolderExceptionMessage;
    //                status = BackupRestoreStatus.Failed;
    //                current.BackupStatus = FileHeaderStatus.Failed;
    //                throw;
    //            }
    //            finally
    //            {
    //                mLog.Debug("Start sending the job progress...Archiver level: {0}", entity.ArchiveLevel.ToString());
    //                JobReportInfo reportInfo = new JobReportInfo() { jobReportType = ArchiveJobReportType.FSArchiver, SourceURL = entity.Path.Trim('/') + "/" + entity.FullPath.TrimEnd('\\'), status = (int)status, cacheNodeType = entity.CacheNodeType, jobID = Configuration.JobId, dataSize = AveSender.BackupStream.StreamTransfered, errorMessage = errorMessage, ruleName = ruleName, mediaName = mediaName };
    //                Configuration.JobReportDto.AddReport(reportInfo);
    //                current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
    //            }
    //        }
    //        return 0;
    //    }
    //}


    internal class FSDocumentBackup : FSObjectBackup
    {
        private AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Dictionary<string,FSBackupSender> AveSender { get; set; }
        private IXSystem mDevice;
        private IReportService<JMJobDetails> mJobDetailService;
        private bool mLeaveStub;
        public FSDocumentBackup(IXSystem device, IReportService<JMJobDetails> JobDetailService)
        {
            mDevice = device;
            mJobDetailService = JobDetailService;
            //mLeaveStub = true;
        }
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        public override int Backup(FSAzureTableEntityDto dto,FileSystemRecordDto record = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("FSDocumentDisposal.FSDocumentBackup"))
            {
                JobDetailsStatus status = JobDetailsStatus.Successful;
                mLog.Debug("Start Backup FSDocument,Document Path: {0}.", dto.FullPath.LogBase64());
                string errorMessage = string.Empty;
                FileAtrributeInfo fileatrrinfo = new FileAtrributeInfo();
                Dictionary<string, string> cgTags = new Dictionary<string, string>();
                string comment = string.Empty;
                bool skipAddReport = false;
                try
                {
                    InitFSBackupSender(dto.RuleId, dto);
                    StorageInfo storageInfo = new StorageInfo(dto.HighName, dto.LowName);
                    XFileInfo fileInfo = mDevice.OpenFile(storageInfo);
                    //current.StorageObject = fileInfo;// just backup the path info and other settings.
                    FSItemBackup aveFile = new FSItemBackup(fileInfo, storageInfo, mDevice);
                    mLog.Debug("Start Backup document name: {0}.", dto.FullPath.LogBase64());
                    //entity.PermissionScopeId = parent.PermissionScopeId;
                    AveSender[dto.RuleId].BackupFSDocumentHeader(fileInfo, dto, mDevice.SystemLocation, fileInfo.FileSize, dto.RuleId);
                    AveSender[dto.RuleId].BackupHeader();
                    //current.PermissionScopeId = dto.PermissionScopeId;
                    var stream = AveSender[dto.RuleId].BackupStream;
                    stream.BeginWriteMetadata();
                    try
                    {
                        mLog.Debug("Start Export document Base Info,name: {0}.", dto.FullPath.LogBase64());
                        aveFile.ExportBaseFileInfo(stream);
                        mLog.Debug("Start Export document file permission,document Url: {0}.", dto.FullPath.LogBase64());
                        aveFile.ExportFSFilePermission(stream);
                        stream.EndWriteMetadata();
                        mLog.Debug("Start Export document content,document Url: {0}.", dto.FullPath.LogBase64());
                        aveFile.ExportContent(stream);
                        AveSender[dto.RuleId].CacheSecondHeader(SerializerHelper.SerializeByDataContractSerializer(dto));
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Error in backup file metadata and content, reason : {0}", e.ToString());
                        status = JobDetailsStatus.Failed;
                        throw;
                    }
                    finally
                    {
                        AveSender[dto.RuleId].BackupStream.EndWriteMetadata();
                        AveSender[dto.RuleId].BackupStream.FlushMetadata(0);
                        AveSender[dto.RuleId].BackupTail(aveFile.GetTailInfo(cgTags), status == JobDetailsStatus.Successful);
                        //current.BackupStatus = FileHeaderStatus.Complete;
                    }
                }
                catch (DataDeviceNotSurpportException e)
                {
                    mLog.Error("Error in DataDeviceNotSurpportException, reason : {0}", e.ToString());
                    status = JobDetailsStatus.Failed;
                    comment = e.Message;
                    throw;
                }
                catch (IndexDeviceNotSurpportException e)
                {
                    mLog.Error("Error in IndexDeviceNotSurpportException, reason : {0}", e.ToString());
                    status = JobDetailsStatus.Failed;
                    skipAddReport = true;
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("Backup document Error: {0}", e.ToString());
                    //errorMessage = SOArchiverInternationalString.StorageOptimization_FSABackUpDocumentExceptionMessage;
                    status = JobDetailsStatus.Failed;
                    //current.BackupStatus = FileHeaderStatus.Failed;
                    throw;
                }
                finally
                {
                    if (!skipAddReport)
                    {
                        AddReport(dto, status, "RM_FS_DisposalAction_ArchiveAndRemove", comment);
                        if (status == JobDetailsStatus.Successful)
                        {
                            dto.RecordStatus = (int)RMRecordStatus.Destroyed;
                        }
                    }
                }
            }
            return 0;
        }
        private void AddReport(FSAzureTableEntityDto dto, JobDetailsStatus status, string ruleAction, string comment = null)
        {
            var detail = JobContext.Current.EnableFSHighPerformanceMode
                ? new JMFSDisposalJobDetailV2
                {
                    Depth = dto.Depth,
                    DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName),
                    DetailAction = string.Equals("RM_FS_DisposalAction_ArchiveAndRemove", ruleAction) ? (int)DetailAction.ArchiveAndMove : (int)DetailAction.Destroy,
                }
                : new JMFSDisposalJobDetails();

            detail.ObjectName = dto.LowName;
            detail.SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName, dto.LowName);
            detail.Size = dto.Size.ToString();
            detail.FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow);
            detail.Action = ruleAction;
            detail.RuleName = FSJobCache.Instance.Rules[new Guid(dto.RuleId)].Name;
            //DetailTab = DetailTab.Deletion.ToString(),
            detail.Status = status;
            detail.Comment = comment;
            detail.Type = "RM_JS_Rule_ObjectLevel_FSFile";
            detail.AgentName = OSInformation.HostName;

            mJobDetailService.Commit(detail);
        }
        private void InitFSBackupSender(string ruleId, FSAzureTableEntityDto dto)
        {
            if (AveSender == null)
            {
                AveSender = new Dictionary<string, FSBackupSender>();
            }
            if (!AveSender.ContainsKey(ruleId))
            {
                IFSBackupDataWriter dataWriter = new FSBackupDataWriter();
                FSArchiverBackupRequest aRequest = new FSArchiverBackupRequest();
                aRequest.RuleId = ruleId;
                aRequest.SourceFlag = (int)SourceFlag.FileSystem;
                aRequest.JobId = GenerageSubJobId(TenantAgentInfo.JobId);//subJobId;
                var currentRule = FSJobCache.Instance.Rules[new Guid(ruleId)].FSRule;

                var indexDeviceDto = HybridApiClient.Instance.GetIndexDevice();
                if (indexDeviceDto.Type == 14 || indexDeviceDto.Type == 407)//google device
                {
                    WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                    throw new IndexDeviceNotSurpportException("RM_FS_Backup_NotSurpportSpecialStorage");
                }
                StorageDeviceDto storage = HybridApiClient.Instance.GetStorageDeviceById(currentRule.StoragePolicyId);
                if(storage.Type == 14 || storage.Type == 407)//google device
                {
                    WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException = true;
                    throw new DataDeviceNotSurpportException("RM_FS_Backup_NotSurpportSpecialStorage");
                }
                aRequest.UseSnapLock = currentRule.UseSnapLock;
                //aRequest.UseArchiverTier = FSJobCache.Instance.Rules[new Guid(ruleId)].FSRule.IsArchivedTier;
                aRequest.StoragePolicyId = storage.Id;
                aRequest.AchiverTime = DateTime.UtcNow.Ticks;
                //set RetentionTimeSpan
                //if (storage.RetentionOption != null && storage.RetentionOption.StorageType == StoragePolicyType.ArchiveType && storage.RetentionOption.ArchiveRetentionRules != null && storage.RetentionOption.ArchiveRetentionRules.Count > 0)
                //{
                //    ArchiveRetentionRule retentionRule = storage.RetentionOption.ArchiveRetentionRules[0];
                //    long keepValue = (long)retentionRule.KeepValue;
                //    switch (retentionRule.ArchiveDateUnit)
                //    {
                //        case DateUnit.Month:
                //            {
                //                TimeSpan resultTime = DateTime.Now.AddMonths((int)keepValue).Subtract(DateTime.Now);
                //                keepValue = resultTime.Days;
                //                break;
                //            }
                //        case DateUnit.Week:
                //            {
                //                keepValue = keepValue * 7;
                //                break;
                //            }
                //        default: break;
                //    }
                //    aRequest.RetentionTimeSpanSeconds = keepValue * 24 * 3600;
                //}
                //else
                //{
                //    //when no retention rule ,we give RetentionTimeSpanSeconds = -1 
                //    aRequest.RetentionTimeSpanSeconds = -1;
                //}

                aRequest.LogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storage);
                aRequest.IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);
                aRequest.CompressionType = currentRule.ArchiverCompressionType;
                aRequest.EncryptionMethods = currentRule.EncryptionMethods;
                aRequest.DataSecurity = currentRule.ArchiverDataSecurity;
                aRequest.DataEncryptionInfoWrapper = currentRule.DataEncryptionInfoWrapper;                                                            //}

                //if (mConfiguration.currentRule.DataEncryptionInfoWrapper != null)
                //{
                //    aRequest.EncryptionInfo = mConfiguration.currentRule.DataEncryptionInfoWrapper.EncryptionInfo;
                //    DataEncryptionInfoManager.PutEncryptionInfo(mConfiguration.currentRule.DataEncryptionInfoWrapper.EncryptionInfo, mConfiguration.currentRule.DataEncryptionInfoWrapper.DynamicKey);
                //}
                //else
                //{
                    aRequest.EncryptionInfo = DataEncryptionInfoManager.DefaultEncryptionInfo;
                //}
                mLog.Info("ArchiverBackupRequest EncryptionInfo is:{0}.", aRequest.EncryptionInfo == null ? string.Empty : aRequest.EncryptionInfo.ToString().LogBase64());
                //string backupRequestXml = MediaTCPRequestSerializerHelper.Serialize(aRequest);
                //TODO:Need remove or modified by ManagerSide
                aRequest.ArchiverSiteInfoDto = new ArchiverSiteInfoDto()
                {
                    FarmName = "",
                    WebApplicationUrl = dto.InternalConnectionId.ToString(),
                    NewWebApplicationUrl = dto.InternalConnectionId.ToString(),
                    //WebApplicationId = mConfiguration.WebAppId,
                    ConnectionId = dto.InternalConnectionId.ToString(),
                    ConnectionName = dto.InternalConnectionId.ToString(),
                    NewSiteUrl = dto.InternalConnectionId.ToString(),
                };
                dataWriter.Open(ConvertBackupRequestToJob(aRequest));
                AveSender[ruleId] = new FSBackupSender(dataWriter);
            }
        }
        private string GenerageSubJobId(string parentJobId)
        {
            BackgroundSettings.subJobNumber++;
            if (BackgroundSettings.subJobNumber >= 1000)
            {
                return string.Format("{0}_{1:D4}", parentJobId, BackgroundSettings.subJobNumber);
            }
            else
            {
                return string.Format("{0}_{1:D3}", parentJobId, BackgroundSettings.subJobNumber);
            }
        }
        private FSArchiverBackupJob ConvertBackupRequestToJob(FSArchiverBackupRequest aRequest)
        {
            FSArchiverBackupJob archiverBackupJob = new FSArchiverBackupJob(aRequest);
            archiverBackupJob.OutFileLevelBlock = true;
            archiverBackupJob.CacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = string.Empty,
                UserName = string.Empty,
                Usage = null
            };
            archiverBackupJob.CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            archiverBackupJob.CacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            return archiverBackupJob;
        }
        public static T Deserialize<T>(string jsonString)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(ms);
            }
        }

        public override void ClearBackupSender()
        {
            foreach (var sender in AveSender)
            {
                sender.Value.CacheSecondHeader("End");
                sender.Value.FileSender.Close();
            }
        }
        public override void MergeIndex()
        {
            //state = JobState.Finished;
            foreach (var sender in AveSender)
            {
                try
                {
                    using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.MergeIndex", addToStatistics: true))
                    {
                        ArchiverMergeIndexJobHandler mergeIndexHandler = new ArchiverMergeIndexJobHandler();
                        mergeIndexHandler.PerformMergeIndexSubJob(GenerateMergeIndexJobInfo(sender.Value), sender.Value.jobId);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(string.Format("Failed to merge current index. Message:{0}", ex.ToString()));
                    //state = JobState.Failed;
                }
                finally
                {
                    try
                    {
                        //mDevice.DeleteDirectory(new StorageInfo() { HighName = CACHEFOLDERNAME });
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(string.Format("Failed to delete cache folder. Message:{0}", ex.ToString()));
                    }
                    //UpdateJobStatus(jobId, state);
                }
            }
            //return state;
        }
        private MergeIndexJobInfo GenerateMergeIndexJobInfo(FSBackupSender sender)
        {
            MergeIndexJobInfo result = new MergeIndexJobInfo();
            var indexDevice = HybridApiClient.Instance.GetIndexDevice();
            result.IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);
            result.ConnectionId = sender.ConnectionId;
            result.ConnectionName = sender.ConnectionName;
            result.JobDto = new BaseJobDto();
            return result;
        }
        public override void RemoveArchivedFiles()
        {
            foreach (var sender in AveSender)
            {
                sender.Value.SendSecondHeaders(RealDeleteDocument);
            }
        }
        
        private bool RemoveReadOnlyAttribute(StorageInfo info)
        {
            bool removed = false;
            using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.RemoveReadOnlyAttribute", addToStatistics: true))
            {
                try
                {
                    XFileInfo file = mDevice.OpenFile(info);
                    if (file is AvePoint.Media.Storage.FS.FSFileInfo)
                    {
                        AvePoint.Media.Storage.FS.FSFileInfo sFileInfo = file as AvePoint.Media.Storage.FS.FSFileInfo;
                        var attribute = sFileInfo.Attribute;
                        if ((attribute & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            sFileInfo.Attribute = attribute & ~FileAttributes.ReadOnly;
                            //File.SetAttributes(file.FileFullPath, attribute & ~FileAttributes.ReadOnly);
                            removed = true;
                            mLog.Debug($"Remove readonly attribute success.");
                        }
                    }
                    else if (file is AvePoint.Media.Storage.FS.AlphaFSFileInfo)
                    {
                        AvePoint.Media.Storage.FS.AlphaFSFileInfo alphaFSFile = file as AvePoint.Media.Storage.FS.AlphaFSFileInfo;
                        var attribute = alphaFSFile.Attribute;
                        if ((attribute & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            alphaFSFile.Attribute = attribute & ~FileAttributes.ReadOnly;
                            //File.SetAttributes(file.FileFullPath, attribute & ~FileAttributes.ReadOnly);
                            removed = true;
                            mLog.Debug($"Remove readonly attribute success.");
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn($"Error occurred while removing readonly attribute. Error:{e.ToString()}");
                }
            }
            return removed;
        }
        private void RealDeleteDocument(FSAzureTableEntityDto dto)
        {
            StorageInfo info = new StorageInfo(dto.HighName, dto.LowName);
            if (info.LastWriteTimeUtc.Ticks < dto.ScanTime.Ticks)
            {
                bool creatStubSuccessful = false;
                string stubName = string.Empty;
                if (mLeaveStub)
                {
                    using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.LeaveStub", addToStatistics: true))
                    {
                        stubName = dto.LowName + "." + JobContext.Current.FSStubNameFormat;
                        StorageInfo stubFile = new StorageInfo(dto.HighName, stubName);
                        string stubPath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config\FSArchiverStub.html");
                        FileStream fs = new FileStream(stubPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        mDevice.CommitStream(fs, stubFile);
                        creatStubSuccessful = true;
                    }
                }
                try
                {
                    using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.DeleteFile", addToStatistics: true))
                    {
                        try
                        {
                            var result = mDevice.DeleteFile(info);
                            if (!result.IsDeleted)
                            {
                                mLog.Warn($"Delete file failed. Name:{info.HighPlusLowName.LogBase64()}");
                                if (result.IsUnauthorizedAccessException)
                                {
                                    throw new UnauthorizedAccessException(result.Message);
                                }
                                throw new Exception(string.IsNullOrWhiteSpace(result.Message) ? "" : result.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            var exceptionType = e.GetType()?.FullName;
                            if (!string.IsNullOrWhiteSpace(exceptionType) && (exceptionType.Equals("System.UnauthorizedAccessException") || exceptionType.Contains("FileReadOnlyException")))
                            {
                                mLog.Warn($"Delete file failed, try to remove readonly attribute.");
                                if (RemoveReadOnlyAttribute(info))
                                {
                                    var result = mDevice.DeleteFile(info);
                                    if (!result.IsDeleted)
                                    {
                                        mLog.Warn($"Delete file failed. Name:{info.HighPlusLowName.LogBase64()}");
                                        throw new Exception(string.IsNullOrWhiteSpace(result.Message) ? "" : result.Message);
                                    }
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    AddReport(dto, JobDetailsStatus.Successful, mLeaveStub ? "RM_FS_DisposalAction_LeaveStub" : "RM_FS_DisposalAction_Remove");
                }
                catch (Exception e)
                {
                    if (creatStubSuccessful)
                    {
                        StorageInfo stubInfo = new StorageInfo(dto.HighName, stubName);
                        mDevice.DeleteFile(stubInfo);
                        mLog.Debug("Delete the stub successful : " + stubInfo.LowName.LogBase64());
                    }
                    throw;
                }
            }
            else
            {
                mLog.Warn("File has been modified after scan, skip delete the file : " + info.LowName.LogBase64());
            }
        }
    }
}


