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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Protobuf;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using Newtonsoft.Json;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Common.Utils;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using RAFileSystem.Disposal.NewLogic;
using AvePoint.GCommon.Contract.Tree.Object;

namespace RAFileSystem.FileSystem.Backup
{
    internal class FSToFSDocumentMoveTo : FSObjectBackup
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IXSystem mSourceDevice;
        private MoveOption mMoveOption;
        private IReportService<JMJobDetails> mJobDetailService;
        private string mDestinationPath;
        public FSToFSDocumentMoveTo(IXSystem sourceDevice, MoveOption moveOption, IReportService<JMJobDetails> JobDetailService)
        {
            mSourceDevice = sourceDevice;
            mMoveOption = moveOption;
            mJobDetailService = JobDetailService;
            GetDestinationPath(mMoveOption);
        }
        public override int Backup(FSAzureTableEntityDto dto, FileSystemRecordDto record = null)
        {
            int returnValue = (int)BackupRestoreStatus.Succeed;
            try
            {
                var connecionInfo = GetAvailableConnectionInfo();

                StorageInfo sourceFileInfo = new StorageInfo(dto.HighName, dto.LowName);
                if (string.IsNullOrWhiteSpace(connecionInfo.Key))
                {
                    logger.Warn("Destination is not registered. File Scope id:{0}", dto?.ScopeID);
                    AddReport(dto, JobDetailsStatus.Failed, "RM_JM_FSDestinationNotRegistered");
                    return (int)BackupRestoreStatus.Failed;
                }
                XFileInfo fileInfo = mSourceDevice.OpenFile(sourceFileInfo);
                if (FSJobCache.Instance.PropertiesMapping != null
                       && FSJobCache.Instance.PropertiesMapping.CommonMapping != null
                       && FSJobCache.Instance.PropertiesMapping.CommonMapping.LengthItem != null
                       && (FSJobCache.Instance.PropertiesMapping.CommonMapping.LengthItem.IsCheckedMaxForlderName && fileInfo.Parent != null && fileInfo.Parent.LowName.Length > FSJobCache.Instance.PropertiesMapping.CommonMapping.LengthItem.MaxForlderNameLength
                       || FSJobCache.Instance.PropertiesMapping.CommonMapping.LengthItem.IsCheckedMaxFileName && fileInfo.Name.Length > FSJobCache.Instance.PropertiesMapping.CommonMapping.LengthItem.MaxFileNameLength)
                       )
                {
                    AddReport(dto, JobDetailsStatus.Failed, "RM_JM_FSMoveToFilePathTooLong");
                    FSJobCache.Instance.FailedCount++;
                    return (int)BackupRestoreStatus.Failed;
                }
                DateTime fileCreateTime = fileInfo.CreationTime;
                DateTime fileCreateTimeUTC = fileInfo.CreationTimeUtc;
                DateTime fileModifiedTime = fileInfo.LastWriteTime;
                IdentityReference fileOwnerSid = null;
                try
                {
                    File.GetAccessControl(fileInfo.FileFullPath).GetOwner(typeof(SecurityIdentifier));
                }
                catch (Exception e)
                {
                    logger.Warn($"GetAccessControl failed. Source File Path:{fileInfo.FileFullPath.LogBase64()} Error:{e.ToString()}");
                }

                var fileUniqueADSStr = AdsHelper.ReadUniqueIdAds(fileInfo.FileFullPath);
                var fileTermADSStr = AdsHelper.ReadTermIdAds(fileInfo.FileFullPath);
                IXSystem destinationDevice = ExternalUtil.OpenXSystem(mDestinationPath);
                //StorageInfo desFolderInfo = new StorageInfo(dto.HighName, "");

                //create folder in destination
                //XDirectoryInfo desDirInfo = destinationDevice.OpenDirectory(desFolderInfo, System.IO.FileMode.OpenOrCreate);
                StorageInfo destinationFileInfo = new StorageInfo("", dto.LowName);
                XFileInfo desFile = destinationDevice.OpenFile(destinationFileInfo);
                if (desFile != null && desFile.Exists)
                {
                    if (mMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                    {
                        AddReport(dto, JobDetailsStatus.Skipped, "RM_JM_FSMoveToSkip");
                        return (int)BackupRestoreStatus.Skipped;
                    }
                    else if (mMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                    {
                        logger.Debug("FSToFSDocumentMoveTo file exist and ContentConflictResolution is Overwrite.FileInfo:{0}.", dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64());
                        //system.DeleteFile(desFileInfo); Default move action is overwrite.
                        using (var pc1 = new AgentPerformanceScope("FSDocumentMoveTo.MoveFile", addToStatistics: true))
                        {
                            mSourceDevice.MoveFile(sourceFileInfo, destinationDevice, destinationFileInfo);
                            logger.Debug("FSToFSDocumentMoveTo file exist and Overwrite file success.FileInfo:{0}.", dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64());
                        }
                        desFile = destinationDevice.OpenFile(destinationFileInfo);
                        UpdateXFileProperty(desFile, fileCreateTime, fileModifiedTime, fileOwnerSid);
                        logger.Debug("FSToFSDocumentMoveTo file exist and UpdateXFileProperty success.FileInfo:{0}.", dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64());
                        AddReport(dto, JobDetailsStatus.Successful, null, Path.Combine(mDestinationPath, dto.LowName));
                    }
                    else if (mMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                    {
                        logger.Debug("FSToFSDocumentMoveTo file exist and ContentConflictResolution is Overwrite.Append:{0}.", dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64());
                        string resetName = ResetNewName(dto.LowName, destinationDevice, "");
                        destinationFileInfo = new StorageInfo("", resetName);
                        mSourceDevice.MoveFile(sourceFileInfo, destinationDevice, destinationFileInfo);
                        logger.Debug("FSToFSDocumentMoveTo file exist and Append file success.FileInfo:{0}.", resetName.LogBase64());
                        desFile = destinationDevice.OpenFile(destinationFileInfo);
                        UpdateXFileProperty(desFile, fileCreateTime, fileModifiedTime, fileOwnerSid);
                        logger.Debug("FSToFSDocumentMoveTo file exist and UpdateXFileProperty success.FileInfo:{0}.", resetName.LogBase64());
                        AddReport(dto, JobDetailsStatus.Successful, null, Path.Combine(mDestinationPath, resetName));
                    }
                    else
                    {
                        throw new Exception("invalid ConflictOption.");
                    }
                }
                else
                {
                    mSourceDevice.MoveFile(sourceFileInfo, destinationDevice, destinationFileInfo);
                    desFile = destinationDevice.OpenFile(destinationFileInfo);
                    logger.Debug("FSToFSDocumentMoveTo success.FileInfo:{0}.", dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64());
                    UpdateXFileProperty(desFile, fileCreateTime, fileModifiedTime, fileOwnerSid);
                    logger.Debug("FSToFSDocumentMoveTo UpdateXFileProperty success.FileInfo:{0}.", dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64());
                    AddReport(dto, JobDetailsStatus.Successful, null, Path.Combine(mDestinationPath, dto.LowName));
                }
                dto.RecordStatus = (int)AvePoint.RA.Contract.Explorer.RMRecordStatus.Moved;
                var desFileADSStr = AdsHelper.ReadUniqueIdAds(desFile.FileFullPath);
                var destDto = GetDesFileRecordDto(desFile, destinationDevice, connecionInfo, dto, record);
                if (!string.IsNullOrEmpty(desFileADSStr))
                {
                    try
                    {
                        var desFileADS = JsonConvert.DeserializeObject<FileSystemADSUniqueInfo>(desFileADSStr);
                        destDto.RecordsId = desFileADS.UniqueId;
                    }
                    catch(Exception e)
                    {
                        logger.Error($"An error occurred while open the des file ads, Path:{dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64()} Error:{e}");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(fileUniqueADSStr))
                    {
                        try
                        {
                            var fileUniqueADS = JsonConvert.DeserializeObject<FileSystemADSUniqueInfo>(fileUniqueADSStr);
                            var fileTermADS = JsonConvert.DeserializeObject<FileSystemADSTermInfo>(fileTermADSStr);
                            AdsHelper.WriteUniqueIdAdsAndRevertTime(desFile.FileFullPath, fileUniqueADS);
                            AdsHelper.WriteTermIdAdsAndRevertTime(desFile.FileFullPath, fileTermADS);
                            destDto.RecordsId = fileUniqueADS.UniqueId;
                        }
                        catch(Exception e)
                        {
                            logger.Error($"An error occurred while set the des file ads, Path:{dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64()} Error:{e}");
                        }
                    }
                }
                destDto.TimeCreated1 = fileCreateTimeUTC;
                if (JobContext.Current.EnableFSHighPerformanceMode)
                {
                    FSJobCache.Instance.WorkerToUpdater.Writer.WriteAsync((dto, destDto)).GetAwaiter().GetResult();
                }
                else
                {
                    FSJobCache.Instance.DisposalMoveToCache.Add(destDto);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred whilemoving file to destination. Path:{dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64()} Error:{e.ToString()}");
                string message = e.Message.EndsWith("\r\n") ? e.Message.Substring(0, e.Message.Length - 2) : e.Message;
                message = message + " " + AvePoint.RA.I18N.Core.I18NEntity.GetString("RM_JS_JMD_Grid_DestinationPath") + ": " + mDestinationPath;
                AddReport(dto, JobDetailsStatus.Failed, message);
                returnValue = (int)BackupRestoreStatus.Failed;
            }
            return returnValue;
        }

        private KeyValuePair<string, Guid> GetAvailableConnectionInfo()
        {
            var connectionPaths = FSJobCache.Instance.ConnectionCache;
            foreach (var connection in connectionPaths)
            {
                if (mDestinationPath.ToLowerInvariant().StartsWith(connection.Key))
                {
                    return connection;
                }
            }
            return new KeyValuePair<string, Guid>();
        }

        private AvePoint.RA.Contract.Explorer.FileSystemRecordDto GetDesFileRecordDto(XFileInfo desFile, IXSystem system, KeyValuePair<string, Guid> desConnectionInfo, FSAzureTableEntityDto sourceDto, FileSystemRecordDto record = null)
        {
            AvePoint.RA.Contract.Explorer.FileSystemRecordDto desDto = new AvePoint.RA.Contract.Explorer.FileSystemRecordDto();
            try
            {
                desDto.SourceFlag = 2;
                desDto.ScopeId = desConnectionInfo.Key.ToLowerInvariant().ToMd5();
                desDto.AveSiteId = desConnectionInfo.Value.ToString();
                desDto.NodeId = system.XriObject["location"].Equals(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ? desFile.FileFullPath.TrimEnd('\\').ToLowerInvariant().ToMd5() : (system.XriObject["location"] + desFile.FileFullPath.TrimEnd('\\').Substring(desFile.FileFullPath.TrimEnd('\\').IndexOf(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) + system.SystemLocation.TrimEnd('\\').Length)).ToLowerInvariant().ToMd5();
                desDto.DirPath = system.XriObject["location"].Equals(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ? desFile.ParentFullName.TrimEnd('\\') : system.XriObject["location"] + desFile.ParentFullName.TrimEnd('\\').Substring(desFile.ParentFullName.TrimEnd('\\').IndexOf(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) + system.SystemLocation.TrimEnd('\\').Length);
                desDto.LeafName = desFile.Name;
                desDto.CreatedBy = desFile.Owner;
                desDto.WebId = Guid.Empty;
                desDto.ListId = Guid.Empty;
                desDto.NodeType = 2200;
                desDto.ExtensionForFile = Alphaleonis.Win32.Filesystem.Path.GetExtension(desFile.FileFullPath).TrimStart(new char[] { '.' });
                desDto.FolderId = system.XriObject["location"].Equals(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ? desFile.ParentFullName.TrimEnd('\\').ToLowerInvariant().ToMd5() : (system.XriObject["location"] + desFile.ParentFullName.TrimEnd('\\').Substring(desFile.ParentFullName.TrimEnd('\\').IndexOf(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) + system.SystemLocation.TrimEnd('\\').Length)).ToLowerInvariant().ToMd5();
                desDto.ItemId = system.XriObject["location"].Equals(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ? desFile.FileFullPath.TrimEnd('\\').ToLowerInvariant().ToMd5() : (system.XriObject["location"] + desFile.FileFullPath.TrimEnd('\\').Substring(desFile.FileFullPath.TrimEnd('\\').IndexOf(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) + system.SystemLocation.TrimEnd('\\').Length)).ToLowerInvariant().ToMd5();
                desDto.FullPath = system.XriObject["location"].Equals(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ? desFile.FileFullPath.TrimEnd('\\') : system.XriObject["location"] + desFile.FileFullPath.TrimEnd('\\').Substring(desFile.FileFullPath.TrimEnd('\\').IndexOf(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) + system.SystemLocation.TrimEnd('\\').Length);
                desDto.ParentId = system.XriObject["location"].Equals(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ? desFile.ParentFullName.TrimEnd('\\').ToLowerInvariant().ToMd5() : (system.XriObject["location"] + desFile.ParentFullName.TrimEnd('\\').Substring(desFile.ParentFullName.TrimEnd('\\').IndexOf(system.SystemLocation.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) + system.SystemLocation.TrimEnd('\\').Length)).ToLowerInvariant().ToMd5();
                desDto.ItemRowId = 0;
                desDto.RecordStatus = 1;
                AvePoint.RA.Contract.Explorer.RecordMetaInfo metaInfo = new AvePoint.RA.Contract.Explorer.RecordMetaInfo
                {
                    FileSize = desFile.FileSize,
                    LastAccessTime = desFile.LastAccessTimeUtc.Ticks,
                    Owner = desFile.Owner,
                    LocalFullPath = desFile.FileFullPath
                };
                desDto.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                desDto.TimeLastModified = desFile.LastWriteTimeUtc.Ticks;
                desDto.SortTicks = Snowflake.Instance().GetTicks();
                desDto.TermId = sourceDto.TermId;
                desDto.TermName = sourceDto.TermName;

                desDto.HoldStatus = sourceDto.HoldStatus;
                desDto.HoldReleaseTime = sourceDto.HoldReleaseTime;
                desDto.HoldBy = sourceDto.HoldBy;
                desDto.HoldId = sourceDto.HoldId;
                desDto.HoldType = sourceDto.HoldType;
                desDto.HoldByUsers = sourceDto.HoldByUsers;
                desDto.HoldUntilTimes = sourceDto.HoldUntilTimes;
                desDto.AppendHolds_Array = sourceDto.AppendHolds_Array;

                desDto.BulkImportEnabled = JobContext.Current.BulkImportEnabled;
                desDto.BulkSize = JobContext.Current.BulkSize;
                desDto.JPMCFSFileSize = desFile.FileSize;
                if (record != null)
                {
                    desDto.CountryCode = record.CountryCode;
                    desDto.ClassCode = record.ClassCode;
                    desDto.RetentionType = record.RetentionType;
                    desDto.StartDate = record.StartDate;
                    desDto.PolicyValueNumber = record.PolicyValueNumber;
                    desDto.PolicyValueUnit = record.PolicyValueUnit;
                    desDto.EndTime = record.EndTime;
                } 
            }
            catch (Exception exceptionInGetProperty)
            {
                logger.Warn(string.Format("Error in get records document properties, reason : {0}", exceptionInGetProperty.ToString()));
            }
            return desDto;
        }

        private void GetDestinationPath(MoveOption moveOption)
        {
            if (moveOption.MoveDestination.DestMode == DestMode.UrlMode)
            {
                mDestinationPath = moveOption.MoveDestination.FSPath;
            }
            else if (moveOption.MoveDestination.DestMode == DestMode.TreeMode)
            {
                mDestinationPath = moveOption.MoveDestination.FSTreeNode.FullPath;
            }
            else
            {
                throw new Exception("DestMode is invalid.");
            }
        }

        private string ResetNewName(string fileName, IXSystem system, string folderPath)
        {
            using (var pc = new AgentPerformanceScope("ResetNewName", addToStatistics: true))
            {
                string newFileName = string.Empty;
                string extension = string.Empty;
                string prevName = string.Empty;
                int pos = fileName.LastIndexOf('.');
                if (pos > 0)
                {
                    extension = fileName.Substring(pos, fileName.Length - pos);
                    prevName = fileName.Substring(0, pos);
                }
                for (int i = 1; i <= 1000; ++i)
                {
                    StringBuilder temp = new StringBuilder(prevName);
                    temp.Append("_");
                    temp.Append(i.ToString());
                    temp.Append(extension);
                    StorageInfo desFileInfo = new StorageInfo(folderPath, temp.ToString());
                    if (system.OpenFile(desFileInfo) == null || !system.OpenFile(desFileInfo).Exists)
                    {
                        newFileName = temp.ToString();
                        break;
                    }
                }
                return newFileName;
            }
        }

        private void UpdateXFileProperty(XFileInfo desFile, DateTime sourceCreationTime, DateTime sourceLastWriteTime, IdentityReference sourceFileOwnerSid)
        {
            using (var pc = new AgentPerformanceScope("UpdateXFileProperty", addToStatistics: true))
            {
                try
                {
                    desFile.CreationTime = sourceCreationTime;
                    FileSecurity fs = new FileSecurity();
                    fs.SetOwner(sourceFileOwnerSid);
                    File.SetAccessControl(desFile.FileFullPath, fs);
                }
                catch (Exception ex)
                {
                    logger.Info("UpdateXFileProperty Failed.Message:{0}.", ex.ToString());
                }
            }
        }

        private void AddReport(FSAzureTableEntityDto dto, JobDetailsStatus status, string comment = null, string destLocation = null)
        {
            var detail = JobContext.Current.EnableFSHighPerformanceMode
                ? new JMFSDisposalJobDetailV2
                {
                    Depth = dto.Depth,
                    DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName),
                    DetailAction = (int)DetailAction.ArchiveAndMove,
                }
                : new JMFSDisposalJobDetails();

            detail.ObjectName = dto.LowName;
            detail.SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName, dto.LowName);
            detail.DestinationLocation = destLocation;
            detail.Size = dto.Size.ToString();
            detail.FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow);
            detail.Action = "RM_FS_DisposalAction_Move";
            detail.RuleName = FSJobCache.Instance.Rules[new Guid(dto.RuleId)].Name;
            //DetailTab = DetailTab.Deletion.ToString(),
            detail.Status = status;
            detail.Comment = comment;
            detail.Type = "RM_JS_Rule_ObjectLevel_FSFile";
            detail.AgentName = OSInformation.HostName;

            mJobDetailService.Commit(detail);
        }

        //private string ConvertToFormatSize(long size)
        //{
        //    int _GB = 1024 * 1024 * 1024;
        //    int _MB = 1024 * 1024;
        //    int _KB = 1024;
        //    var result = string.Empty;
        //    var displayResult = @"{0} ({1} bytes)";
        //    if (size / _GB >= 1)
        //    {
        //        result = string.Format(displayResult, Math.Round(size / (float)_GB, 2) + " GB", size.ToString("N0"));
        //    }
        //    else if (size / _MB >= 1)
        //    {
        //        result = string.Format(displayResult, Math.Round(size / (float)_MB, 2) + " MB", size.ToString("N0"));
        //    }
        //    else if (size / _KB >= 1)
        //    {
        //        result = string.Format(displayResult, Math.Round(size / (float)_KB, 2) + " KB", size.ToString("N0"));
        //    }
        //    else
        //    {
        //        result = size + " bytes";
        //    }
        //    return result;
        //}





        private string GetActionString(int action)
        {
            string actionStr = string.Empty;
            switch (action)
            {
                case 1:
                    actionStr = ".Archive and remove from File System";
                    break;
                case 3:
                    actionStr = ".Moved to destination";
                    break;
            }
            return actionStr;
        }

        public override void ClearBackupSender()
        {
            logger.Info("this action no need to clear BackupSender");
        }

        public override void RemoveArchivedFiles()
        {
            logger.Info("this action no need to remove ArchivedFiles");
        }

        public override void MergeIndex()
        {
            //throw new NotImplementedException();
        }
    }
}
