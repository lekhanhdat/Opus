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
using System.Collections.Generic;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;

namespace RAGoogle.GoogleExplorer
{
    public class RMGoogleDriveExplorerUtility : GoogleReclassifyBaseProcessor
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMGoogleDriveExplorerUtility));
        protected List<Guid> FailedIds { get; set; }
        protected List<Guid> SuccessIds { get; set; }

        protected ChangeTermType mChangeTermType = ChangeTermType.None;

        private readonly List<string> _prefixReclassifyJobIds = new List<string> { "GSA", "MARE" };

        private readonly List<string> _prefixGlobalSearchJobIds = new List<string> { "GSA", "MARE", "MAAP" };

        private IRMRecordsUpdateTempDao mRMRecordsUpdateTempDao { get; set; }
        public IRMRecordsUpdateTempDao RMRecordsUpdateTempDao
        {
            get
            {
                if (mRMRecordsUpdateTempDao == null)
                {
                    mRMRecordsUpdateTempDao = (IRMRecordsUpdateTempDao)PlatformWindsorManager.GetService(typeof(IRMRecordsUpdateTempDao)); ;
                }
                return mRMRecordsUpdateTempDao;
            }
        }

        private IRMMLTermDao mlTermDao = null;
        public IRMMLTermDao TrainingTermDao
        {
            get
            {
                if (mlTermDao == null)
                {
                    mlTermDao = (IRMMLTermDao)PlatformWindsorManager.GetService(typeof(IRMMLTermDao));
                }
                return mlTermDao;
            }
        }

        private ITenantService mTenantService;
        public ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }

        public RMGoogleDriveExplorerUtility(string jobId) : base()
        {
            FailedIds = new List<Guid>();
            SuccessIds = new List<Guid>();
            _jobId = jobId;
        }

        public RMGoogleDriveExplorerUtility(string jobId, ChangeTermType changeTermType = ChangeTermType.None) : base()
        {
            mChangeTermType = changeTermType;
            FailedIds = new List<Guid>();
            SuccessIds = new List<Guid>();
            _jobId = jobId;
        }

        public async Task ChangeAllTermsForGoogleDriveAsync(ChangeTermOption changeTermInfo, string jobId)
        {
            try
            {
                bool waitingForOtherSource = false;
                this.ChangeTermInfo = changeTermInfo;
                List<Guid> successIds = new List<Guid>();
                using (new PerformanceScope("RMGoogleDriveExplorerUtility.ChangeLabelForGoogleDrive"))
                {
                    logger.Info("Is new logic account is {0}", base.isNewLogicAccount);
                    logger.Info($"Change label action start for Job ID: {jobId}");
                    RMRecordsUpdateTempDao.InsertUpdateTemp(jobId, "", RecordsConstants.Explorer_RealTime_Running, "");
                    mRMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(jobId, waitingForOtherSource);
                    logger.Info($"[Change Label] Job {jobId} status set to running.");
                    var records = new List<Record>();

                    if (changeTermInfo.GoogleDriveRecordIds != null && changeTermInfo.GoogleDriveRecordIds.Count > 0)
                    {
                        var startTime = DateTime.Now;
                        using (new PerformanceScope("ChangeLabel.GetRecords"))
                        {
                            records = ExplorerDao.QueryAll(r => changeTermInfo.GoogleDriveRecordIds.Contains(r.Id)).ToList();
                            RMRecordsUpdateTempDao.InsertUpdateTemp(jobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(records.Select(r => r.LeafName).ToList()));
                            logger.Warn($"[Change Label] 1 Time elapsed for querying {records.Count} records from Cosmos: {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        }
                        var termId = changeTermInfo.TargetTermUniqueId;
                        var termName = changeTermInfo.TargetTermName;
                        var trainingTerm = TrainingTermDao.GetTrainingTerm(termId);
                        try
                        {
                            await InitAsync(changeTermInfo.TargetTermUniqueId.ToString());
                            logger.Info($"[Change Label] Job {jobId} initial successful.");
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"[Change Label] Skipped applying label with name : {changeTermInfo.TargetTermName} and Id : {changeTermInfo.TargetTermUniqueId} because of an error. {ex.Message}");
                            RMRecordsUpdateTempDao.InsertUpdateTemp(jobId, changeTermInfo.TargetTermName, RecordsConstants.Explorer_RealTime_Failed_All);
                            RecordsHistoryService.AddRecordsHistory(changeTermInfo.GoogleDriveRecordIds, "RM_JS_Audit_ChangeLabelErrorMessage");
                            return;
                        }

                        if (mChangeTermType == ChangeTermType.AIMADirectlyApprove || mChangeTermType == ChangeTermType.AIMAChangeTerm)
                        {
                            var recordsGroupedByPredictId = records
                                .Where(r => r.PredictTermId != Guid.Empty)
                                .GroupBy(r => r.PredictTermId);
                            foreach (var group in recordsGroupedByPredictId)
                            {
                                List<Record> groupedRecords = group.ToList();
                                List <Record> successRecords = new List<Record>();
                                if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                                {
                                    termId = group.Key;
                                    var trainingTermPredict = TrainingTermDao.GetTrainingTerm(termId);
                                    termName = trainingTermPredict.Name;
                                    successRecords = await HandleRecordsWithResult(groupedRecords, termId, termName);
                                    successIds = successRecords.Select(a => a.Id).ToList();
                                }
                                else
                                {
                                    successRecords = await HandleRecordsWithResult(groupedRecords, termId, termName);
                                    successIds = successRecords.Select(a => a.Id).ToList();
                                }
                                if (successIds.Count > 0)
                                {
                                    if (mChangeTermType == ChangeTermType.AIMAChangeTerm)
                                    {
                                        if (trainingTerm != null && MLTermStatusHelper.ActiveTermStatus.Contains(trainingTerm.Status))
                                        {
                                            var perviousTermId = Guid.Empty;
                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                            {
                                                perviousTermId = rec.TermId;
                                                rec.TermId = AutoJobOption != AutoJobOption.SkipAndKeep ? termId : rec.TermId;
                                                rec.TermName = AutoJobOption != AutoJobOption.SkipAndKeep ? termName : rec.TermName;
                                                rec.RuleId = Guid.Empty;
                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();

                                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                                rec.MLClassificationType = GetMLClassificationType();
                                                rec.TrainingAddType = GetTrainingAddType();
                                                rec.TrainingScope = (int)MLFileStatus.NotTrain;
                                                rec.TrainingTermId = termId;
                                                if(base.isNewLogicAccount && perviousTermId != rec.TermId && IsContainerEnableClassification(rec.ContainerId)) rec.RemoveManualFields();
                                            });
                                            var trainingScopeCount = ExplorerDao.QueryCount(r => r.TrainingTermId == termId);
                                            var updateTrainingTerm = TrainingTermDao.Find(t => t.Id == termId);
                                            if (updateTrainingTerm != null && MLTermStatusHelper.ActiveTermIntStatus.Contains(updateTrainingTerm.Status))
                                            {
                                                updateTrainingTerm.TrainingScopeCount = trainingScopeCount;
                                                await TrainingTermDao.UpdateAsync(updateTrainingTerm);
                                            }
                                        }
                                        else
                                        {
                                            var perviousTermId = Guid.Empty;
                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                            {
                                                rec.TermId = AutoJobOption != AutoJobOption.SkipAndKeep ? termId : rec.TermId;
                                                rec.TermName = AutoJobOption != AutoJobOption.SkipAndKeep ? termName : rec.TermName;
                                                rec.RuleId = Guid.Empty;
                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
                                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                                rec.MLClassificationType = GetMLClassificationType();
                                                if(base.isNewLogicAccount && perviousTermId != rec.TermId && IsContainerEnableClassification(rec.ContainerId)) rec.RemoveManualFields();
                                            });
                                        }
                                    }
                                    else if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                                    {
                                        var perviousTermId = Guid.Empty;
                                        foreach (var tempSuccess in successRecords)
                                        {
                                            var tempTermName = "";
                                            tempSuccess.TermId = AutoJobOption != AutoJobOption.SkipAndKeep ? tempSuccess.PredictTermId : tempSuccess.TermId;
                                            tempSuccess.TermName = AutoJobOption != AutoJobOption.SkipAndKeep ? tempTermName : tempSuccess.TermName;
                                            tempSuccess.RuleId = Guid.Empty;
                                            tempSuccess.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                            tempSuccess.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                            tempSuccess.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                            tempSuccess.RecordOwner_Array = tempSuccess.RecordOwner.ExplorerSearchSplit();

                                            tempSuccess.MLApprovalStatus = GetMLApprovalStatus();
                                            tempSuccess.MLClassificationType = GetMLClassificationType();
                                            if(base.isNewLogicAccount && perviousTermId != tempSuccess.TermId && IsContainerEnableClassification(tempSuccess.ContainerId))
                                                tempSuccess.RemoveManualFields();
                                        }
                                        ExplorerDao.BatchUpdate(successRecords, 5);
                                    }

                                }
                            }
                        }
                        else
                        {
                            await HandleRecords(records, changeTermInfo.TargetTermUniqueId, changeTermInfo.TargetTermName);

                        }
                        var overwriteSuccessful = SucceedItems.Except(CannotOverwriteLabelRecords).ToList();
                        var partialSuccessful = CannotOverwriteLabelRecords.ToList();

                        if (overwriteSuccessful.Any())
                        {
                            RecordsHistoryService.AddRecordsHistory(overwriteSuccessful.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_ChangeLabel", changeTermInfo.Comment);
                            logger.Info($"[Change Label] {overwriteSuccessful.Count} records were successfully overwritten.");
                        }

                        if (partialSuccessful.Any())
                        {
                            RecordsHistoryService.AddRecordsHistory(partialSuccessful.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_SuccessNoOverwrite", changeTermInfo.Comment);
                            logger.Warn($"[Change Label] {partialSuccessful.Count} records were successfully reclassified but could not overwrite labels.");
                        }
                        if (FailedCount > 0)
                        {
                            if (InvalidRecordCache.IsNotNullOrEmpty())
                            {
                                foreach (var invalidRecords in InvalidRecordCache)
                                {
                                    RecordsHistoryService.AddRecordsHistory(invalidRecords.Value.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeLabelErrorMessage", I18NEntity.GetString(invalidRecords.Key));
                                }
                            }
                            else
                            {
                                RecordsHistoryService.AddRecordsHistory(FailedItems.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeLabelErrorMessage");
                            }
                            logger.Error($"[Change Label] {FailedCount} records failed to reclassify.");
                        }

                        if (FailedCount == 0)
                        {
                            RMRecordsUpdateTempDao.InsertUpdateTemp(jobId, "", RecordsConstants.Explorer_RealTime_Finished);
                            logger.Info($"[Change Label] Job {jobId} completed successfully.");
                        }
                        else if (SucceedCount > 0)
                        {
                            string failedNames = string.Join(";", FailedItems.Select(r => r.LeafName));
                            RMRecordsUpdateTempDao.InsertUpdateTemp(jobId, failedNames, RecordsConstants.Explorer_RealTime_Failed_Partial);
                            logger.Warn($"[Change Label] Job {jobId} completed with partial failures.");
                        }
                        else
                        {
                            RMRecordsUpdateTempDao.InsertUpdateTemp(jobId, "", RecordsConstants.Explorer_RealTime_Failed_All);
                            logger.Error($"[Change Label] Job {jobId} completed with all failures.");
                        }
                        logger.Info($"[Change Label] Job {jobId} completed");
                    }
                }
            }
            catch (Exception ex)
            {
                RMRecordsUpdateTempDao.InsertUpdateTemp(jobId, "", RecordsConstants.Explorer_RealTime_Failed_All);
                logger.Error($"[Change Label] Change label error in Job ID {jobId}: {ex}");
                throw;
            }
            finally
            {
                logger.Info("Change label action finish {0}", jobId);
            }
        }

        public async Task ChangeLabelGoogleForGlobalSearchAsync(ChangeTermOption changeTermInfo, string jobId)
        {
            try
            {
                using (new PerformanceScope("RMGoogleDriveExplorerUtility.ChangeLabelForGlobalSearch"))
                {
                    logger.Info($"Change label action start for Job ID: {jobId}");
                    this.ChangeTermInfo = changeTermInfo;
                    var records = new List<Record>();
                    ReportManager.StartUpdateJobProgress();
                    if (changeTermInfo.GoogleDriveRecordIds != null && changeTermInfo.GoogleDriveRecordIds.Count > 0)
                    {
                        var startTime = DateTime.Now;
                        using (new PerformanceScope("ChangeLabel.GetRecordsForGlobalSearch"))
                        {
                            records = ExplorerDao.QueryAll(r => changeTermInfo.GoogleDriveRecordIds.Contains(r.Id)
                                                  && (r.RecordStatus != (int)RMRecordStatus.RMDeleted &&
                                                  r.RecordStatus != (int)RMRecordStatus.Destroyed)).ToList();

                            logger.Warn($"[Change Label] 1 Time elapsed for querying {records.Count} records from Cosmos: {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        }
                        try
                        {
                            await InitAsync(ChangeTermInfo.TargetTermUniqueId.ToString());
                            logger.Info($"[Change Label] Job {jobId} initial successful.");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"[Change Label] Skipped applying label with name : {changeTermInfo.TargetTermName} and Id : {changeTermInfo.TargetTermUniqueId} because of an error. {ex.Message}");
                            RecordsHistoryService.AddRecordsHistory(changeTermInfo.GoogleDriveRecordIds, "RM_JS_Audit_ChangeLabelErrorMessage");
                            return;
                        }
                        await HandleRecords(records, ChangeTermInfo.TargetTermUniqueId, ChangeTermInfo.TargetTermName);

                        var dto = ConverLabelOptiontoDto(changeTermInfo);
                        AddProcessReclassifyItemsToHistory(dto);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error($"[Change Label] Change label error in Job ID {jobId}: {ex}");
                throw;
            }
            finally
            {
                logger.Info("Change label action finish {0}", jobId);
            }
        }

        public override async Task JobReportSuccessfulAction(Record record, Guid sourceTermId)
        {
            SucceedItems.Add(record);
            if (_prefixReclassifyJobIds.Any(id => _jobId.StartsWith(id)))
            {
                AddSucceedDetail(record, sourceTermId);
            }
            if (_jobId.Contains("MAAP"))
            {
                ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                {
                    ObjectName = record.LeafName,
                    FullPath = GetFullPath(record),
                    Action = "RM_MA_Approve",
                    Status = JobDetailsStatus.Successful,
                    Comment = string.Empty,
                    Type = "RM_JS_Rule_ObjectLevel_GoogleFile"
                });
            }

            await Task.CompletedTask;
        }
        public override async Task JobReportFailedAction(Record record, Exception ex)
        {
            FailedItems.Add(record);
            if (_prefixGlobalSearchJobIds.Any(id => _jobId.StartsWith(id)))
            {
                AddFailedDetail(record, ex);
            }
            else
            {
                CacheInvalidRecord(ex, record);
            }
            await Task.CompletedTask;
        }
        public override async Task JobReportSkipAction(Record record, Exception ex)
        {
            AddSkipDetail(record, ex);
            CannotOverwriteLabelRecords.Add(record);
            await Task.CompletedTask;
        }

        private ChangeTermDto ConverLabelOptiontoDto(ChangeTermOption option)
        {
            return new ChangeTermDto()
            {
                GoogleDriveRecordIds = option.GoogleDriveRecordIds,
                Comment = option.Comment,
                TermInfo = new TargetTermInfo()
                {
                    UniqueId = option.TargetTermUniqueId,
                    Name = option.TargetTermName
                },
                OverWriteSubFiles = option.OverWriteSubFiles,
                ReclassifySubFiles = option.ReclassifySubFiles,
                UserId = option.LogonUser,
            };
        }

        protected int GetMLApprovalStatus()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)RMMLApprovalStatus.Rejected,
                ChangeTermType.AIMADirectlyApprove => (int)RMMLApprovalStatus.Approved,
                _ => (int)RMMLApprovalStatus.None
            };
        }

        protected int GetMLClassificationType()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)RMMLClassificationType.ManualClassified,
                ChangeTermType.AIMADirectlyApprove => (int)RMMLClassificationType.AutoClassfied,
                _ => (int)RMMLClassificationType.None
            };
        }

        protected int GetTrainingAddType()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)TrainingAddType.Reclassify,
                _ => (int)TrainingAddType.None
            };
        }
    }
}