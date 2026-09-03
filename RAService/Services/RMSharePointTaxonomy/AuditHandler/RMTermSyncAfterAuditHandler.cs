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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;

namespace AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler
{
    public class RMTermSyncAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTermSyncAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            string reValue = Convert.ToString(returnValue);
            try
            {
                if ((action == (int)AuditAction.RunUniqueIDSettingJob || action == (int)AuditAction.RunSPOnPremUniqueIDSettingJob || action == (int)AuditAction.RunTeamsUniqueIDSettingJob) && reValue == RecordsConstants.UniqueId_NoNeedRunJob)
                {
                    return null;
                }
                info.Module = (AuditModule)model;
                info.Action = (AuditAction)action;
                info.Object = reValue;
                info.Category = (AuditCategory)category;
                bool fromTimerJobPage = false;
                if (string.IsNullOrEmpty(reValue))
                {
                    info.Status = 1;
                }
                if (info.Action == AuditAction.RunUniqueIDSettingJob || info.Action == AuditAction.RunSPOnPremUniqueIDSettingJob || info.Action == AuditAction.RunTeamsUniqueIDSettingJob)
                {
                    fromTimerJobPage = false;//TODO RECO-2623
                }
                else if (info.Action == AuditAction.RunCollectionJob4SPOnPrem || info.Action == AuditAction.RunCollectionJob
                     || info.Action == AuditAction.RunCollectionJob4OneDrive || info.Action == AuditAction.RunCollectionJob4EXO
                     || info.Action == AuditAction.RunCollectionJob4Teams)
                {
                    fromTimerJobPage = true;
                }
                else if (info.Action == AuditAction.ApplyEXOSetting || info.Action == AuditAction.ApplySharePointSetting || info.Action == AuditAction.ApplySharePointSettingSPOnPrem || info.Action == AuditAction.RunApplySharePointSettingSPOnPremSchedule
                     || info.Action == AuditAction.RunSharePointSettingsScheduleJob)
                {
                    if (args.Length >= 3 && args[2] != null)
                    {
                        fromTimerJobPage = (bool)args[2];
                    }
                    else
                    {
                        //RunApplyEXOSettingsScheduleJob
                        fromTimerJobPage = true;
                    }
                }
                else
                {
                    fromTimerJobPage = (bool)args[2];
                }
                if (info.Action == AuditAction.RunLocationTermSyncJob)
                {
                    ArgumentCheck.NotNull(reValue, nameof(reValue));
                    string folderSyncJobId = reValue;
                    string termSyncJobId = "PS" + reValue.Substring(2);
                    info.Object = folderSyncJobId + ";" + termSyncJobId + ";";
                }
                if (fromTimerJobPage)
                {
                    info.Module = AuditModule.ControlPanel;
                    info.Category = AuditCategory.TimerJobSettings;
                }
                if ((int)args[0] == (int)JobRunBy.Schedule)
                {
                    info.UserName = "RM_TS_RunSchedule";
                }
            }
            catch (Exception e)
            {
                logger.Warn("Term sync before audit handler, error message {0}", e.Message);
            }
            return info;
        }
    }
}
