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
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Google.NexusGovernance;
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.Data.Nexus.Common;
using Cloud.Sdk.Data.Nexus.Governance;
using DataObjectType = Cloud.Sdk.Data.Nexus.Common.DataObjectType;
using TaskStatus = Cloud.Sdk.Data.Nexus.Governance.TaskStatus;
using TaskType = Cloud.Sdk.Data.Nexus.Governance.TaskType;
using TriggerType = Cloud.Sdk.Data.Nexus.Governance.TriggerType;

namespace AvePoint.RA.Service.Services.Google.GControlPlatform;

public class GControlPlatformTaskService : NexusGovernanceBaseService, IGControlPlatformTaskService
{
    private readonly string _predefinedObjectId = "01466fd3-4e40-47c7-9dc1-acfe3d392569";
    
    private readonly Guid _taskId = $"{TenantLocalValue.LogonGroupId}_{TaskType.InformationLifecycleManualApproval}".ToMd5();
    
    public Task<WFEngineTask> GetPlatformTask(Guid id)
    {
        return NexusGovernanceApiClient.TaskService.GetWFEngineTask(id);
    }

    public Task<bool> UpdatePlatformTask(Guid id, WFEngineTask platformTask)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CreateOpusTask()
    {
        var opusTask = await GetPlatformTask(_taskId);
        var result = opusTask != null;
        if (opusTask == null)
        {
            Logger.Info($"Creating Opus Manual Approval Task with ID: {_taskId}");
            result = await NexusGovernanceApiClient.TaskService.CreateWFEngineTask(new WFEngineTask()
            {
                Id = _taskId,
                Status = TaskStatus.None,
                CreatedBy = TenantLocalValue.LogonUserEmail,
                TenantId = TenantLocalValue.LogonGroupId,
                CreatedTime = DateTime.UtcNow.Ticks,
                TaskType = TaskType.InformationLifecycleManualApproval,
                StageId = Guid.Empty,
                OwnerId = string.Empty,
                DataObjectType = DataObjectType.File,
                DataObjectId = _predefinedObjectId,
                DataObjectName = "Opus Manual Approval Task",
                LastModifiedTime = DateTime.UtcNow.Ticks,
                TriggerType = TriggerType.OnDemand,
                LastModifiedBy = TenantLocalValue.LogonUserEmail,
                OperatedBy = TenantLocalValue.LogonUserEmail,
                ExpireOn = DateTime.UtcNow.AddDays(10).Ticks,
                Comment = "",
                Summary = "Records review for disposal"
            }); 
        }
        return result;
    }

    public Task<List<WFEngineTask>> SearchPlatformTasks(CommonRequest request)
    {
        throw new NotImplementedException();
    }

    public Guid GetTaskId()
    {
        return _taskId;
    }
}