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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.Dashboard.AuditHandler
{
    public class CollectionDataAfterAuditHandler : IAfterAuditHandler
    {
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            info = new RMAuditInfo();
            info.Action = (AuditAction)action;
            info.Category = (AuditCategory)category;
            info.Module = (AuditModule)model;
            if (info.Object == null && returnValue != null)
            {
                info.Object = returnValue.ToString();
            }
            if (action == (int)AuditAction.DashboardCollectionDataJob)
            {
                if ((int)args[0] == (int)JobRunBy.Schedule)
                {
                    info.UserName = "RM_TS_RunSchedule";
                }
            }
            if(action == (int)AuditAction.EditArchiverPriceConfig)
            {
                info.Object = string.Empty;
            }
            if(action == (int)AuditAction.RunArchiverExportJob)
            {
                info.Object = returnValue as string;
            }
            return info;
        }
    }
}
