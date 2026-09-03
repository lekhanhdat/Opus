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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.Profile.Inactive;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.Profile.Rot;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Telemetry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.Profile
{
    public class RMDiscoveryOffice365ProfileAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ProfileAnalyzer));

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly IRMDiscoveryOffice365ProfileDao _profileDao;

        private readonly IRMDiscoveryOffice365FileExtensionDao _fileExtensionDao;

        private readonly RMDiscoveryProfileJobDefinition _jobDefinition;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        private readonly RMDiscoveryOffice365ProfileTelemeter _telemeter;

        public RMDiscoveryOffice365ProfileAnalyzer(
            RMDiscoveryOffice365ProfileTelemeter telemeter,
            RMDiscoveryProfileJobDefinition jobDefinition,
            List<int> sizeRangeIds,
            List<int> dateRangeIds,
            List<RMDiscoveryOffice365RuleInfo> rules
            )
        {
            _jobDao = new RMDiscoveryOffice365JobDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _profileDao = new RMDiscoveryOffice365ProfileDao();
            _fileExtensionDao = new RMDiscoveryOffice365FileExtensionDao();
            _jobDefinition = jobDefinition;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _rules = rules;
            _telemeter = telemeter;
        }

        public async Task<(JobStatus jobStatus, List<(string profileName, List<string> failedSiteUrls)> profileFailedInfoes)> AnalysisAsync()
        {
            try
            {
                _logger.Info($"Current job run mode [{_jobDefinition.RunMode}].");

                if (_jobDefinition.RunMode == RMDiscoveryJobRunMode.All)
                {
                    return await AnalysisAllAsync();
                }

                return await AnalysisSpecifyAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis profile data. Error: {e}");
                return (JobStatus.Failed, []);
            }
        }

        private async Task<(JobStatus jobStatus, List<(string profileName, List<string> failedSiteUrls)> profileFailedInfoes)> AnalysisAllAsync()
        {

            var jobStatusSet = new HashSet<JobStatus>();
            var profileFailedInfoes = new List<(string profileName, List<string> failedSiteUrl)>();

            _logger.Info($"Start run analysis all profile job.");

            var (_, mainJobInfo) = await _jobDao.TryGetMainJobAsync(_jobDefinition.MainJobId);
            mainJobInfo.ProfileJobInitStatus = RMDiscoveryJobStatus.Running;
            await _jobDao.AddOrUpdateMainJobAsync(mainJobInfo);

            var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_jobDefinition.MainJobId, RMDiscoveryJobStatus.Finished, RMDiscoveryJobStatus.Exception);
            foreach (var discoveryJobO365Dic in discoveryJobs
                .GroupBy(item => item.O365TenantId)
                .ToDictionary(item => item.Key, item => item.ToList()))
            {
                var o365TenantId = discoveryJobO365Dic.Key;
                var profileInfoes = await _profileDao.GetProfileInfoesAsync(o365TenantId);
                var fileExtensions = await _fileExtensionDao.GetAllAsync(o365TenantId);

                _logger.Info($"The o365 tenant [{o365TenantId}] need to run profile count [{profileInfoes.Count}].");
                var needToAnalysisSiteInfoes = new List<RMDiscoveryOffice365SiteInfo>();

                foreach (var discoveryJob in discoveryJobO365Dic.Value)
                {
                    var (hasContainer, containerInfo) = await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(o365TenantId, discoveryJob.ContainerId);
                    var analysisJobEnumerable = _jobDao.GetAnalysisJobsByDiscoveryJobWithPaginationAsync(discoveryJob.Id, 1000, RMDiscoveryJobStatus.Finished);
                    await foreach (var analysisJob in analysisJobEnumerable)
                    {
                        var siteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(o365TenantId, analysisJob.SiteId);
                        if (siteInfo != null && hasContainer && siteInfo.ContainerId == containerInfo.Id)
                        {
                            needToAnalysisSiteInfoes.Add(siteInfo);
                        }
                    }
                }
                _logger.Info($"Need to analysis site count [{needToAnalysisSiteInfoes.Count}].");

                foreach (var profileInfo in profileInfoes)
                {
                    profileInfo.ScanType = mainJobInfo.Type;
                    var (jobStatus, failedSiteUrls) = await AnalysisProfileDataAsync(o365TenantId, profileInfo, needToAnalysisSiteInfoes, fileExtensions);
                    jobStatusSet.Add(jobStatus);
                    if (failedSiteUrls.Count > 0)
                    {
                        profileFailedInfoes.Add((profileInfo.Name, failedSiteUrls));
                    }
                }
            }

            _logger.Info($"End run analysis all profile job.");

            var resJobStatus = JobStatus.Finished;
            if (jobStatusSet.Contains(JobStatus.FinishWithException) ||
                (jobStatusSet.Contains(JobStatus.Failed) && jobStatusSet.Contains(JobStatus.Finished)))
            {
                resJobStatus = JobStatus.FinishWithException;
            }
            else if (jobStatusSet.Contains(JobStatus.Failed))
            {
                resJobStatus = JobStatus.Failed;
            }

            mainJobInfo.ProfileJobInitStatus = RMDiscoveryJobStatus.Finished;
            await _jobDao.AddOrUpdateMainJobAsync(mainJobInfo);

            return (resJobStatus, profileFailedInfoes);
        }

        private async Task<(JobStatus jobStatus, List<(string profileName, List<string> failedSiteUrls)> profileFailedInfoes)> AnalysisSpecifyAsync()
        {
            var profileFailedInfoes = new List<(string profileName, List<string> failedSiteUrl)>();

            _logger.Info($"Start run analysis specify profile job.");

            var o365TenantId = _jobDefinition.O365TenantId;
            var profileId = _jobDefinition.SpecifyProfileId;
            var profileInfo = await _profileDao.GetProfileInfoByIdAsync(o365TenantId, profileId);
            var fileExtensions = await _fileExtensionDao.GetAllAsync(o365TenantId);

            var failedInfoes = await _profileDao.GetProfileFailedInfoesAsync(o365TenantId, profileId);

            var needToAnalysisSiteInfoes = new List<RMDiscoveryOffice365SiteInfo>();

            if (_jobDefinition.JobType == RMDiscoveryJobType.Retry && failedInfoes.Count > 0)
            {
                foreach (var failedInfo in failedInfoes)
                {
                    var siteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(o365TenantId, failedInfo.SiteId);
                    needToAnalysisSiteInfoes.Add(siteInfo);
                }
            }
            else
            {
                var siteInfoEnumerable = _nodeDao.GetDiscoverySiteInfoesAsync(o365TenantId);
                await foreach (var siteInfo in siteInfoEnumerable)
                {
                    needToAnalysisSiteInfoes.Add(siteInfo);
                }
            }

            await _profileDao.DeleteProfileFailedInfoesAsync(o365TenantId, profileId);
            _logger.Info($"Successful delete profile [{profileInfo.Id} {profileInfo.Name}] prev failed infoes.");

            _logger.Info($"Profile [{profileInfo.Id} {profileInfo.Name}] need to analysis site count [{needToAnalysisSiteInfoes.Count}].");
            var (jobStatus, failedSiteUrls) = await AnalysisProfileDataAsync(o365TenantId, profileInfo, needToAnalysisSiteInfoes, fileExtensions);

            if (failedSiteUrls.Count > 0)
            {
                profileFailedInfoes.Add((profileInfo.Name, failedSiteUrls));
            }

            _logger.Info($"End run analysis specify profile job.");

            return (jobStatus, profileFailedInfoes);
        }

        private async Task<(JobStatus status, List<string> failedSiteUrls)> AnalysisProfileDataAsync(
            Guid o365TenantId,
            RMDiscoveryOffice365ProfileInfo profileInfo,
            List<RMDiscoveryOffice365SiteInfo> siteInfoes,
            List<RMDiscoveryOffice365FileExtension> fileExtensions)
        {
            try
            {
                _logger.Info($"Will analysis profile [{profileInfo.Id}] info {JsonConvert.SerializeObject(profileInfo)}");

                profileInfo.CurrentScanStatus = RMDiscoveryJobStatus.Running;
                profileInfo.StartScanTime = DateTime.UtcNow.Ticks;
                await _profileDao.AddOrUpdateProfileInfoAsync(o365TenantId, profileInfo);

                var failedSiteInfoes = new List<RMDiscoveryOffice365SiteInfo>();

                if (profileInfo.ProfileType == RMDiscoveryProfileType.Inactive)
                {
                    if (profileInfo.ScanType == RMDiscoveryJobType.Newly)
                    {
                        await RMDiscoveryDBManager.DropOffice365InactiveProfileTablsAsync(o365TenantId, profileInfo.Id);
                        _logger.Info($"The o365 tenant [{o365TenantId}] profile [{profileInfo.Id}] inactive table has been deleted.");
                    }
                    var inactiveRules = _rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.Inactive).ToList();
                    await RMDiscoveryDBManager.InitOffice365InactiveProfileTabls(o365TenantId, profileInfo.Id, inactiveRules.ConvertAll(item => item.ToCustomColumn()).ToList());

                    var inactiveDataAnalyzer = new RMDiscoveryOffice365InactiveDataAnalyzer(o365TenantId, profileInfo, siteInfoes, inactiveRules);
                    failedSiteInfoes = await inactiveDataAnalyzer.AnalysisAsync();
                }
                else
                {
                    if (profileInfo.ScanType == RMDiscoveryJobType.Newly)
                    {
                        await RMDiscoveryDBManager.DropOffice365RotProfileTablsAsync(o365TenantId, profileInfo.Id);
                        _logger.Info($"The o365 tenant [{o365TenantId}] profile [{profileInfo.Id}] rot table has been deleted.");
                    }

                    var selectedRuleIds = JsonConvert.DeserializeObject<HashSet<int>>(profileInfo.RuleIdsJson);
                    await RMDiscoveryDBManager.InitOffice365RotProfileTabls(o365TenantId, profileInfo.Id, _rules.Where(item => selectedRuleIds.Contains(item.Id)).ConvertAll(item => item.ToCustomColumn()).ToList());

                    var rotRules = _rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT && item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                    var rotDataAnalyzer = new RMDiscoveryOffice365RotDataAnalyzer(
                        o365TenantId,
                        profileInfo,
                        siteInfoes,
                        rotRules,
                        _sizeRangeIds,
                        _dateRangeIds,
                        fileExtensions);
                    failedSiteInfoes = await rotDataAnalyzer.AnalysisAsync();
                }

                var currentJobStatus = failedSiteInfoes.Count > 0 ?
                    (failedSiteInfoes.Count == siteInfoes.Count ? RMDiscoveryJobStatus.Failed : RMDiscoveryJobStatus.Exception) :
                    RMDiscoveryJobStatus.Finished;
                profileInfo.CurrentScanStatus = CalculateJobStatus(currentJobStatus, profileInfo.PrevScanStatus);
                profileInfo.PrevScanStatus = profileInfo.CurrentScanStatus;
                profileInfo.EndScanTime = DateTime.UtcNow.Ticks;

                await _profileDao.AddOrUpdateProfileFailedInfoesAsync(o365TenantId, failedSiteInfoes.ConvertAll(item => new RMDiscoveryProfileFailedInfo
                {
                    ProfileId = profileInfo.Id,
                    SiteId = item.Id,
                    FailedTime = DateTime.UtcNow.Ticks
                }).ToArray());

                await _profileDao.AddOrUpdateProfileInfoAsync(o365TenantId, profileInfo);

                return (ConvertTo(profileInfo.CurrentScanStatus), failedSiteInfoes.Select(item => item.Url).ToList());
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis profile [{profileInfo.Id} {profileInfo.Name}] data. Error: {e}");

                profileInfo.CurrentScanStatus = RMDiscoveryJobStatus.Failed;
                profileInfo.PrevScanStatus = RMDiscoveryJobStatus.Failed;
                profileInfo.EndScanTime = DateTime.UtcNow.Ticks;
                await _profileDao.AddOrUpdateProfileInfoAsync(o365TenantId, profileInfo);
                return (JobStatus.Failed, []);
            }
            finally
            {
                await _telemeter.RecordAsync(o365TenantId, profileInfo);
            }
        }

        private RMDiscoveryJobStatus CalculateJobStatus(RMDiscoveryJobStatus currentStatus, RMDiscoveryJobStatus prevStatus)
        {
            if (_jobDefinition.RunMode == RMDiscoveryJobRunMode.Specify)
            {
                return currentStatus;
            }

            if (_jobDefinition.JobType == RMDiscoveryJobType.Newly)
            {
                return currentStatus;
            }

            if (currentStatus != RMDiscoveryJobStatus.Finished)
            {
                if (prevStatus == RMDiscoveryJobStatus.Finished ||
                    prevStatus == RMDiscoveryJobStatus.Exception ||
                    prevStatus == RMDiscoveryJobStatus.None)
                {
                    return currentStatus;
                }

                return prevStatus;
            }
            else
            {
                if (prevStatus == RMDiscoveryJobStatus.Exception || prevStatus == RMDiscoveryJobStatus.Failed)
                {
                    return prevStatus;
                }

                return currentStatus;
            }
        }

        private static JobStatus ConvertTo(RMDiscoveryJobStatus jobStatus)
        {
            return jobStatus switch
            {
                RMDiscoveryJobStatus.Finished => JobStatus.Finished,
                RMDiscoveryJobStatus.Exception => JobStatus.FinishWithException,
                _ => JobStatus.Failed
            };
        }
    }
}
