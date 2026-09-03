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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.AccountManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static RecordManager.Controllers.BusinessClassification.TermSynchronizationApiController;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidManualReviewParameterFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidManualReviewParameterFilter));
        public IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public ValidManualReviewParameterFilter()
        { }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parmObj != null)
            {
                var itemIds = GetItemIds(parmObj);
                if (!(await ValidPermissionAsync(itemIds)))
                {
                    actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
        }

        private async Task<bool> ValidPermissionAsync(List<int> ids)
        {
            using (var performance0 = new PerformanceScope("ValidManualReviewParameterFilter.ValidPermission"))
            {
                IUserService UserService = new UserService();
                IRMManualApproveDao ManualApproveDao = new RMManualApproveDao();
                List<RMManualApprove> manualApproveDtos;
                //using (var performance = new PerformanceScope("ValidManualReviewParameterFilter.GetRMManualApproves"))
                {
                    manualApproveDtos = await ManualApproveDao.FindListWithColumnsAsync(m => new { m.SourceFlag, m.Id, m.NodeId, m.SiteId, m.EscalateTo, m.WorkflowInstanceId }, r => ids.Contains(r.Id));
                }
               
                if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin)))
                {
                    var account = await AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId);
                    var userIdAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                    var userIdAndGroupIdStrs = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    foreach (var dto in manualApproveDtos)
                    {
                        //using (var performance = new PerformanceScope("ValidManualReviewParameterFilter.IsEscalateToUser"))
                        {
                            var isEscalateToUser = IsEscalateToUser(dto.EscalateTo, userIdAndGroupIds);
                            if (!isEscalateToUser)
                            {
                                if (dto.WorkflowInstanceId != Guid.Empty)
                                {
                                    Dictionary<Guid, List<string>> dictionary = new Dictionary<Guid, List<string>>();
                                    //using (var performance1 = new PerformanceScope("ValidManualReviewParameterFilter.GetManualNodeAndApproverMapping"))
                                    {
                                        dictionary = ManualApproveDao.GetManualNodeAndApproverMapping(dto.SiteId, new List<Guid>() { dto.NodeId });
                                    }
                                    if (dictionary != null && dictionary.Count > 0)
                                    {
                                        List<string> tempUserIds = new List<string>();
                                        foreach (var id in dictionary.Values)
                                        {
                                            tempUserIds.AddRange(id);
                                        }
                                        List<string> uniqueUserIds = tempUserIds.Where(a => a != null).Distinct().ToList();
                                        if (userIdAndGroupIdStrs.Any(a => uniqueUserIds.Contains(a)))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            logger.Warn("Current user has no permission for workflow. Workflow id:{0} Node id:{1}", dto.WorkflowInstanceId, dto.NodeId);
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        logger.Warn("No user under workflow. Workflow id:{0} Node id:{1}", dto.WorkflowInstanceId, dto.NodeId);
                                        return false;
                                    }
                                }
                                else
                                {
                                    logger.Warn("Current user doesn't have permission for node:{0}", dto.NodeId);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private List<int> GetEscalateToList(string escalateToStr)
        {
            List<int> escalateIds = new List<int>();
            if (!string.IsNullOrEmpty(escalateToStr))
            {
                try
                {
                    foreach (var escalateIdStr in escalateToStr.Split('|'))
                    {
                        if (string.IsNullOrWhiteSpace(escalateIdStr))
                        {
                            continue;
                        }
                        int escalateId = Convert.ToInt32(escalateIdStr);
                        if (!escalateIds.Contains(escalateId))
                        {
                            escalateIds.Add(escalateId);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while getting escalate user list. Error:{0}", e.ToString());
                }
            }
            return escalateIds;
        }

        private bool IsEscalateToUser(string escalateToStr, List<int> userIdAndGroupIds)
        {
            bool isEscalateUser = false;
            var escalateToList = GetEscalateToList(escalateToStr);
            if (escalateToList != null && escalateToList.Count > 0)
            {
                if (userIdAndGroupIds.Any(a => escalateToList.Contains(a)))
                {
                    isEscalateUser = true;
                }
            }
            return isEscalateUser;
        }

        private List<int> GetItemIds(Object parmObj)
        {
            List<int> result = new List<int>();
            if (parmObj as ManualReviewStatus != null)
            {
                result.AddRange(((ManualReviewStatus)parmObj).ids);
            }
            else if (parmObj as EscalateModel != null)
            {
                result.AddRange(((EscalateModel)parmObj).ids);
            }
            else if (parmObj as ChangeActionModel != null)
            {
                result.AddRange(((ChangeActionModel)parmObj).ids.Select(i => i.id).ToList());
            }
            return result;
        }

    }
}