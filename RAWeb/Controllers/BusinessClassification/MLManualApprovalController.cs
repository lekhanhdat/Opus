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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.ManualApproval;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.Filters.MachineLearning;
using AvePoint.RA.Web.Common.Performance;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.Wrapper.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Cosmos;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser, preferred: false)] //TODO 确认
    public class MLManualApprovalController : BaseApiController
    {
        private IRMMLManualApprovalService _ManualApprovalService;
        private IRMMLManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService(ref _ManualApprovalService);

        [HttpPost]
        [PerformanceTestMonitor(FunctionName = "MLUnderReviewQuery")]
        public Task<ManualApprovalPaginateResult> UnderReviewQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            return ManualApprovalService.UnderReviewQueryAsync(queryDefinition);
        }

        [HttpPost]
        public Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptions()
        {
            return ManualApprovalService.GetFilterDefaultOptionsAsync();
        }

        [HttpPost]
        public Task<ManualApprovalWorkspacePaginateResult> QueryWorkspaces([FromBody] ManualApprovalWorkspaceQueryDefinition queryDefinition)
        {
            return ManualApprovalService.QueryWorkspacesAsync(queryDefinition);
        }

        [HttpPost]
        [ValidMLManualApprovalParameterFilter(MLManualApprovalActionType.Reassign)]
        public Task<ManualApprovalActionResult> Reassign([FromBody] ManualAprovalEscalateDefinition definition)
        {
            return ManualApprovalService.ReassignAsync(definition);
        }

        [HttpPost]
        [ValidTermTreeParameterFilter("ChangeTerm")]
        //[ValidReclassifyParameterFilter]
        public string ChangeTerm([FromBody] ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(ManualApprovalService.ChangeTerm(RealTimeAction.MLReviewChangeTerm,termDto));
        }

        [HttpPost]
        public string StartReclassifyJob([FromBody] ChangeTermDto termDto)
        {
            termDto.QueryDefintion??= new();
            return JsonConvert.SerializeObject(ManualApprovalService.ChangeTerm(RealTimeAction.MLReviewChangeTerm, termDto));
        }

        [HttpPost]
        public string Approve([FromBody] List<Guid> ids)
        {
            ChangeTermDto termDto = new();
            termDto.RecordIds = ids;
            return JsonConvert.SerializeObject(ManualApprovalService.ChangeTerm(RealTimeAction.MLReviewApprove, termDto));
        }

        [HttpPost]
        public string StartApproveJob([FromBody] List<ManualApprovalFilterDefinition> queryDefinition)
        {
            ChangeTermDto termDto = new();
            queryDefinition ??= new();
            termDto.QueryDefintion = queryDefinition;
            return JsonConvert.SerializeObject(ManualApprovalService.ChangeTerm(RealTimeAction.MLReviewApprove, termDto));
        }

    }
}