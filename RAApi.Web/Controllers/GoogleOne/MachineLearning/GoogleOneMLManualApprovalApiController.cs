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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.MachineLearning
{
    [Route("api/googleone/machinelearning/manualapproval")]
    public class GoogleOneMLManualApprovalApiController : GoogleOneApiBaseController
    {
        private readonly IRMManualApprovalService _manualApprovalService = PlatformWindsorManager.GetService<IRMManualApprovalService>();

        private readonly IRMMLManualApprovalService _mlmanualApprovalService = PlatformWindsorManager.GetService<IRMMLManualApprovalService>();

        private readonly ITrainingScopeService _trainingScopeService = PlatformWindsorManager.GetService<ITrainingScopeService>();

        [HttpPost("getclassificationfilters")]
        public Task<List<MLTermDto>> MLClassificationFilters()
        {
            return Task.Run(() => _trainingScopeService.GetAllMLTerm());
        }

        [HttpPost("underreviewquery")]
        public Task<ManualApprovalPaginateResult> UnderReviewQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            queryDefinition.FromGControl = true;
            return _mlmanualApprovalService.UnderReviewQueryAsync(queryDefinition);
        }

        [HttpPost("getfilterdefaultoptions")]
        public Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptions()
        {
            return _mlmanualApprovalService.GetFilterDefaultOptionsAsync();
        }

        [HttpPost("workspaces")]
        public Task<ManualApprovalWorkspacePaginateResult> QueryWorkspaces([FromBody] ManualApprovalWorkspaceQueryDefinition queryDefinition)
        {
            queryDefinition.ContentSource = SourceFlag.Google;
            return _manualApprovalService.QueryWorkspacesAsync(queryDefinition);
        }

        [HttpPost("changeclassification")]
        public Task<string> ChangeClassification([FromBody] ChangeTermDto classificationDto)
        {
            return Task.Run(() => JsonConvert.SerializeObject(_mlmanualApprovalService.ChangeTerm(RealTimeAction.MLReviewChangeTerm, classificationDto)));
        }

        [HttpPost("startreclassifyjob")]
        public Task<string> StartReclassifyJob([FromBody] ChangeTermDto classificationDto)
        {
            classificationDto.QueryDefintion ??= new();
            return Task.Run(() => JsonConvert.SerializeObject(_mlmanualApprovalService.ChangeTerm(RealTimeAction.MLReviewChangeTerm, classificationDto)));
        }

        [HttpPost("approve")]
        public Task<string> Approve([FromBody] List<Guid> ids)
        {
            ChangeTermDto classificationDto = new();
            classificationDto.RecordIds = ids;
            return Task.Run(() => JsonConvert.SerializeObject(_mlmanualApprovalService.ChangeTerm(RealTimeAction.MLReviewApprove, classificationDto)));
        }

        [HttpPost("startapprovejob")]
        public Task<string> StartApproveJob([FromBody] List<ManualApprovalFilterDefinition> queryDefinition)
        {
            ChangeTermDto classificationDto = new();
            queryDefinition ??= new();
            classificationDto.QueryDefintion = queryDefinition;
            return Task.Run(() => JsonConvert.SerializeObject(_mlmanualApprovalService.ChangeTerm(RealTimeAction.MLReviewApprove, classificationDto)));
        }

        [HttpPost("reassign")]
        public Task<ManualApprovalActionResult> Reassign([FromBody] ManualAprovalEscalateDefinition definition)
        {
            return _mlmanualApprovalService.ReassignAsync(definition);
        }
    }
}
