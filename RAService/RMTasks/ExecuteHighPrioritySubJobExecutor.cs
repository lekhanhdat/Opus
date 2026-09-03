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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class ExecuteHighPrioritySubJobExecutor : ITaskExecutor
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(ExecuteHighPrioritySubJobExecutor));

        private static readonly ITenantService s_tenantService = PlatformWindsorManager.GetService<ITenantService>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tenantInfoes = s_tenantService.GetAllAvailableTenantInfo();

                foreach(var tenantInfo in tenantInfoes)
                {
                    await TenantUtil.RunUnderTenantAsync(tenantInfo.TenantId, tenantInfo.RegisterEmail, async () =>
                    {
                        try
                        {
                            var controller = new RMO365TenantSubJobController();
                            await controller.RunAsync();
                        }
                        catch (Exception e)
                        {
                            s_logger.Error($"An error occurred while process tenant [{tenantInfo.TenantId}]. Error: {e}");
                        }
                    });
                }

            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while execute high priority sub job. Error: {e}");
            }
        }
    }
}
