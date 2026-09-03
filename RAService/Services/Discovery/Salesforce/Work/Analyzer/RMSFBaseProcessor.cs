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
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Salesforce;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Analyzer;

public abstract class RMSFBaseProcessor
{
    protected readonly RALogger _logger;
    
    protected List<RMDiscoverySalesforceSizeRange> SizeRanges;
    
    protected List<RMDiscoverySalesforceWithoutInDate> ModifiedRanges;

    protected readonly IRMDiscoveryConfigurationDao _configurationDao;
    
    private readonly IRMDiscoverySalesforceSizeRangeDao _sizeRangeDao = new RMDiscoverySalesforceSizeRangeDao();

    private readonly IRMDiscoverySalesforceWithoutInDateDao _dateRangeDao = new RMDiscoverySalesforceWithoutInDateDao();
    
    protected RASalesforce.Report.ReportCenter ReportCenter;

    protected RMSubJob SubJobInfo;
    
    protected List<SfObjectJobDto> SfObjectJobs;
    
    protected readonly IRMDiscoverySalesforceDataDao SalesforceDiscoveryJobDao = new RMDiscoverySalesforceDataDao();
    
    protected StopJobCts Cts;
    
    protected readonly IRMDiscoverySalesforceJobDao _jobDao = new RMDiscoverySalesforceJobDao();
    
    protected readonly IRMDiscoverySalesforceExecutionInfoDao ExecutionInfoDao = new RMDiscoverySalesforceExecutionInfoDao();


    
    protected RMSFBaseProcessor()
    {
        _logger = RALogger.GetInstance(GetType()); 
        _configurationDao = new RMDiscoveryConfigurationDao();
    }

    public async Task BuildServiceAsync(RMSubJob subJobInfo)
    {
        SubJobInfo = subJobInfo;
        if (SubJobInfo.JobContext.Content.IsNotNullOrEmpty())
        {
            SfObjectJobs = JsonConvert.DeserializeObject<List<SfObjectJobDto>>(SubJobInfo.JobContext.Content)!;
        }
        SizeRanges = await _sizeRangeDao.GetAllAsync();
        ModifiedRanges = await _dateRangeDao.GetAllAsync();
        ReportCenter = new();
        ReportCenter.InitCurrentJobInfo(SubJobInfo.Id, JobType.SFDiscoveryJob);
        Cts = new();
    }
    
    protected async Task SetDiscoveryJob(RMDiscoverySalesforceMainJob job, RMDiscoveryJobStatus jobStatus)
    {
        job.EndTime = DateTime.UtcNow.Ticks;
        job.Status = jobStatus;
        await _jobDao.AddOrUpdateMainJobAsync(job);
    }
    
    public abstract Task RunAsync();

}