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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.Object;

namespace RAFileSystem.FileSystem.Backup
{
    internal class FSDocumentBackupImps : FSObjectBackup
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IXSystem mDevice;
        private bool mLeaveStub;
        private bool mIncludeRelated;
        private IReportService<JMJobDetails> mJobDetailService;
        public FSDocumentBackupImps(IXSystem device, bool leaveStub, bool includeRelated, IReportService<JMJobDetails> JobDetailService)
        {
            mDevice = device;
            mLeaveStub = leaveStub;
            mIncludeRelated = includeRelated;
            mJobDetailService = JobDetailService;
        }
        public override int Backup(FSAzureTableEntityDto dto,FileSystemRecordDto record=null)
        {
            int returnValue = (int)BackupRestoreStatus.Succeed;
            try
            {
                RealDeleteDocument(dto);
                dto.RecordStatus = (int)RMRecordStatus.Destroyed;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while deleting file. Path:{dto.HighName.LogBase64() + "/" + dto.LowName.LogBase64()} Error:{e.ToString()}");
                AddReport(dto, JobDetailsStatus.Failed, mLeaveStub ? "RM_FS_DisposalAction_LeaveStub" : "RM_FS_DisposalAction_Remove", e.Message);
                returnValue = (int)BackupRestoreStatus.Failed;
            }
            return returnValue;
        }

        private void AddReport(FSAzureTableEntityDto dto, JobDetailsStatus status, string ruleAction, string comment = null)
        {
            var detail = JobContext.Current.EnableFSHighPerformanceMode
                ? new JMFSDisposalJobDetailV2
                {
                    Depth = dto.Depth,
                    DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName),
                    DetailAction = (int)DetailAction.Destroy,
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
                                logger.Warn($"Delete file failed. Name:{info.HighPlusLowName.LogBase64()}");
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
                                logger.Warn($"Delete file failed, try to remove readonly attribute.");
                                if (RemoveReadOnlyAttribute(info))
                                {
                                    var result = mDevice.DeleteFile(info);
                                    if (!result.IsDeleted)
                                    {
                                        logger.Warn($"Delete file failed. Name:{info.HighPlusLowName.LogBase64()}");
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
                        logger.Debug("Delete the stub successful : " + stubInfo.LowName.LogBase64());
                    }
                    throw;
                }
            }
            else
            {
                logger.Warn("File has been modified after scan, skip delete the file : " + info.LowName.LogBase64());
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
                            logger.Debug($"Remove readonly attribute success.");
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
                            logger.Debug($"Remove readonly attribute success.");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while removing readonly attribute. Error:{e.ToString()}");
                }
            }
            return removed;
        }

        private string ConvertToFormatSize(long size)
        {
            int _GB = 1024 * 1024 * 1024;
            int _MB = 1024 * 1024;
            int _KB = 1024;
            var result = string.Empty;
            var displayResult = @"{0} ({1} bytes)";
            if (size / _GB >= 1)
            {
                result = string.Format(displayResult, Math.Round(size / (float)_GB, 2) + " GB", size.ToString("N0"));
            }
            else if (size / _MB >= 1)
            {
                result = string.Format(displayResult, Math.Round(size / (float)_MB, 2) + " MB", size.ToString("N0"));
            }
            else if (size / _KB >= 1)
            {
                result = string.Format(displayResult, Math.Round(size / (float)_KB, 2) + " KB", size.ToString("N0"));
            }
            else
            {
                result = size + " bytes";
            }
            return result;
        }

        private string GetActionString(int action)
        {
            string actionStr = string.Empty;
            switch (action)
            {
                case 1:
                    actionStr = "RM_FS_DisposalAction_Remove";
                    break;
                case 3:
                    actionStr = "RM_FS_DisposalAction_Move";
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
            logger.Info("this action no need to remove archived files");
        }

        public override void MergeIndex()
        {
            //throw new NotImplementedException();
        }
    }
}
