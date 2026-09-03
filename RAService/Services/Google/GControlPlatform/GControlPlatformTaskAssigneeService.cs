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
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Google.NexusGovernance;
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.Data.Nexus.Common;
using Cloud.Sdk.Data.Nexus.Governance;
using TaskAssigneeType = Cloud.Sdk.Data.Nexus.Governance.TaskAssigneeType;
using TaskType = Cloud.Sdk.Data.Nexus.Governance.TaskType;

namespace AvePoint.RA.Service.Services.Google.GControlPlatform;

public class GControlPlatformTaskAssigneeService : NexusGovernanceBaseService, IGControlPlatformTaskAssigneeService
{
    private readonly Guid _taskId = $"{TenantLocalValue.LogonGroupId}_{TaskType.InformationLifecycleManualApproval}".ToMd5();
    
    public Task<int> CountPlatformTaskAssignees(CommonRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<WFEngineTaskAssignee> GetPlatformTaskAssignee(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<WFEngineTaskAssignee> AddPlatformTaskAssignee(WFEngineTaskAssignee model)
    {
        throw new NotImplementedException();
    }

    public async Task<List<WFEngineTaskAssignee>> GetCurrentPlatformTaskAssignees()
    {
        try
        {
            Logger.Info("Querying current task assignees");
            return await NexusGovernanceApiClient.TaskService.GetWFEngineTaskAssigneesByTaskId(_taskId);
        }
        catch (Exception ex)
        {
            Logger.Error($"Cannot get current task assignees {ex}");
            return [];
        }
    }

    public async Task<bool> AddPlatformTaskAssigneesAsync(IEnumerable<string> userIds)
    {
        if (userIds.IsNullOrEmpty())
        {
            return true;
        }
        var assigneesToAdd = userIds.Select(userId => new WFEngineTaskAssignee
        {
            AssigneeId = userId,
            TaskId = _taskId,
            TaskAssigneeType = TaskAssigneeType.User
        }).ToList();
        
        try
        {
            await NexusGovernanceApiClient.TaskService.AddWFEngineTaskAssignees(assigneesToAdd);
            Logger.Info($"Totally added assignee count :{assigneesToAdd.Count}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Occured error when add assignee :{ex}, will store the mapping in local db with the count: {assigneesToAdd.Count}");
            return false;
        }
    }

    public Task<bool> UpdatePlatformTaskAssignee(WFEngineTaskAssignee model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePlatformTaskAssignee(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<int> DeletePlatformTaskAssignees(IEnumerable<Guid> ids)
    {
        if (ids.IsNullOrEmpty())
        {
            return 0;
        }
        try
        {
            Logger.Info($"Deleting task assignees user Ids: {ids.Count()}");
            WFEngineTaskAssigneeBatchDeleteModel deleteModel = new ()
            {
                TaskAssigneeMappings = new Dictionary<Guid, List<string>>()
                {
                    {_taskId, ids.Select(id => id.ToString()).ToList()}
                }
            };
            await NexusGovernanceApiClient.TaskService.DeleteWFEngineTaskAssignees(deleteModel);

            return 1;
        }
        catch (Exception ex)
        {
            Logger.Error($"Occured error while deleting task assignees {ex}");
            return 0;
        }
    }
}