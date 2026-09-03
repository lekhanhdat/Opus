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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Records.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FSActions
{
    public class FSApplyClassCodeProcesser
    {
        private const int ChildQueryPageSize = 200;
        private const int UpdateBatchSize = 50;
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(FSApplyClassCodeProcesser));
        private JobContext jobContext = null;
        private string JobId = string.Empty;
        private JobType mJobType;
        ScheduleConfiguration mConfiguration;
        private IExplorerDao explorerDao;
        private OlderThanTimeDto timerDto;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IFileSystemSettingDao FileSystemSettingDAO = PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private List<string> AllDisablePath;
        public FSApplyClassCodeProcesser(string jobId, JobType jobType)
        {
            JobId = jobId;
            mJobType = jobType;
            jobContext = JobContext.GetInstance(jobId, JobType.ApplyClassCode);
            jobContext.ReportManager.StartUpdateJobProgress();
            mConfiguration = new ScheduleConfiguration(JobId);
            mConfiguration.jobtype = jobType;
            explorerDao = new ExplorerDao();
        }
        public async Task RunNowAsync()
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    ApplyClassCodeSettingDto settingDto = GetApplyClassCodeSetting();
                    List<RMFSTreeNode> targetNodes = settingDto?.FSTreeNode ?? new List<RMFSTreeNode>();
                    _ = RMKeyValueDao.TryGetBoolValue("MakeApplyClassCodeJobFailed", out var isEnable);
                    if (isEnable)
                    {
                        jobContext.HasErrorNode = true;
                        mLog.Warn($"Apply class code failed. JobId:{JobId}");
                        return;
                    }
                    if (settingDto == null)
                        {
                            jobContext.HasErrorNode = true;
                        mLog.Warn($"Apply class code settings are empty. JobId:{JobId}");
                            return;
                        }

                    if (targetNodes.Count == 0)
                    {
                        jobContext.HasErrorNode = true;
                        mLog.Warn($"No target nodes found for apply class code. JobId:{JobId}");
                        return;
                    }

                    mLog.Info($"Start apply class code job. JobId:{JobId}, TargetNodeCount:{targetNodes.Count}, ApplyToExistingDoc:{settingDto.ApplyToExistingDoc}, ClassCode:{settingDto.ClassCode}, CountryCode:{settingDto.CountryCode}, RetentionType:{settingDto.RetentionType}, StartDate:{settingDto.StartDate}");
                        jobContext.ReportManager.IncreaseBase(targetNodes.Count);
                        timerDto = TaxonomyService.GetTheRetentionUnitByClassCode(settingDto);
                    if (timerDto == null)
                    {
                        mLog.Warn($"apply class code can not find any rule for caculate end time,will skip apply.");
                        return;
                    }
                    foreach (RMFSTreeNode targetNode in targetNodes)
                    {
                        CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();

                        AllDisablePath = FileSystemSettingDAO.GetAllDisableRecordManagementPath(targetNode.ConnGroupId);

                        if (AllDisablePath != null && AllDisablePath.Count > 0)
                        {
                            mLog.Info($"apply class code has disable node,count:{AllDisablePath.Count}");
                            foreach (var path in AllDisablePath)
                            {
                                mLog.Info($"apply class code has disable node,path:{path}");
                            }
                        }
                        await ProcessTargetNodeAsync(targetNode, settingDto);
                        jobContext.ReportManager.Increase();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is AvePoint.RA.Contract.Exceptions.JobStopException || ex is AvePoint.RA.Contract.Global.Exceptions.JobStopException)
                {
                    mLog.Error($"Apply class code job {JobId} was stopped by user.");
                    jobContext.JobHasStopped = true;
                }
                else
                {
                    jobContext.HasErrorNode = true;
                    mLog.Error($"An error occurred while applying class code. JobId:{JobId}, Error:{ex}");
                }
                
            }
            finally
            {
                jobContext.Finish();

            }
        }

        private ApplyClassCodeSettingDto GetApplyClassCodeSetting()
        {
            try
            {
                return SerializerHelper.DeserializeByDataContractSerializer<ApplyClassCodeSettingDto>(jobContext.JobContextContent);
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to deserialize apply class code settings from JobContextContent. JobId:{JobId}, Error:{ex.Message}");
                return null;
            }
        }

        private Task ProcessTargetNodeAsync(RMFSTreeNode targetNode, ApplyClassCodeSettingDto settingDto)
        {
            if (targetNode == null)
            {
                jobContext.HasErrorNode = true;
                mLog.Warn($"Skip null target node for apply class code. JobId:{JobId}");
                return Task.CompletedTask;
            }

            Record rootRecord = GetRootRecord(targetNode);
            if (rootRecord == null)
            {
                mLog.Warn($"Cannot find target record for apply class code. JobId:{JobId}, NodeId:{targetNode?.Id}, FullPath:{targetNode?.FullPath}, Level:{targetNode?.Level}");
                SendDetail(targetNode, null, JobDetailsStatus.Successful, "RM_ApplyClassCode_Detail_NotFoundRecords");
                return Task.CompletedTask;
            }

            if (!CanUpdateRecord(rootRecord))
            {
                mLog.Warn($"Skip apply class code because root record is not in an updatable status. JobId:{JobId}, RecordId:{rootRecord.Id}, RecordStatus:{rootRecord.RecordStatus}, NodeType:{rootRecord.NodeType}");
                SendDetail(targetNode, rootRecord, JobDetailsStatus.Skipped, "RM_ApplyClassCode_Detail_SkipApplyClassCode");
                return Task.CompletedTask;
            }

            (int updatedCount, int failedCount, List<Guid> failedIds) = UpdateRecords(rootRecord, settingDto);
            if (failedIds.Count > 0)
            {
                jobContext.HasErrorNode = true;
                if (settingDto.NeedToUpdateConnectionGroup && settingDto.IsConnectionGroup && !settingDto.ApplyToExistingDoc)
                {
                    SendDetailForGroup(rootRecord, JobDetailsStatus.Failed, $"RM_ApplyClassCode_Detail_CalculateFailedItems{I18NEntity.Separator}{updatedCount - failedCount}{I18NEntity.Separator}{failedCount}");
                }
                else if (!settingDto.NeedToUpdateConnectionGroup && settingDto.IsConnectionGroup && !settingDto.ApplyToExistingDoc)
                {
                    mLog.Warn("no need add detail for it");
                }
                else
                {
                    mLog.Error($"Apply class code batch update completed with failures. JobId:{JobId}, RootRecordId:{rootRecord.Id}, UpdatedCount:{updatedCount}, FailedCount:{failedCount}, FailedIds:{string.Join(",", failedIds)}");
                    SendDetail(targetNode, rootRecord, JobDetailsStatus.Failed,$"RM_ApplyClassCode_Detail_CalculateFailedItems{I18NEntity.Separator}{updatedCount - failedCount}{I18NEntity.Separator}{failedCount}");
                }
            }
            else
            {
                jobContext.HasSuccessNode = true;
                if (settingDto.NeedToUpdateConnectionGroup && settingDto.IsConnectionGroup && !settingDto.ApplyToExistingDoc)
                {
                    SendDetailForGroup(rootRecord, JobDetailsStatus.Successful,$"RM_ApplyClassCode_Detail_CalculateSuccessItems{I18NEntity.Separator}{updatedCount}");
                }
                else if (!settingDto.NeedToUpdateConnectionGroup && settingDto.IsConnectionGroup && !settingDto.ApplyToExistingDoc)
                {
                    mLog.Warn("no need add detail for it");
                }
                else
                {
                    mLog.Info($"Apply class code batch update completed successfully. JobId:{JobId}, RootRecordId:{rootRecord.Id}, UpdatedCount:{updatedCount}");
                    SendDetail(targetNode, rootRecord, JobDetailsStatus.Successful, $"RM_ApplyClassCode_Detail_CalculateSuccessItems{I18NEntity.Separator}{updatedCount}");
                }
            }

            return Task.CompletedTask;
        }

        private Record GetRootRecord(RMFSTreeNode targetNode)
        {
            if (targetNode.Level == (int)NodeLevel.SiteCollection)
            {
                return explorerDao.GetRecordByIds(new List<Guid> { targetNode.ConnGroupId }).FirstOrDefault();
            }
            else
            {
                return explorerDao.GetRecordByIds(new List<Guid> { targetNode.Id }).FirstOrDefault();
            }
        }

        private (int UpdatedCount, int FailedCount, List<Guid> FailedIds) UpdateRecords(Record rootRecord, ApplyClassCodeSettingDto settingDto)
        {
            CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
            List<Guid> failedIds = new List<Guid>();
            List<Record> pendingUpdates = new List<Record>();
            int updatedCount = 0;

            if (ApplySetting(rootRecord, settingDto))
            {
                if (settingDto.NeedToUpdateConnectionGroup)
                {
                    pendingUpdates.Add(rootRecord);
                    updatedCount++;
                }
            }
            if (!settingDto.ApplyToExistingDoc)
            {

                if (!settingDto.NeedToUpdateConnectionGroup && !settingDto.IsConnectionGroup)
                {
                    if (rootRecord.NodeType == (int)NodeLevel.FSFolder)
                    {
                        pendingUpdates.Add(rootRecord);
                        updatedCount++;
                    }
                    else
                    {
                        string fullPath = settingDto.FSTreeNode.FirstOrDefault().FullPath;
                        var recordResult = GetConnectionLevelNode(rootRecord.Id, fullPath.Substring(fullPath.LastIndexOf("\\") + 1));
                        List<Record> realRecords = recordResult.Item1?.ToList() ?? new List<Record>();
                        if (realRecords != null && realRecords.Count > 0)
                        {
                            var childRecord = realRecords.FirstOrDefault();
                            ApplySetting(childRecord, settingDto);
                            pendingUpdates.Add(childRecord);
                            updatedCount++;
                        }
                    }
                }
                failedIds.AddRange(FlushUpdates(pendingUpdates));
                return (updatedCount, failedIds.Count, failedIds);
            }

            Queue<Guid> pendingIds = new Queue<Guid>();
            HashSet<Guid> visitedIds = new HashSet<Guid> { rootRecord.Id };
            if (rootRecord.NodeType == (int)NodeLevel.FSConnectionGroup)
            {
                string fullPath = settingDto.FSTreeNode.FirstOrDefault().FullPath;
                var recordResult = GetConnectionLevelNode(rootRecord.Id, fullPath.Substring(fullPath.LastIndexOf("\\") + 1));
                List<Record> realRecords = recordResult.Item1?.ToList() ?? new List<Record>();
                if (realRecords != null && realRecords.Count > 0)
                {
                    var childRecord = realRecords.FirstOrDefault();
                    pendingIds.Enqueue(childRecord.Id);
                    ApplySetting(childRecord, settingDto);
                    pendingUpdates.Add(childRecord);
                }
                else
                {
                    mLog.Warn($"can not get real records by the path leaf name:{fullPath},will try get folder");
                    var recordFolderResult = GetFolderLevelNode(fullPath, settingDto.FSTreeNode.FirstOrDefault().Id.ToString());
                    List<Record> realFolderRecords = recordFolderResult.Item1?.ToList() ?? new List<Record>();
                    if (realFolderRecords != null && realFolderRecords.Count > 0)
                    {
                        foreach (var childRecord in realFolderRecords)
                        {
                            var tempPath = childRecord.DirPath + "\\" + childRecord.LeafName;
                            if (!CurrentNodeIsDisable(tempPath))
                            {
                                mLog.Info($"process realFolderRecords,current record path:{childRecord.DirPath},leafName:{childRecord.LeafName}");
                                visitedIds.Add(childRecord.Id);
                                updatedCount++;
                                pendingIds.Enqueue(childRecord.Id);
                                ApplySetting(childRecord, settingDto);
                                pendingUpdates.Add(childRecord);
                            }
                            else
                            {
                                mLog.Info($"process realFolderRecords,has disable,current record path:{childRecord.DirPath},leafName:{childRecord.LeafName}");
                            }
                        }

                    }
                    else
                    {
                        mLog.Warn("can not get real records,will return");
                        return (0, 0, new List<Guid>());
                    }
                }
            }
            else if (rootRecord.NodeType == (int)NodeLevel.FSFolder)
            {
                mLog.Info($"Processing FSFolder root node. JobId:{JobId}, RecordId:{rootRecord.Id}, Path:{rootRecord.DirPath}\\{rootRecord.LeafName}");

                if (CanUpdateRecord(rootRecord) && ApplySetting(rootRecord, settingDto))
                {
                    pendingUpdates.Add(rootRecord);
                    updatedCount++;
                    mLog.Info($"Applied settings to root folder. JobId:{JobId}, RecordId:{rootRecord.Id}");
                }

                pendingIds.Enqueue(rootRecord.Id);
            }
            else
            {
                pendingIds.Enqueue(rootRecord.Id);
            }
            while (pendingIds.Count > 0)
            {
                CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                Guid currentId = pendingIds.Dequeue();
                string continuation = string.Empty;
                do
                {
                    Tuple<IEnumerable<Record>, string> pageResult = explorerDao.QueryByPage(
                        r => r.SourceFlag == (int)SourceFlag.FileSystem
                            && (r.ParentId == currentId || r.ScopeId == currentId)
                            && r.RecordStatus != (int)RMRecordStatus.Destroyed
                            && r.RecordStatus != (int)RMRecordStatus.RMDeleted
                            && r.RecordStatus != (int)RMRecordStatus.Hidden,
                        pageCount: ChildQueryPageSize,
                        continuation: continuation,
                        convertCustomColumn2Metainfo: false);

                    List<Record> childRecords = pageResult.Item1?.ToList() ?? new List<Record>();
                    continuation = pageResult.Item2;
                    mLog.Info($"Loaded child records for apply class code. JobId:{JobId}, ParentRecordId:{currentId}, ChildCount:{childRecords.Count}, HasMore:{!string.IsNullOrEmpty(continuation)}");

                    foreach (Record childRecord in childRecords)
                    {
                        var tempPath = childRecord.DirPath + "\\" + childRecord.LeafName;
                        if (CurrentNodeIsDisable(tempPath))
                        {
                            continue;
                        }
                        if (rootRecord.Id == childRecord.NodeId || childRecord.NodeId == childRecord.ScopeId)
                        {
                            continue;
                        }
                        if (!visitedIds.Add(childRecord.Id))
                        {
                            continue;
                        }

                        if (!CanUpdateRecord(childRecord))
                        {
                            mLog.Warn($"Skip child record during apply class code traversal. JobId:{JobId}, RecordId:{childRecord.Id}, RecordStatus:{childRecord.RecordStatus}, NodeType:{childRecord.NodeType}");
                            continue;
                        }
                        if (rootRecord.NodeType == (int)NodeLevel.FSFolder && childRecord.NodeType == (int)NodeLevel.FSFolder)
                        {
                            pendingIds.Enqueue(childRecord.Id);
                        }
                        if (ApplySetting(childRecord, settingDto))
                        {
                            pendingUpdates.Add(childRecord);
                            updatedCount++;
                        }
                        if (pendingUpdates.Count >= UpdateBatchSize)
                        {
                            failedIds.AddRange(FlushUpdates(pendingUpdates));
                        }
                    }
                }
                while (!string.IsNullOrEmpty(continuation));
            }

            failedIds.AddRange(FlushUpdates(pendingUpdates));
            return (updatedCount, failedIds.Count, failedIds);
        }
        private bool CurrentNodeIsDisable(string folderPath)
        {
            if (AllDisablePath != null && AllDisablePath.Count > 0)
            {
                foreach (var path in AllDisablePath)
                {
                    string tempPath = path.TrimEnd('\\')+"\\";
                    string tempFolderPath = folderPath.TrimEnd('\\') + "\\";
                    if (tempFolderPath.StartsWith(tempPath))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private Tuple<IEnumerable<Record>, string> GetConnectionLevelNode(Guid parentId, string leafName)
        {
            string continuation = string.Empty;
            Tuple<IEnumerable<Record>, string> pageResult = explorerDao.QueryByPage(
    r => r.SourceFlag == (int)SourceFlag.FileSystem
        && r.ParentId == parentId
        && r.RecordStatus != (int)RMRecordStatus.Destroyed
        && r.RecordStatus != (int)RMRecordStatus.RMDeleted
        && r.RecordStatus != (int)RMRecordStatus.Hidden
        && r.LeafName.Equals(leafName),
    pageCount: ChildQueryPageSize,
    continuation: continuation,
    convertCustomColumn2Metainfo: false);
            return pageResult;
        }

        private Tuple<IEnumerable<Record>, string> GetFolderLevelNode(string dirPath, string connectionId)
        {
            string continuation = string.Empty;
            Tuple<IEnumerable<Record>, string> pageResult = explorerDao.QueryByPage(
    r => r.SourceFlag == (int)SourceFlag.FileSystem
        && r.RecordStatus != (int)RMRecordStatus.Destroyed
        && r.RecordStatus != (int)RMRecordStatus.RMDeleted
        && r.RecordStatus != (int)RMRecordStatus.Hidden
        && r.NodeType == (int)NodeLevel.FSFolder
        && (r.DirPath.StartsWith(dirPath) || r.AveSiteId == connectionId),
    pageCount: ChildQueryPageSize,
    continuation: continuation,
    convertCustomColumn2Metainfo: false);
            return pageResult;
        }
        private bool CanUpdateRecord(Record record)
        {
            if (record == null)
            {
                return false;
            }

            return record.SourceFlag == (int)SourceFlag.FileSystem
                && record.RecordStatus != (int)RMRecordStatus.Destroyed
                && record.RecordStatus != (int)RMRecordStatus.RMDeleted
                && record.RecordStatus != (int)RMRecordStatus.Hidden;
        }
        private List<Guid> FlushUpdates(List<Record> pendingUpdates)
        {
            if (pendingUpdates.Count == 0)
            {
                return new List<Guid>();
            }

            List<Record> recordsToUpdate = pendingUpdates.ToList();
            pendingUpdates.Clear();
            mLog.Info($"Flush apply class code updates. JobId:{JobId}, BatchSize:{recordsToUpdate.Count}");

            if (explorerDao is ExplorerDao cosmosExplorerDao)
            {
                List<Guid> failedIds = new List<Guid>();

                for (int i = 0; i < recordsToUpdate.Count; i += UpdateBatchSize)
                {
                    CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();

                    List<Record> batch = recordsToUpdate.Skip(i).Take(UpdateBatchSize).ToList();
                    List<(Record, Exception)> failedRecords = cosmosExplorerDao.BulkUpsertDirectly(batch);

                    foreach ((Record failedRecord, Exception exception) in failedRecords)
                    {
                        if (!failedIds.Contains(failedRecord.NodeId))
                        {
                            failedIds.Add(failedRecord.NodeId);
                        }

                        mLog.Error($"Apply class code update failed. JobId:{JobId}, RecordId:{failedRecord.Id}, NodeId:{failedRecord.NodeId}, ParentId:{failedRecord.ParentId}, FullPath:{BuildFullPath(null, failedRecord)}, Error:{exception}");
                    }
                }

                return failedIds;
            }
            CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
            return explorerDao.BatchUpdate(recordsToUpdate, UpdateBatchSize);
        }

        private bool ApplySetting(Record record, ApplyClassCodeSettingDto settingDto)
        {
            if (record == null || settingDto == null)
            {
                return false;
            }

            record.ClassCode = settingDto.ClassCode;
            record.CountryCode = settingDto.CountryCode;
            record.RetentionType = settingDto.RetentionType.ToString();
            record.StartDate = settingDto.StartDate;
            record.TermId = new Guid(settingDto.TermId);
            record.TermName = settingDto.ClassCode;
            record.EndTime = settingDto.RetentionType == (int)RetentionScheduleType.Event ? CalculateEndTime(settingDto.StartDate) : CalculateEndTime(record.TimeModified);
            record.PolicyValueNumber = timerDto.Number.ToString();
            record.PolicyValueUnit = ((int)timerDto.PolicyValueUnit).ToString();
            mLog.Info($"current node apply setting,recordId:{record.Id},retention type:{settingDto.RetentionType}，settingDto.StartDate：{settingDto.StartDate}，record.TimeModified：{record.TimeModified}，calculate result:{record.EndTime},policy unit:{record.PolicyValueUnit},policy number:{record.PolicyValueNumber}");
            return true;
        }

        private long CalculateEndTime(long tempTime)
        {
            DateTime baseTime = new DateTime(tempTime, DateTimeKind.Utc);
            if (tempTime == 0)
            {
                return 0;//in the cosmos,the endtime column is string type
            }
            try
            {
                if (timerDto == null)
                {
                    mLog.Warn($"timerDto is null when calculating end time. JobId:{JobId}, BaseTime:{baseTime}");
                    return 0;//in the cosmos,the endtime column is string type
                }

                DateTime endTime;
                switch (timerDto.PolicyValueUnit)
                {
                    case PolicyValueUnit.Days:
                        endTime = baseTime.AddDays(timerDto.Number);
                        break;
                    case PolicyValueUnit.Weeks:
                        endTime = baseTime.AddDays(timerDto.Number * 7);
                        break;
                    case PolicyValueUnit.Months:
                        endTime = baseTime.AddMonths(Convert.ToInt32(timerDto.Number));
                        break;
                    case PolicyValueUnit.Years:
                        endTime = baseTime.AddYears(Convert.ToInt32(timerDto.Number));
                        break;
                    default:
                        mLog.Warn($"Unsupported retention unit when calculating end time. JobId:{JobId}, BaseTime:{baseTime}, Number:{timerDto.Number}, Unit:{timerDto.PolicyValueUnit}");
                        throw new Exception($"policyValueUnit is incorrect,value:{timerDto.PolicyValueUnit}");
                }

                return endTime.Ticks;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                mLog.Error($"End time exceeds valid range. JobId:{JobId}, BaseTime:{baseTime}, Number:{timerDto?.Number}, Unit:{timerDto?.PolicyValueUnit}. Error:{ex}");
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error($"Failed to calculate end time. JobId:{JobId}, BaseTime:{baseTime}, Number:{timerDto?.Number}, Unit:{timerDto?.PolicyValueUnit}. Error:{ex}");
                throw;
            }
        }

        private void SendDetail(RMFSTreeNode targetNode, Record record, JobDetailsStatus status, string comment)
        {
            string objectName = targetNode?.Name ?? string.Empty;
            string fullPath = BuildFullPath(targetNode, record);
            int itemType = record?.NodeType ?? targetNode?.Level ?? 0;

            jobContext.ReportManager.SendJobDetail(new JMFSReclassifierJobDetails()
            {
                FinishTime = DateTime.UtcNow.Ticks,
                ObjectName = objectName,
                FullPath = fullPath,
                ItemType = itemType,
                Status = status,
                Comment = comment,
            });
        }
        private void SendDetailForGroup(Record record, JobDetailsStatus status, string comment)
        {
            string objectName = record?.LeafName ?? string.Empty;
            int itemType = record?.NodeType ?? 0;

            jobContext.ReportManager.SendJobDetail(new JMFSReclassifierJobDetails()
            {
                FinishTime = DateTime.UtcNow.Ticks,
                ObjectName = objectName,
                FullPath = record?.LeafName,
                ItemType = itemType,
                Status = status,
                Comment = comment,
            });
        }
        private static string BuildFullPath(RMFSTreeNode targetNode, Record record)
        {
            if (!string.IsNullOrWhiteSpace(targetNode?.FullPath))
            {
                return targetNode.FullPath;
            }

            if (record == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(record.DirPath))
            {
                return record.LeafName ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(record.LeafName))
            {
                return record.DirPath;
            }

            return string.Concat(record.DirPath.TrimEnd('\\'), "\\", record.LeafName);
        }
    }
}
