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
using PnP.Framework.Diagnostics;

namespace RASalesforce.APIs;

public sealed class SalesforceAPIHelper
{
    private static readonly SalesforceAPIHelper _instance = new SalesforceAPIHelper();
    private readonly static IRALogger logger = RALogger.GetInstance(typeof(SalesforceAPIHelper));
    private readonly object lockObj = new object();
    public static SalesforceAPIHelper Instance { get { return _instance; } }

    public int ApiUsed;
    public int RequestCount;
    public int MaxRequest;
    public int UpdatedRequest;
    public int MaxBulkQueryJobs;
    public int BulkQueryJobApiUsed;
    public int RequestQueryJobCount;
    public int UpdatedQueryJobCount;
    public int BulkQueryTotalStorageMB;
    public int BulkQueryUsedStorageMB;
    public int MaxBulkBatchCount;
    public int BulkBatchUsedCount;
    public int BulkV1BatchRequestCount;
    public int UpdatedBulkV1BatchCount;
    private bool isPaused = false;
    public bool IsNeedPostPond;

    public bool IsPaused => isPaused;
    //public bool IsPausedBulkQueryJob = false;
    //public bool IsPausedBulkV1QueryJob = false;

    internal int IncRequest()
    {
        return Interlocked.Increment(ref this.RequestCount);
    }

    public void Refresh(OrganizationLimits orgLimits)
    {
        this.MaxRequest = orgLimits.DailyApiRequests?.Max ?? 0;
        this.ApiUsed = orgLimits.DailyApiRequests?.Used ?? 0 - this.RequestCount;
        this.MaxBulkQueryJobs = orgLimits.DailyBulkV2QueryJobs?.Max ?? 0;
        this.BulkQueryJobApiUsed = orgLimits.DailyBulkV2QueryJobs?.Used ?? 0 - this.RequestQueryJobCount;
        this.BulkQueryTotalStorageMB = orgLimits.DailyBulkV2QueryFileStorageMB?.Max ?? 0;
        this.BulkQueryUsedStorageMB = orgLimits.DailyBulkV2QueryFileStorageMB?.Used ?? 0;
        this.MaxBulkBatchCount = orgLimits.DailyBulkApiBatches?.Max ?? 0;
        this.BulkBatchUsedCount = orgLimits.DailyBulkApiBatches?.Used ?? 0 - this.BulkV1BatchRequestCount;
        this.IsNeedPostPond = ((float) orgLimits.DailyApiRequests!.Used / orgLimits.DailyApiRequests.Max * 100) >= 80;
    }

    public void Pause()
    {
        if (!this.isPaused)
        {
            lock (lockObj)
            {
                if (!this.isPaused)
                {
                    this.isPaused = true;
                    logger.Warn($"API: Entering pause state. Current api quota: {this.ApiUsed + this.RequestCount} / {this.MaxRequest}, {this.RequestCount} used by this job.");
                }
            }
        }
    }

    public void Resume()
    {
        if (this.isPaused)
        {
            lock (lockObj)
            {
                if (this.isPaused)
                {
                    this.isPaused = false;
                    logger.Warn($"API: Entering pause state. Current api quota: {this.ApiUsed + this.RequestCount} / {this.MaxRequest}, {this.RequestCount} used by this job.");
                }
            }
        }
    }

    public int GetAPIUsage()
    {
        if (this.MaxRequest == 0) { return 0; }
        return (this.ApiUsed + this.RequestCount) * 100 / this.MaxRequest;
    }
}
