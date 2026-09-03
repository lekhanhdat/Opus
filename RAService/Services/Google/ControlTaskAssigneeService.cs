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
using System.Linq.Expressions;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.Service.Services.Google;

public class GControlTaskAssigneeService : IGControlTaskAssigneeService
{
    private readonly IGControlTaskAssigneeDao _controlTaskAssigneeDao;

    public GControlTaskAssigneeService(IGControlTaskAssigneeDao controlTaskAssigneeDao)
    {
        _controlTaskAssigneeDao = controlTaskAssigneeDao;
    }
    public async Task<List<GControlWorkflowDto>> GetAllAsync()
    {
        var result = await _controlTaskAssigneeDao.FindListAsync(item => !item.IsRemoved);
        return result.Select(item => GControlWorkflowDto.Init(item.Id, item.WorkflowId, item.StageId, (ApprovalProcessStatus)item.Status)).ToList();
    }

    public async Task<List<GControlWorkflowDto>> GetAllPendingTaskAssigneeAsync()
    {
        var result = await _controlTaskAssigneeDao.FindListAsync(item => !item.IsRemoved && item.Status == 0);
        return result.Select(item => GControlWorkflowDto.Init(item.Id, item.WorkflowId, item.StageId, (ApprovalProcessStatus)item.Status)).ToList();
    }

    public async Task<List<GControlWorkflowDto>> GetAllApprovalTaskAssigneeAsync()
    {
        List<int> approvedStatusList = 
        [
            (int)ApprovalProcessStatus.Approved,
            (int)ApprovalProcessStatus.Rejected
        ];
        var result = await _controlTaskAssigneeDao.FindListAsync(item => !item.IsRemoved && approvedStatusList.Contains(item.Status));
        return result.Select(item => GControlWorkflowDto.Init(item.Id, item.WorkflowId, item.StageId, (ApprovalProcessStatus)item.Status)).ToList();
    }

    public async Task<List<GControlWorkflowDto>> GetAllPendingReviewersAsync()
    {
        List<int> statusList = 
        [
            (int)ApprovalProcessStatus.AddMapping,
            (int)ApprovalProcessStatus.RemoveMapping
        ];
        var result = await _controlTaskAssigneeDao.FindListAsync(item => !item.IsRemoved && statusList.Contains(item.Status));
        return result.Select(item => GControlWorkflowDto.Init(item.Id, item.UserId,  (ApprovalProcessStatus)item.Status)).ToList();
    }

    public Task<GControlWorkflowDto> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<int> BatchAddAsync(IEnumerable<GControlWorkflowDto> userTaskMappingList)
    {
        if (userTaskMappingList.IsNullOrEmpty())
        {
            return 0;
        }
        var neededAddList = userTaskMappingList.ConvertAll(item => new GControlTaskAssigneeMapping
        {
            WorkflowId = item.WorkflowId,
            StageId = item.StageId,
            UserId = item.ManualReviewerId ,
            Status = (int)item.Status,
            ActionTime = DateTime.UtcNow.Ticks
        });
        return await _controlTaskAssigneeDao.BatchCreateAsync(neededAddList);
    }

    public Task<bool> UpdateAsync(GControlWorkflowDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<int> BatchDeleteAsync(IEnumerable<int> ids)
    {
        if (ids.IsNullOrEmpty())
        {
            return 0;
        }
        
        return await _controlTaskAssigneeDao.BatchSoftDeleteAsync(ids);
    }

    public Task DeleteByIdAsync(int id)
    {
        return Task.Run(() =>_controlTaskAssigneeDao.DeleteByKey(id));
    }
}