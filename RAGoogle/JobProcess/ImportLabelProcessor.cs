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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Google.Apis.DriveLabels.v2.Data;
using RAGoogle.Extension;
using RAGoogle.Models.Contract;
using RAGoogle.Services;
using RAGoogle.Util;

namespace RAGoogle.JobProcess
{
    public class ImportLabelProcessor : BaseProcessor
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(ImportLabelProcessor));

        #region properties

        private readonly string _termGroupId;
        private readonly GoogleLabelApi _googleLabelApi;
        private readonly GoogleLabelInfoSubscription _labelSubcription;

        private readonly ITermGroupDao _termGroupDao = PlatformWindsorManager.GetService<ITermGroupDao>();
        private readonly ITermSetDao _termSetDao = PlatformWindsorManager.GetService<ITermSetDao>();
        private readonly ITermDao _termDao = PlatformWindsorManager.GetService<ITermDao>();

        #endregion

        public ImportLabelProcessor(string jobId, JobType jobType, string termGroupId) : base(jobId, jobType)
        {
            this.jobType = jobType;
            ReportCenter.InitCurrentJobInfo(jobId, jobType);
            _termGroupId = termGroupId;
            _googleLabelApi = new();
            _labelSubcription = new(_googleLabelApi);
        }

        public override async Task RunNowAsync(RMGoogleSetting? setting, GoogleDriveTreeNodeDto? node)
        {
            logger.Info("Start to import term from Google.");
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    if (_termGroupId.IsNullOrEmpty())
                    {
                        throw new Exception("RM_TM_TermGroupIsNull");
                    }
                    RMTermGroup termGroup = GetTermGroupById();
                    var tenantIds = termGroup.GoogleTermSyncOption switch
                    {
                        TermSyncOption.All => RMAosApiClient.GetGoogleTenantIds(TenantLocalValue.LogonGroupId),
                        TermSyncOption.Specified => await _termGroupDao.GetSpecifiedGoogleTenants(new Guid(_termGroupId)),
                        _ => throw new NotSupportedException(nameof(termGroup.GoogleTermSyncOption)),
                    };
                    foreach (var tenantId in tenantIds)
                    {
                        List<GoogleAppsDriveLabelsV2Label> publishedLabels = await GetAllLabelsAsync(tenantId);
                        ProcessImportLabels(publishedLabels, termGroup, tenantId);
                    }
                }
            }
            catch (JobStopException)
            {
                ReportCenter.JobHasStopped = true;
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while running import label job. Error:{e.ToString()}");
                throw;
            }
        }

        private void ProcessImportLabels(List<GoogleAppsDriveLabelsV2Label> labels, RMTermGroup termGroup, string tenantId)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    string detailAction = string.Empty;
                    string detailMessage = string.Empty;
                    int termSetId = CreateAndGetTermSet(termGroup);
                    string curTermName;

                    Dictionary<string, (Guid termId, string termName)> newTermList = [];

                    foreach (var label in labels)
                    {
                        try
                        {
                            _googleLabelApi.SetGoogleLabel(label);
                            RMGoogleLabelInfo labelInfo = _labelSubcription.GetGoogleLabelInfo();
                            bool isTermExist = _termDao.CheckTermExistByLabelId(label.Id, termGroup.UniqueId, out int curTermId);
                            if (!isTermExist)
                            {
                                detailAction = "RM_TS_Action_New";
                                detailMessage = "RM_JS_TM_TermImport_ItemIsTerm";
                                var termDto = new TermInfo()
                                {
                                    TermName = label.Properties.Title,
                                    TermSetId = termSetId,
                                    Description = label.Properties.Description,
                                };
                                var term = _termDao.CreateGoogleTerm(termDto, labelInfo);
                                var currentTermUniqueId = term.UniqueId;
                                curTermName = term.Name;
                                newTermList.Add(labelInfo.LabelId, (currentTermUniqueId, curTermName));
                            }
                            else
                            {
                                RMTerm oldTerm = _termDao.GetRMTermByTermId(curTermId, false);
                                bool isDeprecated = GoogleLabelExtension.ConvertState(label.Lifecycle.State) == State.Disabled;
                                if (IsSkip(label, oldTerm, isDeprecated))
                                {
                                    detailAction = "RM_TS_Action_Skip";
                                    detailMessage = "RM_TS_ITS_ExistTerm";
                                    curTermName = oldTerm.Name;
                                }
                                else
                                {
                                    detailAction = "RM_TS_Action_Update";
                                    detailMessage = "RM_JS_TM_TermUpdate_ItemIsTerm";
                                    var termDto = new TermInfo()
                                    {
                                        TermName = label.Properties.Title,
                                        TermSetId = oldTerm.IsRemoved ? termSetId : oldTerm.TermSetId,
                                        Description = label.Properties.Description,
                                    };
                                    RMTerm newTerm = _termDao.UpdateGoogleTerm(curTermId, false, termDto, labelInfo);
                                    UpdateExistRecords(newTerm);
                                    curTermName = newTerm.Name;
                                }
                            }

                            switch (detailAction)
                            {
                                case "RM_TS_Action_Skip":
                                    ReportCenter.AddJobDetail(GenerateJobDetail(curTermName, detailAction, JobDetailsStatus.Skipped, detailMessage));
                                    break;
                                case "RM_TS_Action_Update":
                                    ReportCenter.AddJobDetail(GenerateJobDetail(curTermName, detailAction, JobDetailsStatus.Successful, detailMessage));
                                    break;
                                default:
                                    ReportCenter.AddJobDetail(GenerateJobDetail(curTermName, detailAction, JobDetailsStatus.Successful, detailMessage));
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Import label [{0}] error: {1}", label.Properties.Title, ex.ToString());
                            ReportCenter.AddJobDetail(GenerateJobDetail(label.Properties.Title, detailAction, JobDetailsStatus.Failed, ex.Message));
                        }
                    }
                    ProcessDeletedLabels(labels.Select(l => l.Id).ToList(), termGroup.UniqueId, tenantId);
                    UpdateDeletedTermToNewTermInRecord(tenantId, newTermList);
                }
            }
            catch (JobStopException)
            {
                ReportCenter.JobHasStopped = true;
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while importing labels. Error: {0}", ex.Message);
                throw;
            }
        }

        private void UpdateDeletedTermToNewTermInRecord(string tenantId, Dictionary<string, (Guid termId, string termName)> newTermLabelList)
        {
            Dictionary<Guid, string> deletedTermAndLabelList = _termDao.GetAllDeletedTermAndLabelByTenantId(tenantId).GetAwaiter().GetResult();
            if (deletedTermAndLabelList.Count == 0)
            {
                return;
            }
            var allRecordsWithDeletedTerms = RecordManager.GetRecordsByTermIds(deletedTermAndLabelList.Keys);
            if (allRecordsWithDeletedTerms.Count == 0)
            {
                return;
            }
            foreach (var recordWithDeletedTerm in allRecordsWithDeletedTerms)
            {
                logger.Info($"Update record id {recordWithDeletedTerm.Id} with name {recordWithDeletedTerm.LeafName}");
                if (deletedTermAndLabelList.TryGetValue(recordWithDeletedTerm.TermId, out var labelId) && newTermLabelList.TryGetValue(labelId, out var newTermInfo))
                {
                    recordWithDeletedTerm.TermId = newTermInfo.termId;
                    recordWithDeletedTerm.TermName = newTermInfo.termName;
                    logger.Info($"Update record id {recordWithDeletedTerm.Id} to new term info id: {newTermInfo.termId} and {newTermInfo.termName}");
                }
            }
            var failedRecordIds = RecordManager.UpdateTermInfoInRecord(allRecordsWithDeletedTerms);
            if (failedRecordIds.Count > 0)
            {
                logger.Error($"Update deleted google term to new google term failed to records: {string.Join(',', failedRecordIds)}");
            }
        }

        private void ProcessDeletedLabels(List<string> availableLabelIds, Guid termGroupUniqueId, string tenantId)
        {
            if (availableLabelIds.IsNullOrEmpty())
            {
                return;
            }
            var deletedLabelUniqueIds = _termDao.GetDeletedLableUniqueIds(tenantId, termGroupUniqueId, availableLabelIds);
            foreach (var labelUniqueId in deletedLabelUniqueIds)
            {
                logger.Info($"Label is deleted, change label state in Opus to deleted. LabelId: {labelUniqueId}");
                _termDao.UpdateLabelState(labelUniqueId, State.Deleted);
            }
        }

        private async Task<List<GoogleAppsDriveLabelsV2Label>> GetAllLabelsAsync(string tenantId)
        {
            List<GoogleAppsDriveLabelsV2Label> publishedLabels;
            List<GoogleAppsDriveLabelsV2Label> draffLabels;
            var appInfo = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, tenantId);
            using (GoogleLabelService service = new(appInfo))
            {
                draffLabels = await service.ListDraffLabelsAsync();
                publishedLabels = await service.ListLabelsPublishedAsync();
                publishedLabels = publishedLabels.Where(l =>
                    (GoogleLabelExtension.ConvertState(l.Lifecycle.State) == State.Published
                    || GoogleLabelExtension.ConvertState(l.Lifecycle.State) == State.Disabled) &&
                    !draffLabels.Any(d => d.Id == l.Id)).ToList();
                foreach (var draffLabel in draffLabels)
                {
                    ReportCenter.AddJobDetail(GenerateJobDetail(draffLabel.Properties.Title, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_HasUnpublishedChangesLabel"));
                }
            }
            return publishedLabels;
        }

        private RMTermGroup GetTermGroupById()
        {
            RMTermGroup termGroup = _termGroupDao.GetTermGroupByGuid(new Guid(_termGroupId));
            if (termGroup == null)
            {
                logger.Error("Term group is invalid. Term group id: {0}", _termGroupId);
                throw new Exception("RM_TM_TermGroupIsNull");
            }
            else return termGroup;
        }

        private int CreateAndGetTermSet(RMTermGroup termGroup)
        {
            if (termGroup != null)
            {
                RMTermSet termSet = _termSetDao.GetGoogleTermSetByGroupUniqueId(termGroup.UniqueId);
                if (termSet == null)
                {
                    termSet = _termSetDao.CreateGoogleTermSet(I18NEntity.GetString(I18NResource.DefaultGoogleTermSet), termGroup.UniqueId).Result;
                    return termSet.Id;
                }
                else return termSet.Id;
            }
            else
            {
                throw new Exception(string.Format(I18NEntity.GetString("Import term set error.There is no termGroup.Term Group Name:[{0}]"), termGroup?.Name));
            }
        }

        private void UpdateExistRecords(RMTerm newTerm)
        {
            var records = RecordManager.GetRecordsByTermId(newTerm.UniqueId);
            var neededUpdateNameRecords = records.Where(record => record.TermName != newTerm.Name).ToList();
            if (neededUpdateNameRecords.Count == 0)
            {
                return;
            }
            neededUpdateNameRecords.ForEach(record =>
            {
                logger.Info($"Update term name in record id {record.Id}, original term name: {record.TermName}, new term name: {newTerm.Name}");
                record.TermName = newTerm.Name;
            });
            var failedRecordIds = RecordManager.UpdateTermInfoInRecord(records);
            if (failedRecordIds.Count > 0)
            {
                logger.Error($"Update google term name failed to records: {string.Join(',', failedRecordIds)}");
            }
        }

        private bool IsSkip(GoogleAppsDriveLabelsV2Label label, RMTerm oldTerm, bool isDeprecated)
        {
            if ((oldTerm.Name.Equals(label.Properties.Title) || oldTerm.Name.Equals($"{label.Properties.Title}_{oldTerm.Id}")) &&
                label.Properties.Description == oldTerm.Description && isDeprecated == oldTerm.IsDeprecated && !oldTerm.IsRemoved)
            {
                return true;
            }
            return false;
        }

        private JMImportTermDetail GenerateJobDetail(string name, string action, JobDetailsStatus status, string comment = "")
        {
            return new()
            {
                Action = action,
                Term = name,
                Status = status,
                Comment = comment
            };
        }
    }
}
