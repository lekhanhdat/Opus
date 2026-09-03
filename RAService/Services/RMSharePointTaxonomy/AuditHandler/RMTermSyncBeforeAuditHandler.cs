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

namespace AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler
{
    public class RMTermSyncBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTermSyncBeforeAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            try
            {
                info.Module = (AuditModule)model;
                info.Action = (AuditAction)action;
                info.Category = (AuditCategory)category;

                //bool fromTimerJobPage = false;
                //if (info.Action == AuditAction.RunUniqueIDSettingJob)
                //{
                //    fromTimerJobPage = false;//TODO RECO-2623
                //}
                //else if (info.Action == AuditAction.RunCollectionJob)
                //{
                //    fromTimerJobPage = true;
                //}
                //else
                //{
                //    fromTimerJobPage = (bool)args[2];
                //}

                //if (fromTimerJobPage)
                //{
                //    info.Module = AuditModule.ControlPanel;
                //    info.Category = AuditCategory.TimerJobSettings;
                //}
                return info;
            }
            catch (Exception e)
            {
                logger.Warn("Term sync before audit handler, error message {0}", e.Message);
            }

            return info;
        }
    }
}
