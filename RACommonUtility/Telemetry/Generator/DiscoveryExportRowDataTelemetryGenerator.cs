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
using Cloud.Sdk.Telemetry.Data.CloudRecords;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator;

public class DiscoveryExportRowDataTelemetryGenerator : TelemetryGenerator
{
    public override TelemetryModule Module => TelemetryModule.DiscoveryExportRowData;

    protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping
        => new()
        {
            { TelemetryEventType.ExportCsvFile, GetExportFileInformation }
        };

    public CloudRecordsCommonRecord GetExportFileInformation(IList<object> args)
    {
        var reportName = args[0].ToString();
        var reportSize = Convert.ToInt64(args[1]);
        var fileCount = Convert.ToInt32(args[2]);
        var runningTime = Convert.ToInt64(args[3]);
        CloudRecordsExportRowDataRecord record = new ()
        {
            ReportName = reportName,
            ReportSize = reportSize,
            FileCount = fileCount,
            RunningTime = runningTime
        };

        return record;
    }
}