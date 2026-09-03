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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.MediaDatas;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [Route("api/MediaDatas/[action]")]
    public class MediaDatasController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(MediaDatasController));
        private IMediaDatasService _MediaDatasService;
        private IMediaDatasService MediaDatasService => PlatformWindsorManager.GetService(ref _MediaDatasService);

        [HttpPost]
        public async Task<bool> UpdateOrInsertMediaData([FromBody]KeyValuePair<string, string> keyValue)
        {
            try
            {
                await MediaDatasService.UpdateOrInsertMediaDataAsync(keyValue.Key, keyValue.Value);
                return true;
            }
            catch (Exception e)
            {
                logger.Error($@"Fail update or insert media data,ex:{e}");
                return false;
            }
        }

        [HttpGet]
        public async Task<string> GetMediaDatas([FromBody]string key)
        {
            try
            {
                List<MediaDataDto> result = await MediaDatasService.GetMediaDatasAsync(key);
                return SerializerHelper.SerializeByJsonConvert(result);
            }
            catch (Exception e)
            {
                logger.Error($@"Fail Get Media Datas,ex:{e}");
                return null;
            }
        }
    }
}
