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
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public abstract class TelemetryGenerator
    {
        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public abstract TelemetryModule Module { get; }

        protected abstract Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping { get; }

        public CloudRecordsCommonRecord GenerateTelemetryRecord(TelemetryEventType eventType, IList<object> args)
        {
            if(EventTypeGenerateMapping == null)
            {
                //throw new Exception("The event type generate mapping is empty.");
                return null;
            }

            if(!EventTypeGenerateMapping.TryGetValue(eventType, out var telemetryGenerateFunc))
            {
                //throw new Exception($"The event: [{eventType}] can't find telemetry generate function.");
                return null;
            }

            var record = telemetryGenerateFunc(args);

            if(record == null)
            {
                return null;
            }

            record.DataCenter = DataCenterManagent.GetDataCenter();
            record.TenantId = TenantLocalValue.LogonGroupId;
            record.UserName = TenantLocalValue.LogonUserEmail;
            record.DateTime = DateTime.UtcNow;
            record.EventType = eventType.ToString();
            record.Module = Module.ToString();
            return record;
        }
    }
}
