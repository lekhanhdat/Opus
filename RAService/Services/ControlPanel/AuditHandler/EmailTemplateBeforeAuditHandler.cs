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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class EmailTemplateBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(EmailTemplateBeforeAuditHandler));

        private IEmailTemplateService _EmailTemplateService;
        private IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService(ref _EmailTemplateService);
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();
            try
            {
                switch ((AuditAction)action)
                {
                    case AuditAction.EditEmailTempalte:
                        EmailTemplateDto emailTemplate = args[0] as EmailTemplateDto;
                        EmailTemplateDto oldTemplate = EmailTemplateService.GetEmailTemplateById(emailTemplate.Id);
                        info.Object = emailTemplate?.Name ?? string.Empty;
                        var audit = new AuditItem
                        {
                            TargetSetting = "RM_CP_EamilTemplate_IfUseDefaultFooter",
                            NewValue = emailTemplate.IsUseDefaultFooter == (int)DefaultFooterStatus.UseDefaultFooter ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                            OldValue = oldTemplate.IsUseDefaultFooter == (int)DefaultFooterStatus.UseDefaultFooter ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                        };
                        info.ModifyContent.Add(audit);
                        break;
					case AuditAction.CreateEmailTemplate:
						EmailTemplateDto createdEmailTemplate = args[0] as EmailTemplateDto;
						info.Object = createdEmailTemplate?.Name ?? string.Empty;
						var createdEmailTemplateAudit = new AuditItem
						{
							TargetSetting = "RM_CP_EamilTemplate_IfUseDefaultFooter",
							NewValue = createdEmailTemplate?.IsUseDefaultFooter == (int)DefaultFooterStatus.UseDefaultFooter ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
						};
						info.ModifyContent.Add(createdEmailTemplateAudit);
						break;
					case AuditAction.DeleteEmailTemplate:
						EmailTemplateDto deletedEmailTemplate = args[0] as EmailTemplateDto;
						info.Object = deletedEmailTemplate?.Name ?? string.Empty;
						break;
					default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return info;
        }

    }
}
