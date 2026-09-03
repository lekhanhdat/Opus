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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class EmailTemplateAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(EmailTemplateAfterAuditHandler));
		public IRMEamilTemplateDao EmailTemplateDao => PlatformWindsorManager.GetService<IRMEamilTemplateDao>();

		public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = null;
            try
            {
                auditInfo = new RMAuditInfo();
                switch ((AuditAction)action)
                {
                    case AuditAction.EditEmailTempalte:
					case AuditAction.CreateEmailTemplate:
					case AuditAction.DeleteEmailTemplate:
						auditInfo.Module = (AuditModule)model;
                        auditInfo.Category = (AuditCategory)category;
                        auditInfo.Action = (AuditAction)action;
						EmailTemplateDto emailTemplate = new();
						if ((AuditAction)action == AuditAction.DeleteEmailTemplate)
                        {
							RMEmailTemplate templateInfo = EmailTemplateDao.GetEmailTemplateByUniqueId((Guid)args[0]);
                            emailTemplate.Name = templateInfo.DisplayName;
                        }
                        else
                        {
                            emailTemplate = args[0] as EmailTemplateDto;
                        }
                        auditInfo.Object = emailTemplate != null ? emailTemplate.Name : string.Empty;
                        auditInfo.ModifyContent = info.ModifyContent;
						auditInfo.Status = returnValue.ToString() == string.Empty ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        break;
                    default:
                        break;
                }
                auditInfo.Module = (AuditModule)model;
                return auditInfo;
            }
            catch (Exception e)
            {
                ArgumentCheck.NotNull(auditInfo, nameof(auditInfo));
                auditInfo.Status = (int)AuditStatus.Failed;
                logger.Error(e.Message);
                throw;
            }
        }
    }
}
