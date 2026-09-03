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
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class RestoreJobTelemetryGenerator : TelemetryGenerator
    {
        public override TelemetryModule Module => TelemetryModule.RestoreJob;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.RunJob, RunJob },
            };

        public CloudRecordsCommonRecord RunJob(IList<object> args)
        {
            var record = new CloudRecordsRestoreJobRecord();
            var jobId = Convert.ToString(args[0]);
            record.JobId = jobId;
            record.StartTime = new DateTime(Convert.ToInt64(args[1]));
            record.FinishTime = new DateTime(Convert.ToInt64(args[2]));
            record.Restoresize = Convert.ToInt64(args[3]);
            record.RestoreJobStatus = Convert.ToString(args[4]);
            record.RestoreFileCount = Convert.ToInt64(args[5]);
            record.RestoreFileAverageAge = Convert.ToInt64(args[6]);
            record.RestoreFileCurrentVersionCount = Convert.ToInt64(args[7]);
            record.RestoreFileHisVersionCount = Convert.ToInt64(args[8]);
            record.TenantId = TenantLocalValue.LogonGroupId;
            if (string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail))
            {
                TenantLocalValue.LogonUserEmail = jobId;
            }
            return record;
        }

    }
}
