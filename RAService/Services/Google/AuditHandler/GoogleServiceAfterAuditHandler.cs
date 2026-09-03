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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Google.AuditHandler
{
    public class GoogleServiceAfterAuditHandler : IAfterAuditHandler
    {
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            string returnValueToString = Convert.ToString(returnValue);
            info.Module = (AuditModule)model;
            info.Action = (AuditAction)action;
            info.Category = (AuditCategory)category;

            if (info.Object == null && returnValue != null)
            {
                info.Object = returnValueToString;
            }

            if (string.IsNullOrEmpty(returnValueToString))
            {
                info.Status = 1;
            }
            switch ((AuditAction)action)
            {
                case AuditAction.SaveGeneralSetting:
                    HandleAuditSaveGeneralSetting(info, returnValue);
                    break;
                case AuditAction.SaveLabelSetting:
                    HandleAuditSaveLabelSetting(info, args, returnValue);
                    break;
                case AuditAction.GoogleApplySettings:
                    HandleAuditGoogleApplySettings(info, args);
                    break;
                case AuditAction.GoogleDataSynchronization:
                    HandleAuditGoogleDataSynchronization(info, args);
                    break;
            }
            return info;
        }
        private void HandleAuditSaveGeneralSetting(RMAuditInfo info, object returnValue)
        {
            RAReturnMessage msg = (RAReturnMessage)returnValue;
            if (msg != null)
            {
                info.Status = (int)msg.MessageType;
            }
        }
        private void HandleAuditSaveLabelSetting(RMAuditInfo info, object[] args, object returnValue)
        {
            List<AuditItem> cretiaAudit = info.ModifyContent.Where(a => a.Id == Guid.Empty).ToList();

            if (cretiaAudit.Count > 0)
            {
                RMGoogleTreeNode node = (RMGoogleTreeNode)args[0];
                var enableLabelSettings = !node.IsNullClassificationSetting;
                if (enableLabelSettings)
                {
                    if (node.DeployLabelMethod == DeployLabelMethod.UseAutoClassification)
                    {
                        AuditHelper.SaveNewAuditItem(info, "RM_JS_SPS_AutoClassification_ApplyPolicy", ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules));
                    }
                }
            }

            RAReturnMessage msg = returnValue is string strValue ?
            new RAReturnMessage
            {
                ErrorMessage = strValue,
                MessageType = string.IsNullOrWhiteSpace(strValue) ? RAMessageType.Successful : RAMessageType.Failed
            } : (RAReturnMessage)returnValue;

            if (msg != null)
            {
                info.Status = (int)msg.MessageType;
            }
        }
        private void HandleAuditGoogleApplySettings(RMAuditInfo info, object[] args)
        {
            bool fromTimerJobPage = args.Length >= 3 && args[2] != null ? (bool)args[2] : true;

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
        private void HandleAuditGoogleDataSynchronization(RMAuditInfo info, object[] args)
        {
            bool fromTimerJobPage = args[2] == null;

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
    }

}
