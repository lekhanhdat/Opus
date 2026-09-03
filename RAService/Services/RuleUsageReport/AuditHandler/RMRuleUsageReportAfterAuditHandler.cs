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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RuleUsageReport.AuditHandler
{
    public class RMRuleUsageReportAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMRuleUsageReportBeforeAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            try
            {
                info.Module = (AuditModule)model;
                info.Action = (AuditAction)action;
                info.Category = (AuditCategory)category;
                try
                {
                    info.Object = args[1].ToString();
                }
                catch (Exception ex)
                {
                    logger.Warn("Rule usage report get rule name ,message detail {0}", ex.ToString());
                }
            }
            catch (Exception e)
            {
                logger.Warn("Rule usage report after audit handler, error message {0}", e.Message);
            }
            return info;
        }
    }
}
