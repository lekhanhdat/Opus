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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using Cloud.Sdk.Telemetry.Data;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class TermManagementTelemetryGenerator : TelemetryGenerator
    {
        private readonly ITermDao TermDao = PlatformWindsorManager.GetService<ITermDao>();

        private readonly ITermSetMembershipDao TermSetMembershipDao = PlatformWindsorManager.GetService<ITermSetMembershipDao>();

        public override TelemetryModule Module => TelemetryModule.TermManagement;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.TermSynchronise, TermSynchronise }
            };

        public CloudRecordsCommonRecord TermSynchronise(IList<object> args)
        {
            var record = new CloudRecordsTermManagementRecord();
            var maxLenTermPath = TermSetMembershipDao.GetMaxDeepTermPath();

            if (string.IsNullOrEmpty(maxLenTermPath))
            {
                Logger.Warn($"The tenant {TenantLocalValue.LogonGroupId} can't find max deep term path.");
                return null;
            }

            record.TermMaxLevel = maxLenTermPath.Split('/').Count();
            record.TermCount = TermDao.Count(item => !item.IsRemoved);
            return record;
        }
    }
}
