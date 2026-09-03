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
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Service.Services.ManualApproval;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.ManualApproval;

[Route("api/googleone/admin/manualapproval")]
public class GoogleOneManualApprovalAdminApiController : GoogleOneApiBaseController
{
    private IRMManualApprovalService _manualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();

    public GoogleOneManualApprovalAdminApiController()
    {
    }
    
    [HttpPost("underreviewquery")]
    public Task<ManualApprovalPaginateResult> UnderReviewQuery([FromBody]ManualApprovalQueryDefinition queryDefinition)
    {
        queryDefinition.FromGControl = true;
        return _manualApprovalService.UnderReviewQueryAsync(queryDefinition);
    }
    
    [HttpPost("waitdisposalquery")]
    public Task<ManualApprovalPaginateResult> WaitDisposalQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
    {
        queryDefinition.FromGControl = true;
        return _manualApprovalService.WaitDiposalQueryAsync(queryDefinition);
    }
    
    [HttpPost("extendedquery")]
    public Task<ManualApprovalPaginateResult> ExtendQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
    {
        queryDefinition.FromGControl = true;
        return _manualApprovalService.ExtendQueryAsync(queryDefinition);
    }
    
    [HttpPost("historyazuretablequery")]
    public Task<List<ManualApprovalItem>> HistoryAzureTableQuery()
    {
        return _manualApprovalService.HistoryAzureTableQueryForGControlAsync();
    }
    
    [HttpPost("approve")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.Approve)]
    public Task<ManualApprovalActionResult> Approve([FromBody] ManualApprovalActionParams approveParameters)
    {
        using var performance = new PerformanceScope("GoogleOneAdminManualApprovalApiController.Approve");
        approveParameters.FromGControl = true;
        return _manualApprovalService.ApproveAsync(approveParameters);
    }
    
    [HttpPost("reject")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.Reject)]
    public Task<ManualApprovalActionResult> Reject([FromBody]ManualApprovalActionParams rejectParameters)
    {
        
        using var performance = new PerformanceScope("GoogleOneAdminManualApprovalApiController.Reject");
        rejectParameters.FromGControl = true;
        return _manualApprovalService.RejectAsync(rejectParameters);
    }
    [HttpPost("resetforworkflow")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.GControlResetManualWorkflow)]
    public Task<ManualApprovalActionResult> ResetManualReviewForWorkflow([FromBody] List<Guid> itemIds)
    {
        return _manualApprovalService.ResetManualReviewForWorkflow(itemIds, true);
    }
    [HttpPost("escalate")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.GControlEscalate)]
    public Task<ManualApprovalActionResult> Escalate([FromBody] ManualAprovalEscalateDefinition definition)
    {
        definition.FromGControl = true;
        return _manualApprovalService.EscalateAsync(definition);
    }

    [HttpPost("reassign")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.GControlReassign)]
    public Task<ManualApprovalActionResult> Reassign([FromBody] ManualAprovalEscalateDefinition definition)
    {
        definition.FromGControl = true;
        return _manualApprovalService.ReassignAsync(definition);
    }
    
    [HttpPost("extend")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.Extend)]
    public Task<ManualApprovalActionResult> Extend([FromBody] ManualApprovalExtendDefinition definition)
    {
        return _manualApprovalService.Extend(definition);
    }
    
    [HttpPost("restoreextended")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.GControlRestoreExtend)]
    public Task<ManualApprovalActionResult> RestoreExtended([FromBody]List<Guid> itemIds)
    {
        return _manualApprovalService.RestoreExtended(itemIds);
    }
    
    [HttpPost("updatesettinginfo")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.UpdateSetting)]
    public Task<bool> UpdateSettingInfo([FromBody] ManualApprovalSettings setting)
    {
        return _manualApprovalService.UpdateManualApprovalSetting(setting);
    }
    
    [HttpPost("disabledescalate")]
    public Task<bool> DisabledEscalate()
    {
        return _manualApprovalService.DisabledEscalateAsync();
    }
    
    [HttpPost("saveapprovalcommentoption")]
    [ValidManualApprovalParameterFilter(ManualApprovalActionType.SaveConfigOption)]
    public Task<bool> SaveApprovalCommentOption([FromBody]ManualApprovalCommentInfos option)
    {
        return _manualApprovalService.SaveApprovalCommentOptionAsync(option);
    }
    
    [HttpPost("getfilterdefaultoptions")]
    public Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptions()
    {
        return _manualApprovalService.GetFilterDefaultOptionsAsync();
    }

    [HttpPost("runbulkactionjob")]
    public string RunBulkActionJob(ManualApprovalJobParam param)
    {
        param.QueryDefintion.FromGControl = true;
        return JsonConvert.SerializeObject(_manualApprovalService.RunBulkActionJob(param));
    }
}