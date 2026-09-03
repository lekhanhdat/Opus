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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Profile;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile.Inactive;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile.ROT;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile;

public class RMDiscoveryGoogleProfileAnalyzer
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleProfileAnalyzer));

    private readonly IRMDiscoveryGoogleJobDao _jobDao = new RMDiscoveryGoogleJobDao();

    private readonly IRMDiscoveryGoogleNodeDao _nodeDao = new RMDiscoveryGoogleNodeDao();

    private readonly IRMDiscoveryGoogleProfileDao _profileDao = new RMDiscoveryGoogleProfileDao();

    private readonly IRMDiscoveryGoogleFileExtensionDao _fileExtensionDao = new RMDiscoveryGoogleFileExtensionDao();
    
    private readonly RMDiscoveryGoogleProfileJobDefinition _jobDefinition;
    
    private readonly List<int> _sizeRangeIds;
    
    private readonly List<int> _dateRangeIds;
    
    private readonly List<RMDiscoveryGoogleRuleInfo> _rules;

    public RMDiscoveryGoogleProfileAnalyzer(RMDiscoveryGoogleProfileJobDefinition jobDefinition, List<int> sizeRangeIds, List<int> dateRangeIds, List<RMDiscoveryGoogleRuleInfo> rules)
    {
        _jobDefinition = jobDefinition;
        _sizeRangeIds = sizeRangeIds;
        _dateRangeIds = dateRangeIds;
        _rules = rules;
    }
    
    public async Task<(JobStatus jobStatus, List<(string profileName, List<string> failedDriveIds)> profileFailedInfoes)> AnalysisAsync()
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

    private async Task<(JobStatus jobStatus, List<(string profileName, List<string> failedDriveIds)> profileFailedInfoes)> AnalysisSpecifyAsync()
    {
        List<(string profileName, List<string> failedDriveIds)> profileFailedInfoList = [];

        _logger.Info($"Start run analysis specify profile job.");

        var googleOrganizationId = _jobDefinition.GoogleOrganizationId;
        var profileId = _jobDefinition.SpecifyProfileId;
        var profileInfo = await _profileDao.GetProfileInfoByIdAsync(googleOrganizationId, profileId);
        var fileExtensions = await _fileExtensionDao.GetAllAsync(googleOrganizationId);

        var failedInfoList = await _profileDao.GetProfileFailedInfoesAsync(googleOrganizationId, profileId);

        List<RMDiscoveryGoogleDriveInfo> needToAnalysisDriveInfoList = [];

        if (_jobDefinition.JobType == RMDiscoveryJobType.Retry && failedInfoList.Count > 0)
        {
            foreach (var failedInfo in failedInfoList)
            {
                var driveInfo =
                    await _nodeDao.GetDiscoveryGoogleDriveInfoAsync(googleOrganizationId, failedInfo.DriveId);
                needToAnalysisDriveInfoList.Add(driveInfo);
            }
        }
        else
        {
            var driveInfoEnumerable = _nodeDao.GetDiscoveryGoogleDriveInfoesAsync(googleOrganizationId);
            await foreach (var driveInfo in driveInfoEnumerable)
            {
                needToAnalysisDriveInfoList.Add(driveInfo);
            }
        }

        await _profileDao.DeleteProfileFailedInfoesAsync(googleOrganizationId, profileId);
        _logger.Info($"Successful delete profile [{profileInfo.Id} {profileInfo.Name}] prev failed info list.");

        _logger.Info(
            $"Profile [{profileInfo.Id} {profileInfo.Name}] need to analysis drive count [{needToAnalysisDriveInfoList.Count}].");
        var (jobStatus, failedSiteUrls) = await AnalysisProfileDataAsync(googleOrganizationId, profileInfo,
            needToAnalysisDriveInfoList, fileExtensions);

        if (failedSiteUrls.Count > 0)
        {
            profileFailedInfoList.Add((profileInfo.Name, failedSiteUrls));
        }

        _logger.Info($"End run analysis specify profile job.");

        return (jobStatus, profileFailedInfoList);
    }

    private async Task<(JobStatus jobStatus, List<(string profileName, List<string> failedDriveIds)> profileFailedInfoes)> AnalysisAllAsync()
    {
        var jobStatusSet = new HashSet<JobStatus>();
        List<(string profileName, List<string> failedSiteUrl)> profileFailedInfoList = [] ;

        _logger.Info($"Start run analysis all profile job.");

        var (_, mainJobInfo) = await _jobDao.TryGetMainJobAsync(_jobDefinition.MainJobId);
        mainJobInfo.ProfileJobInitStatus = RMDiscoveryJobStatus.Running;
        await _jobDao.AddOrUpdateMainJobAsync(mainJobInfo);

        var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_jobDefinition.MainJobId, RMDiscoveryJobStatus.Finished, RMDiscoveryJobStatus.Exception);
        foreach (var discoveryJobGoogleDic in discoveryJobs
                     .GroupBy(item => item.OrganizationId)
                     .ToDictionary(item => item.Key, item => item.ToList()))
        {
            var organizationId = discoveryJobGoogleDic.Key;
            var profileInfoList = await _profileDao.GetProfileInfoesAsync(organizationId);
            var fileExtensions = await _fileExtensionDao.GetAllAsync(organizationId);

            _logger.Info($"The google organization id [{organizationId}] need to run profile count [{profileInfoList.Count}].");
            List<RMDiscoveryGoogleDriveInfo> needToAnalysisDriveInfoList = [];

            foreach (var discoveryJob in discoveryJobGoogleDic.Value)
            {
                var (hasContainer, containerInfo) =
                    await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(organizationId, discoveryJob.ContainerId);
                var analysisJobEnumerable =
                    _jobDao.GetAnalysisJobsByDiscoveryJobWithPaginationAsync(discoveryJob.Id, 1000,
                        RMDiscoveryJobStatus.Finished);
                await foreach (var analysisJob in analysisJobEnumerable)
                {
                    var driveInfo = await _nodeDao.GetDiscoveryGoogleDriveInfoAsync(organizationId, analysisJob.DriveId);
                    if (driveInfo != null && hasContainer && driveInfo.ContainerId == containerInfo.Id)
                    {
                        needToAnalysisDriveInfoList.Add(driveInfo);
                    }
                }
            }

            _logger.Info($"Need to analysis drive count [{needToAnalysisDriveInfoList.Count}].");

            foreach (var profileInfo in profileInfoList)
            {
                profileInfo.ScanType = mainJobInfo.Type;
                var (jobStatus, failedDriveIds) = await AnalysisProfileDataAsync(organizationId, profileInfo,
                    needToAnalysisDriveInfoList, fileExtensions);
                jobStatusSet.Add(jobStatus);
                if (failedDriveIds.Count > 0)
                {
                    profileFailedInfoList.Add((profileInfo.Name, failedDriveIds));
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

        return (resJobStatus, profileFailedInfoList);
    }
    
    private async Task<(JobStatus status, List<string> failedDriveIds)> AnalysisProfileDataAsync(
            string organizationId,
            RMDiscoveryGoogleProfileInfo profileInfo,
            List<RMDiscoveryGoogleDriveInfo> driveInfoList,
            List<RMDiscoveryGoogleFileExtension> fileExtensions)
    {
        try
        {
            _logger.Info($"Will analysis profile [{profileInfo.Id}] info {JsonConvert.SerializeObject(profileInfo)}");

            profileInfo.CurrentScanStatus = RMDiscoveryJobStatus.Running;
            profileInfo.StartScanTime = DateTime.UtcNow.Ticks;
            await _profileDao.AddOrUpdateProfileInfoAsync(organizationId, profileInfo);

            List<RMDiscoveryGoogleDriveInfo> failedDriveInfoList = [];

            if (profileInfo.ProfileType == RMDiscoveryProfileType.Inactive)
            {
                if (profileInfo.ScanType == RMDiscoveryJobType.Newly)
                {
                    await RMDiscoveryDBManager.DropGoogleInactiveProfileTablesAsync(organizationId, profileInfo.Id);
                    _logger.Info(
                        $"The google organization id [{organizationId}] profile [{profileInfo.Id}] inactive table has been deleted.");
                }

                var inactiveRules = _rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.Inactive)
                    .ToList();
                await RMDiscoveryDBManager.InitGoogleInactiveProfileTables(organizationId, profileInfo.Id,
                    inactiveRules.ConvertAll(item => item.ToCustomColumn()).ToList());

                var inactiveDataAnalyzer =
                    new RMDiscoveryGoogleInactiveDataAnalyzer(organizationId, profileInfo, driveInfoList,
                        inactiveRules);
                failedDriveInfoList = await inactiveDataAnalyzer.AnalysisAsync();
            }
            else
            {
                if (profileInfo.ScanType == RMDiscoveryJobType.Newly)
                {
                    await RMDiscoveryDBManager.DropGoogleRotProfileTablesAsync(organizationId, profileInfo.Id);
                    _logger.Info(
                        $"The google organization id [{organizationId}] profile [{profileInfo.Id}] rot table has been deleted.");
                }

                var selectedRuleIds = JsonConvert.DeserializeObject<HashSet<int>>(profileInfo.RuleIdsJson);
                await RMDiscoveryDBManager.InitGoogleRotProfileTables(organizationId, profileInfo.Id,
                    _rules.Where(item => selectedRuleIds.Contains(item.Id)).ConvertAll(item => item.ToCustomColumn())
                        .ToList());

                var rotRules = _rules.Where(item =>
                    item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT &&
                    item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                var rotDataAnalyzer = new RMDiscoveryGoogleRotDataAnalyzer(
                    organizationId,
                    profileInfo,
                    driveInfoList,
                    rotRules,
                    _sizeRangeIds,
                    _dateRangeIds,
                    fileExtensions);
                failedDriveInfoList = await rotDataAnalyzer.AnalysisAsync();
            }

            var currentJobStatus = failedDriveInfoList.Count > 0
                ? (failedDriveInfoList.Count == driveInfoList.Count
                    ? RMDiscoveryJobStatus.Failed
                    : RMDiscoveryJobStatus.Exception)
                : RMDiscoveryJobStatus.Finished;
            profileInfo.CurrentScanStatus = CalculateJobStatus(currentJobStatus, profileInfo.PrevScanStatus);
            profileInfo.PrevScanStatus = profileInfo.CurrentScanStatus;
            profileInfo.EndScanTime = DateTime.UtcNow.Ticks;

            await _profileDao.AddOrUpdateProfileFailedInfoesAsync(organizationId, failedDriveInfoList.ConvertAll(item =>
                new RMDiscoveryGoogleProfileFailedInfo()
                {
                    ProfileId = profileInfo.Id,
                    DriveId = item.Id,
                    FailedTime = DateTime.UtcNow.Ticks
                }).ToArray());

            await _profileDao.AddOrUpdateProfileInfoAsync(organizationId, profileInfo);

            return (JobStatus.Finished, failedDriveInfoList.Select(item => item.DriveName).ToList());
        }
        catch (Exception e)
        {
            _logger.Error(
                $"An error occurred while analysis profile [{profileInfo.Id} {profileInfo.Name}] data. Error: {e}");

            profileInfo.CurrentScanStatus = RMDiscoveryJobStatus.Failed;
            profileInfo.PrevScanStatus = RMDiscoveryJobStatus.Failed;
            profileInfo.EndScanTime = DateTime.UtcNow.Ticks;
            await _profileDao.AddOrUpdateProfileInfoAsync(organizationId, profileInfo);
            return (JobStatus.Failed, []);
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
            return prevStatus is RMDiscoveryJobStatus.Finished or RMDiscoveryJobStatus.Exception or RMDiscoveryJobStatus.None ? currentStatus : prevStatus;
        }
        
        return prevStatus is RMDiscoveryJobStatus.Exception or RMDiscoveryJobStatus.Failed ? prevStatus : currentStatus;
    }

}