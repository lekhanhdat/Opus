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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class DiscoveryAndAnalysisTelemetryGenerator : TelemetryGenerator
    {
        public override TelemetryModule Module => TelemetryModule.DiscoveryAndAnalysis;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping => new()
        {
            {TelemetryEventType.DiscoveryAndAnalysisEachJobInfo, RecordDiscoveryAndAnalysisInfo},
            {TelemetryEventType.DiscoveryAndAnalysisEachProfileJobInfo, RecordDiscoveryAndAnalysisProfileInfo}
        };

        public CloudRecordsCommonRecord RecordDiscoveryAndAnalysisInfo(IList<object> args)
        {
            var record = new CloudRecordsDiscoveryRecord
            {
                JobId = args[0].ToString(),
                ContentSource = args[1].ToString(),
                JobType = args[2].ToString(),
                JobVersion = args[3].ToString(),
                Duration = Convert.ToInt64(args[4]),
                JobStatus = args[5].ToString(),
                ScannedNodeCount = Convert.ToInt32(args[6]),
                ScannedFileSumCount = Convert.ToInt64(args[7]),
                ScannedFileTotalSize = Convert.ToInt64(args[8]),
            };
            return record;
        }

        public CloudRecordsCommonRecord RecordDiscoveryAndAnalysisProfileInfo(IList<object> args)
        {
            var record = new CloudRecordsDiscoveryProfileRecord
            {
                JobId = args[0].ToString(),
                ScanType = args[1].ToString(),
                ProfileType = args[2].ToString(),
                Definition = args[3].ToString(),
                Duration = Convert.ToInt64(args[4]),
                JobStatus = args[5].ToString(),
            };
            return record;
        }
    }
}
