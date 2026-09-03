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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidateHoldActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidateHoldActionFilter));
        public ValidateHoldActionFilter()
        {
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (await SecurityTrimmingHelper.EqualsThisPermission(RACommonUtility.Permission.PermissionWrappers.StandardUser))
            {
                logger.Info($"User have no permission access hold {TenantLocalValue.LogonUserId}");
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (!ValidateParameter(parmObj))
            {
                logger.Info($"Action parameter incorrect.");
                actionContext.Result = new ObjectResult("Incorrect Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            var errorMessage = await ValidateEmailNotificatonAsync(parmObj);
            if (errorMessage.IsNotNullOrEmpty())
            {
                actionContext.Result = new ObjectResult(errorMessage) { StatusCode = (int)HttpStatusCode.Forbidden };
            }
        }

        private bool ValidateParameter(Object parmObj)
        {
            List<Guid> recordIds = new List<Guid>();
            if (parmObj as ChangeHoldDto != null)
            {
                var dto = (ChangeHoldDto)parmObj;
                if (dto.recordsId != null && dto.recordsId.Count > 0)
                {
                    recordIds.AddRange(dto.recordsId);
                }
            }
            else if (parmObj as UpdateHoldDto != null)
            {
                var dto = (UpdateHoldDto)parmObj;
                if (dto.ReletedIds != null && dto.ReletedIds.Count > 0)
                {
                    recordIds.AddRange(dto.ReletedIds);
                }
            }
            if (recordIds.Count > 0)
            {
                AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao ExplorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                var notSupportNodeLevelList = new List<int>() { (int)RMNodeLevel.PhysicalCustom, (int)RMNodeLevel.Folder };
                List<AvePoint.RA.DB.Explorer.Model.Record> allRecord = ExplorerDao.GetRecordByIds(recordIds);
                if (allRecord.Count > 0 && allRecord.Where(r => notSupportNodeLevelList.Contains(r.NodeType)).ToList().Count > 0)
                {
                    logger.Error("Hold action doesn't support physical customer container or sharepoint/onedirve folder.");
                    return false;
                }
            }
            return true;
        }

        private async Task<string> ValidateEmailNotificatonAsync(Object parmObj)
        {
            if (parmObj as UpdateHoldDto != null)
            {
                var dto = (UpdateHoldDto)parmObj;
                if (dto.HoldSetting.EmailNotification != null && dto.HoldSetting.EmailNotification.IsEnabled)
                {
                    if (dto.HoldSetting.EmailNotification.ReminderDurationDays < 1 || dto.HoldSetting.EmailNotification.ReminderDurationDays > 365)
                    {
                        return "Reminder duration must be between 1 and 365.";
                    }
                }
            }
            return string.Empty;

        }
    }
}