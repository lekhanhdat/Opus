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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Report;
using RAExportCommon;
using System.Threading;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.Resetter;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using Cloud.Sdk.Data.IE;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V1;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryOffice365AnalysisJobRunner : RMDiscoveryOffice365Worker
    {

        private readonly Guid _analysisJobId;

        private readonly IRMReportManager _reportManager;

        public RMDiscoveryOffice365AnalysisJobRunner(string subJobId) : base()
        {
            var subJobInfo = PlatformWindsorManager.GetService<IRMSubJobDao>().GetSubJob(subJobId);
            ReportMangerFactory.Instance.Init(subJobId, Contract.JobMonitor.JobType.DiscoveryJob);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            _analysisJobId = subJobInfo.DiscoveryAnalysisJobId;
            _reportManager.StartUpdateJobProgress(60);
            _reportManager.IncreaseBase(10000);
        }

        public async Task RunAsync()
        {
            try
            {
                var analysisJob = await _jobDao.GetAnalysisJobByIdAsync(_analysisJobId);
                analysisJob.Status = RMDiscoveryJobStatus.Running;
                await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
                _logger.Info($"Process analysis job [{_analysisJobId}].");

                using (var cts = new CancellationTokenSource())
                {
                    _ = RefreshJobProgressAsync(analysisJob, cts.Token);

                    var (_, mainJob) = await _jobDao.TryGetMainJobAsync(analysisJob.MainJobId);

                    var needToExecute = await CheckIsNeedToExecuteAsync(analysisJob, mainJob);

                    if (needToExecute)
                    {
                        _logger.Info($"The analysis job need to execute.");

                        var exclusionInfo = await _configurationDao.GetAsync<RMDiscoveryExclusionInfo>(RMDiscoveryConfigurationType.Office365Exclusion, new());

                        await ResetDataAsync(analysisJob, mainJob);

                        var discoveryJob = await _jobDao.GetDiscoveryJobAsync(analysisJob.DiscoveryJobId);

                        if(await RegisterIndexAsync(analysisJob.O365TenantId, discoveryJob.ContentSource))
                        {
                            var analyzer = new RMDiscoveryOffice365Analyzer(discoveryJob.ContentSource, exclusionInfo, analysisJob);

                            var succeed = await analyzer.AnalysisAsync();
                            analysisJob.Status = succeed ? RMDiscoveryJobStatus.Finished : RMDiscoveryJobStatus.Failed;
                        }
                        else
                        {
                            analysisJob.Status = RMDiscoveryJobStatus.Failed;
                        }
                    }
                    else
                    {
                        analysisJob.Status = RMDiscoveryJobStatus.Finished;
                    }

                    analysisJob.EndTime = DateTime.UtcNow.Ticks;
                    await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
                    _reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);

                    var cacheManager = new RMDiscoveryCacheManager(analysisJob.O365TenantId, RMDiscoveryCacheDataSource.Office365);
                    await cacheManager.ClearAsync();
                    _logger.Info($"The tenant [{analysisJob.O365TenantId}] cache cleared.");

                    cts.Cancel(false);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run discovery analysis job. Error: {e}");
                _reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
            }
        }

        private async Task<bool> RegisterIndexAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            try
            {
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                var indexModels = rules.ConvertAll(item => new IndexModel
                {
                    Name = item.ToTagColumn(),
                    Definition = JsonConvert.SerializeObject(new Dictionary<string, int>
                    {
                        {item.ToTagColumn(), 1 }
                    }),
                });

                if(indexModels.Count > 0)
                {
                    await _ieApiClient.DatabaseManagementService.CreateIndexAsync(new IndexCreationModel
                    {
                        DataType = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                        Office365TenantId = o365TenantId.ToString(),
                        Indexes = indexModels
                    });
                }

                _logger.Info($"Successful register index for o365 tenant [{o365TenantId}] content source [{contentSource}].");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while register index for o365 tenant [{o365TenantId}] content source [{contentSource}]. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CheckIsNeedToExecuteAsync(RMDiscoveryOffice365AnalysisJob jobInfo, RMDiscoveryOffice365MainJob mainJob)
        {
            if(mainJob.Type == RMDiscoveryJobType.Retry)
            {
                return true;
            }
            var siteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(jobInfo.O365TenantId, jobInfo.SiteId);
            return siteInfo == null;
        }

        private async Task ResetDataAsync(RMDiscoveryOffice365AnalysisJob jobInfo, RMDiscoveryOffice365MainJob mainJob)
        {
            _logger.Info($"The site [{jobInfo.SiteId}] is need [{mainJob.Type}].");

            if(mainJob.Type == RMDiscoveryJobType.Retry)
            {
                var resetter = new RMDiscoveryOffice365AnalysisRetryResetter(jobInfo);
                await resetter.ResetAsync();
                _logger.Info($"Successfl reset site [{jobInfo.SiteId}] data.");
            }
        }

        private async Task RefreshJobProgressAsync(RMDiscoveryOffice365AnalysisJob jobInfo, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _reportManager.Increase();
                jobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
                await _jobDao.AddOrUpdateAnalysisJobAsync(jobInfo);
                await Task.Delay(1000 * 60 * 5, token);
            }
        }
    }
}
