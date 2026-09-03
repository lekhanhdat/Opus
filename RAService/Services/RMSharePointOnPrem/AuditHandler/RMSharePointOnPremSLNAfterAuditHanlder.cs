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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMSharePointOnPrem.AuditHandler
{
    public class RMSharePointOnPremSLNAfterAuditHanlder : IAfterAuditHandler
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMSharePointOnPremSLNAfterAuditHanlder));

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            var auditInfo = info ?? new RMAuditInfo();

            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            auditInfo.Action = (AuditAction)action;

            try
            {
                if(returnValue == null)
                {
                    auditInfo.Status = (int)AuditStatus.Failed;
                }
                else
                {
                    auditInfo.Status = (int)AuditStatus.Successful;
                    auditInfo.Object = Convert.ToString(returnValue);
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while collect sharepoint on-premise scan local node after audit. Error: {e}");
            }

            return auditInfo;
        }
    }
}
