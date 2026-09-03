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
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Service.Services.Discovery.FileSystem;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/FSDiscovery/[action]")]
    [APIScopeFilter(RA.Contract.Common.ContractConstants.HybridAgentScope)]
    public class FSDiscoveryController : RAWebApiBase
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(FSDiscoveryController));

        [HttpPost]
        public Task UploadAnalyzedFileToStorage([FromBody] DiscoveryAnalyzedDataInfo dataInfo)
        {
            try
            {
                IRMDiscoveryFSConfigurationService fsConfigurationService = new RMDiscoveryFSConfigurationService();
                fsConfigurationService.UploadAnalyzedFileToStorage(dataInfo);
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while upload analyzed file to storage. Ex: {ex.Message}.");
            }
            return Task.CompletedTask;
        }

        [HttpGet]
        public async Task<string> GetDiscoveryFSTagRuleInfos()
        {
            try
            {
                IRMDiscoveryFSConfigurationService fsConfigurationService = new RMDiscoveryFSConfigurationService();
                return await fsConfigurationService.LoadAllTagRuleInfos();
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while load discovery FS tag rule infoes. Ex: {ex.Message}.");
                return JsonConvert.SerializeObject(new List<string>());
            }
        }
    }
}
