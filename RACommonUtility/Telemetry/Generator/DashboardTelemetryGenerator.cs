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
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class DashboardTelemetryGenerator : TelemetryGenerator
    {
        private readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        protected static readonly IDashboardDao DashboardDao = PlatformWindsorManager.GetService<IDashboardDao>();

        public override TelemetryModule Module => TelemetryModule.Dashboard;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.DashboardLoaded, DashboardPageLoaded }
            };

        public CloudRecordsCommonRecord DashboardPageLoaded(IList<object> args)
        {
            var record = new CloudRecordsDashboardRecord
            {
                NonAdministrator = !AccountDao.CheckAdminRole(TenantLocalValue.LogonUserId)
            };
            var sourceActiveCounts = DashboardDao.GetActiveCountGroupBySource();
            record.ManagedRecordsCount = sourceActiveCounts.Values.Sum();

            var i18nSourceFlag = sourceActiveCounts.Keys.ToList().ConvertAll(item => TelemetryUtility.ConvertSourceFlag(item)).Where(item => item != null);
            record.DataSource = string.Join(", ", i18nSourceFlag);

            record.ManagedRecordCountBySources = string.Join(",", sourceActiveCounts.ToList());

            return record;
        }
    }
}
