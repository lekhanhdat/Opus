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
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Google;
using Google.Apis.DriveLabels.v2.Data;
using RAGoogle.Extension;
using RAGoogle.Models.Enums;
using RAGoogle.Services;

namespace RAGoogle.JobProcess
{
    public class SyncTermToGoogleProcessor : BaseProcessor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SyncTermToGoogleProcessor));
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private List<RMTerm> mTermsDao = [];
        private List<RMGoogleLabelInfo> mTermsInfoDao = [];
        private List<GoogleAppsDriveLabelsV2Label> mLabelGoogle = [];
        private List<GoogleAppsDriveLabelsV2Label> mDraftLabelGoogle = [];
        private Dictionary<string, string> aosGoogleTenants;
        private JobType jobType;
        private readonly List<Guid> _termGroupUniqueIds;

        public SyncTermToGoogleProcessor(string jobId, JobType jobType, List<Guid> termGroupUniqueId) : base(jobId, jobType)
        {
            this.jobType = jobType;
            ReportCenter.InitCurrentJobInfo(jobId, jobType);
            _termGroupUniqueIds = termGroupUniqueId;
        }

        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();

        public override async Task RunNowAsync(RMGoogleSetting? setting, GoogleDriveTreeNodeDto? node)
        {
            logger.Info("Start to sync term to Google.");
            try
            {
                aosGoogleTenants = await RMAosApiClient.GetGoogleTenants(TenantLocalValue.LogonGroupId);

                foreach (var termGroupUniqueId in _termGroupUniqueIds)
                {
                    using (CheckJobStopScope jScope = new())
                    {
                        if (termGroupUniqueId == Guid.Empty) throw new Exception("RM_TM_TermGroupIsNull");
                        var termGroup = TermGroupDao.GetTermGroupByGuid(termGroupUniqueId);
                        var tenantIds = termGroup.GoogleTermSyncOption switch
                        {
                            TermSyncOption.All => RMAosApiClient.GetGoogleTenantIds(TenantLocalValue.LogonGroupId),
                            TermSyncOption.Specified => await TermGroupDao.GetSpecifiedGoogleTenants(termGroupUniqueId),
                            _ => throw new NotSupportedException(nameof(termGroup.GoogleTermSyncOption)),
                        };
                        foreach (var tenantId in tenantIds)
                        {
                            var existTenant = aosGoogleTenants.FirstOrDefault(aosGoogleTenants => aosGoogleTenants.Key == tenantId);
                            if (existTenant.Key != null)
                            {
                                await Initialize(tenantId, termGroupUniqueId);
                                await SyncTermToGoogle(existTenant);
                            }
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while running sync term to google. Error:{e.ToString()}");
                throw;
            }
        }
        private async Task SyncTermToGoogle(KeyValuePair<string, string> tenantAOS)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    var appInfo = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, tenantAOS.Key);
                    using (GoogleLabelService service = new(appInfo))
                    {
                        List<RMGoogleLabelInfo> termUpdateds = [];

                        var termDel = mTermsDao.Where(term => term.IsRemoved).ToList();

                        var termNeedDelete = TermNeedDelete();

                        var termNeedDisable = TermNeedDisable();

                        var (labelDelSuccessfullCount, labelRemoveDB) = await HandelDeleteTermsAsync(service, termNeedDelete, tenantAOS.Value);

                        var (handleAdd, termNeedCreate) = TermNeedCreate(labelDelSuccessfullCount, termNeedDisable);

                        var termNeedUpdate = mTermsDao.Except(termDel).Except(termNeedCreate).Except(termNeedDisable).ToList();

                        await HandleDisableTermsAsync(service, termNeedDisable, tenantAOS.Value, termUpdateds);
                        List<RMGoogleLabelInfo> termCreateds = [];
                        if (handleAdd)
                        {
                            termCreateds = await HandleCreateTermAsync(service, termNeedCreate, tenantAOS.Value);
                        }
                        else
                        {
                            var labelExist = mLabelGoogle.Count + mDraftLabelGoogle.Count - labelDelSuccessfullCount;
                            string message = string.Format(I18NEntity.GetString("RM_TS_LimitLabelExceeded"), termNeedCreate.Count, tenantAOS.Value, labelExist);
                            var termLimit = termNeedCreate.Select(term => term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantAOS.Value, "RM_TS_Action_New", message));
                            ReportCenter.AddJobDetails(termLimit, (int)RMNodeLevel.GoogleFile);
                        }

                        await HandleUpdateTermAsync(service, termNeedUpdate, tenantAOS.Value, termUpdateds);

                        termNeedDisable.Where(t => !t.IsDeprecated).ToList().ForEach(t => t.IsDeprecated = true);

                        List<RMTerm> termUpdatestoDB = new(termNeedCreate);
                        termUpdatestoDB.AddRange(termNeedDisable);

                        UpdateRMGoogleLabelInfoToDB(termCreateds, termUpdateds, labelRemoveDB, termUpdatestoDB);

                    }
                }
            }
            catch (JobStopException)
            {
                logger.Warn("The job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred during syncing terms to google. Error: {ex}");
                throw;
            }
        }
        private async Task Initialize(string tenantId, Guid termGroupUniqueId)
        {
            try
            {
                var appInfo = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, tenantId);

                using (GoogleLabelService service = new(appInfo))
                {
                    mDraftLabelGoogle = await service.ListDraftLabelsAsync();
                    mLabelGoogle = await service.ListLabelsPublishedAsync();
                }

                mLabelGoogle = mLabelGoogle.Where(x => GoogleLabelExtension.ConvertState(x.Lifecycle.State) == State.Published
                                                       || GoogleLabelExtension.ConvertState(x.Lifecycle.State) == State.Disabled).ToList();

                mTermsDao = TermDao.GetTermByTermGroupIdIncludeTermRemoved(termGroupUniqueId);

                var mTermsDaoIds = mTermsDao.Select(term => term.UniqueId).ToList();
                mTermsInfoDao = GoogleLabelInfoDao.GetGoogleTermsInforByTenantIdAndTermUniqueIds(tenantId, mTermsDaoIds);
            }
            catch (Exception ex)
            {
                logger.Error("error message {0}", ex.ToString());
                throw;
            }
        }
        private async Task<(int count, List<RMGoogleLabelInfo>)> HandelDeleteTermsAsync(GoogleLabelService service, List<RMTerm> terms, string tenantUrl)
        {
            List<RMGoogleLabelInfo> labelInfors = [];
            var count = 0;
            foreach (var term in terms)
            {
                logger.Info($"processing delete label name :{term.Name}");
                try
                {
                    using (CheckJobStopScope jScope = new())
                    {
                        var labelInfo = GetLabelInfoByTermUniqueId(term.UniqueId);
                        var labelgoogle = mLabelGoogle.FirstOrDefault(labelgoogle => labelgoogle.Id == labelInfo?.LabelId);

                        if (labelgoogle != null)
                        {
                            await service.DeleteTermToGoogleAsync(labelgoogle);
                            labelInfors.Add(labelInfo);
                            count++;
                            ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Successful, tenantUrl, "RM_TS_Action_Delete", string.Empty), (int)RMNodeLevel.GoogleFile);
                        }
                        else
                        {
                            labelInfors.Add(labelInfo);
                            ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Skipped, tenantUrl, "RM_TS_Action_Delete", "RM_TS_LabelDelInGoogle"), (int)RMNodeLevel.GoogleFile);
                        }
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (GoogleApiException ex)
                {
                    if (ex.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_Delete", "RM_TS_LabelLocked"), (int)RMNodeLevel.GoogleFile);
                    }
                    logger.Error($"processing delete label name :{term.Name}, occur error " + ex.Message);
                }
                catch (Exception ex)
                {
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_Delete", ex.Message), (int)RMNodeLevel.GoogleFile);
                    logger.Error($"processing delete label name :{term.Name}, occur error " + ex.Message);
                }
            }
            return (count, labelInfors);
        }

        private async Task HandleDisableTermsAsync(GoogleLabelService service, List<RMTerm> terms, string tenantUrl, List<RMGoogleLabelInfo> rMGoogleLabelInfo)
        {
            foreach (var term in terms)
            {
                logger.Info($"processing disable label name :{term.Name}");
                try
                {
                    using (CheckJobStopScope jScope = new())
                    {
                        var labelInfo = GetLabelInfoByTermUniqueId(term.UniqueId);
                        if (labelInfo != null)
                        {
                            var labelgoogle = mLabelGoogle.FirstOrDefault(labelgoogle => labelgoogle.Id == labelInfo.LabelId);
                            var (jobStatus, message) = await UpdateTermToGoogleAsync(service, term, tenantUrl, labelgoogle, labelInfo, true);
                            HandleUpdateTermJobDetail(jobStatus, message, term, rMGoogleLabelInfo, labelInfo, tenantUrl);
                        }
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Google.GoogleApiException ex)
                {
                    if (ex.HttpStatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        string message = "RM_TS_ResourcesHasBeenModified";
                        ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_Update", message), (int)RMNodeLevel.GoogleFile);
                    }
                    logger.Error($"processing disable label name :{term.Name}, occur error " + ex.Message);
                }
                catch (Exception ex)
                {
                    string message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_Update", message), (int)RMNodeLevel.GoogleFile);
                    logger.Error($"processing disable label name :{term.Name}, occur error " + ex.Message);
                }
            }
        }
        private async Task<(bool, GoogleAppsDriveLabelsV2Label)> EnableTermToGoogle(GoogleLabelService service, RMGoogleLabelInfo labelInfo, GoogleAppsDriveLabelsV2Label labelGoogle, RMTerm term, string tenantUrl)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    if (GoogleLabelExtension.ConvertState(labelGoogle.Lifecycle.State) == State.Disabled)
                    {
                        var updatedLabelGoogle = await service.EnableTermToGoogle(term, labelGoogle.Name, labelInfo);
                        labelGoogle.Lifecycle.State = updatedLabelGoogle.Lifecycle.State;
                        return (true, updatedLabelGoogle);
                    }
                }
                return (false, labelGoogle);
            }
            catch (JobStopException)
            {
                logger.Warn("The job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred during enable label when sync to tenant google {tenantUrl}. Error: {ex}");
                throw;
            }
        }

        private async Task<List<RMGoogleLabelInfo>> HandleCreateTermAsync(GoogleLabelService service, List<RMTerm> terms, string tenantUrl)
        {
            List<RMGoogleLabelInfo> rMGoogleLabelInfos = [];
            foreach (var term in terms)
            {
                logger.Info($"processing create label name :{term.Name}");
                try
                {
                    await CreateTermToGoogleAsync(service, term, tenantUrl, rMGoogleLabelInfos);
                }
                catch (GoogleApiException ex)
                {
                    string message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_New", message), (int)RMNodeLevel.GoogleFile);
                    logger.Error($"processing create label name :{term.Name}, occur error " + ex.Message);
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception ex)
                {
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_New", "RM_RDM_Explorer_ChangeLabel_All_Failed"), (int)RMNodeLevel.GoogleFile);
                    logger.Error($"processing create label name :{term.Name}, occur error " + ex.Message);
                }
            }
            return rMGoogleLabelInfos;
        }
        private void UpdateRMGoogleLabelInfoToDB(List<RMGoogleLabelInfo> labelInfoCreateds, List<RMGoogleLabelInfo> labelInfoUpdateds, List<RMGoogleLabelInfo> labelInfoDeletes, List<RMTerm> termUpdates)
        {
            if (labelInfoCreateds.Count > 0 || labelInfoUpdateds.Count > 0 || termUpdates.Count > 0 || labelInfoDeletes.Count > 0)
            {
                try
                {
                    using (CheckJobStopScope subJScope = new())
                    {
                        GoogleLabelInfoDao.BatchCreate(labelInfoCreateds);
                        GoogleLabelInfoDao.BatchUpdate(labelInfoUpdateds);
                        GoogleLabelInfoDao.BatchDelete(labelInfoDeletes);
                        TermDao.BatchUpdate(termUpdates);
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occurred during update information of label from google to local when sync to google. Error: {ex}");
                    throw;
                }
            }
        }
        private async Task HandleUpdateTermAsync(GoogleLabelService service, List<RMTerm> terms, string tenantUrl, List<RMGoogleLabelInfo> rMGoogleLabelInfoUpdates)
        {
            foreach (var term in terms)
            {
                logger.Info($"processing update label name :{term.Name}");
                try
                {
                    using (CheckJobStopScope jScope = new())
                    {
                        var labelInfo = GetLabelInfoByTermUniqueId(term.UniqueId);
                        var labelGoogle = mLabelGoogle.FirstOrDefault(labelgoogle => labelgoogle.Id == labelInfo.LabelId);


                        if (labelGoogle == null)
                        {
                            continue;
                        }

                        logger.Info($"The term {term.Name} is deprecated: {term.IsDeprecated}");

                        if (term.IsDeprecated)
                        {
                            var (jobStatus, message) = await UpdateTermToGoogleAsync(service, term, tenantUrl, labelGoogle, labelInfo, true);
                            HandleUpdateTermJobDetail(jobStatus, message, term, rMGoogleLabelInfoUpdates, labelInfo, tenantUrl);
                        }
                        else
                        {
                            var (jobStatus, message) = await UpdateTermToGoogleAsync(service, term, tenantUrl, labelGoogle, labelInfo, false, true);
                            HandleUpdateTermJobDetail(jobStatus, message, term, rMGoogleLabelInfoUpdates, labelInfo, tenantUrl);
                        }
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Google.GoogleApiException ex)
                {
                    if (ex.HttpStatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        string message = "RM_TS_ResourcesHasBeenModified";
                        ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_Update", message), (int)RMNodeLevel.GoogleFile);
                    }
                    logger.Error($"processing update label name :{term.Name}, occur error " + ex.Message);
                }
                catch (Exception ex)
                {
                    string message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_Update", message), (int)RMNodeLevel.GoogleFile);
                    logger.Error($"processing update label name :{term.Name}, occur error " + ex.Message);
                }
            }
        }
        private async Task<(JobDetailsStatus, string)> UpdateTermToGoogleAsync(GoogleLabelService service, RMTerm term, string tenantUrl, GoogleAppsDriveLabelsV2Label labelGoogle, RMGoogleLabelInfo labelInfo, bool isDisable, bool isEnable = false)
        {
            using (CheckJobStopScope scope = new())
            {
                var hasUpdate = false;
                if (CheckLabelMaxLengthReached(term))
                {
                    return (JobDetailsStatus.Failed, "RM_TS_LabelMaxLengthReached");
                }

                if (isEnable)
                {
                    (hasUpdate, labelGoogle ) = await EnableTermToGoogle(service, labelInfo, labelGoogle, term, tenantUrl);
                    logger.Info($"The term {term.Name} change status to PUBLISHED, {hasUpdate}");
                }

                var changedTerm = CheckNeedUpdateProperty(term, labelGoogle);

                if (GoogleLabelExtension.ConvertState(labelGoogle.Lifecycle.State) == State.Disabled)
                {
                    if (changedTerm != TermChanged.None)
                    {
                        return (JobDetailsStatus.Skipped, "RM_TS_Skip_LabelDisable");
                    }
                    return (JobDetailsStatus.Skipped, "RM_TS_NoChangeTerm");
                }

                if (changedTerm != TermChanged.None)
                {
                    
                    var updatedLabel = await service.UpdateTermToGoogleAsync(term, labelGoogle, labelInfo, changedTerm);
                    if (isDisable)
                    {
                        await service.DisableTermToGoogle(term, updatedLabel.Name, labelInfo);
                    }
                    logger.Info($"The term {term.Name} updated properties and status DISABLED: {isDisable}");

                    return (JobDetailsStatus.Successful, string.Empty);
                }
                else
                {
                    if (isDisable)
                    {
                        await service.DisableTermToGoogle(term, labelGoogle.Name, labelInfo);
                        hasUpdate = true;
                        logger.Info($"The term {term.Name} change status to DISABLED, {hasUpdate}");
                    }

                    if (hasUpdate)
                    {
                        return (JobDetailsStatus.Successful, string.Empty);
                    }
                    return (JobDetailsStatus.Skipped, "RM_TS_NoChangeTerm");
                }
            }
        }
        private async Task CreateTermToGoogleAsync(GoogleLabelService service, RMTerm term, string tenantUrl, List<RMGoogleLabelInfo> rMGoogleLabelInfos)
        {
            using (CheckJobStopScope scope = new())
            {
                var createdTerm = new RMGoogleLabelInfo();
                if (CheckLabelMaxLengthReached(term))
                {
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_New", "RM_TS_LabelMaxLengthReached"), (int)RMNodeLevel.GoogleFile);
                    return;
                }
                if (term.IsDeprecated)
                {
                    var labelName = await service.CreateTermToGoogleAsync(term, createdTerm);
                    await service.DisableTermToGoogle(term, labelName, createdTerm);
                }
                else
                {
                    await service.CreateTermToGoogleAsync(term, createdTerm);
                }
                if (createdTerm != null)
                {
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Successful, tenantUrl, "RM_TS_Action_New", string.Empty), (int)RMNodeLevel.GoogleFile);
                    rMGoogleLabelInfos.Add(createdTerm);
                }
            }
        }

        private TermChanged CheckNeedUpdateProperty(RMTerm term, GoogleAppsDriveLabelsV2Label labelGoogle)
        {
            var labelInfo = GetLabelInfoByTermUniqueId(term.UniqueId);
            if (labelInfo != null)
            {
                var changedName = CheckNameChange(term, labelInfo.LabelName, labelGoogle.Properties.Title);
                var changedDescription = term.Description != labelGoogle.Properties.Description;
                switch (changedName)
                {
                    case true when changedDescription:
                        return TermChanged.NameAndDescriptionChanged;
                    case true:
                        return TermChanged.NameChanged;
                }

                if (changedDescription)
                {
                    return TermChanged.DescriptionChanged;
                }

            }
            return TermChanged.None;
        }

        private bool CheckNameChange(RMTerm term, string labelNameInfoDao, string labelGoogleName)
        {
            var isDuplicateName = mTermsInfoDao.Where(x => x.LabelName == labelNameInfoDao).Count() > 1;
            var termChange = term.Name;

            if (isDuplicateName)
            {
                var termNameOrginal = $"{labelNameInfoDao}_{term.Id}";
                if (termNameOrginal == term.Name) termChange = labelNameInfoDao;
            }

            if (termChange != labelGoogleName) return true;

            return false;
        }
        private bool CheckLabelMaxLengthReached(RMTerm term)
        {
            if (term.Name.Length > 40 || term.Description?.Length > 255)
            {
                return true;
            }
            return false;
        }

        private (bool, List<RMTerm>) TermNeedCreate(int countTermDel, List<RMTerm> termNeedDisable)
        {
            List<RMTerm> terms = [];
            List<RMGoogleLabelInfo> googleLabelInfos = [];
            var mTermChange = mTermsDao.Except(termNeedDisable).Where(x => !x.IsRemoved).ToList();

            var mLabelGoogleIds = mLabelGoogle.Select(x => x.Id).ToList();
            foreach (var mTerm in mTermChange)
            {
                var labelInfo = GetLabelInfoByTermUniqueId(mTerm.UniqueId);

                if (labelInfo == null || !mLabelGoogleIds.Contains(labelInfo.LabelId)) terms.Add(mTerm);

                if (labelInfo != null && !mLabelGoogleIds.Contains(labelInfo.LabelId)) googleLabelInfos.Add(labelInfo);
            }

            GoogleLabelInfoDao.BatchDelete(googleLabelInfos);

            if (terms.Count + mLabelGoogle.Count + mDraftLabelGoogle.Count - countTermDel > 150)
            {
                return (false, terms);
            }
            return (true, terms);
        }

        private List<RMTerm> TermNeedDisable()
        {
            var termDisable = new List<RMTerm>();
            var mTermDeprecated = mTermsDao.Where(x => !x.IsRemoved);
            var mLabelGoogleIds = mLabelGoogle.Select(x => x.Id).ToList();

            foreach (var mTerm in mTermDeprecated)
            {
                var labelInfo = GetLabelInfoByTermUniqueId(mTerm.UniqueId);

                if (labelInfo != null && mLabelGoogleIds.Contains(labelInfo.LabelId))
                {
                    if (!IsInTime(mTerm.TermExpirationFrom, mTerm.TermExpirationTo) || mTerm.IsDeprecated)
                    {
                        termDisable.Add(mTerm);
                    }
                }
            }

            return termDisable;
        }


        private List<RMTerm> TermNeedDelete()
        {
            var termInforDeleteDB = mTermsInfoDao
                .Where(termInfor => termInfor.State == (int)State.Deleted).ToList();

            var termNeedDelete = mTermsDao
                .Where(term => termInforDeleteDB.Any(info => info.TermUniqueId == term.UniqueId)).ToList();

            var groupTermByTermSetId = termNeedDelete.GroupBy(term => term.TermSetId).ToDictionary(termSet => termSet.Key, terms => terms.ToList());

            var activeTermSetIds = TermDao.GetActiveTermSets(groupTermByTermSetId.Select(group => group.Key).ToList()).GetAwaiter().GetResult();

            return groupTermByTermSetId.Where(group => activeTermSetIds.Contains(group.Key))
                .SelectMany(group => group.Value)
                .ToList();
        }

        private RMGoogleLabelInfo? GetLabelInfoByTermUniqueId(Guid UniqueId)
        {
            return mTermsInfoDao.FirstOrDefault(termInfo => termInfo.TermUniqueId == UniqueId);
        }

        private void HandleUpdateTermJobDetail(JobDetailsStatus jobStatus, string message, RMTerm term, List<RMGoogleLabelInfo> rMGoogleLabelInfoUpdates, RMGoogleLabelInfo labelInfo, string tenantUrl)
        {
            switch (jobStatus)
            {
                case JobDetailsStatus.Successful:
                    rMGoogleLabelInfoUpdates.Add(labelInfo);
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Successful, tenantUrl, "RM_TS_Action_Update", message), (int)RMNodeLevel.GoogleFile);
                    break;
                case JobDetailsStatus.Skipped:
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Skipped, tenantUrl, "RM_TS_Action_Skip", message), (int)RMNodeLevel.GoogleFile);
                    break;
                case JobDetailsStatus.Failed:
                    ReportCenter.AddJobDetail(term.ConvertGoogleTermToJobDetail(JobDetailsStatus.Failed, tenantUrl, "RM_TS_Action_Update", message), (int)RMNodeLevel.GoogleFile);
                    break;
                default:
                    break;
            }
        }
        private bool IsInTime(long TermExpirationFrom, long TermExpirationTo)
        {
            if (TermExpirationFrom == 0 && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom == 0 && TermExpirationTo > DateTime.UtcNow.Ticks)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && DateTime.UtcNow.Ticks <= TermExpirationTo)
            {
                return true;
            }
            return false;
        }
    }
}
