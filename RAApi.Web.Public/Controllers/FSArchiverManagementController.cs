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
using AvePoint.Api.Web.ApiControllers;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Service.Services.Archiver;
using DocAveOnline.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [Route("api/FSArchiverManagement/[action]")]
    public class FSArchiverManagementController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(FSArchiverManagementController));
        private IMFSArchiverJobManagementService _FSArchiverJobManagementService;
        private IMFSArchiverJobManagementService FSArchiverJobManagementService => PlatformWindsorManager.GetService(ref _FSArchiverJobManagementService);

        [HttpPost]
        public async Task<bool> UpdateSiteMasterMediaDataSize([FromBody] string infoString)
        {
            try
            {
                JobIdStateInfo info =  SerializerHelper.DeserializeByJsonSerializer<JobIdStateInfo>(infoString);
                string subjobId = info.JobId;
                long mediaDataSize = info.MediaDataSize;
                await FSArchiverJobManagementService.UpdateSiteMasterMediaDataSizeAsync(subjobId, mediaDataSize, "");
                return true;
            }
            catch(Exception ex)
            {
                logger.Error($@"Fail update site master media data size,ex:{ex}");
                return false;
            }
        }
        [HttpPost]
        public async Task<bool> CheckCurrentJobHasMerged([FromBody] string jobId)
        {
            try
            {
                return FSArchiverJobManagementService.CheckCurrentJobHasMerged(jobId,"");
            }
            catch (Exception ex)
            {
                logger.Error($@"Fail CheckCurrentJobHasMerged,ex:{ex}");
                return false;
            }
        }
        [HttpPost]
        public async Task UpdateMergeIndexState([FromBody] string infoString)
        {
            try
            {
                JobIdStateInfo info = SerializerHelper.DeserializeByJsonSerializer<JobIdStateInfo>(infoString);
                string jobId = info.JobId;
                int mergeIndexState = info.MergeIndexState;
                FSArchiverJobManagementService.UpdateMergeIndexStateAsync(jobId,null, (MergeIndexState)mergeIndexState,"").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error($@"Fail CheckCurrentJobHasMerged,ex:{ex}");
            }
        }

    }
}
