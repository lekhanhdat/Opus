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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.Discovery;
using System.Threading;
using RAExportCommon;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using static Microsoft.Office.Project.Server.Schema.AnalysisDataSet;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.Profile;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Telemetry;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryOffice365AnalysisProfileJobRunner : RMDiscoveryOffice365Worker
    {
        private readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao;

        private readonly IRMDiscoveryOffice365WithoutInDateDao _dateRangeDao;

        private readonly IRMReportManager _reportManager;

        private readonly string _jobId;

        private readonly RMDiscoveryProfileJobDefinition _jobDefinition;

        public RMDiscoveryOffice365AnalysisProfileJobRunner(JobQueueMessage message)
        {
            _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();
            _dateRangeDao = new RMDiscoveryOffice365WithoutInDateDao();
            _jobId = message.JobId;
            _jobDefinition = JsonConvert.DeserializeObject<RMDiscoveryProfileJobDefinition>(message.Extension);
            ReportMangerFactory.Instance.Init(_jobId, JobType.DiscoveryProfileJob);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
        }

        public async Task RunAsync()
        {
            var telemeter = new RMDiscoveryOffice365ProfileTelemeter(_jobId);
            try
            {
                _reportManager.IncreaseBase(300);
                _reportManager.StartUpdateJobProgress();

                var sizeRangeIds = (await _sizeRangeDao.GetAllAsync()).Select(item => item.Id).ToList();
                var dateRangeIds = (await _dateRangeDao.GetAllAsync()).Select(item => item.Id).Concat(new List<int> { -1, 999 }).ToList();
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);

                using (var cts = new CancellationTokenSource())
                {
                    _ = RefreshJobProgressAsync(cts.Token);

                    await RMDiscoveryOffice365SQLiteDBManager.DownloadDatabaseAsync();

                    var analyzer = new RMDiscoveryOffice365ProfileAnalyzer(telemeter, _jobDefinition, sizeRangeIds, dateRangeIds, rules);
                    var (jobStatus, profileFailedInfoes) = await analyzer.AnalysisAsync();

                    foreach (var (profileName, failedSiteUrls) in profileFailedInfoes)
                    {
                        foreach (var failedSiteUrl in failedSiteUrls)
                        {
                            _reportManager.SendJobDetail(new JMDiscoveryProfileJobDetails
                            {
                                ProfileName = profileName,
                                Url = failedSiteUrl,
                                Status = JobDetailsStatus.Failed,
                                Comment = "RM_HS_Criteria_View_Msg_ValidOtherError"
                            });
                        }
                    }

                    cts.Cancel(false);

                    _reportManager.SetJobFinished(jobStatus, jobStatus == JobStatus.Failed ? "RM_HS_Criteria_View_Msg_ValidOtherError" : "");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run job. Error: {e}");
                _reportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
            }

            await telemeter.FlushAsync();
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
}
