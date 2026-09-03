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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class WorkflowInstanceDao : BaseDao<RMWorkflowInstance>, IWorkflowInstanceDao
    {
        public void DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }

        public RMWorkflowInstance GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAsync(RMWorkflowInstance data)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var exist = await context.WorkflowInstance.Where(o => o.Id == data.Id).Select(o => o.Id).FirstOrDefaultAsync();
                    if (exist == Guid.Empty)
                    {
                        context.WorkflowInstance.Add(data);
                        return context.SaveChanges() > 0;
                    }
                    else
                    {
                        return await UpdateAsync(data);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> UpdateStepInfoAsync(Guid id, string stepId, string stepName)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var data = await context.WorkflowInstance.Where(o => o.Id == id).FirstOrDefaultAsync();
                    if (data == null) throw new Exception($"No such workflow instance data with id {id}");

                    data.CurStepId = stepId;
                    data.CurStepName = stepName;
                    data.ModifiedTime = DateTime.UtcNow;
                    return await UpdateAsync(data);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(Guid id, RMWorkflowStatus status)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var data = await context.WorkflowInstance.Where(o => o.Id == id).FirstOrDefaultAsync();
                    if (data == null) throw new Exception($"No such workflow instance data with id {id}");

                    data.Status = status;
                    data.ModifiedTime = DateTime.UtcNow;
                    return await UpdateAsync(data);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<string> GetReviewUserIdsByWFInstanceId(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                var stepId = ctx.WorkflowInstance.Where(d => d.Id == id).Select(w => w.CurStepId).FirstOrDefault();
                Guid.TryParse(stepId, out Guid sId); 
                var ownerIds = ctx.WorkflowStepConfiguration.Where(t => sId == t.StepId).Select(t => t.OwnerId).Distinct().ToList();
                var excludeOwner = ctx.WorkflowExcludeInstanceOwner.Where(w => w.InstanceId == id && w.StepId == stepId).Select(u => u.OwnerId).ToList();
                return ownerIds.Where(o => !excludeOwner.Contains(o)).ToList();
            }

        }

        public List<string> GetReviewUserIdsByManualInfo(RMManualApprove manualApprove)
        {
            using (var ctx = GetNewContext())
            {
                List<string> ownerIds;
                var workflowInstance = ctx.WorkflowInstance.First(d => d.Id == manualApprove.WorkflowInstanceId);
                var step = ctx.WorkflowStep.First(item => item.Id.ToString() == workflowInstance.CurStepId);
                if(step.ReviewerType == Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners)
                {
                    ownerIds = ctx.WorkflowSiteOwner.Where(item => item.DefinitionId == workflowInstance.DefinitionId.ToString() && item.SiteId == manualApprove.SiteId).Select(item => item.OwnerId).ToList();
                }
                else
                {
                    ownerIds = ctx.WorkflowStepConfiguration.Where(t => step.Id == t.StepId).Select(t => t.OwnerId).Distinct().ToList();
                }
                var excludeOwner = ctx.WorkflowExcludeInstanceOwner.Where(w => w.InstanceId == manualApprove.WorkflowInstanceId && w.StepId == step.Id.ToString()).Select(u => u.OwnerId).ToList();
                return ownerIds.Where(o => !excludeOwner.Contains(o)).ToList();
            }
        }

        public List<int> GetWorkflowInstanceCurrentStepUserIntIds(Guid workflowInstanceId, Guid siteId)
        {
            using(var context = GetNewContext())
            {
                var query = from instance in context.WorkflowInstance
                            join step in context.WorkflowStep
                            on instance.CurStepId equals step.Id.ToString()
                            where instance.Id == workflowInstanceId
                            select step;
                var workflowStep = query.FirstOrDefault();
                var ownerIds = new List<string>();

                if(workflowStep?.ReviewerType == Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners)
                {
                    ownerIds = context.WorkflowSiteOwner.Where(item =>
                    item.DefinitionId == workflowStep.DefinitionId.ToString() && item.SiteId == siteId
                    ).Select(item => item.OwnerId).ToList();
                }
                else
                {
                    ownerIds = context.WorkflowStepConfiguration.Where(item => workflowStep.Id == item.StepId)
                        .Select(t => t.OwnerId).Distinct().ToList();
                }

                var userIntIds = context.Account.Where(item => ownerIds.Contains(item.UserId)).Select(item => item.Id).ToList();
                return userIntIds;
            }
        }

        public List<int> GetWorkflowStepUserIntIds(Guid stepId, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var step = context.WorkflowStep.Find(stepId);
                var ownerIds = new List<string>();
                if (step.ReviewerType == Contract.RMWeb.CP.WorkflowReviewerType.SiteOwners)
                {
                    ownerIds = context.WorkflowSiteOwner.Where(item =>
                    item.DefinitionId == step.DefinitionId.ToString() && item.SiteId == siteId
                    ).Select(item => item.OwnerId).ToList();
                }
                else
                {
                    ownerIds = context.WorkflowStepConfiguration.Where(item => step.Id == item.StepId)
                        .Select(t => t.OwnerId).Distinct().ToList();
                }
                var userIntIds = context.Account.Where(item => ownerIds.Contains(item.UserId)).Select(item => item.Id).ToList();
                return userIntIds;
            }
        }
    }
}
