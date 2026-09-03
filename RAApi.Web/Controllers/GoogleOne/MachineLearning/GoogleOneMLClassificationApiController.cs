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
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.MachineLearning;

[Route("api/googleone/machinelearning/classifications")]
[TypeFilter(typeof(ValidateEnableMLFilter))]
public class GoogleOneMLClassificationsApiController : GoogleOneApiBaseController
{
    private readonly IRMMLTermService _mlClassificationService = PlatformWindsorManager.GetService<IRMMLTermService>();
    
    [HttpPost("getlist")]
    public async Task<MLTermResponseResult> LoadClassifications(MLTermQueryParam param)
    {
        return await Task.Run(() =>_mlClassificationService.LoadTerms(param));
    }
    
    [HttpPost("bulkadd")]
    public async Task<MLTermResponseResult> AddClassifications(List<MLTermDto> dtoList)
    {
        return await _mlClassificationService.AddTerms(dtoList);
    }
    
    [HttpPost("checkpredictionjobrunning")]
    public async Task<bool> CheckPredictionJobRunning(int action)
    {
        var result = await _mlClassificationService.CheckPredictionJobRunning(action);
        return result.MessageType == RAMessageType.Failed;
    }
    
    [HttpPost("bulkdelete")]
    public async Task<MLTermResponseResult> DeleteClassifications(List<Guid> ids)
    {
        return await _mlClassificationService.DeleteTerms(ids);
    }
    
    [HttpPost("updatedescription")]
    public async Task<MLTermResponseResult> UpdateDescription(MLTermDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description) )
        {
            return new MLTermResponseResult
            {
                HasError = true,
                ErrorMsg = "Description do not allow null"
            };
        }
        return await _mlClassificationService.UpdateDescription(dto);
    }
    
    [HttpPost("usageclassifications")]
    public async Task<MLTermResponseResult> LoadUsageClassifications(UsageTermQueryParam param)
    {
        return await Task.Run(() =>_mlClassificationService.LoadUsageTerms(param));
    }

    [HttpPost("setautoapply")]
    public async Task<MLTermResponseResult> SetAutoApply([FromBody] SetAutoApplyParam param)
    {
        return await _mlClassificationService.SetAutoApplyAsync(param.TermId, param.AutoApply);
    }
}