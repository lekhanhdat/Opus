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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Tenant.Upgrade
{
    public class RMTenantDelayUpgradeProcessor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMTenantDelayUpgradeProcessor));

        private readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        public RMTenantDelayUpgradeProcessor(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.ManualApproval);
            ReportManager.StartUpdateJobProgress();
            ReportManager.IncreaseBase(100000);
            System.Threading.Tasks.Task.Run(() =>
            {
                ReportManager.Increase();
            });
        }

        public async System.Threading.Tasks.Task RunAsync()
        {
            try
            {
                s_logger.Info($"Start process [{TenantLocalValue.LogonGroupId}] upgrade logic.");
                var upgraderDefinitions = RMTenantUpgradeHelper.GetDelayUpgraderDefinitions(TenantLocalValue.LogonGroupId);
                foreach (var upgradeDefinition in upgraderDefinitions)
                {
                    await RunAsync(upgradeDefinition);
                }
                s_logger.Info($"Finish process [{TenantLocalValue.LogonGroupId}] upgrade logic.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process delay upgrade logic. Error: {e}");
            }

            ReportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
        }

        private async Task<bool> RunAsync(RMUpgraderDefinition upgraderDefinition)
        {
            try
            {
                s_logger.Info($"Start process [{upgraderDefinition.Feature}] upgrade logic.");

                RMTenantUpgradeHelper.SetToUpgrading(TenantLocalValue.LogonGroupId);
                var finalStatus = await upgraderDefinition.Upgrader.RunAsync();

                s_logger.Info($"Process [{upgraderDefinition.Feature}] upgrade logic [{finalStatus}].");

                RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, upgraderDefinition.Feature, finalStatus);
                return true;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process [{upgraderDefinition.Feature}]. Error: {e}");
                return false;
            }
        }
    }
}
