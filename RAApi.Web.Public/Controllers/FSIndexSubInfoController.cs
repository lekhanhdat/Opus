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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.FSMasterIndex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Configuration;
using System;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [Route("api/FSIndexSubInfo/[action]")]
    public class FSIndexSubInfoController : RAWebApiBase
    {
        private IFSIndexSubInfoService _FSIndexSubInfoService;
        private IFSIndexSubInfoService FSIndexSubInfoService => PlatformWindsorManager.GetService(ref _FSIndexSubInfoService);
        [HttpPost]
        public string GetFSIndexSubinfoBySubsubJobId([FromBody] string subsubJobId)
        {
            return SerializerHelper.SerializeByJsonSerializer(FSIndexSubInfoService.GetFSIndexSubinfoBySubsubJobId(subsubJobId));
        }
        [HttpPost]
        public void UpdateFSIndexSubInfo([FromBody] string subInfo)
        {
            FSIndexSubInfoService.UpdateFSIndexSubInfo(SerializerHelper.DeserializeByJsonSerializer<ArchiverIndexSubInfoContract>(subInfo));
        }
        [HttpPost]
        public bool ExistFSIndexSubInfoBySubJobId([FromBody] string subJobId)
        {
            return FSIndexSubInfoService.ExistFSIndexSubInfoBySubJobId(subJobId);
        }
        [HttpPost]
        public void DeleteFSIndexSubInfo([FromBody] string subInfo)
        {
            FSIndexSubInfoService.DeleteFSIndexSubInfo(SerializerHelper.DeserializeByJsonSerializer<ArchiverIndexSubInfoContract>(subInfo));
        }
        [HttpPost]
        public void UpdateRetainedSizeInfo([FromBody] string retainedInfo)
        {
            RetainedInfo info = SerializerHelper.DeserializeByJsonSerializer<RetainedInfo>(retainedInfo);
            if (info.IsSimulateJob)
            {
                FSIndexSubInfoService.UpdateArchiverRetentionSimulateSize(info.RetainSize, info.RetainFileNumber);
            }
            else
            {
                FSIndexSubInfoService.UpdateArchiverIndexSubInfoMediaSize(info.SubSubJobId, info.RetainSize);
            }
        }
    }
}
