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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using RACloudFS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RACloudFS.FSFolderJob
{
    public class FSFolderHold : IDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(FSFolderHold));
        private string mJobId;
        private HoldOption mHoldOption;
        private HoldSettingDto holdSetting;
        private ICacheService<Record> Tasks = new MemoryStackCacheService<Record>();
        private int ActionKind;
        private bool HasFailedObject { get; set; }
        private ExplorerDao mExplorerDao = new ExplorerDao();
        private bool mIsOverWrite = false;
        private HoldDateUnit mUnit;
        private int mNumber;
        private List<Guid> rootFolders = new List<Guid>();
        private string mRootFolderHoldId;
        private int ClassificationLevel;
        private IRMFunctionSettingDao FunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        #region Interface
        private IRMSubJobDao mSubJobDao;
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if (mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }
        private IJobInfoUpdater _jobInfoUpdater;
        protected IJobInfoUpdater JobInfoUpdater
        {
            get
            {
                if (_jobInfoUpdater == null)
                {
                    _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
                }
                return _jobInfoUpdater;
            }
        }
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        private IRecordAllianceDao _RecordAllianceDao;
        protected IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (_RecordAllianceDao == null)
                {
                    _RecordAllianceDao = (IRecordAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordAllianceDao));
                }
                return _RecordAllianceDao;
            }
        }
        private IRMRuleDao mRMRuleDao;
        protected IRMRuleDao RMRuleDao
        {
            get
            {
                if (mRMRuleDao == null)
                {
                    mRMRuleDao = (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao));
                }
                return mRMRuleDao;
            }
        }

        private IGeneralSettingService mGeneralSettingService = null;
        public IGeneralSettingService GeneralSettingService
        {
            get
            {
                if (mGeneralSettingService == null)
                {
                    mGeneralSettingService = (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));
                }
                return mGeneralSettingService;
            }
        }

        private IHoldDao mHoldDao = null;
        public IHoldDao HoldDao
        {
            get
            {
                if (mHoldDao == null)
                {
                    mHoldDao = (IHoldDao)PlatformWindsorManager.GetService(typeof(IHoldDao));
                }
                return mHoldDao;
            }
        }

        private IRecordsHistoryService mRecordsHistoryService = null;
        public IRecordsHistoryService RecordsHistoryService
        {
            get
            {
                if (mRecordsHistoryService == null)
                {
                    mRecordsHistoryService = (IRecordsHistoryService)PlatformWindsorManager.GetService(typeof(IRecordsHistoryService));
                }
                return mRecordsHistoryService;
            }
        }
        #endregion
        public FSFolderHold(string jobId)
        {
            mJobId = jobId;
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
            var jobContext = SerializerHelper.DeserializeByDataContractSerializer<HoldOption>(subJobWithContext.JobContext.Content);
            mHoldOption = jobContext;
            TenantLocalValue.LogonUserId = mHoldOption.UserId;
            holdSetting = new HoldSettingDto()
            {
                AllianceType = 1,
                HoldBy = mHoldOption.HoldBy,
                HoldId = mHoldOption.HoldId,
                ReleaseTime = mHoldOption.ReleaseTime,
                HoldAction = mHoldOption.PlaceHoldAction,
                RemoveHolds = mHoldOption.RemoveHolds,
            };
            ActionKind = mHoldOption.Action;
            JobInfoUpdater.UpdateJobProgress(mJobId, 1);
            ReportManager.StartUpdateJobProgress();
            mIsOverWrite = jobContext.IsOverWrite;
            mNumber = jobContext.Number;
            mUnit = jobContext.Unit;
            ClassificationLevel = this.GetClassificationLevel();
            logger.Info("Object count from the message is :{0}", mHoldOption.RelatedRecords.Count);
            List<Record> records = mExplorerDao.GetRecordByIds(mHoldOption.RelatedRecords);
            mRootFolderHoldId = mHoldOption.FolderOriginalHoldId;
            rootFolders.AddRange(records.Select(r => r.Id).ToList());
            Tasks.AddBatch(records);
            ReportManager.IncreaseBase(records.Count + 100);
        }

        public async Task RunAsync()
        {
            try
            {
                using (FSFolderJobDiscover discover = new FSFolderJobDiscover())
                {
                    logger.Info($"ClassificationLevel: {ClassificationLevel}");
                    while (Tasks.Count > 0)
                    {
                        var tempTasks = Tasks.Take(30);
                        logger.Info("Got {0} tasks and {1} left in the cache.", tempTasks.Count(), Tasks.Count);
                        if (ClassificationLevel == (int)NodeLevel.FSFile)
                        {
                            foreach (var record in tempTasks)
                            {
                                try
                                {
                                    var fullPath = Path.Combine(record.DirPath.TrimEnd('\\').TrimEnd('/'), record.LeafName);
                                    using (new PerformanceScope("ProcessFolder"))
                                    {
                                        if (!rootFolders.Contains(record.Id))
                                        {
                                            var folder = FilterFolders(new List<Record>() { record });
                                            if (folder != null && folder.Count > 0)
                                            {
                                                TakeAction(record);
                                                AddRecordsHistory(new List<Guid>() { record.Id });
                                                AddDetail(record, JobDetailsStatus.Successful);
                                            }
                                        }
                                        else
                                        {
                                            AddDetail(record, JobDetailsStatus.Successful);
                                        }
                                    }

                                    List<Record> children = new List<Record>();
                                    using (new PerformanceScope("QueryChildren"))
                                    {
                                        logger.Debug($"Begin to load children from Records table. Folder is [{fullPath}]");
                                        Tasks.AddBatch(discover.ProcessSubFolders(fullPath));
                                        children = await discover.ProcessFilesAsync(record.Id);
                                    }

                                    var files = GetFiles(children);
                                    using (new PerformanceScope(string.Format("Process {0} files", files.Count)))
                                    {
                                        TakeAction(files);
                                    }
                                    using (new PerformanceScope(string.Format("Add Record History {0} files", files.Count)))
                                    {
                                        AddRecordsHistory(files);
                                    }
                                    children.ForEach(t =>
                                    {
                                        if (files.Contains(t.Id) && t.NodeType == (int)NodeLevel.FSFile)
                                        {
                                            AddDetail(t, JobDetailsStatus.Successful);
                                        }
                                    });

                                }
                                catch (Exception e)
                                {
                                    HasFailedObject = true;
                                    logger.Error("Failed to hold the record with ID[{0}], Exception:{1}", record.Id, e.ToString());
                                    AddDetail(record, JobDetailsStatus.Failed);
                                }
                            }
                        }
                        else
                        {
                            //Folder level classification
                            var availableTaks = this.FilterFolders(tempTasks);
                            using (new PerformanceScope(string.Format("Process {0} folders", availableTaks.Count)))
                            {
                                foreach (var item in availableTaks)
                                {
                                    try
                                    {
                                        if (!rootFolders.Contains(item.Id))
                                        {
                                            TakeAction(new List<Guid>() { item.Id });
                                            AddDetail(item, JobDetailsStatus.Successful);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        HasFailedObject = true;
                                        logger.Error("Failed to hold the record with ID[{0}], Exception:{1}", item.Id, e.ToString());
                                        AddDetail(item, JobDetailsStatus.Failed);
                                    }
                                }
                            }
                            if (availableTaks.Count > 0)
                            {
                                using (new PerformanceScope(string.Format("Add Record History {0} Folders", availableTaks.Count)))
                                {
                                    //availableTaks.Select(r => r.Id).ToList()
                                    AddRecordsHistory(availableTaks.Where(item => item.ScopeId != item.ParentId).Select(r => r.Id).ToList());    
                                }
                            }
                            foreach (var temp in tempTasks)
                            {
                                if (rootFolders.Contains(temp.Id))
                                {
                                    var fullPath = Path.Combine(temp.DirPath.TrimEnd('\\').TrimEnd('/'), temp.LeafName);
                                    Tasks.AddBatch(discover.ProcessSubFolders(fullPath));
                                }
                            }
                        }
                    }
                    logger.Info("Job finished. Begin to set final state.");
                    ReportManager.SetJobFinished(HasFailedObject ? JobStatus.FinishWithException : JobStatus.Finished);
                    logger.Info("The final job state was sent to manager.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to hold the folders. Exception:{0}", ex.ToString());
                ReportManager.SetJobFinished(JobStatus.Failed, ex.Message);
            }
        }

        private List<Record> FilterFolders(IEnumerable<Record> records)
        {
            //if (ClassificationLevel == (int)NodeLevel.FSFile)
            //{
            //    return records.ToList();
            //}
            if (mIsOverWrite)
            {
                return records.Where(t => t.NodeType == (int)NodeLevel.FSFolder).ToList();
            }
            else
            {
                // or place hold, will process files not on hold
                if (ActionKind == (int)AuditAction.ReuseHoldTypeWithRecord || ActionKind == (int)AuditAction.CreateHoldTypeWithRecord)
                {
                    if (holdSetting.HoldAction == RecordsConstants.HOLD_ACTION_APPEND)
                    {
                        return records.Where(t => t.NodeType == (int)NodeLevel.FSFolder && t.HoldId != holdSetting.HoldId && !t.AppendHolds_Array.Contains(holdSetting.HoldId)).ToList();
                    }
                    else
                    {
                        return records.Where(t => t.NodeType == (int)NodeLevel.FSFolder && (!t.HoldStatus || (t.HoldId == holdSetting.HoldId || (t.AppendHolds_Array != null && t.AppendHolds_Array.Contains(holdSetting.HoldId))))).ToList();
                    }
                }
                //extend hold or change hold, will process file not on hold or using the same hold as folder
                else if (ActionKind == (int)AuditAction.ChangeHoldCreate
                    || ActionKind == (int)AuditAction.ChangeHoldReuse)
                {
                    //return records.Where(t => t.NodeType == (int)NodeLevel.FSFile && (!t.HoldStatus || (t.HoldStatus && t.HoldId == mRootFolderHoldId))).Select(t => t.Id).ToList();
                    return records.Where(t => t.NodeType == (int)NodeLevel.FSFolder).ToList();
                }
                else if (ActionKind == (int)AuditAction.SusPendRecords)
                {
                    return records.Where(t => t.NodeType == (int)NodeLevel.FSFolder && t.HoldStatus && (t.HoldId == holdSetting.HoldId || (t.AppendHolds_Array != null && t.AppendHolds_Array.Contains(holdSetting.HoldId)))).ToList();
                }
                //cancel hold, will process files on hold 
                else if (ActionKind == (int)AuditAction.CancelHoldByRecords)
                {
                    return records.Where(t => t.NodeType == (int)NodeLevel.FSFolder && t.HoldStatus).ToList();
                }
                else
                {
                    throw new Exception("Invalid ActionKind: " + ActionKind);
                }
            }
        }

        private List<Guid> GetFiles(List<Record> records)
        {
            if (mIsOverWrite)
            {
                return records.Where(t => t.NodeType == (int)NodeLevel.FSFile).Select(t => t.Id).ToList();
            }
            else
            {
                // or place hold, will process files not on hold
                if (ActionKind == (int)AuditAction.ReuseHoldTypeWithRecord || ActionKind == (int)AuditAction.CreateHoldTypeWithRecord)
                {
                    if (holdSetting.HoldAction == RecordsConstants.HOLD_ACTION_APPEND)
                    {
                        return records.Where(t => t.NodeType == (int)NodeLevel.FSFile && t.HoldId != holdSetting.HoldId && !t.AppendHolds_Array.Contains(holdSetting.HoldId)).Select(t => t.Id).ToList();
                    }
                    else
                    {
                        return records.Where(t => t.NodeType == (int)NodeLevel.FSFile && (!t.HoldStatus || (t.HoldId == holdSetting.HoldId || (t.AppendHolds_Array != null && t.AppendHolds_Array.Contains(holdSetting.HoldId))))).Select(t => t.Id).ToList();
                    }
                }
                //extend hold or change hold, will process file not on hold or using the same hold as folder
                else if (ActionKind == (int)AuditAction.ChangeHoldCreate
                    || ActionKind == (int)AuditAction.ChangeHoldReuse)
                {
                    //return records.Where(t => t.NodeType == (int)NodeLevel.FSFile && (!t.HoldStatus || (t.HoldStatus && t.HoldId == mRootFolderHoldId))).Select(t => t.Id).ToList();
                    return records.Where(t => t.NodeType == (int)NodeLevel.FSFile).Select(t => t.Id).ToList();
                }
                else if (ActionKind == (int)AuditAction.SusPendRecords)
                {
                    return records.Where(t => t.NodeType == (int)NodeLevel.FSFile && t.HoldStatus && (t.HoldId == holdSetting.HoldId || (t.AppendHolds_Array != null && t.AppendHolds_Array.Contains(holdSetting.HoldId)))).Select(t => t.Id).ToList();
                }
                //cancel hold, will process files on hold 
                else if (ActionKind == (int)AuditAction.CancelHoldByRecords)
                {
                    return records.Where(t => t.NodeType == (int)NodeLevel.FSFile && t.HoldStatus).Select(t => t.Id).ToList();
                }
                else
                {
                    throw new Exception("Invalid ActionKind: " + ActionKind);
                }
            }
        }

        private void TakeAction(List<Guid> files)
        {
            try
            {
                if (ActionKind != (int)AuditAction.CancelHold && ActionKind != (int)AuditAction.DeleteHold && ActionKind != (int)AuditAction.CancelHoldByRecords)
                {
                    logger.Info("Begin to hold {0} files. ", files.Count);
                    if (!string.IsNullOrEmpty(holdSetting.HoldId))
                    {
                        if (ActionKind == (int)AuditAction.SusPendRecords)
                        {
                            logger.Info("Suspend hold");
                            SuspendHold(files);
                        }
                        else
                        {
                            PlaceHold(files, holdSetting, null);
                        }
                    }
                    else
                    {
                        //logger.Info("Suspend hold");
                        //SuspendHold(files);
                        throw new Exception("No hold setting");
                    }
                }
                else
                {
                    logger.Info("Begin to cancel holds on {0} files. ", files.Count);
                    CancelHold(files);
                }
            }
            finally
            {
                ReportManager.Increase(files.Count);
            }
        }

        private void SuspendHold(List<Guid> fileIds)
        {
            long releaseTime = 0;
            var records = mExplorerDao.GetRecordByIds(fileIds);
            var settingHoldRecords = mExplorerDao.GetRecordByIds(fileIds);
            if (settingHoldRecords != null && settingHoldRecords.Count > 0)
            {
                foreach (var record in records)
                {
                    try
                    {
                        List<HoldUser> allHoldByUsers = GetAllHoldByUsers(record);
                        List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(record);
                        var selectedHoldReleaseTime = allHoldUntilTimes.FirstOrDefault(h => h.HoldId == mHoldOption.HoldId);
                        if (selectedHoldReleaseTime != null)
                        {
                            long oldReleaseTime = selectedHoldReleaseTime.UntilTime;
                            if (mUnit == HoldDateUnit.Day)
                            {
                                releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddDays(mNumber).Ticks;
                            }
                            else if (mUnit == HoldDateUnit.Week)
                            {
                                releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddDays(7 * mNumber).Ticks;
                            }
                            else if (mUnit == HoldDateUnit.Month)
                            {
                                releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddMonths(mNumber).Ticks;
                            }
                            else if (mUnit == HoldDateUnit.Years)
                            {
                                releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddYears(mNumber).Ticks;
                            }
                            selectedHoldReleaseTime.UntilTime = releaseTime;
                            record.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                        }

                        Tuple<long, string> holdTimeAndHoldId = GetMaxHoldTime(record, out string[] appendHoldsArray, null);
                        long firstMaxHoldTime = holdTimeAndHoldId.Item1;
                        string firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                        //Hold状态Record重新计算Due Date;
                        var isRemoveRuleData = false;
                        if (record != null && record.RuleId != null && record.RuleId != Guid.Empty)
                        {
                            var tempRule = RMRuleDao.GetRuleById(record.RuleId);
                            if (tempRule != null && IsRemoveRule(tempRule, record.SourceFlag))
                            {
                                isRemoveRuleData = true;
                                var newDisposalDueDate = new List<long>() { record.PreviosDisposalDueDate, firstMaxHoldTime }.Max();
                                //Remove Rule需要计算Due Date
                                //更新Remove类型Item的Due Date为新值
                                mExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                                {
                                    s.HoldReleaseTime = firstMaxHoldTime;
                                    s.HoldId = firstMaxHoldSettingId;
                                    s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                                    s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                                    s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                    s.AppendHolds_Array = appendHoldsArray;
                                    s.DisposalDueDate = newDisposalDueDate;
                                });
                            }
                        }
                        if (!isRemoveRuleData)
                        {
                            mExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                            {
                                s.HoldReleaseTime = firstMaxHoldTime;
                                s.HoldId = firstMaxHoldSettingId;
                                s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                                s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                                s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                s.AppendHolds_Array = appendHoldsArray;
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Failed to extend hold the record with id [{record.Id}], Exception:{e}");
                    }
                }
            }
        }
        private void PlaceHold(List<Guid> ids, HoldSettingDto holdDto, List<CompactRecord> fileIds)
        {
            UpdateHoldRecordInExplorer(ids, holdDto);
        }

        private void CancelHold(List<Guid> recordsIds)
        {
            var userName = TenantLocalValue.LogonUserEmail;
            if (holdSetting.RemoveHolds == null || holdSetting.RemoveHolds.Count == 0)
            {
                mExplorerDao.UpdateAll(r => recordsIds.Contains(r.Id), s =>
                {
                    s.HoldStatus = false; s.HoldType = 0;
                    s.HoldReleaseTime = DateTime.MinValue.Ticks;
                    s.HoldId = null; s.HoldBy = null;
                    s.HoldByUsers = null;
                    s.HoldUntilTimes = null;
                    s.AppendHolds_Array = new string[0];
                    s.DisposalDueDate = s.PreviosDisposalDueDate;
                });
                return;
            }

            var records = mExplorerDao.GetRecordByIds(recordsIds);
            foreach (var record in records)
            {
                var recordId = record.Id;
                Tuple<long, string> holdTimeAndHoldId = GetMaxHoldTime(record, out string[] appendHoldsArray, null, holdSetting.RemoveHolds);

                if (holdTimeAndHoldId == null)
                {
                    mExplorerDao.UpdateAll(r => recordId == r.Id, s =>
                    {
                        s.HoldStatus = false; s.HoldType = 0;
                        s.HoldReleaseTime = DateTime.MinValue.Ticks;
                        s.HoldId = null; s.HoldBy = null;
                        s.HoldByUsers = null;
                        s.HoldUntilTimes = null;
                        s.AppendHolds_Array = new string[0];
                        s.DisposalDueDate = s.PreviosDisposalDueDate;
                    });
                }
                else
                {
                    long firstMaxHoldTime = holdTimeAndHoldId.Item1;
                    string firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                    var allHolds = new List<string>(appendHoldsArray) { firstMaxHoldSettingId };

                    var allHoldByUsers = GetAllHoldByUsers(record);
                    allHoldByUsers = allHoldByUsers.Where(h => allHolds.Contains(h.HoldId)).ToList();

                    List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(record);
                    allHoldUntilTimes = allHoldUntilTimes.Where(h => allHolds.Contains(h.HoldId)).ToList();

                    var isRemoveRuleData = false;
                    if (record.RuleId != null && record.RuleId != Guid.Empty)
                    {
                        var tempRule = RMRuleDao.GetRuleById(record.RuleId);
                        if (tempRule != null && IsRemoveRule(tempRule, record.SourceFlag))
                        {
                            isRemoveRuleData = true;
                            //Remove Rule需要计算Due Date
                            var caculateDisposalDueDate = new List<long>() { record.PreviosDisposalDueDate, firstMaxHoldTime }.Max();
                            //更新Remove类型Item的Due Date为新值
                            mExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                            {
                                s.HoldStatus = true;
                                s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                                s.HoldReleaseTime = firstMaxHoldTime;
                                s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                                s.HoldId = firstMaxHoldSettingId;
                                s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                                s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                s.AppendHolds_Array = appendHoldsArray;
                                s.DisposalDueDate = caculateDisposalDueDate;
                            });
                        }
                    }
                    if (!isRemoveRuleData)
                    {
                        mExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                        {
                            s.HoldStatus = true;
                            s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                            s.HoldReleaseTime = firstMaxHoldTime;
                            s.HoldId = firstMaxHoldSettingId;
                            s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                            s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                            s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                            s.AppendHolds_Array = appendHoldsArray;
                        });
                    }
                }
            }
        }

        private void UpdateHoldRecordInExplorer(List<Guid> ids, HoldSettingDto holdDto)
        {
            var caculateDisposalDueDate = DateTime.MinValue.Ticks;
            var tempExplorers = mExplorerDao.GetRecordByIds(ids);
            if (tempExplorers != null && tempExplorers.Count > 0)
            {
                foreach (var tempExplorerItem in tempExplorers)
                {
                    //if (ClassificationLevel == (int)NodeLevel.FSFile && tempExplorerItem.NodeType == (int)NodeLevel.FSFolder && tempExplorerItem.HoldStatus && !this.mIsOverWrite)
                    //{
                    //    logger.Info("Skip folder {0}", tempExplorerItem.LeafName);
                    //    continue;
                    //}
                    Tuple<long, string> holdTimeAndHoldId = GetMaxHoldTime(tempExplorerItem, out string[] appendHoldsArray, holdDto);
                    long firstMaxHoldTime = holdTimeAndHoldId.Item1;
                    string firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                    var allHolds = new List<string>(appendHoldsArray) { firstMaxHoldSettingId };

                    List<HoldUser> allHoldByUsers = GetAllHoldByUsers(tempExplorerItem);
                    allHoldByUsers.Add(new HoldUser() { HoldId = holdDto.HoldId, HoldBy = holdDto.HoldBy });
                    allHoldByUsers = allHoldByUsers.Where(h => allHolds.Contains(h.HoldId)).ToList();

                    List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(tempExplorerItem);
                    allHoldUntilTimes.Add(new HoldUntilTime() { HoldId = holdDto.HoldId, UntilTime = holdDto.ReleaseTime });
                    allHoldUntilTimes = allHoldUntilTimes.Where(h => allHolds.Contains(h.HoldId)).ToList();

                    var isRemoveRuleData = false;
                    if (tempExplorerItem.RuleId != null && tempExplorerItem.RuleId != Guid.Empty)
                    {
                        var tempRule = RMRuleDao.GetRuleById(tempExplorerItem.RuleId);
                        if (tempRule != null && IsRemoveRule(tempRule, tempExplorerItem.SourceFlag))
                        {
                            isRemoveRuleData = true;
                            //Remove Rule需要计算Due Date
                            caculateDisposalDueDate = new List<long>() { tempExplorerItem.PreviosDisposalDueDate, firstMaxHoldTime }.Max();
                            //更新Remove类型Item的Due Date为新值
                            mExplorerDao.UpdateAll(r => tempExplorerItem.Id == r.Id, s =>
                            {
                                s.HoldStatus = true;
                                s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                                s.HoldReleaseTime = firstMaxHoldTime;
                                s.HoldId = firstMaxHoldSettingId;
                                s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                                s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                                s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                s.AppendHolds_Array = appendHoldsArray;
                                s.DisposalDueDate = caculateDisposalDueDate;
                            });
                        }
                    }
                    if (!isRemoveRuleData)
                    {
                        mExplorerDao.UpdateAll(r => tempExplorerItem.Id == r.Id, s =>
                        {
                            s.HoldStatus = true;
                            s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                            s.HoldReleaseTime = firstMaxHoldTime;
                            s.HoldId = firstMaxHoldSettingId;
                            s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                            s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                            s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                            s.AppendHolds_Array = appendHoldsArray;
                            s.HoldBy = holdDto.HoldBy;
                        });
                    }
                }
            }
        }

        private static List<HoldUntilTime> GetAllHoldUntilTimes(Record tempExplorerItem)
        {
            var allHoldUntilTimes = string.IsNullOrEmpty(tempExplorerItem.HoldUntilTimes) ? new List<HoldUntilTime>() : JsonConvert.DeserializeObject<List<HoldUntilTime>>(tempExplorerItem.HoldUntilTimes);
            if (tempExplorerItem.HoldStatus && allHoldUntilTimes.Count == 0)
            {
                allHoldUntilTimes.Add(new HoldUntilTime() { HoldId = tempExplorerItem.HoldId, UntilTime = tempExplorerItem.HoldReleaseTime });
            }
            return allHoldUntilTimes;
        }

        private static List<HoldUser> GetAllHoldByUsers(Record tempExplorerItem)
        {
            List<HoldUser> allHoldByUsers = string.IsNullOrEmpty(tempExplorerItem.HoldByUsers) ? new List<HoldUser>() : JsonConvert.DeserializeObject<List<HoldUser>>(tempExplorerItem.HoldByUsers);
            if (tempExplorerItem.HoldStatus && allHoldByUsers.Count == 0)
            {
                allHoldByUsers.Add(new HoldUser() { HoldId = tempExplorerItem.HoldId, HoldBy = tempExplorerItem.HoldBy });
            }
            return allHoldByUsers;
        }

        private Tuple<long, string> GetMaxHoldTime(Record tempExplorerItem, out string[] appendHoldsArray, HoldSettingDto holdDto, List<string> removeHoldIds = null)
        {
            Tuple<long, string> holdTuple = null;
            string firstMaxHoldSettingId = string.Empty;
            long firstMaxHoldTime = 0;//最长时间的Hold Time相同，以第一个hold为准

            List<Tuple<long, string>> holdTimeAndHoldIdList = new List<Tuple<long, string>>();
            if (!((ActionKind == (int)AuditAction.ReuseHoldTypeWithRecord || ActionKind == (int)AuditAction.CreateHoldTypeWithRecord) && mIsOverWrite))
            {
                if (holdDto == null || holdDto.HoldAction != RecordsConstants.HOLD_ACTION_CHANGE)
                {
                    var recordAllExistHoldIds = GetAllExistHoldIds(tempExplorerItem);
                    if (removeHoldIds != null)
                    {
                        recordAllExistHoldIds.RemoveAll(h => removeHoldIds.Contains(h));
                    }

                    var recordAllExistHolds = HoldDao.GetHoldByIds(recordAllExistHoldIds);

                    List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(tempExplorerItem);
                    if (tempExplorerItem.HoldStatus && allHoldUntilTimes.Count == 0)
                    {
                        allHoldUntilTimes.Add(new HoldUntilTime() { HoldId = tempExplorerItem.HoldId, UntilTime = tempExplorerItem.HoldReleaseTime });
                    }

                    foreach (var hold in recordAllExistHolds)
                    {
                        long? untilTime = allHoldUntilTimes.FirstOrDefault(h => h.HoldId == hold.Id)?.UntilTime;
                        if (untilTime.HasValue)
                        {
                            holdTimeAndHoldIdList.Add(new Tuple<long, string>(untilTime.Value, hold.Id));
                        }
                    }
                }
            }

            if (holdDto != null && !holdTimeAndHoldIdList.Any(h => h.Item2 == holdDto.HoldId))
            {
                holdTimeAndHoldIdList.Add(new Tuple<long, string>(holdDto.ReleaseTime, holdDto.HoldId));
            }
            holdTimeAndHoldIdList = holdTimeAndHoldIdList.Distinct().ToList();
            foreach (var holdTimeAndHoldId in holdTimeAndHoldIdList)
            {
                if (holdTimeAndHoldId.Item1 > firstMaxHoldTime)
                {
                    holdTuple = holdTimeAndHoldId;
                    firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                    firstMaxHoldTime = holdTimeAndHoldId.Item1;
                }
            }
            appendHoldsArray = holdTimeAndHoldIdList.Select(h => h.Item2).Where(h => h != firstMaxHoldSettingId).ToArray();
            return holdTuple;
        }

        private List<string> GetAllExistHoldIds(Record tempExplorerItem)
        {
            List<string> recordAllExistHoldIds = new List<string>();
            if (!string.IsNullOrEmpty(tempExplorerItem.HoldId))
            {
                recordAllExistHoldIds.Add(tempExplorerItem.HoldId);
            }
            if (tempExplorerItem.AppendHolds_Array != null)
            {
                recordAllExistHoldIds.AddRange(tempExplorerItem.AppendHolds_Array.ToList());
            }
            return recordAllExistHoldIds;
        }

        

      
        private bool IsRemoveRule(RMRule tempRule, int sourceFlag)
        {
            var result = false;
            int disposalAction = -1;
            if ((int)SourceFlag.SharePoint == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.DisposalAction);
                if (disposalAction == 0 || disposalAction == 2 || disposalAction == 5 || disposalAction == 7 || disposalAction == 8
                || disposalAction == 10 || disposalAction == 13 || disposalAction == 15 || disposalAction == 16 || disposalAction == 18
                || disposalAction == 21 || disposalAction == 23 || disposalAction == 24 || disposalAction == 26 || disposalAction == 29
                || disposalAction == 31 || disposalAction == 130 || disposalAction == 135 || disposalAction == 138 || disposalAction == 143
                || disposalAction == 146 || disposalAction == 151 || disposalAction == 154 || disposalAction == 156 || disposalAction == 159)
                {
                    result = true;
                }
            }
            else if ((int)SourceFlag.Exchange == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.ExchangeDisposalAction);
                if (disposalAction == 0)
                {
                    result = true;
                }
            }
            else if ((int)SourceFlag.Physical == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.PhysicalDisposalAction);
                if (disposalAction == (int)RMContentDisposalAction.Remove)
                {
                    return true;
                }
            }
            else if ((int)SourceFlag.FileSystem == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.FSDisposalAction);
                switch (disposalAction)
                {
                    case (int)RMContentDisposalAction.Remove:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.RelatedRecords:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords:
                        return true;
                    default:
                        break;
                }
            }
            return result;
        }
        private void AddRecordsHistory(List<Guid> files)
        {
            try
            {
                logger.Info("Begin to add history for hold action");
                //RecordHistoryXml historyXml = new RecordHistoryXml();
                //RecordHistory history = new RecordHistory
                //{
                //    Action = GetHoldActionString(ActionKind),
                //    DisplayTime = DateTime.UtcNow.ToString(),
                //    TimeUTC = DateTime.UtcNow.Ticks,
                //    User = holdSetting.HoldBy
                //};
                //var list = new List<RecordHistory>
                //    {
                //        history
                //    };
                //historyXml.HistoryList = list;
                //mExplorerDao.AddReocrdHistory(files, historyXml);
                RecordsHistoryService.AddRecordsHistory(files, GetHoldActionString(ActionKind));
            }
            catch (Exception e)
            {
                logger.Warn("Failed to add records histroy for move action {0}", e.ToString());
            }
        }
        private void TakeAction(Record record)
        {
            TakeAction(new List<Guid>() { record.NodeId });
        }
        private string GetHoldActionString(int HoldAction)
        {
            string actionString = string.Empty;
            switch (HoldAction)
            {
                case (int)AuditAction.ChangeHoldCreate:
                    actionString = "RM_BCM_Audit_Action_ChangeHoldCreate";
                    break;
                case (int)AuditAction.ChangeHoldReuse:
                    actionString = "RM_BCM_Audit_Action_ChangeHoldReuse";
                    break;
                case (int)AuditAction.CreateHoldTypeWithRecord:
                    actionString = "RM_BCM_Audit_Action_CreateHoldTypeWithRecord";
                    if (holdSetting.HoldAction == RecordsConstants.HOLD_ACTION_APPEND)
                    {
                        actionString = "RM_BCM_Audit_Action_CreateAppendHoldTypeWithRecord";
                    }
                    break;
                case (int)AuditAction.ReuseHoldTypeWithRecord:
                    actionString = "RM_BCM_Audit_Action_ReuseHoldTypeWithRecord";
                    if (holdSetting.HoldAction == RecordsConstants.HOLD_ACTION_APPEND)
                    {
                        actionString = "RM_BCM_Audit_Action_ReuseAppendHoldTypeWithRecord";
                    }
                    break;
                case (int)AuditAction.CancelHoldByRecords:
                    actionString = "RM_BCM_Audit_Action_CancelHoldByRecords";
                    break;
                case (int)AuditAction.SuspendHold:
                    actionString = "RM_BCM_Audit_Action_SuspendHold";
                    break;
                case (int)AuditAction.CancelHold:
                    actionString = "RM_BCM_Audit_Action_CancelHold";
                    break;
                case (int)AuditAction.DeleteHold:
                    actionString = "RM_BCM_Audit_Action_DeleteHold";
                    break;

                case (int)AuditAction.SusPendRecords:
                    actionString = "RM_BCM_Audit_Action_SusPendRecords";
                    break;
            }
            return actionString;
        }
        private void AddDetail(Record record, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMFSHoldJobDetails()
            {
                ObjectName = record.LeafName,
                FullPath = Path.Combine(record.DirPath, record.LeafName).Replace('/', '\\'),
                Status = status,
                Action = GetHoldActionString(ActionKind),
                Comment = comment
            });
        }

        public int GetClassificationLevel()
        {
            RMFunctionSetting setting;
            FunctionSettingDao.TryGet(AvePoint.RA.Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, out setting);
            NodeLevel result;
            if (setting == null)
            {
                return (int)NodeLevel.FSFile;
            }
            if (Enum.TryParse<NodeLevel>(setting.SettingInfo, out result))
            {
                return (int)result;
            }
            return (int)NodeLevel.FSFolder;
        }
        public void Dispose()
        {
            if (mExplorerDao != null)
            {
                mExplorerDao.Dispose();
            }
        }
    }
}
