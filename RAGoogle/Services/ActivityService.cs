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
using AvePoint.RA.Contract.Aos;
using Google.Apis.Admin.Reports.reports_v1.Data;
using RAGoogle.API.Query;
using RAGoogle.GoogleObjDiscover.Services;
using Activity = Google.Apis.Admin.Reports.reports_v1.Data.Activity;

namespace RAGoogle.Services;

public class GoogleActivityService : BaseService, IDisposable
{
    private readonly RALogger logger = RALogger.GetInstance(typeof(GoogleActivityService));
    private ReportApi _reportApi;

    public GoogleActivityService(RMAosGoogleAppProfile app) : base(app, string.Empty, GoogleScopeType.GoogleReport)
    {
        _reportApi = new(initializer);
    }

    public async Task<List<Activity>> GetDriveActivitiesAsync(string gDriveId, DateTime startTime, DateTime endTime, string gDriveObjectId, bool isShared = false, CancellationToken token = default)
    {
        List<Activity> activities = [];
        try
        {
            if (startTime >= endTime)
            {
                logger.Error($"Invalid time range. StartTime: {startTime}, EndTime: {endTime}");
                return activities;
            }

            logger.Info($"Start to get activities, driveId: {gDriveObjectId}, startTime:{startTime}, quitTime:{endTime}");
            var totalFeedCount = 0;
            Activities driveActivities;
            ActivityQuery query = new()
            {
                StartTime = startTime,
                EndTime = endTime
            };
            if (isShared)
            {
                query.Filters = $"shared_drive_id=={gDriveId}";
            }
            else
            {
                query.UserKey = "all";
                query.Filters = $"owner=={gDriveId}";
            }
            do
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                (driveActivities, query.PageToken) = await _reportApi.GetPagedDriveActivity(query);
                if (driveActivities.Items != null)
                {
                    foreach (var activity in driveActivities.Items)
                    {
                        activities.Add(activity);
                    }
                }
            } while (query.PageToken != null);
            logger.Info($"Query complete, driveId:{gDriveObjectId}, start:{startTime}, end:{endTime}, feed Count:{totalFeedCount}");
        }
        catch (Exception ex)
        {
            logger.Warn($"Get activity failed, exception:{ex}.");
        }
        return activities;
    }

    public async Task<List<Activity>> GetItemActivitiesAsync(string gItemId, DateTime startTime, DateTime endTime, CancellationToken token = default)
    {
        List<Activity> activities = [];
        try
        {
            if (startTime >= endTime)
            {
                logger.Error($"Invalid time range. StartTime: {startTime}, EndTime: {endTime}");
                return activities;
            }

            logger.Info($"Start to get activities, itemId: {gItemId}, startTime:{startTime}, quitTime:{endTime}");
            var totalFeedCount = 0;
            ActivityQuery query = new()
            {
                StartTime = startTime,
                EndTime = endTime,
                Filters = $"doc_id=={gItemId}"
            };
            var itemActivities = await _reportApi.GetDriveActivity(query);
            foreach (var activity in itemActivities.Items)
            {
                activities.Add(activity);
            }
            logger.Info($"Query complete, start:{startTime}, end:{endTime}, feed Count:{totalFeedCount}");
        }
        catch (Exception ex)
        {
            logger.Warn($"Get activity failed, exception:{ex}.");
        }
        return activities;
    }

    public async Task<long?> GetCustomerDriveReportUsageAsync(DateTime datetime)
    {
        try
        {
            var report = await _reportApi.GetCustomerDriveReportUsage(datetime);
            var drivesUsed = report.UsageReportsValue[0].Parameters[0].IntValue;
            var shareDrivesUsed = report.UsageReportsValue[0].Parameters[1].IntValue;
            return drivesUsed + shareDrivesUsed;
        }
        catch (Exception ex)
        {
            logger.Warn($"Get customer report usage failed, exception:{ex}.");
            return null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _reportApi?.Dispose();
        _reportApi = null;
    }

    ~GoogleActivityService()
    {
        Dispose(false);
    }
}
