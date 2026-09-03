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
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Analyzer;

public class RMSFAnalysisJobRunner(string jobId)
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMSFAnalysisJobRunner));
    
    private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

    public async Task RunAsync()
    {
        try
        {
            RMSubJob subJobInfo = _subJobDao.GetSubJob(jobId, true);
            var finalSubJob = string.IsNullOrEmpty(subJobInfo.JobContext.Content);
            RMSFBaseProcessor processor = finalSubJob switch
            {
                true => new RMSFSummary(),
                false => new RMSFAnalyzeObjects()
            };
            await processor.BuildServiceAsync(subJobInfo);
            await processor.RunAsync(); 
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while run job. Error: {ex}");
        }
    }
}