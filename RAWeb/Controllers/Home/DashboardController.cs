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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Dedeplication;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Dashboard;
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Service.Services.RMReport;
using AvePoint.RA.Service.Services.Settings;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Home
{
    [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess, preferred: false)]
    public class DashboardController : BaseApiController
    {
        private IDashboardService _DashboardService;
        private IDashboardService DashboardService => PlatformWindsorManager.GetService(ref _DashboardService);
        private IValueAndSavingsService _valueAndSavingsService;
        private IValueAndSavingsService ValueAndSavingsService => PlatformWindsorManager.GetService(ref _valueAndSavingsService);
        private ISettingProfileService _SettingProfileService;
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService(ref _SettingProfileService);
        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        [HttpPost]
        public async Task<string> GetProfileReport([FromBody] ShowProfilesReportPageInfo pageInfo)
        {
            var result = await RMReportService.GetProfilesAsync(pageInfo);
            if (result?.Profiles != null)
            {
                foreach (var profile in result.Profiles)
                {
                    profile.Extension1 = null;
                    profile.Extension2 = null;
                }
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidReportProfileParameterActionFilter]
        public string GenerateReport([FromBody] RMProfileDto profile)
        {
            if (profile == null || profile.Id <= 0 || !IsSupportedArchivedSiteType(profile.Type))
            {
                return string.Empty;
            }

            if (JobTypeConstants.ArchivedSiteReportJobTypes.Contains((int)profile.Type))
            {
                return RMReportService.StartArchivedSiteReportJob(profile.Id);
            }

            return RMReportService.StartReportJob(profile.Type, profile.Id);
        }

        [HttpPost]
        [ValidCreateReportProfileParameterActionFilter]
        public async Task<RAReturnMessage> CreateProfile([FromBody] RMProfileDto profile)
        {
            if (!TryValidateArchivedSiteProfile(profile, out var validationError))
            {
                return CreateArchivedSiteParameterError(validationError);
            }

            if (!TryNormalizeArchivedSiteProfile(profile, out var treeError))
            {
                return CreateArchivedSiteParameterError(treeError);
            }

            var returnMessage = await RMReportService.BuildProfileAsync(profile);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("An error occurred while creating archived site profile. Name: {0}, Type: {1}, Error: {2}",
                    profile.ProfileName, profile.Type, returnMessage.ErrorMessage);
                return returnMessage;
            }

            await ConfigureArchivedSiteScheduleAsync(profile, true);
            if (!string.IsNullOrWhiteSpace(profile.ScheduleId))
            {
                await RMReportService.UpdateProfileScheduleIdAsync(profile.Id, profile.ScheduleId);
            }
            return returnMessage;
        }

        [HttpPost]
        [ValidEditReportProfileParameterActionFilter]
        public async Task<RAReturnMessage> EditProfile([FromBody] RMProfileDto profile)
        {
            if (!TryValidateArchivedSiteProfile(profile, out var validationError))
            {
                return CreateArchivedSiteParameterError(validationError);
            }

            if (!TryNormalizeArchivedSiteProfile(profile, out var treeError))
            {
                return CreateArchivedSiteParameterError(treeError);
            }

            await ConfigureArchivedSiteScheduleAsync(profile, false);

            var returnMessage = await RMReportService.EidtProfileAsync(profile);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("An error occurred while editing archived site profile. Name: {0}, Type: {1}, Error: {2}",
                    profile.ProfileName, profile.Type, returnMessage.ErrorMessage);
            }
            return returnMessage;
        }

        [HttpPost]
        [ValidReportIdParameterActionFilter]
        public async Task<RMProfileDto> LoadProfileById([FromBody] string id)
        {
            var profile = await RMReportService.GetProfileByIdAsync(id);
            if (profile == null || !IsSupportedArchivedSiteType(profile.Type))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(profile.ScheduleId))
            {
                profile.scheduleInfo = await ScheduleService.GetScheduleByIdAsync(profile.ScheduleId);
            }

            profile.Extension2 = await RestoreArchivedSiteScopeAsync(profile);
            return profile;
        }

        private static bool TryValidateArchivedSiteProfile(RMProfileDto profile, out string errorMessage)
        {
            errorMessage = "Parameter exception";
            if (profile == null || string.IsNullOrWhiteSpace(profile.ProfileName)
                || string.IsNullOrWhiteSpace(profile.Extension2)
                || !IsSupportedArchivedSiteType(profile.Type)
                || !TryValidateArchivedSiteObjectLevel(profile.ObjectLevel))
            {
                return false;
            }

            if (profile.RangeType == TimeRangeType.None)
            {
                return true;
            }

            if (profile.RangeType != TimeRangeType.Custom)
            {
                errorMessage = "The specified time frame is invalid.";
                return false;
            }

            try
            {
                var timeFrame = JObject.Parse(profile.Extension1 ?? string.Empty);
                var start = ReadTimeFrameValue(timeFrame, "StartTime", "StartDateTime");
                var end = ReadTimeFrameValue(timeFrame, "EndTime", "EndDateTime");
                var valid = DateTime.TryParse(start, out var startTime)
                    && DateTime.TryParse(end, out var endTime)
                    && startTime <= endTime;
                if (!valid)
                {
                    errorMessage = "The custom time frame must contain valid StartTime and EndTime values, and StartTime must be earlier than or equal to EndTime.";
                }
                return valid;
            }
            catch (Exception)
            {
                errorMessage = "The custom time frame must be valid JSON containing StartTime and EndTime values.";
                return false;
            }
        }

        private static bool TryValidateArchivedSiteObjectLevel(int? objectLevel)
        {
            return objectLevel == (int)ReportType.AllItem || objectLevel == (int)ReportType.AllSubSite;
        }

        private static string ReadTimeFrameValue(JObject timeFrame, string primaryName, string alternateName)
        {
            return (string)timeFrame[primaryName] ?? (string)timeFrame[alternateName];
        }

        private async Task ConfigureArchivedSiteScheduleAsync(RMProfileDto profile, bool isCreate)
        {
            if (profile.scheduleInfo != null && !profile.scheduleInfo.NoSchedule)
            {
                profile.scheduleInfo.JobCategory = ScheduleType.ArchivedSiteReport;
                profile.scheduleInfo.ProfileId = profile.Id.ToString();
                profile.scheduleInfo.Id = string.IsNullOrWhiteSpace(profile.scheduleInfo.Id)
                    ? (isCreate ? Guid.NewGuid().ToString() : (string.IsNullOrWhiteSpace(profile.ScheduleId) ? Guid.NewGuid().ToString() : profile.ScheduleId))
                    : profile.scheduleInfo.Id;
                profile.ScheduleId = isCreate
                    ? await ScheduleService.CreateScheduleServiceAsync(profile.scheduleInfo)
                    : (string.IsNullOrWhiteSpace(profile.ScheduleId)
                        ? await ScheduleService.CreateScheduleServiceAsync(profile.scheduleInfo)
                        : await ScheduleService.UpdateScheduleServiceAsync(profile.scheduleInfo));
                profile.scheduleInfo.Id = profile.ScheduleId;
                return;
            }

            if (!isCreate && !string.IsNullOrWhiteSpace(profile.ScheduleId))
            {
                ScheduleService.DeleteScheduleService(profile.ScheduleId);
            }

            profile.ScheduleId = null;
            if (profile.scheduleInfo != null)
            {
                profile.scheduleInfo.Id = null;
            }
        }

        [HttpPost]
        [ValidDeleteReportProfileParameterActionFilter]
        public async Task<List<string>> DeleteProfiles([FromBody] DelProfileInfo profileInfo)
        {
            var deleteJobProfileNames = new Dictionary<int, string>();
            for (var index = 0; index < profileInfo.Ids.Count; index++)
            {
                deleteJobProfileNames.Add(profileInfo.Ids[index], profileInfo.Names[index]);
            }
            profileInfo.ProfileNames = deleteJobProfileNames;

            var (_, blockedProfiles) = await RMReportService.DeleteProfilesAsync(profileInfo);
            return blockedProfiles ?? new List<string>();
        }

        private static bool IsSupportedArchivedSiteType(JobType type)
        {
            return JobTypeConstants.ArchivedSiteReportJobTypes.Contains((int)type);
        }

        private static bool IsGoogleType(JobType type)
        {
            return type == JobType.GoogleArchivedSiteReport;
        }

        private static bool TryNormalizeArchivedSiteProfile(RMProfileDto profile, out string errorMessage)
        {
            errorMessage = "Parameter exception";
            if (profile == null || string.IsNullOrWhiteSpace(profile.ProfileName)
                || string.IsNullOrWhiteSpace(profile.Extension2)
                || !IsSupportedArchivedSiteType(profile.Type))
            {
                return false;
            }

            try
            {
                if (IsGoogleType(profile.Type))
                {
                    profile.Extension2 = RuleSPTreeUtil.ConvertGoogleTreeJsonStrToListStr(profile.Extension2);
                }
                else
                {
                    profile.Extension2 = SerializerHelper.SerializeByDataContractSerializer(
                        SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                }
                return true;
            }
            catch (Exception)
            {
                errorMessage = "The selected scope tree format is incompatible with the specified report Type.";
                return false;
            }
        }

        private async Task<string> RestoreArchivedSiteScopeAsync(RMProfileDto profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Extension2))
            {
                return profile.Extension2;
            }

            if (IsGoogleType(profile.Type))
            {
                return RuleSPTreeUtil.BuildGoogleTreeJsonStr(profile.Extension2);
            }

            var util = new ValidReportUtil();
            var treeJson = SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profile.Extension2);

            if (profile.Type == JobType.OneDriveArchivedSiteReport)
            {
                return await util.GetFilteredOneDriveTreeNodesAsync(treeJson, profile.Type);
            }

            if (profile.Type == JobType.TeamsArchivedSiteReport)
            {
                return await util.GetFilteredTeamsTreeNodesAsync(treeJson, profile.Type);
            }

            return await util.GetFilteredSPTreeNodesAsync(treeJson, profile.Type);
        }

        private static RAReturnMessage CreateArchivedSiteParameterError(string errorMessage)
        {
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Exception,
                ErrorMessage = errorMessage
            };
        }

        [HttpPost]
        public Task<bool> IsAdmin()
        {
            return DashboardService.IsAdminAsync();
        }

        [HttpPost]
        public Task<bool> IsSOAdmin()
        {
            return DashboardService.IsSOAdminAsync();
        }

        [HttpPost]
        public Task<bool> IsEndUser()
        {
            return DashboardService.IsEndUserAsync();
        }

        [HttpPost]
        public Task<int> GetEndUserPermission()
        {
            return DashboardService.GetEndUserPermissionAsync();
        }

        [HttpPost]
        public Task<List<DashboardKeyValue<int>>> GetSourceFlags()
        {
            return DashboardQuerier.GetSourceFlagsAsync();
        }

        [HttpPost]
        public Task<List<DashboardKeyValue<int>>> GetManualApprovalStatus()
        {
            return DashboardQuerier.GetManualApprovalStatusAsync();
        }

        [HttpPost]
        [ValidOnlyHasPhyEndUserPermissionFilter]
        public Task<List<RMDashboardTermUsage>> GetTop10TermUsages([FromBody] SourceFlag sourceFlag)
        {
            return DashboardQuerier.GetTop10MostUsedTermsAsync(sourceFlag);
        }

        [HttpPost]
        [ValidOnlyHasPhyEndUserPermissionFilter]
        public Task<List<RMDashboardDataUsage>> GetTop10MostUsedSites([FromBody] SourceFlag sourceFlag)
        {
            return DashboardQuerier.GetTop10MostUsedSitesAsync(sourceFlag);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public List<RMDashboardUserWaitingApprovalCount> GetTop10UserRecordsWaitingApproval([FromBody] SourceFlag flag)
        {
            return DashboardQuerier.GetTop10UserRecordsWaitingApproval(flag);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public List<DashboardLineChartItem> GetLineChartItems([FromBody] LineChartRequestParameter parameter)
        {
            return DashboardQuerier.GetLineChartItems(parameter.Flag, parameter.DateRange);
        }

        [HttpPost]
        [ValidOnlyHasPhyEndUserPermissionFilter]
        public Task<List<DashboardKeyValue<long>>> GetTermApplyRuleUsages()
        {
            return DashboardQuerier.GetTermApplyRuleUsagesAsync();
        }
        
        [HttpPost]
        [ValidOnlyHasPhyEndUserPermissionFilter]
        public List<DashboardKeyValue<long>> GetLabelApplyRuleUsages()
        {
            return DashboardQuerier.GetLabelApplyRuleUsagesAsync();
        }

        [HttpPost]
        public Task<List<DashboardSourceFlagKeyValue>> GetSourcesSettingCount()
        {
            return DashboardQuerier.GetSourcesSettingCountAsync();
        }

        [HttpPost]
        public Task<List<DashboardSourceFlagKeyValue>> GetSourcesActiveCount()
        {
            return DashboardQuerier.GetSourcesActiveCountAsync();
        }

        [HttpPost]
        [ValidOnlyHasPhyEndUserPermissionFilter]
        public Task<List<DashboardKeyValue<long>>> GetManagedRecordsCount()
        {
            return DashboardQuerier.GetManagedRecordsCountAsync();
        }

        [HttpPost]
        [ValidOnlyHasPhyEndUserPermissionFilter]
        public Task<List<DashboardKeyValue<long>>> GetPhysicalRequest()
        {
            return DashboardQuerier.GetPhysicalRequestsAsync();
        }

        [HttpPost]
        public Task<List<DashboardSourceFlagKeyValue>> GetWaitingDisposalApproval()
        {
            return DashboardQuerier.GetWaitingDisposalApprovalAsync();
        }

        [HttpPost]
        public async Task<ValueAndSavingsResponse> GetStorageValueSummary([FromBody] ValueAndSavingsRequest request)
        {
            try
            {
                var result = await ValueAndSavingsService.GetStorageValueSummaryAsync(request);
                return result;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn($"Invalid StorageValueSummary request. Exception: {ex}");
                return null;
            }
        }

        [HttpPost]
        public async Task<ArchivedOverviewResponse> GetArchivedOverview([FromBody] ArchivedOverviewRequest request)
        {
            try
            {
                var result = await ValueAndSavingsService.GetArchivedOverviewAsync(request);
                return result;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn($"Invalid ArchivedOverview request. Exception: {ex}");
                return null;
            }
        }

        [HttpPost]
        public async Task<OptimizationOverviewBySourceResponse> GetOptimizationOverviewBySource([FromBody] OptimizationOverviewBySourceRequest request)
        {
            try
            {
                var result = await ValueAndSavingsService.GetOptimizationOverviewBySourceAsync(request);
                return result;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn($"Invalid OptimizationOverviewBySource request. Exception: {ex}");
                return null;
            }
        }

        [HttpPost]
        public async Task<OptimizationContributionBySourceResponse> GetOptimizationContributionBySource([FromBody] OptimizationContributionBySourceRequest request)
        {
            try
            {
                var result = await ValueAndSavingsService.GetOptimizationContributionBySourceAsync(request);
                return result;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn($"Invalid OptimizationContributionBySource request. Exception: {ex}");
                return null;
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<bool> SaveValueAndSavingsPriceConfiguration([FromBody] ValueAndSavingsPriceConfiguration priceConfiguration)
        {
            try
            {
                return await ValueAndSavingsService.SaveValueAndSavingsPriceConfigurationAsync(priceConfiguration);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while save value and savings price configuration, Error: {e}");
            }

            return false;
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ValueAndSavingsPriceConfiguration> GetValueAndSavingsPriceConfiguration()
        {
            try
            {
                return await ValueAndSavingsService.GetValueAndSavingsPriceConfigurationAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get value and savings price configuration, Error: {e}");
            }

            return new ValueAndSavingsPriceConfiguration();
        }

        [HttpPost]
        public List<DashboardKeyValue<long>> GetMyCreationPhysicalRequest()
        {
            return DashboardQuerier.GetMyCreationPhysicalRequest();
        }

        [HttpPost]
        public List<DashboardKeyValue<long>> GetMyLoanPhysicalRequest()
        {
            return DashboardQuerier.GetMyLoanPhysicalRequest();
        }

        [HttpPost]
        public List<DashboardKeyValue<long>> GetMyMovePhysicalRequest()
        {
            return DashboardQuerier.GetMyMovePhysicalRequest();
        }

        [HttpPost]
        public Task<string> GetLastCollectTime()
        {
            return DashboardQuerier.GetLastCollectTimeAsync();
        }

        [HttpPost]
        public Task<string> GetNextCollectTime()
        {
            return DashboardQuerier.GetNextCollectTimeAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ContentRepositoyAdmin)]
        public DashboardJobCreationStatus RunDashboardCollectJob()
        {
            try
            {
                if (DashboardService.ExistsJobQueue())
                {
                    return DashboardJobCreationStatus.ExistsJobQueue;
                }
                if (DashboardService.HasRunningJob())
                {
                    return DashboardJobCreationStatus.HasRunningJob;
                }

                var creationSuccess = DashboardService.SchduleRunDashboardJob(JobRunBy.Control);
                if (creationSuccess)
                {
                    return DashboardJobCreationStatus.Succeed;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run dashboard collect job. Error: {e}");
            }
            return DashboardJobCreationStatus.Failed;
        }

        [HttpPost]
        public DashboardJobCreationStatus CheckDashboardJobStatus()
        {
            try
            {
                if (DashboardService.ExistsJobQueue())
                {
                    return DashboardJobCreationStatus.ExistsJobQueue;
                }
                if (DashboardService.HasRunningJob())
                {
                    return DashboardJobCreationStatus.HasRunningJob;
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while check dashboard job status. Error: {e}");
            }
            return DashboardJobCreationStatus.None;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin | RMPermissionMasks.ManageHold)]
        public async Task<long> GetPhysicalTermTotal()
        {
            try
            {
                return await DashboardQuerier.GetPhysicalTermTotalAsync();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get physical term total. Error: {e}");
            }
            return 0;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public Task<bool> IsPhysicalEndUser()
        {
            return DashboardQuerier.IsPhysicalEndUserAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<long> GetPhysicalLocationTotal()
        {
            try
            {
                return await DashboardQuerier.GetPhysicalLocationTotalAsync();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get physical location total. Error: {e}");
            }
            return 0;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser)]
        public async Task<Dictionary<DashboardPhysicalLoan, long>> GetPhysicalLoanExpriedAndTotal()
        {
            try
            {
                return await DashboardQuerier.GetPhysicalLoanExpriedAndTotalAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get physical loan expried and total. Error: {e}");
            }
            return new Dictionary<DashboardPhysicalLoan, long>();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser)]
        public async Task<List<DashboardKeyValue<long>>> GetPhysicalRequestsByPhysicalExplorer()
        {
            try
            {
                return await DashboardQuerier.GetPhysicalRequestsByPhysicalExplorerAsync();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get physical requests by physical explorer. Error: {e}");
            }

            return new List<DashboardKeyValue<long>>();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<long> GetPhysicalLoanPenddingTotal()
        {
            try
            {
                return await DashboardQuerier.GetPhysicalLoanPenddingTotalAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get physical loan by physical explorer. Error: {e}");
            }
            return 0;
        }
        
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<long> GetPhysicalDestructionPenddingTotal()
        {
            try
            {
                return await DashboardQuerier.GetPhysicalDestructionPenddingTotalAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get physical destruction by physical explorer. Error: {e}");
            }
            return 0;
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ArchiverDataDetails> GetArchiverDataSize()
        {
            try
            {
                return await SODashboardQuerier.GetArchiverDataSizeAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived data size, Error: {e}");
            }
            return new ArchiverDataDetails() { TotalSize = string.Empty, ArchiverDataUnit = ArchiverDataUnit.GB };
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin, RMPermissionExtensionMasks.GoogleAdmin, PermissionJoinType.Any)]
        public async Task<ArchiverRetentionDataDetails> GetArchiverRetentionDataSize()
        {
            try
            {
                return await SODashboardQuerier.GetArchiverRetentionDataSizeAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived data size, Error: {e}");
            }
            return new ArchiverRetentionDataDetails()
            {
                DeleteTime = I18NEntity.GetString("RM_JS_Common_Pending")
            };
        }


        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin, RMPermissionExtensionMasks.GoogleAdmin, PermissionJoinType.Any)]
        public Task<string> GetArchiverRetentionSimulateDataDetails([FromBody] JMDetailsQuery queryModel)
        {
            try
            {
                return SODashboardQuerier.GetArchiverRetentionSimulateDataDetails(queryModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived data size, Error: {e}");
            }
            return Task.FromResult<String>(null);
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ArchiverDataDetails> GetArchiverFileCount()
        {
            try
            {
                return await SODashboardQuerier.GetArchiverFileCountAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived file count, Error: {e}");
            }
            return new ArchiverDataDetails() { TotalSize = string.Empty, ArchiverDataUnit = ArchiverDataUnit.K };
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ArchiverDataDetails> GetArchiverVersionCount()
        {
            try
            {
                return await SODashboardQuerier.GetArchiverVersionCountAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived version count, Error: {e}");
            }
            return new ArchiverDataDetails() { TotalSize = string.Empty, ArchiverDataUnit = ArchiverDataUnit.K };
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<List<ArchiverSiteSizeInfo>> GetArchiverSiteInfo()
        {
            try
            {
                return await SODashboardQuerier.GetArchiverTop50SitesAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived site size infos, Error: {e}");
            }
            return new List<ArchiverSiteSizeInfo>();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ArchiverSiteSizeInfoWithCount> GetArchiverSiteInfoByPager([FromBody] ArchiverSitePageMode queryMode)
        {
            try
            {
                return await SODashboardQuerier.GetArchiverSitesByPagerAsync(queryMode);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived site size infos, Error: {e}");
            }
            return new ArchiverSiteSizeInfoWithCount();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionExtensionMasks.GoogleAdmin)]
        public async Task<ArchiverSiteSizeInfoWithCount> GetGoogleArchiverInfoByPager([FromBody] ArchiverSitePageMode queryMode)
        {
            try
            {
                return await SODashboardQuerier.GetGoogleArchiverByPagerAsync(queryMode);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get google archived size infos, Error: {e}");
            }
            return new ArchiverSiteSizeInfoWithCount();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ArchiverDataDetails> GetYearlySaving()
        {
            try
            {
                return await SODashboardQuerier.GetYearlySavingAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get yearly saving, Error: {e}");
            }
            return new ArchiverDataDetails() { TotalSize = string.Empty, ArchiverDataUnit = ArchiverDataUnit.Unknown };
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<bool> SaveSOPriceConfiguration([FromBody] ArchiverPriceConfiguration priceConfiguration)
        {
            try
            {
                return await DashboardService.SaveSOPriceConfigurationAsync(priceConfiguration);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while save so price configuration, Error: {e}");
            }
            return false;
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ArchiverPriceConfiguration> GetSOPriceConfiguration()
        {
            try
            {
                return await DashboardService.GetSOPriceConfigurationAsync();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get so price configuration, Error: {e}");
            }
            return new ArchiverPriceConfiguration();
        }

        [HttpPost]
        public async Task<bool> IsRunSODashboardJob()
        {
            try
            {
                return await DashboardService.IsRunSODashboardJobAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get so price configuration, Error: {e}");
            }
            return false;
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<RAReturnMessage> RunExportArchiverSiteInfoJob([FromBody] ArchiverExportReportDto reportDto)
        {
            return await DashboardService.RunExportArchiverSiteInfoJobAsync(reportDto);
        }
        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin, RMPermissionExtensionMasks.GoogleAdmin, PermissionJoinType.Any)]
        public async Task<RAReturnMessage> RunExportArchiveGoogleDriveInfoJob([FromBody] ArchiverExportReportDto reportDto)
        {
            return await DashboardService.RunExportArchiverGDriveInfoJobAsync(reportDto);
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin, RMPermissionExtensionMasks.GoogleAdmin, PermissionJoinType.Any)]
        public async Task<RAReturnMessage> RunExportArchiverRetentionSimulateInfoJob()
        {
            return await DashboardService.RunExportArchiverRetentionSimulateInfoJobAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<RAReturnMessage> RunArchiverDeduplicationReportJob([FromBody] DedeplicationExportReportDto reportDto)
        {
            if (!SettingProfileService.IsEnableArchiverDeduplication())
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = "De-duplication not enabled" };
            }
            return await DashboardService.RunArchiverDeduplicationReportJobAsync(reportDto);
        }

        #region Teams
        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<List<ArchiverTeamsGroupSizeInfo>> GetArchiverTeamsGroupInfo()
        {
            try
            {
                return await SODashboardQuerier.GetArchiverTop50TeamsGroupsAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived teams group size infos, Error: {e}");
            }
            return new List<ArchiverTeamsGroupSizeInfo>();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<ArchiverTeamsGroupInfoWithCount> GetArchiverTeamsGroupInfoByPager([FromBody] ArchiverSitePageMode queryMode)
        {
            try
            {
                return await SODashboardQuerier.GetArchiverTeamsGroupByPagerAsync(queryMode);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get archived teams group size infos, Error: {e}");
            }
            return new ArchiverTeamsGroupInfoWithCount();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<RAReturnMessage> RunExportArchiverTeamsGroupInfoJob([FromBody] ArchiverExportReportDto reportDto)
        {
            return await DashboardService.RunExportArchiverSiteInfoJobAsync(reportDto);
        }
        #endregion
    }
}