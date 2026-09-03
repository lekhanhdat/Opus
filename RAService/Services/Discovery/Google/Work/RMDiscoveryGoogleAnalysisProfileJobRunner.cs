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
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work;

public class RMDiscoveryGoogleAnalysisProfileJobRunner : RMDiscoveryGoogleWorker
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleAnalysisProfileJobRunner));
    
    private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();

    private readonly IRMDiscoveryGoogleWithoutInDateDao _dateRangeDao = new RMDiscoveryGoogleWithoutInDateDao();

    private readonly IRMReportManager _reportManager;

    private readonly string _jobId;

    private readonly RMDiscoveryGoogleProfileJobDefinition _jobDefinition;

    public RMDiscoveryGoogleAnalysisProfileJobRunner(JobQueueMessage message)
    {
        _jobId = message.JobId;
        _jobDefinition = JsonConvert.DeserializeObject<RMDiscoveryGoogleProfileJobDefinition>(message.Extension);
        ReportMangerFactory.Instance.Init(_jobId, JobType.DiscoveryGoogleProfileJob);
        _reportManager = ReportMangerFactory.Instance.ReportManager;
    }

    public async Task RunAsync()
    {
        try
        {
            _reportManager.IncreaseBase(300);
            _reportManager.StartUpdateJobProgress();

            var sizeRangeIds = (await _sizeRangeDao.GetAllAsync()).Select(item => item.Id).ToList();
            var dateRangeIds = (await _dateRangeDao.GetAllAsync()).Select(item => item.Id)
                .Concat(new List<int> { -1, 999 }).ToList();
            var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive,
                RMDiscoveryRuleDefinitionKind.ROT);

            using (var cts = new CancellationTokenSource())
            {
                _ = RefreshJobProgressAsync(cts.Token);

                await RMDiscoveryGoogleSQLiteDBManager.DownloadDatabaseAsync();

                var analyzer = new RMDiscoveryGoogleProfileAnalyzer(_jobDefinition, sizeRangeIds, dateRangeIds, rules);
                var (jobStatus, profileFailedInfoList) = await analyzer.AnalysisAsync();

                foreach (var (profileName, failedSiteUrls) in profileFailedInfoList)
                {
                    foreach (var failedSiteUrl in failedSiteUrls)
                    {
                        _reportManager.SendJobDetail(new JMDiscoveryGoogleProfileJobDetails
                        {
                            ProfileName = profileName,
                            DriveName = failedSiteUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = "RM_HS_Criteria_View_Msg_ValidOtherError"
                        });
                    }
                }

                cts.Cancel(false);

                _reportManager.SetJobFinished(jobStatus,
                    jobStatus == JobStatus.Failed ? "RM_HS_Criteria_View_Msg_ValidOtherError" : "");
            }
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while run job. Error: {e}");
            _reportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
        }
    }

    private async Task RefreshJobProgressAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _reportManager.Increase();
            await Task.Delay(1000 * 60 * 5, token);
        }
    }
}