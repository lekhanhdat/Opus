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
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Service.Services.DataIngestion;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/dataingestion/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [RMAgentApiPerformanceLogger]
    public class DataIngestionApiController : ControllerBase
    {
        private readonly RMDataIngestionService _dataIngestionService = new();

        [HttpPost]
        public Task<RMDataIngestionMessageSendReceipt> SendMessage([FromBody] RMDataIngestionMessageDto message)
        {
            return _dataIngestionService.SendMessageAsync(message);
        }
        
        [HttpPost]
        public Task<RMDataIngestionBlobReference> GenerateBlobReference([FromBody] RMDataIngestionBlobNamingContext blobNamingContext)
        {
            return _dataIngestionService.GenerateBlobReferenceAsync(blobNamingContext);
        }

        [HttpGet]
        public Task<string> GenerateBlobSasUri([FromQuery] RMDataIngestionType ingestionType, [FromQuery] string blobName)
        {
            return _dataIngestionService.GenerateBlobSasUri(ingestionType, blobName);
        }

        [HttpGet]
        public Task<RMDataIngestionExecutionResult> GetIngestionExecutionResults([FromQuery] string uniqueId, [FromQuery] string messageId)
        {
            return _dataIngestionService.GetExecutionResultAsync(uniqueId, messageId);
        }

        [HttpPost]
        public Task<bool> DeleteBlob([FromBody] RMDataIngestionBlobDto blobDto)
        {
            return _dataIngestionService.DeleteBlobAsync(blobDto.IngestionType, blobDto.BlobName);
        }
    }
}
