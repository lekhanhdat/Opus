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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class ContentRepositoryManagementTelemetryGenerator : TelemetryGenerator
    {
        private readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        public override TelemetryModule Module => TelemetryModule.ContentRepositoryManagement;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.ApplySettings, ApplySettings },
                { TelemetryEventType.RunEnforceRuleActions, EnforceRuleActions },
                { TelemetryEventType.ContentPageLoaded, PageLoaded }
            };



        public CloudRecordsCommonRecord ApplySettings(IList<object> args)
        {
            var record = new CloudRecordsApplySettingsRecord();
            var dataSource = (SourceFlag)Convert.ToInt32(args[0]);
            record.DataSource = TelemetryUtility.ConvertSourceFlag(dataSource);
            if (dataSource == SourceFlag.SharePoint)
            {
                record.ApplySettingToAllNodes = (RunApplySettingMethod)Convert.ToInt32(args[1]) == RunApplySettingMethod.AllScope;
            }
            else if (dataSource == SourceFlag.Exchange)
            {
                record.ApplySettingToAllNodes = false;
            }
            else if (dataSource == SourceFlag.SharePointOnPrem)
            {
                record.ApplySettingToAllNodes = (RunApplySettingMethod)Convert.ToInt32(args[1]) == RunApplySettingMethod.AllScope;
            }

            //if(AnalysisSettingsMapping.TryGetValue(dataSource, out var analysisSettingAction))
            //{
            //    analysisSettingAction(record);
            //}

            return record;
        }

        public CloudRecordsCommonRecord EnforceRuleActions(IList<object> args)
        {
            var record = new CloudRecordsEnforceRuleActionRecord();
            var dataSource = Convert.ToInt32(args[0]);
            record.DataSource = TelemetryUtility.ConvertSourceFlag(dataSource);
            return record;
        }

        public CloudRecordsCommonRecord PageLoaded(IList<object> args)
        {
            var record = new CloudRecordsContentRepositoryRecord();
            record.NonAdministrator = !AccountDao.CheckAdminRole(TenantLocalValue.LogonUserId);
            return record;
        }
    }
}
