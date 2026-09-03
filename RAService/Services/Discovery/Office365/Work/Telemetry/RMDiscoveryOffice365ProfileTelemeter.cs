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
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Telemetry;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Telemetry;

public class RMDiscoveryOffice365ProfileTelemeter
{
    private readonly string _jobId;

    public RMDiscoveryOffice365ProfileTelemeter(string jobId)
    {
        _jobId = jobId;
    }

    public async Task RecordAsync(Guid o365TenantId, RMDiscoveryOffice365ProfileInfo profileInfo)
    {
        var telemetryData = new List<object>
        {
            _jobId,
            profileInfo.ScanType,
            profileInfo.ProfileType,
            JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                { "O365TenantId", o365TenantId.ToString() },
                { "Name", profileInfo.Name },
                { "SizeRange", profileInfo.SizeRange.ToString() },
                { "SizeRangeQueryMode", profileInfo.SizeRangeQueryMode.ToString() },
                { "GreaterThanEqualWithoutInDate", profileInfo.GreaterThanEqualWithoutInDate.ToString() },
                { "LessThanEqualWithoutInDate", profileInfo.LessThanEqualWithoutInDate.ToString() },
                { "FileExtensionIds", profileInfo.FileExtensionIdsJson },
                { "RuleIds", profileInfo.RuleIdsJson },
                { "SortBy", profileInfo.SortBy },
            }),
            (new DateTime(profileInfo.EndScanTime) - new DateTime(profileInfo.StartScanTime)).TotalSeconds,
            profileInfo.CurrentScanStatus
        };
        TelemetryContext.SendToQueue(TelemetryModule.DiscoveryAndAnalysis, TelemetryEventType.DiscoveryAndAnalysisEachProfileJobInfo, telemetryData);
    }

    public async Task FlushAsync()
    {
        await TelemetryContext.FlushAsync();
    }
}
