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
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class DbUpgradeGenerator : TelemetryGenerator
    {
        public override TelemetryModule Module => TelemetryModule.DBUpgrade;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.DBUpgradeInfo, GetUpgradeInfoRecord },
            };

        public CloudRecordsCommonRecord GetUpgradeInfoRecord(IList<object> args)
        {
            var record = new CloudRecordsReportJobExecutionRecord();
            record.JobType = args[0].ToString();
            record.JobStatus = args[1].ToString();
            return record;
        }
    }
}
